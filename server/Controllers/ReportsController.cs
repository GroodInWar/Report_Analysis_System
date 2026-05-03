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
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private const string ErrorRetrievingReportsMessage = "An error occurred while retrieving reports.";

    public ReportsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Roles = "Analyst,analyst")]
    public async Task<ActionResult<IEnumerable<Report>>> GetAll()
    {
        try
        {
            var reports = await _dbContext.Reports.AsNoTracking().ToListAsync();
            return Ok(reports);
        }
        catch (Exception)
        {
            return StatusCode(500, ErrorRetrievingReportsMessage);
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Report>>> GetReportsByUser(uint userId)
    {
        if (!IsCurrentUser(userId))
        {
            return Forbid();
        }

        try
        {
            var reports = await _dbContext.Reports.AsNoTracking().Where(r => r.submitted_by_user_id == userId).ToListAsync();
            return Ok(reports);
        }
        catch (Exception)
        {
            return StatusCode(500, ErrorRetrievingReportsMessage);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetReport(uint id)
    {
        var report = await _dbContext.Reports
            .AsNoTracking()
            .Where(r => r.report_id == id)
            .Select(r => new
            {
                r.report_id,
                r.submitted_by_user_id,
                SubmittedByUser = r.SubmittedByUser == null
                    ? null
                    : new
                    {
                        r.SubmittedByUser.user_id,
                        r.SubmittedByUser.first_name,
                        r.SubmittedByUser.last_name,
                        r.SubmittedByUser.username,
                        r.SubmittedByUser.email
                    },
                r.incident_id,
                Incident = r.Incident == null
                    ? null
                    : new
                    {
                        r.Incident.incident_id,
                        r.Incident.incident_title,
                        r.Incident.incident_description,
                        r.Incident.updated_at,
                        r.Incident.resolved_at
                    },
                r.title,
                r.report_text,
                r.status,
                r.submitted_at,
                r.updated_at,
                Files = (
                    from reportFile in _dbContext.ReportFiles.AsNoTracking()
                    join file in _dbContext.Files.AsNoTracking()
                        on reportFile.file_id equals file.file_id
                    where reportFile.report_id == r.report_id
                    orderby file.uploaded_at descending, file.file_id descending
                    select new
                    {
                        file.file_id,
                        file.file_name,
                        file.file_hash,
                        file.uploaded_at
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return NotFound();
        }

        if (!IsCurrentUser(report.submitted_by_user_id) && !User.IsInRole("Analyst") && !User.IsInRole("analyst"))
        {
            return Forbid();
        }

        return Ok(report);
    }

    [HttpPost]
    public async Task<ActionResult<Report>> PostReport(Report report)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        report.submitted_by_user_id = currentUserId.Value;
        report.status = ReportStatus.submitted;
        report.submitted_at = DateTime.UtcNow;
        report.updated_at = report.submitted_at;

        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReport), new { id = report.report_id }, report);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutReport(uint id, Report report)
    {
        if (id != report.report_id)
        {
            return BadRequest();
        }

        var existing = await _dbContext.Reports.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        if (!IsCurrentUser(existing.submitted_by_user_id))
        {
            return Forbid();
        }

        existing.title = report.title;
        existing.report_text = report.report_text;
        existing.updated_at = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReportExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Analyst,analyst,Admin,admin")]
    public async Task<IActionResult> UpdateReportStatus(uint id, ReportStatusUpdateRequest request)
    {
        if (!Enum.IsDefined(request.status))
        {
            return BadRequest("Invalid report status.");
        }

        var report = await _dbContext.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        if (request.status == ReportStatus.linked && report.incident_id == null)
        {
            return BadRequest("A report must be linked to an incident before using linked status.");
        }

        report.status = request.status;
        report.updated_at = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{reportId}/link-incident/{incidentId}")]
    [Authorize(Roles = "Analyst,analyst,Admin,admin")]
    public async Task<IActionResult> LinkReportToIncident(uint reportId, uint incidentId, [FromQuery] bool copyFiles = true)
    {
        var report = await _dbContext.Reports.FindAsync(reportId);
        if (report == null)
        {
            return NotFound("Report not found.");
        }

        if (report.status is ReportStatus.closed or ReportStatus.rejected)
        {
            return BadRequest("Closed or rejected reports cannot be linked to an incident.");
        }

        var incidentExists = await _dbContext.Incidents
            .AsNoTracking()
            .AnyAsync(i => i.incident_id == incidentId);
        if (!incidentExists)
        {
            return NotFound("Incident not found.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        report.incident_id = incidentId;
        report.status = ReportStatus.linked;
        report.updated_at = DateTime.UtcNow;

        if (copyFiles)
        {
            var reportFileIds = await _dbContext.ReportFiles
                .AsNoTracking()
                .Where(rf => rf.report_id == reportId)
                .Select(rf => rf.file_id)
                .ToListAsync();

            var existingIncidentFileIds = await _dbContext.IncidentFiles
                .AsNoTracking()
                .Where(ifile => ifile.incident_id == incidentId && reportFileIds.Contains(ifile.file_id))
                .Select(ifile => ifile.file_id)
                .ToListAsync();

            var existingFileIdSet = existingIncidentFileIds.ToHashSet();
            foreach (var fileId in reportFileIds)
            {
                if (!existingFileIdSet.Contains(fileId))
                {
                    _dbContext.IncidentFiles.Add(new Incident_File
                    {
                        incident_id = incidentId,
                        file_id = fileId
                    });
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReport(uint id)
    {
        var report = await _dbContext.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        if (!IsCurrentUser(report.submitted_by_user_id))
        {
            return Forbid();
        }

        _dbContext.Reports.Remove(report);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private bool ReportExists(uint id)
    {
        return _dbContext.Reports.Any(e => e.report_id == id);
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

public class ReportStatusUpdateRequest
{
    public ReportStatus status { get; set; }
}
