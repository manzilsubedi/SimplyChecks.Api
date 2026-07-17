using Microsoft.EntityFrameworkCore;
using SimplyChecks.Api.Models;

namespace SimplyChecks.Api.Data;

// The DbContext is EF Core's "session" with the database: it tracks
// entities, translates LINQ queries into SQL, and saves changes.
// The constructor receives options (connection string, provider) from
// dependency injection — see AddDbContext in Program.cs.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Each DbSet<T> represents one table. "db.Jobs.Where(...)" becomes
    // "SELECT ... FROM Jobs WHERE ..." under the hood.
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<CheckItem> CheckItems => Set<CheckItem>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<InspectionResult> InspectionResults => Set<InspectionResult>();
    public DbSet<User> Users => Set<User>();

    // Fine-tunes the schema beyond what EF infers from the classes.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // A device could submit the same inspection twice on a flaky network —
        // this unique index lets us detect and ignore the duplicate.
        modelBuilder.Entity<Inspection>()
            .HasIndex(i => new { i.JobId, i.DeviceId })
            .IsUnique();

        // No two accounts may share an email address.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Seed the master checklist. HasData is baked into migrations —
        // these rows are inserted when the database is created/updated.
        modelBuilder.Entity<CheckItem>().HasData(
            new CheckItem { Id = "smoke-alarms", Category = "Safety", Label = "Smoke alarms working", SortOrder = 1 },
            new CheckItem { Id = "co-detector", Category = "Safety", Label = "Carbon monoxide detector", SortOrder = 2 },
            new CheckItem { Id = "exits-clear", Category = "Safety", Label = "Exits clear and accessible", SortOrder = 3 },
            new CheckItem { Id = "kitchen-taps", Category = "Kitchen", Label = "Taps and sink drainage", SortOrder = 4 },
            new CheckItem { Id = "kitchen-appliances", Category = "Kitchen", Label = "Appliances working", SortOrder = 5 },
            new CheckItem { Id = "kitchen-cabinets", Category = "Kitchen", Label = "Cabinets and drawers", SortOrder = 6 },
            new CheckItem { Id = "bathroom-toilet", Category = "Bathroom", Label = "Toilet flushing properly", SortOrder = 7 },
            new CheckItem { Id = "bathroom-shower", Category = "Bathroom", Label = "Shower / bath condition", SortOrder = 8 },
            new CheckItem { Id = "bathroom-mould", Category = "Bathroom", Label = "No mould or damp", SortOrder = 9 },
            new CheckItem { Id = "walls-ceilings", Category = "General", Label = "Walls and ceilings condition", SortOrder = 10 },
            new CheckItem { Id = "windows-locks", Category = "General", Label = "Windows open and lock", SortOrder = 11 },
            new CheckItem { Id = "doors-locks", Category = "General", Label = "Doors and locks working", SortOrder = 12 },
            new CheckItem { Id = "flooring", Category = "General", Label = "Flooring condition", SortOrder = 13 },
            new CheckItem { Id = "heating", Category = "General", Label = "Heating system working", SortOrder = 14 },
            new CheckItem { Id = "electrical", Category = "General", Label = "Power points and lights", SortOrder = 15 }
        );
    }
}
