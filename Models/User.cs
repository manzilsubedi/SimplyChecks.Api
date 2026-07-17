namespace SimplyChecks.Api.Models;

// An account that can log into the app and the admin page.
public class User
{
    public Guid Id { get; set; }

    // Used as the login identifier; a unique index (in AppDbContext)
    // prevents two accounts with the same email.
    public required string Email { get; set; }

    // Display name shown after login.
    public required string Name { get; set; }

    // NEVER the password itself — a one-way, salted hash of it.
    // At login we hash the attempt and compare hashes; the original
    // password can't be recovered even if the database leaks.
    public required string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
