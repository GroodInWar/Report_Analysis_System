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
public class ReportFilesController : ControllerBase
{
  private readonly ApplicationDbContext _dbContext;

  public ReportFilesController(ApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  [HttpGet("reports/{reportId}")]
  public async Task<ActionResult<IEnumerable<Report_File>>> GetLinksByReport(uint reportId)
  {
    if (!await CanAccessReport(reportId))
    {
      return Forbid();
    }

    var reportFiles = await _dbContext.ReportFiles
      .AsNoTracking()
      .Where(rf => rf.report_id == reportId)
      .ToListAsync();

    return Ok(reportFiles);
  }

  [HttpGet("files/{fileId}")]
  [Authorize(Roles = "Analyst,analyst,Admin,admin")]
  public async Task<ActionResult<IEnumerable<Report_File>>> GetLinksByFile(uint fileId)
  {
    var reportFiles = await _dbContext.ReportFiles
      .AsNoTracking()
      .Where(rf => rf.file_id == fileId)
      .ToListAsync();

    return Ok(reportFiles);
  }

  [HttpGet("{reportId}/{fileId}")]
  public async Task<ActionResult<Report_File>> GetReportFileLink(uint reportId, uint fileId)
  {
    if (!await CanAccessReport(reportId))
    {
      return Forbid();
    }

    var reportFile = await _dbContext.ReportFiles
      .AsNoTracking()
      .FirstOrDefaultAsync(rf => rf.report_id == reportId && rf.file_id == fileId);

    if (reportFile == null)
    {
      return NotFound();
    }

    return Ok(reportFile);
  }

  [HttpPost]
  public async Task<IActionResult> AddReportFileLink([FromBody] Report_File reportFile)
  {
    if (reportFile == null || reportFile.report_id == 0 || reportFile.file_id == 0)
    {
      return BadRequest("Invalid report-file link data.");
    }

    if (!await CanAccessReport(reportFile.report_id))
    {
      return Forbid();
    }

    var fileExists = await _dbContext.Files
      .AsNoTracking()
      .AnyAsync(f => f.file_id == reportFile.file_id);
    if (!fileExists)
    {
      return NotFound("File not found.");
    }

    var exists = await _dbContext.ReportFiles
      .AnyAsync(rf => rf.report_id == reportFile.report_id && rf.file_id == reportFile.file_id);
    if (exists)
    {
      return Conflict("The report-file link already exists.");
    }

    _dbContext.ReportFiles.Add(reportFile);
    await _dbContext.SaveChangesAsync();

    return CreatedAtAction(
      nameof(GetReportFileLink),
      new { reportId = reportFile.report_id, fileId = reportFile.file_id },
      reportFile);
  }

  [HttpDelete("{reportId}/{fileId}")]
  public async Task<IActionResult> DeleteReportFileLink(uint reportId, uint fileId)
  {
    if (!await CanAccessReport(reportId))
    {
      return Forbid();
    }

    var reportFile = await _dbContext.ReportFiles
      .FirstOrDefaultAsync(rf => rf.report_id == reportId && rf.file_id == fileId);
    if (reportFile == null)
    {
      return NotFound("The report-file link not found.");
    }

    _dbContext.ReportFiles.Remove(reportFile);
    await _dbContext.SaveChangesAsync();

    return NoContent();
  }

  private async Task<bool> CanAccessReport(uint reportId)
  {
    if (User.IsInRole("Analyst") ||
      User.IsInRole("analyst") ||
      User.IsInRole("Admin") ||
      User.IsInRole("admin"))
    {
      return await _dbContext.Reports
        .AsNoTracking()
        .AnyAsync(r => r.report_id == reportId);
    }

    var currentUserId = GetCurrentUserId();
    if (currentUserId == null)
    {
      return false;
    }

    return await _dbContext.Reports
      .AsNoTracking()
      .AnyAsync(r => r.report_id == reportId && r.submitted_by_user_id == currentUserId.Value);
  }

  private uint? GetCurrentUserId()
  {
    var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
    return uint.TryParse(userIdValue, out var userId) ? userId : null;
  }
}
