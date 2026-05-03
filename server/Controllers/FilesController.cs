using System.Security.Cryptography;
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

  // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.requestsizelimitattribute
  [HttpPost("upload")]
  [RequestSizeLimit(MaxUploadBytes)]
  public async Task<ActionResult<Shared.Models.File>> UploadFile([FromForm] IFormFile uploadedFile, [FromQuery] uint? reportId)
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

      await _dbContext.SaveChangesAsync();
      await transaction.CommitAsync();

      return CreatedAtAction(nameof(GetFile), new { id = file.file_id }, file);
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
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    return uint.TryParse(userIdValue, out var userId) ? userId : null;
  }

  private bool IsCurrentUser(uint userId)
  {
    return GetCurrentUserId() == userId;
  }
}
