using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using Shared.Models;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private const string ErrorRetrievingReportsMessage = "An error occurred while retrieving reports.";

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Report>>> GetAll()
    {
        try
        {
            var reports = await _db.Reports.AsNoTracking().ToListAsync();
            return Ok(reports);
        }
        catch (Exception)
        {
            return StatusCode(500, ErrorRetrievingReportsMessage);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Report>> GetReport(int id)
    {
        var report = await _db.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    [HttpPost]
    public async Task<ActionResult<Report>> PostReport(Report report)
    {
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReport), new { id = report.report_id }, report);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutReport(int id, Report report)
    {
        if (id != report.report_id)
        {
            return BadRequest();
        }

        _db.Entry(report).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
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
    public async Task<IActionResult> DeleteReport(int id)
    {
        var report = await _db.Reports.FindAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        _db.Reports.Remove(report);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool ReportExists(int id)
    {
        return _db.Reports.Any(e => e.report_id == id);
    }
}