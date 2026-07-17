using Microsoft.EntityFrameworkCore;
using SimplyChecks.Api.Data;
using SimplyChecks.Api.Models;

namespace SimplyChecks.Api.Endpoints;

public static class InspectionEndpoints
{
    public static void MapInspectionEndpoints(this WebApplication app)
    {
        // Every route in this group requires a valid JWT.
        var group = app.MapGroup("/api").RequireAuthorization();

        // POST /api/inspections — step 1 of a sync: the app sends the
        // inspection JSON (answers, notes, signature — no photos yet).
        // Returns the new inspection id, which the photo uploads reference.
        group.MapPost("/inspections", async (SubmitInspectionRequest request, AppDbContext db) =>
        {
            // Same device already submitted this job? Return the existing
            // record instead of erroring — this makes the endpoint
            // "idempotent": retrying after a network drop is harmless.
            var existing = await db.Inspections.FirstOrDefaultAsync(
                i => i.JobId == request.JobId && i.DeviceId == request.DeviceId);
            if (existing is not null)
            {
                return Results.Ok(new { existing.Id, duplicate = true });
            }

            // Build the inspection graph: the header + all result rows.
            var inspection = new Inspection
            {
                Id = Guid.NewGuid(),
                JobId = request.JobId,
                Notes = request.Notes,
                DeviceId = request.DeviceId,
                // "?? """ = if the app sent null (old versions), store empty.
                SignerName = request.SignerName ?? "",
                SignatureJson = request.SignatureJson ?? "",
                Results = request.Results.Select(r => new InspectionResult
                {
                    Id = Guid.NewGuid(),
                    ItemId = r.ItemId,
                    Status = r.Status,
                }).ToList(),
            };
            // Adding the parent stages the children too (EF follows the list).
            db.Inspections.Add(inspection);

            // The job is now done — hide it from future GET /api/jobs calls.
            var job = await db.Jobs.FindAsync(request.JobId);
            if (job is not null)
            {
                job.Status = "completed"; // EF tracks this change automatically
            }

            // One SaveChanges = one database transaction: the inspection,
            // its results, and the job update all succeed or all fail.
            await db.SaveChangesAsync();
            return Results.Created($"/api/inspections/{inspection.Id}", new { inspection.Id });
        });

        // POST /api/inspections/{id}/photos — step 2 of a sync: one photo
        // per request, so a dropped connection only loses that photo and
        // the app can retry just the missing ones.
        // IFormFile = an uploaded file from a multipart/form-data body.
        group.MapPost("/inspections/{id:guid}/photos", async (
            Guid id, IFormFile photo, [Microsoft.AspNetCore.Mvc.FromForm] string itemId,
            AppDbContext db, IConfiguration config) =>
        {
            // The photo must belong to a result row we already received.
            var result = await db.InspectionResults.FirstOrDefaultAsync(
                r => r.InspectionId == id && r.ItemId == itemId);
            if (result is null)
            {
                return Results.NotFound($"No result for item '{itemId}' on inspection {id}");
            }

            // Photos are stored on DISK, only the filename goes in the DB —
            // keeps the database small and backups simple.
            var photosDir = config["Storage:PhotosPath"] ?? "storage/photos";
            Directory.CreateDirectory(photosDir); // no-op if it exists

            var fileName = $"{id}_{itemId}.jpg";
            var fullPath = Path.Combine(photosDir, fileName);
            // Stream the upload straight to the file (no loading into memory).
            await using (var stream = File.Create(fullPath))
            {
                await photo.CopyToAsync(stream);
            }

            result.PhotoFileName = fileName; // link the file to the result
            await db.SaveChangesAsync();
            return Results.Ok(new { fileName });
        }).DisableAntiforgery(); // form posts from a native app have no antiforgery token

        // GET /api/inspections — inspection reports for the admin UI.
        group.MapGet("/inspections", async (AppDbContext db) =>
            await db.Inspections
                // Include = also load these related rows (SQL JOIN),
                // otherwise Job/Results would be null/empty in the JSON.
                .Include(i => i.Job)
                .Include(i => i.Results)
                .OrderByDescending(i => i.SubmittedAt) // newest first
                .ToListAsync());

        // GET /api/photos/{fileName} — serves an uploaded photo. The admin
        // page fetches these with the JWT header and displays them as blobs.
        group.MapGet("/photos/{fileName}", (string fileName, IConfiguration config) =>
        {
            // Prevent path tricks like ../../secrets — only bare file names.
            if (fileName != Path.GetFileName(fileName))
            {
                return Results.BadRequest();
            }
            var photosDir = config["Storage:PhotosPath"] ?? "storage/photos";
            var fullPath = Path.Combine(photosDir, fileName);
            return File.Exists(fullPath)
                ? Results.File(Path.GetFullPath(fullPath), "image/jpeg")
                : Results.NotFound();
        });
    }
}

// The JSON body for submitting an inspection. The signature fields have
// "= null" defaults so older app versions that don't send them still work.
public record SubmitInspectionRequest(
    Guid JobId,
    string DeviceId,
    string Notes,
    List<SubmittedResult> Results,
    string? SignerName = null,
    string? SignatureJson = null);

// One check's answer inside the submission.
public record SubmittedResult(string ItemId, string Status);
