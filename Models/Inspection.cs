namespace SimplyChecks.Api.Models;

// One submitted inspection for one job — the "header" record.
public class Inspection
{
    public Guid Id { get; set; }

    // Foreign key to the job this inspection belongs to.
    public Guid JobId { get; set; }
    // Navigation property: lets EF load the related Job object with
    // .Include(i => i.Job). Nullable because it's only filled on demand.
    public Job? Job { get; set; }

    // Free-text notes typed by the inspector.
    public string Notes { get; set; } = "";

    // Which phone sent this — used to detect duplicate submissions.
    public string DeviceId { get; set; } = "";

    // Tenant sign-off: typed name + the signature as stroke data (JSON)
    // which the admin page renders back into an SVG drawing.
    public string SignerName { get; set; } = "";
    public string SignatureJson { get; set; } = "";

    // Server-side timestamp of when the submission arrived (UTC).
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // One-to-many: an inspection has many results. Because this list
    // exists, EF automatically creates the foreign key relationship.
    public List<InspectionResult> Results { get; set; } = [];
}

// The outcome of ONE check item within an inspection — the "line items".
public class InspectionResult
{
    public Guid Id { get; set; }

    // Foreign key back to the parent inspection.
    public Guid InspectionId { get; set; }

    // Which checklist item this answers (e.g. "smoke-alarms").
    public required string ItemId { get; set; }

    // The inspector's verdict: "pass" or "fail".
    public required string Status { get; set; }

    // Filename of the uploaded photo, set by the photo upload endpoint.
    // Null when no photo was taken for this check.
    public string? PhotoFileName { get; set; }
}
