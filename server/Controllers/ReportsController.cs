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
    public async Task<ActionResult<Report>> GetReport(uint id)
    {
        var report = await _dbContext.Reports.FindAsync(id);
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
