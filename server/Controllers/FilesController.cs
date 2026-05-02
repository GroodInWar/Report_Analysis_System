using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FilesController : ControllerBase
{
  private readonly ApplicationDbContext _dbContext;
  private static readonly Regex Sha256HashRegex = new(
    "^[0-9A-Fa-f]{64}$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  public FilesController(ApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  [HttpGet]
  [AllowAnonymous]
  public async Task<ActionResult<IEnumerable<Shared.Models.File>>> GetAll()
  {
    var files = await _dbContext.Files
      .AsNoTracking()
      .OrderByDescending(f => f.uploaded_at)
      .ToListAsync();

    return Ok(files);
  }

  [HttpGet("{id}")]
  [AllowAnonymous]
  public async Task<ActionResult<Shared.Models.File>> GetFile(uint id)
  {
    var file = await _dbContext.Files
      .AsNoTracking()
      .FirstOrDefaultAsync(f => f.file_id == id);

    if (file == null)
    {
      return NotFound();
    }

    return Ok(file);
  }

  [HttpPost]
  public async Task<ActionResult<Shared.Models.File>> PostFile(Shared.Models.File file)
  {
    var validationError = ValidateFileHash(file.file_hash);
    if (validationError != null)
    {
      return BadRequest(validationError);
    }

    file.file_id = 0;
    if (file.uploaded_at == default)
    {
      file.uploaded_at = DateTime.UtcNow;
    }

    _dbContext.Files.Add(file);
    await _dbContext.SaveChangesAsync();

    return CreatedAtAction(nameof(GetFile), new { id = file.file_id }, file);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> PutFile(uint id, Shared.Models.File file)
  {
    if (id != file.file_id)
    {
      return BadRequest();
    }

    var validationError = ValidateFileHash(file.file_hash);
    if (validationError != null)
    {
      return BadRequest(validationError);
    }

    var existing = await _dbContext.Files.FindAsync(id);
    if (existing == null)
    {
      return NotFound();
    }

    existing.file_name = file.file_name;
    existing.file_path = file.file_path;
    existing.file_hash = file.file_hash;
    existing.uploaded_at = file.uploaded_at == default ? existing.uploaded_at : file.uploaded_at;

    await _dbContext.SaveChangesAsync();

    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteFile(uint id)
  {
    var file = await _dbContext.Files.FindAsync(id);
    if (file == null)
    {
      return NotFound();
    }

    await using var transaction = await _dbContext.Database.BeginTransactionAsync();

    await _dbContext.ReportFiles
      .Where(rf => rf.file_id == id)
      .ExecuteDeleteAsync();

    await _dbContext.IncidentFiles
      .Where(ifile => ifile.file_id == id)
      .ExecuteDeleteAsync();

    _dbContext.Files.Remove(file);
    await _dbContext.SaveChangesAsync();

    await transaction.CommitAsync();

    return NoContent();
  }

  private static string? ValidateFileHash(string fileHash)
  {
    if (fileHash.Length != 64 || !Sha256HashRegex.IsMatch(fileHash))
    {
      return "file_hash must be a 64-character hexadecimal SHA-256 hash.";
    }

    return null;
  }
}
