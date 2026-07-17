using Microsoft.EntityFrameworkCore;
using SimplyChecks.Api.Data;
using SimplyChecks.Api.Models;

namespace SimplyChecks.Api.Endpoints;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this WebApplication app)
    {
        // A route group: every endpoint added to it shares the "/api"
        // prefix AND the RequireAuthorization() rule — one line protects
        // them all. Requests without a valid JWT get 401 automatically.
        var group = app.MapGroup("/api").RequireAuthorization();

        // GET /api/jobs — the app calls this on sync: all open jobs.
        // The admin UI passes ?all=true to see completed ones too.
        // ("bool all = false" binds from the query string automatically.)
        group.MapGet("/jobs", async (AppDbContext db, bool all = false) =>
            await db.Jobs
                .Where(j => all || j.Status == "pending") // filter in SQL
                .OrderBy(j => j.DueDate)
                .ToListAsync()); // async: doesn't block a thread while Postgres works

        // GET /api/checklist — the master checklist the app should render.
        group.MapGet("/checklist", async (AppDbContext db) =>
            await db.CheckItems.OrderBy(c => c.SortOrder).ToListAsync());

        // POST /api/jobs — create a new job (used by the admin page).
        group.MapPost("/jobs", async (CreateJobRequest request, AppDbContext db) =>
        {
            var job = new Job
            {
                Id = Guid.NewGuid(), // server generates the id
                Address = request.Address,
                Tenant = request.Tenant,
                Phone = request.Phone,
                DueDate = request.DueDate,
            };
            db.Jobs.Add(job); // stage the insert
            await db.SaveChangesAsync(); // actually write to Postgres
            // 201 Created + a Location header pointing at the new resource.
            return Results.Created($"/api/jobs/{job.Id}", job);
        });
    }
}

// The JSON body shape for creating a job.
public record CreateJobRequest(string Address, string Tenant, string Phone, DateOnly DueDate);
