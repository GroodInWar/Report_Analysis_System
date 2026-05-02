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
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] uint? categoryId = null,
        [FromQuery] uint? severityId = null,
        [FromQuery] uint? createdByUserId = null,
        [FromQuery] bool? resolved = null,
        [FromQuery] string? search = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var currentUserId = GetCurrentUserId();
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var isAnalyst = User.IsInRole("Analyst") || User.IsInRole("analyst");

        var query = _dbContext.Incidents.AsNoTracking();

        if (categoryId.HasValue)
        {
            query = query.Where(i => i.category_id == categoryId.Value);
        }

        if (severityId.HasValue)
        {
            query = query.Where(i => i.severity_id == severityId.Value);
        }

        if (createdByUserId.HasValue && isAnalyst)
        {
            query = query.Where(i => i.CreatedByUserId == createdByUserId.Value);
        }

        if (resolved.HasValue)
        {
            query = resolved.Value
                ? query.Where(i => i.resolved_at != null)
                : query.Where(i => i.resolved_at == null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i =>
                EF.Functions.Like(i.incident_title, $"%{term}%") ||
                (isAuthenticated && EF.Functions.Like(i.incident_description, $"%{term}%")));
        }

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;

        var incidents = await (
            from incident in query
            join category in _dbContext.Categories.AsNoTracking()
                on incident.category_id equals category.category_id
            join severity in _dbContext.Severities.AsNoTracking()
                on incident.severity_id equals severity.severity_id
            join createdByUser in _dbContext.Users.AsNoTracking()
                on incident.CreatedByUserId equals createdByUser.user_id into createdByUsers
            from createdByUser in createdByUsers.DefaultIfEmpty()
            orderby incident.updated_at descending, incident.incident_id descending
            select new
            {
                incident.incident_id,
                CreatedByUserId = isAuthenticated ? incident.CreatedByUserId : (uint?)null,
                CreatedByUser = isAuthenticated && createdByUser != null
                    ? new
                    {
                        createdByUser.user_id,
                        createdByUser.first_name,
                        createdByUser.last_name,
                        createdByUser.username
                    }
                    : null,
                incident.category_id,
                Category = new
                {
                    category.category_id,
                    category.category_name
                },
                incident.severity_id,
                Severity = new
                {
                    severity.severity_id,
                    severity.severity_name
                },
                incident.incident_title,
                incident_description = isAuthenticated ? incident.incident_description : null,
                incident.created_at,
                incident.updated_at,
                incident.resolved_at,
                CanEdit = currentUserId.HasValue && incident.CreatedByUserId == currentUserId.Value,
                IsOwner = currentUserId.HasValue && incident.CreatedByUserId == currentUserId.Value,
                IsReadOnly = !isAuthenticated
            })
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            items = incidents
        });
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

    // TODO: Add an analyst/admin delete endpoint for incidents and define expected behavior
    // for linked reports, comments, and incident_files.

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
