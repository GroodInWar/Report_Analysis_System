using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SeveritiesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public SeveritiesController(ApplicationDbContext dbContext)
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

    public sealed record SeverityLookupResponse(
        uint severity_id,
        string severity_name
    );
}