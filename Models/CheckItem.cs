namespace SimplyChecks.Api.Models;

// One item on the master checklist. The list lives on the SERVER now —
// add a row here (or via the DB) and every phone receives the new check
// on its next sync, without an app update.
public class CheckItem
{
    // A human-readable slug like "smoke-alarms". Using it as the primary
    // key (instead of a Guid) keeps the phone's data readable/debuggable.
    public required string Id { get; set; }

    // Section grouping shown as headers in the app ("Safety", "Kitchen"...).
    public required string Category { get; set; }

    // The text the inspector actually reads.
    public required string Label { get; set; }

    // Controls the display order in the app.
    public int SortOrder { get; set; }
}
