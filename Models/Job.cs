namespace SimplyChecks.Api.Models;

// A house that needs inspecting. EF Core turns this class into the
// "Jobs" table in Postgres — each property becomes a column.
public class Job
{
    // Guid = a universally unique id. EF treats a property named "Id"
    // as the primary key by convention (no attribute needed).
    public Guid Id { get; set; }

    // "required" = C# forces callers to set this when creating a Job,
    // and EF makes the column NOT NULL.
    public required string Address { get; set; }
    public required string Tenant { get; set; }
    public required string Phone { get; set; }

    // DateOnly stores just a calendar date (no time-of-day) — right for
    // a due date. Maps to Postgres's "date" column type.
    public DateOnly DueDate { get; set; }

    // Job lifecycle: "pending" until an inspection is submitted for it.
    public string Status { get; set; } = "pending"; // pending | completed

    // Set once at creation time. Always store times in UTC on servers —
    // convert to local time only when displaying.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
