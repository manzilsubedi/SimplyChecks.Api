// The entry point of the API. In a minimal API there's no Startup class —
// everything is configured right here, top to bottom, then app.Run()
// starts the web server and blocks forever serving requests.
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimplyChecks.Api.Data;
using SimplyChecks.Api.Endpoints;

// The builder collects configuration + services before the app is built.
var builder = WebApplication.CreateBuilder(args);

// Register the DbContext in the dependency injection (DI) container with
// the Postgres connection from appsettings.json. After this, any endpoint
// can just ask for "AppDbContext db" as a parameter and receive one.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Teach the app how to VALIDATE incoming JWTs (which our own
// /api/auth/login endpoint issued). On every request with an
// "Authorization: Bearer <token>" header, this middleware checks the
// token's signature and expiry and, if valid, sets the current user.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // token must say it came from us
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false, // we don't use the audience claim
            ValidateLifetime = true, // reject expired tokens
            // The same secret key used to SIGN tokens is used to verify
            // them (symmetric key). If the signature doesn't match, the
            // token was forged or tampered with -> rejected.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });
// Enables the .RequireAuthorization() checks used on our route groups.
builder.Services.AddAuthorization();

// Build the configured application.
var app = builder.Build();

// Serve the admin UI from wwwroot: UseDefaultFiles maps "/" to
// index.html, UseStaticFiles actually serves the files.
app.UseDefaultFiles();
app.UseStaticFiles();

// Order matters: authentication (who are you?) must run before
// authorization (are you allowed?).
app.UseAuthentication();
app.UseAuthorization();

// Simple unauthenticated endpoint to check the service is alive.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Wire up all our endpoint groups (defined in the Endpoints folder).
app.MapAuthEndpoints();
app.MapJobEndpoints();
app.MapInspectionEndpoints();

// Make sure there's at least one user to log in with (first run only).
await app.SeedDefaultUserAsync();

// Start listening for HTTP requests. This line blocks until shutdown.
app.Run();
