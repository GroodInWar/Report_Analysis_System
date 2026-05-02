using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs;
using Shared.Models;

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

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<RoleLookupResponse>> GetById(uint id)
    {
        var role = await _dbContext.Roles
            .AsNoTracking()
            .Where(r => r.role_id == id)
            .Select(r => new RoleLookupResponse(
                r.role_id,
                r.role_name
            ))
            .FirstOrDefaultAsync();

        return role == null ? NotFound() : Ok(role);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult<RoleLookupResponse>> Create([FromBody] RoleCreateRequest request)
    {
        var name = request.role_name.Trim();
        if (name.Length == 0)
        {
            return BadRequest("Role name is required.");
        }

        var exists = await _dbContext.Roles
            .AsNoTracking()
            .AnyAsync(r => r.role_name == name);

        if (exists)
        {
            return Conflict("A role with this name already exists.");
        }

        var role = new Role
        {
            role_name = name
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var response = new RoleLookupResponse(
            role.role_id,
            role.role_name
        );

        return CreatedAtAction(nameof(GetById), new { id = role.role_id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult> Update(uint id, [FromBody] RoleUpdateRequest request)
    {
        var name = request.role_name.Trim();
        if (name.Length == 0)
        {
            return BadRequest("Role name is required.");
        }

        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var exists = await _dbContext.Roles
            .AsNoTracking()
            .AnyAsync(r => r.role_id != id && r.role_name == name);

        if (exists)
        {
            return Conflict("A role with this name already exists.");
        }

        role.role_name = name;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult> Delete(uint id)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var isInUse = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.role_id == id);

        if (isInUse)
        {
            return Conflict("Cannot delete a role that is assigned to users.");
        }

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

}
