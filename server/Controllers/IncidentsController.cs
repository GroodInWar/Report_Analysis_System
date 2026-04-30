using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using Shared.Models;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public IncidentsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Incident>>> GetAll()
    {
        // TODO: Add paging, filters, lookup-name projection, and public-safe read-only visibility rules for anonymous visitors.
        var incidents = await _dbContext.Incidents
            .AsNoTracking()
            .OrderByDescending(i => i.updated_at)
            .ToListAsync();

        return Ok(incidents);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Incident>> GetIncident(uint id)
    {
        // TODO: Include only public-safe linked reports, files, comments, creator details, category, and severity for the incident detail page.
        var incident = await _dbContext.Incidents
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.incident_id == id);

        return incident == null ? NotFound() : Ok(incident);
    }

    [HttpPost]
    [Authorize(Roles = "Analyst,analyst")]
    public async Task<ActionResult<Incident>> PostIncident(Incident incident, [FromQuery] uint? sourceReportId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        incident.CreatedByUserId = currentUserId.Value;
        incident.created_at = DateTime.UtcNow;
        incident.updated_at = incident.created_at;

        _dbContext.Incidents.Add(incident);

        // TODO: Model report-to-incident links so a report can support multiple analyst-created incidents.
        // The current Report.incident_id field is one-to-one from the report side, so this intentionally
        // avoids claiming or mutating the source report until the relationship design is finalized.
        if (sourceReportId != null)
        {
            var reportExists = await _dbContext.Reports.AnyAsync(r => r.report_id == sourceReportId.Value);
            if (!reportExists)
            {
                return BadRequest("Source report does not exist.");
            }
        }

        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetIncident), new { id = incident.incident_id }, incident);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Analyst,analyst")]
    public async Task<IActionResult> PutIncident(uint id, Incident incident)
    {
        if (id != incident.incident_id)
        {
            return BadRequest();
        }

        var existing = await _dbContext.Incidents.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        if (!IsCurrentUser(existing.CreatedByUserId))
        {
            return Forbid();
        }

        // TODO: Validate category/severity ids, add concurrency handling, and restrict fields that are not analyst-editable.
        existing.incident_title = incident.incident_title;
        existing.incident_description = incident.incident_description;
        existing.category_id = incident.category_id;
        existing.severity_id = incident.severity_id;
        existing.updated_at = DateTime.UtcNow;
        existing.resolved_at = incident.resolved_at;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private uint? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return uint.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private bool IsCurrentUser(uint userId)
    {
        return GetCurrentUserId() == userId;
    }
}
