using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using Shared.Models;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class IncidentFilesController : ControllerBase
{
  private readonly ApplicationDbContext _dbContext;

  public IncidentFilesController(ApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  [HttpGet("incident/{incidentId}")]
  public async Task<ActionResult<IEnumerable<Incident_File>>> GetLinksByIncident(uint incidentId)
  {
    var incidentFiles = await _dbContext.IncidentFiles
      .AsNoTracking()
      .Where(i_f => i_f.incident_id == incidentId)
      .ToListAsync();

    return Ok(incidentFiles);
  }

  [HttpGet("files/{fileId}")]
  public async Task<ActionResult<IEnumerable<Incident_File>>> GetLinksByFile(uint fileId)
  {
    var incidentFiles = await _dbContext.IncidentFiles
      .AsNoTracking()
      .Where(i_f => i_f.file_id == fileId)
      .ToListAsync();

    return Ok(incidentFiles);
  }

  [HttpGet("{incidentId}/{fileId}")]
  public async Task<ActionResult<Incident_File>> GetIncidentFileLink(uint incidentId, uint fileId)
  {
    var incidentFile = await _dbContext.IncidentFiles
      .AsNoTracking()
      .FirstOrDefaultAsync(i_f => i_f.incident_id == incidentId && i_f.file_id == fileId);

    if (incidentFile == null)
    {
      return NotFound();
    }

    return Ok(incidentFile);
  }

  [HttpPost]
  [Authorize(Roles = "Analyst,analyst,Admin,admin")]
  public async Task<IActionResult> AddIncidentFileLink([FromBody] Incident_File incidentFile)
  {
    if (incidentFile == null || incidentFile.incident_id == 0 || incidentFile.file_id == 0)
    {
      return BadRequest("Invalid incident-file link data.");
    }

    var exists = await _dbContext.IncidentFiles
      .AnyAsync(i_f => i_f.incident_id == incidentFile.incident_id && i_f.file_id == incidentFile.file_id);
    if (exists)
    {
      return Conflict("The incident-file link already exists.");
    }

    _dbContext.IncidentFiles.Add(incidentFile);
    await _dbContext.SaveChangesAsync();

    return CreatedAtAction(
      nameof(GetIncidentFileLink),
      new { incidentId = incidentFile.incident_id, fileId = incidentFile.file_id },
      incidentFile);
  }

  [HttpDelete("{incidentId}/{fileId}")]
  [Authorize(Roles = "Analyst,analyst,Admin,admin")]
  public async Task<IActionResult> DeleteIncidentFileLink(uint incidentId, uint fileId)
  {
    var deleted = await _dbContext.IncidentFiles
      .Where(i_f => i_f.incident_id == incidentId && i_f.file_id == fileId)
      .ExecuteDeleteAsync();
    if (deleted == 0)
    {
      return NotFound("The incident-file link not found.");
    }

    return NoContent();
  }
}
