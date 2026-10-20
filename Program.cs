
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimplyChecks.Api.Data;
using SimplyChecks.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));


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

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });
// Enables the .RequireAuthorization() checks used on our route groups.
builder.Services.AddAuthorization();

// Build the configured application.
var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();

.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));


app.MapAuthEndpoints();
app.MapJobEndpoints();
app.MapInspectionEndpoints();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Make sure there's at least one user to log in with (first run only).
await app.SeedDefaultUserAsync();

// Start listening for HTTP requests. This line blocks until shutdown.
app.Run();
