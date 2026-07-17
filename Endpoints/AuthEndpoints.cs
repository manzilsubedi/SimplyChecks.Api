using System.IdentityModel.Tokens.Jwt; // builds/serializes JWT tokens
using System.Security.Claims; // the "facts about a user" inside a token
using System.Text;
using Microsoft.AspNetCore.Identity; // PasswordHasher
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimplyChecks.Api.Data;
using SimplyChecks.Api.Models;

namespace SimplyChecks.Api.Endpoints;

// "static class + extension methods" is the standard way to organize
// minimal-API endpoints into separate files.
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // POST /api/auth/login — the ONLY data endpoint without auth
        // (you can't need a token to get your first token).
        // Parameters are bound automatically: LoginRequest from the JSON
        // body, AppDbContext and IConfiguration from dependency injection.
        app.MapPost("/api/auth/login", async (
            LoginRequest request, AppDbContext db, IConfiguration config) =>
        {
            // Normalize the email so "Foo@Bar.com " still matches.
            var user = await db.Users.FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLowerInvariant().Trim());

            // Same response whether the email or the password was wrong —
            // don't help attackers figure out which emails exist.
            if (user is null)
            {
                return Results.Unauthorized();
            }

            // Hash the attempted password the same way and compare.
            var hasher = new PasswordHasher<User>();
            var verdict = hasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);
            if (verdict == PasswordVerificationResult.Failed)
            {
                return Results.Unauthorized();
            }

            // Build the JWT: signed claims about who this user is.
            // The app sends it back on every request; the signature proves
            // it was us who issued it, without any database lookup.
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"], // "who issued this token"
                claims:
                [
                    // Standard claim types the framework understands:
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                ],
                expires: DateTime.UtcNow.AddDays(30), // token lifetime
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)); // HMAC-SHA256 signature

            // Return the serialized token plus display info for the app.
            return Results.Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                name = user.Name,
                email = user.Email,
            });
        });
    }

    // Creates the first user at startup if none exist, so you can log in.
    // Called from Program.cs after the app is built.
    public static async Task SeedDefaultUserAsync(this WebApplication app)
    {
        // Endpoints get scoped services automatically; here at startup we
        // must create a scope ourselves to resolve the DbContext.
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await db.Users.AnyAsync())
        {
            return; // users exist — nothing to seed
        }

        // Credentials come from appsettings.json (SeedUser section) with
        // fallbacks, so production can use different values than dev.
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var email = config["SeedUser:Email"] ?? "admin@simplychecks.local";
        var password = config["SeedUser:Password"] ?? "ChangeMe123!";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = "Admin",
            PasswordHash = "", // placeholder — set right below
        };
        // HashPassword needs the user instance, hence the two-step dance.
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}

// A "record": a compact immutable class. ASP.NET binds the incoming JSON
// body's fields to these constructor parameters by name.
public record LoginRequest(string Email, string Password);
