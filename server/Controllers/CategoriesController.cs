using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.DTOs;
using server.Data;
using Shared.Models;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public CategoriesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryLookupResponse>>> GetAll()
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.category_name)
            .Select(c => new CategoryLookupResponse(
                c.category_id,
                c.category_name
            ))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryLookupResponse>> GetById(uint id)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.category_id == id)
            .Select(c => new CategoryLookupResponse(
                c.category_id,
                c.category_name
            ))
            .FirstOrDefaultAsync();

        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult<CategoryLookupResponse>> Create([FromBody] CategoryCreateRequest request)
    {
        var name = request.category_name.Trim();
        if (name.Length == 0)
        {
            return BadRequest("Category name is required.");
        }

        var exists = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(c => c.category_name == name);

        if (exists)
        {
            return Conflict("A category with this name already exists.");
        }

        var category = new Category
        {
            category_name = name
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var response = new CategoryLookupResponse(
            category.category_id,
            category.category_name
        );

        return CreatedAtAction(nameof(GetById), new { id = category.category_id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult> Update(uint id, [FromBody] CategoryUpdateRequest request)
    {
        var name = request.category_name.Trim();
        if (name.Length == 0)
        {
            return BadRequest("Category name is required.");
        }

        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var exists = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(c => c.category_id != id && c.category_name == name);

        if (exists)
        {
            return Conflict("A category with this name already exists.");
        }

        category.category_name = name;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,admin")]
    public async Task<ActionResult> Delete(uint id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        var isInUse = await _dbContext.Incidents
            .AsNoTracking()
            .AnyAsync(i => i.category_id == id);

        if (isInUse)
        {
            return Conflict("Cannot delete a category that is assigned to incidents.");
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
