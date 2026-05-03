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
public class CommentsController : ControllerBase
{
  private readonly ApplicationDbContext _dbContext;

  public CommentsController(ApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Comment>> GetComment(uint id)
  {
    var comment = await _dbContext.Comments
      .AsNoTracking()
      .FirstOrDefaultAsync(c => c.comment_id == id);

    if (comment == null)
    {
      return NotFound();
    }

    return Ok(comment);
  }

  [HttpGet("incidents/{incidentId}")]
  public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByIncident(uint incidentId)
  {
    var comments = await _dbContext.Comments
      .AsNoTracking()
      .Where(c => c.incident_id == incidentId)
      .OrderBy(c => c.created_at)
      .ThenBy(c => c.comment_id)
      .ToListAsync();

    return Ok(comments);
  }

  [HttpGet("users/{userId}")]
  public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByUser(uint userId)
  {
    if (!IsCurrentUser(userId) && !IsAdmin())
    {
      return Forbid();
    }

    var comments = await _dbContext.Comments
      .AsNoTracking()
      .Where(c => c.user_id == userId)
      .OrderByDescending(c => c.created_at)
      .ThenByDescending(c => c.comment_id)
      .ToListAsync();

    return Ok(comments);
  }

  [HttpPost]
  public async Task<ActionResult<Comment>> PostComment(Comment comment)
  {
    var currentUserId = GetCurrentUserId();
    if (currentUserId == null)
    {
      return Unauthorized();
    }

    if (comment == null || comment.incident_id == 0 || string.IsNullOrWhiteSpace(comment.comment_text))
    {
      return BadRequest("Incident and comment text are required.");
    }

    var incident = await _dbContext.Incidents
      .AsNoTracking()
      .Where(i => i.incident_id == comment.incident_id)
      .Select(i => new { i.resolved_at })
      .FirstOrDefaultAsync();

    if (incident == null)
    {
      return NotFound("Incident not found.");
    }

    if (incident.resolved_at != null)
    {
      return BadRequest("Comments cannot be added to resolved incidents.");
    }

    comment.comment_id = 0;
    comment.user_id = currentUserId.Value;
    comment.comment_text = comment.comment_text.Trim();
    comment.created_at = DateTime.UtcNow;
    comment.updated_at = comment.created_at;

    _dbContext.Comments.Add(comment);
    await _dbContext.SaveChangesAsync();

    return CreatedAtAction(nameof(GetComment), new { id = comment.comment_id }, comment);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> PutComment(uint id, Comment comment)
  {
    if (comment == null || string.IsNullOrWhiteSpace(comment.comment_text))
    {
      return BadRequest("Comment text is required.");
    }

    var existing = await _dbContext.Comments.FindAsync(id);
    if (existing == null)
    {
      return NotFound();
    }

    if (!IsCurrentUser(existing.user_id) && !IsAdmin())
    {
      return Forbid();
    }

    existing.comment_text = comment.comment_text.Trim();
    existing.updated_at = DateTime.UtcNow;

    await _dbContext.SaveChangesAsync();

    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteComment(uint id)
  {
    var comment = await _dbContext.Comments.FindAsync(id);
    if (comment == null)
    {
      return NotFound();
    }

    if (!IsCurrentUser(comment.user_id) && !IsAdmin())
    {
      return Forbid();
    }

    _dbContext.Comments.Remove(comment);
    await _dbContext.SaveChangesAsync();

    return NoContent();
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

  private bool IsAdmin()
  {
    return User.IsInRole("Admin") || User.IsInRole("admin");
  }
}
