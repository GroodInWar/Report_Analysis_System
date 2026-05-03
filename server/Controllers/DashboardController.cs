using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
  private readonly ApplicationDbContext _dbContext;

  public DashboardController(ApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  [HttpGet("incidents-by-severity")]
  public async Task<IActionResult> GetIncidentsBySeverity()
  {
    var result = await (
      from incident in _dbContext.Incidents.AsNoTracking()
      join severity in _dbContext.Severities.AsNoTracking()
        on incident.severity_id equals severity.severity_id
      group incident by new { severity.severity_id, severity.severity_name } into g
      orderby g.Key.severity_id
      select new
      {
        SeverityId = g.Key.severity_id,
        Severity = g.Key.severity_name,
        Count = g.Count()
      })
      .ToListAsync();

    return Ok(result);
  }

  [HttpGet("incidents-by-category")]
  public async Task<IActionResult> GetIncidentsByCategory()
  {
    var result = await (
      from incident in _dbContext.Incidents.AsNoTracking()
      join category in _dbContext.Categories.AsNoTracking()
        on incident.category_id equals category.category_id
      group incident by new { category.category_id, category.category_name } into g
      orderby g.Key.category_id
      select new
      {
        CategoryId = g.Key.category_id,
        Category = g.Key.category_name,
        Count = g.Count()
      })
      .ToListAsync();

    return Ok(result);
  }

  [HttpGet("reports-by-status")]
  public async Task<IActionResult> GetReportsByStatus()
  {
    var statusCounts = await _dbContext.Reports
      .AsNoTracking()
      .GroupBy(r => r.status)
      .Select(g => new { Status = g.Key, Count = g.Count() })
      .ToListAsync();
    var result = statusCounts
      .Select(g => new { Status = g.Status.ToString(), g.Count });

    return Ok(result);
  }

  [HttpGet("user-report-counts")]
  public async Task<IActionResult> GetUserReportCounts()
  {
    var result = await (
      from report in _dbContext.Reports.AsNoTracking()
      join user in _dbContext.Users.AsNoTracking()
        on report.submitted_by_user_id equals user.user_id
      group report by new { user.user_id, user.username } into g
      orderby g.Count() descending, g.Key.user_id
      select new
      {
        UserId = g.Key.user_id,
        Username = g.Key.username,
        Count = g.Count()
      })
      .ToListAsync();

    return Ok(result);
  }

  [HttpGet("average-time-to-resolution")]
  public async Task<IActionResult> GetAverageTimeToResolution()
  {
    var resolvedIncidents = await _dbContext.Incidents
      .AsNoTracking()
      .Where(i => i.resolved_at != null)
      .Select(i => new { i.created_at, ResolvedAt = i.resolved_at!.Value })
      .ToListAsync();

    var averageSeconds = resolvedIncidents.Count == 0
      ? 0
      : resolvedIncidents.Average(i => (i.ResolvedAt - i.created_at).TotalSeconds);

    return Ok(new
    {
      ResolvedIncidentCount = resolvedIncidents.Count,
      AverageSeconds = averageSeconds,
      AverageHours = averageSeconds / 3600,
      AverageDays = averageSeconds / 86400
    });
  }

  [HttpGet("file-count-per-incident")]
  public async Task<IActionResult> GetFileCountPerIncident()
  {
    var result = await (
      from incident in _dbContext.Incidents.AsNoTracking()
      join incidentFileCount in
        (
          from incidentFile in _dbContext.IncidentFiles.AsNoTracking()
          group incidentFile by incidentFile.incident_id into g
          select new { IncidentId = g.Key, FileCount = g.Count() }
        )
        on incident.incident_id equals incidentFileCount.IncidentId into incidentFileCounts
      from incidentFileCount in incidentFileCounts.DefaultIfEmpty()
      orderby incident.incident_id
      select new
      {
        IncidentId = incident.incident_id,
        IncidentTitle = incident.incident_title,
        FileCount = incidentFileCount == null ? 0 : incidentFileCount.FileCount
      })
      .ToListAsync();

    return Ok(result);
  }
}
