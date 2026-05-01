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
    public async Task<ActionResult<object>> GetIncident(uint id)
    {
        var currentUserId = GetCurrentUserId();
        var incident = await _dbContext.Incidents
            .AsNoTracking()
            .Where(i => i.incident_id == id)
            .Select(i => new
            {
                i.incident_id,
                i.CreatedByUserId,
                CreatedByUser = i.CreatedByUser == null
                    ? null
                    : new
                    {
                        i.CreatedByUser.user_id,
                        i.CreatedByUser.first_name,
                        i.CreatedByUser.last_name,
                        i.CreatedByUser.username,
                        i.CreatedByUser.email
                    },
                i.category_id,
                Category = _dbContext.Categories
                    .Where(c => c.category_id == i.category_id)
                    .Select(c => new
                    {
                        c.category_id,
                        c.category_name
                    })
                    .FirstOrDefault(),
                i.severity_id,
                Severity = _dbContext.Severities
                    .Where(s => s.severity_id == i.severity_id)
                    .Select(s => new
                    {
                        s.severity_id,
                        s.severity_name
                    })
                    .FirstOrDefault(),
                i.incident_title,
                i.incident_description,
                i.created_at,
                i.updated_at,
                i.resolved_at,
                CanEdit = currentUserId != null && i.CreatedByUserId == currentUserId.Value,
                IsOwner = currentUserId != null && i.CreatedByUserId == currentUserId.Value,
                Reports = i.Reports
                    .Select(r => new
                    {
                        r.report_id,
                        r.title,
                        status = r.status.ToString(),
                        r.submitted_at
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

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

        Report? sourceReport = null;

        if (sourceReportId != null)
        {
            sourceReport = await _dbContext.Reports
                .FirstOrDefaultAsync(r => r.report_id == sourceReportId.Value);

            if (sourceReport == null)
            {
                return BadRequest("Source report does not exist.");
            }

            if (sourceReport.status is ReportStatus.closed or ReportStatus.rejected)
            {
                return BadRequest("Closed or rejected reports cannot be linked to a new incident.");
            }
        }

        incident.CreatedByUserId = currentUserId.Value;
        incident.created_at = DateTime.UtcNow;
        incident.updated_at = incident.created_at;
        incident.resolved_at = null;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _dbContext.Incidents.Add(incident);
            await _dbContext.SaveChangesAsync();

            if (sourceReport != null)
            {
                sourceReport.incident_id = incident.incident_id;
                sourceReport.status = ReportStatus.linked;
                sourceReport.updated_at = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

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
