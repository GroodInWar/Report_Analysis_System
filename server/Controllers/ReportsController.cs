using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using Shared.Models;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private const string ErrorRetrievingReportsMessage = "An error occurred while retrieving reports.";

    public ReportsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Report>> GetReport(uint id)
    {
        var report = await _dbContext.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    [HttpPost]
    public async Task<ActionResult<Report>> PostReport(Report report)
    {
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

        _dbContext.Entry(report).State = EntityState.Modified;

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

        _dbContext.Reports.Remove(report);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private bool ReportExists(uint id)
    {
        return _dbContext.Reports.Any(e => e.report_id == id);
    }
}
