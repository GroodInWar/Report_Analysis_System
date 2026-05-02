using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs;
using Shared.Models;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SeverityController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public SeverityController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SeverityLookupResponse>>> GetAll()
    {
        var severities = await _dbContext.Severities
            .AsNoTracking()
            .OrderBy(s => s.severity_id)
            .Select(s => new SeverityLookupResponse(
                s.severity_id,
                s.severity_name
            ))
            .ToListAsync();

        return Ok(severities);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<SeverityLookupResponse>> GetById(uint id)
    {
        var severity = await _dbContext.Severities
            .AsNoTracking()
            .Where(s => s.severity_id == id)
            .Select(s => new SeverityLookupResponse(
                s.severity_id,
                s.severity_name
            ))
            .FirstOrDefaultAsync();

        return severity == null ? NotFound() : Ok(severity);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult<SeverityLookupResponse>> Create([FromBody] SeverityCreateRequest request)
    {
        var name = request.severity_name.Trim();
        if (name.Length == 0)
        {
            return BadRequest("Severity name is required.");
        }

        var exists = await _dbContext.Severities
            .AsNoTracking()
            .AnyAsync(s => s.severity_name == name);

        if (exists)
        {
            return Conflict("A severity with this name already exists.");
        }

        var severity = new Severity
        {
            severity_name = name
        };

        _dbContext.Severities.Add(severity);
        await _dbContext.SaveChangesAsync();

        var response = new SeverityLookupResponse(
            severity.severity_id,
            severity.severity_name
        );

        return CreatedAtAction(nameof(GetById), new { id = severity.severity_id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult> Update(uint id, [FromBody] SeverityUpdateRequest request)
    {
        var name = request.severity_name.Trim();
        if (name.Length == 0)
        {
            return BadRequest("Severity name is required.");
        }

        var severity = await _dbContext.Severities.FindAsync(id);
        if (severity == null)
        {
            return NotFound();
        }

        var exists = await _dbContext.Severities
            .AsNoTracking()
            .AnyAsync(s => s.severity_id != id && s.severity_name == name);

        if (exists)
        {
            return Conflict("A severity with this name already exists.");
        }

        severity.severity_name = name;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult> Delete(uint id)
    {
        var severity = await _dbContext.Severities.FindAsync(id);
        if (severity == null)
        {
            return NotFound();
        }

        var isInUse = await _dbContext.Incidents
            .AsNoTracking()
            .AnyAsync(i => i.severity_id == id);

        if (isInUse)
        {
            return Conflict("Cannot delete a severity that is assigned to incidents.");
        }

        _dbContext.Severities.Remove(severity);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

}
