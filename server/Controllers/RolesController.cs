using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin,Admin")]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public RolesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleLookupResponse>>> GetAll()
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.role_id)
            .Select(r => new RoleLookupResponse(
                r.role_id,
                r.role_name
            ))
            .ToListAsync();

        return Ok(roles);
    }
}