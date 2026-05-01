using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.DTOs;
using server.Data;

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
}