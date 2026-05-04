using System.Security.Cryptography;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using Shared.Models;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FilesController : ControllerBase
{
  private readonly ApplicationDbContext _dbContext;
  private readonly IWebHostEnvironment _environment;
  private const long MaxUploadBytes = 25 * 1024 * 1024;
  private const string UploadFolderName = "files";
  private static readonly Regex Sha256HashRegex = new(
    "^[0-9A-Fa-f]{64}$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  public FilesController(ApplicationDbContext dbContext, IWebHostEnvironment environment)
  {
    _dbContext = dbContext;
    _environment = environment;
  }

  [HttpGet]
  [Authorize(Roles = "Analyst,analyst,Admin,admin")]
  public async Task<ActionResult<IEnumerable<object>>> GetAll()
  {
    var files = await _dbContext.Files
      .AsNoTracking()
      .OrderByDescending(f => f.uploaded_at)
      .ToListAsync();

    return Ok(files.Select(ToFileMetadata));
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<object>> GetFile(uint id)
  {
    var file = await _dbContext.Files
      .AsNoTracking()
      .FirstOrDefaultAsync(f => f.file_id == id);

    if (file == null)
    {
      return NotFound();
    }

    if (!await CanAccessFile(id))
    {
      return Forbid();
    }

    return Ok(ToFileMetadata(file));
  }

  [HttpGet("{id}/download")]
  public async Task<IActionResult> DownloadFile(uint id)
  {
    var file = await _dbContext.Files
      .AsNoTracking()
      .FirstOrDefaultAsync(f => f.file_id == id);

    if (file == null)
    {
      return NotFound();
    }

    if (!await CanAccessFile(id))
    {
      return Forbid();
    }

    var storedPath = GetStoredFilePath(file.file_path);
    if (storedPath == null || !System.IO.File.Exists(storedPath))
    {
      return NotFound("The stored file is missing.");
    }

    return PhysicalFile(storedPath, "application/octet-stream", file.file_name);
  }

  [HttpPost]
  [Authorize(Roles = "Admin,admin")]
  public async Task<ActionResult<object>> PostFile(Shared.Models.File file)
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

    return CreatedAtAction(nameof(GetFile), new { id = file.file_id }, ToFileMetadata(file));
  }

  // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.requestsizelimitattribute
  [HttpPost("upload")]
  [RequestSizeLimit(MaxUploadBytes)]
  public async Task<ActionResult<object>> UploadFile(
    [FromForm] IFormFile uploadedFile,
    [FromQuery] uint? reportId,
    [FromQuery] uint? incidentId)
  {
    if (uploadedFile == null || uploadedFile.Length == 0)
    {
      return BadRequest("A non-empty file is required.");
    }

    if (uploadedFile.Length > MaxUploadBytes)
    {
      return BadRequest($"File exceeds the {MaxUploadBytes / 1024 / 1024} MB upload limit.");
    }

    var originalFileName = Path.GetFileName(uploadedFile.FileName);
    if (string.IsNullOrWhiteSpace(originalFileName))
    {
      return BadRequest("File name is required.");
    }

    if (originalFileName.Length > 255)
    {
      originalFileName = originalFileName[..255];
    }

    if (reportId.HasValue && incidentId.HasValue)
    {
      return BadRequest("Upload can be linked to either a report or an incident, not both.");
    }

    Report? reportToLink = null;
    if (reportId.HasValue)
    {
      reportToLink = await _dbContext.Reports
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.report_id == reportId.Value);

      if (reportToLink == null)
      {
        return NotFound("Report not found.");
      }

      if (!IsCurrentUser(reportToLink.submitted_by_user_id) && !User.IsInRole("Analyst") && !User.IsInRole("analyst"))
      {
        return Forbid();
      }
    }

    Incident? incidentToLink = null;
    if (incidentId.HasValue)
    {
      incidentToLink = await _dbContext.Incidents
        .AsNoTracking()
        .FirstOrDefaultAsync(i => i.incident_id == incidentId.Value);

      if (incidentToLink == null)
      {
        return NotFound("Incident not found.");
      }

      if (!CanManageIncident(incidentToLink.CreatedByUserId))
      {
        return Forbid();
      }
    }

    var uploadRoot = GetUploadRoot();
    Directory.CreateDirectory(uploadRoot);

    var tempPath = Path.Combine(uploadRoot, $"{Guid.NewGuid():N}.uploading");
    string? finalPath = null;

    await using var transaction = await _dbContext.Database.BeginTransactionAsync();

    try
    {
      var hash = await SaveUploadToTempFile(uploadedFile, tempPath);
      var file = new Shared.Models.File
      {
        file_name = originalFileName,
        file_path = $"{UploadFolderName}/pending",
        file_hash = hash,
        uploaded_at = DateTime.UtcNow
      };

      _dbContext.Files.Add(file);
      await _dbContext.SaveChangesAsync();

      var storedFileName = $"{file.file_id}_{hash}";
      finalPath = Path.Combine(uploadRoot, storedFileName);
      EnsurePathIsInsideUploadRoot(uploadRoot, finalPath);

      if (System.IO.File.Exists(finalPath))
      {
        await transaction.RollbackAsync();
        DeleteIfExists(tempPath);
        return Conflict("A stored file with this generated name already exists.");
      }

      System.IO.File.Move(tempPath, finalPath);

      file.file_path = $"{UploadFolderName}/{storedFileName}";

      if (reportToLink != null)
      {
        _dbContext.ReportFiles.Add(new Report_File
        {
          report_id = reportToLink.report_id,
          file_id = file.file_id
        });
      }

      if (incidentToLink != null)
      {
        _dbContext.IncidentFiles.Add(new Incident_File
        {
          incident_id = incidentToLink.incident_id,
          file_id = file.file_id
        });
      }

      await _dbContext.SaveChangesAsync();
      await transaction.CommitAsync();

      return CreatedAtAction(nameof(GetFile), new { id = file.file_id }, ToFileMetadata(file));
    }
    catch
    {
      await transaction.RollbackAsync();
      DeleteIfExists(tempPath);
      if (finalPath != null)
      {
        DeleteIfExists(finalPath);
      }

      throw;
    }
  }

  [HttpPut("{id}")]
  [Authorize(Roles = "Admin,admin")]
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
  [Authorize(Roles = "Admin,admin")]
  public async Task<IActionResult> DeleteFile(uint id)
  {
    var file = await _dbContext.Files.FindAsync(id);
    if (file == null)
    {
      return NotFound();
    }

    await using var transaction = await _dbContext.Database.BeginTransactionAsync();

    var reportFiles = await _dbContext.ReportFiles
      .Where(rf => rf.file_id == id)
      .ToListAsync();
    _dbContext.ReportFiles.RemoveRange(reportFiles);

    var incidentFiles = await _dbContext.IncidentFiles
      .Where(ifile => ifile.file_id == id)
      .ToListAsync();
    _dbContext.IncidentFiles.RemoveRange(incidentFiles);

    var storedPath = GetStoredFilePath(file.file_path);

    _dbContext.Files.Remove(file);
    await _dbContext.SaveChangesAsync();

    await transaction.CommitAsync();

    if (storedPath != null)
    {
      DeleteIfExists(storedPath);
    }

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

  private string GetUploadRoot()
  {
    return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, UploadFolderName));
  }

  private static async Task<string> SaveUploadToTempFile(IFormFile uploadedFile, string tempPath)
  {
    await using var input = uploadedFile.OpenReadStream();
    await using var output = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

    var buffer = new byte[81920];
    int bytesRead;
    while ((bytesRead = await input.ReadAsync(buffer)) > 0)
    {
      hasher.AppendData(buffer, 0, bytesRead);
      await output.WriteAsync(buffer.AsMemory(0, bytesRead));
    }

    return Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
  }

  private static void EnsurePathIsInsideUploadRoot(string uploadRoot, string path)
  {
    var normalizedRoot = Path.GetFullPath(uploadRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    var normalizedPath = Path.GetFullPath(path);

    if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
    {
      throw new InvalidOperationException("Resolved file path escaped the upload folder.");
    }
  }

  private string? GetStoredFilePath(string filePath)
  {
    if (string.IsNullOrWhiteSpace(filePath))
    {
      return null;
    }

    var normalizedRelativePath = filePath.Replace('/', Path.DirectorySeparatorChar);
    var uploadRoot = GetUploadRoot();
    var fullPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, normalizedRelativePath));
    EnsurePathIsInsideUploadRoot(uploadRoot, fullPath);
    return fullPath;
  }

  private static void DeleteIfExists(string path)
  {
    if (System.IO.File.Exists(path))
    {
      System.IO.File.Delete(path);
    }
  }

  private uint? GetCurrentUserId()
  {
    var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return uint.TryParse(userIdValue, out var userId) ? userId : null;
  }

  private bool IsCurrentUser(uint userId)
  {
    return GetCurrentUserId() == userId;
  }

  private bool IsAnalystOrAdmin()
  {
    return User.IsInRole("Analyst") ||
      User.IsInRole("analyst") ||
      User.IsInRole("Admin") ||
      User.IsInRole("admin");
  }

  private bool CanManageIncident(uint createdByUserId)
  {
    return IsAnalystOrAdmin();
  }

  private async Task<bool> CanAccessFile(uint fileId)
  {
    if (IsAnalystOrAdmin())
    {
      return true;
    }

    var currentUserId = GetCurrentUserId();
    if (currentUserId == null)
    {
      return false;
    }

    var ownsLinkedReport = await (
      from reportFile in _dbContext.ReportFiles.AsNoTracking()
      join report in _dbContext.Reports.AsNoTracking()
        on reportFile.report_id equals report.report_id
      where reportFile.file_id == fileId && report.submitted_by_user_id == currentUserId.Value
      select reportFile.file_id)
      .AnyAsync();

    if (ownsLinkedReport)
    {
      return true;
    }

    return await (
      from incidentFile in _dbContext.IncidentFiles.AsNoTracking()
      join incident in _dbContext.Incidents.AsNoTracking()
        on incidentFile.incident_id equals incident.incident_id
      where incidentFile.file_id == fileId && incident.CreatedByUserId == currentUserId.Value
      select incidentFile.file_id)
      .AnyAsync();
  }

  private static object ToFileMetadata(Shared.Models.File file)
  {
    return new
    {
      file.file_id,
      file.file_name,
      file.file_hash,
      file.uploaded_at
    };
  }
}
