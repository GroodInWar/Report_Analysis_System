using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using server.Data;
using server.DTOs;
using Shared.Models;

namespace server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    private const string ErrorRetrievingUsersMessage = "An error occurred while retrieving users.";

    public AuthController(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.FirstName)
        || string.IsNullOrEmpty(request.LastName)
        || string.IsNullOrEmpty(request.Username)
        || string.IsNullOrEmpty(request.Email)
        || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("All fields are required.");
        }

        var existingUserByUsername = _dbContext.Users.FirstOrDefault(u => u.username == request.Username);
        var existingUserByEmail = _dbContext.Users.FirstOrDefault(u => u.email == request.Email);

        if (existingUserByUsername != null)
        {
            return Conflict("Username is already taken.");
        }

        if (existingUserByEmail != null)
        {
            return Conflict("Email is already registered.");
        }

        var defaultRoleId = _dbContext.Roles.FirstOrDefault(r => r.role_name == "user")?.role_id;

        if (defaultRoleId == null)
        {
            return StatusCode(500, ErrorRetrievingUsersMessage);
        }

        var user = new User
        {
            first_name = request.FirstName,
            last_name = request.LastName,
            username = request.Username,
            email = request.Email,
            password_hash = request.Password, // The client is responsible for hashing the password before sending.
            role_id = defaultRoleId ?? 2, // Assign a default role ID, or handle the case where it doesn't exist.
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCurrentUser),
            new { id = user.user_id },
            new
            {
                user_id = user.user_id,
                username = user.username,
                email = user.email,
                role_id = user.role_id
            });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.EmailOrUsername) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Email/Username and password are required.");
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.email == request.EmailOrUsername || u.username == request.EmailOrUsername);

        if (user == null || user.password_hash != request.Password)
        {
            return Unauthorized("Invalid email/username or password.");
        }

        var role = _dbContext.Roles.FirstOrDefault(r => r.role_id == user.role_id)?.role_name ?? string.Empty;
        var token = CreateJwtToken(user, role);

        user.last_login_at = DateTime.UtcNow;
        user.updated_at = DateTime.UtcNow;
        _dbContext.SaveChanges();

        return Ok(new
        {
            token,
            user = new
            {
                user_id = user.user_id,
                username = user.username,
                email = user.email,
                role
            }
        });
    }

    [HttpGet("current-user")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!uint.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.user_id == userId);

        if (user == null)
        {
            return Unauthorized();
        }

        var role = _dbContext.Roles.FirstOrDefault(r => r.role_id == user.role_id)?.role_name ?? string.Empty;

        return Ok(new
        {
            user_id = user.user_id,
            username = user.username,
            email = user.email,
            role
        });
    }

    private string CreateJwtToken(User user, string role)
    {   
        // https://www.c-sharpcorner.com/article/jwt-token-creation-authentication-and-authorization-in-asp-net-core-6-0-with-po/
        // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0&source=recommendations
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is missing.");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.user_id.ToString()),
            new(ClaimTypes.Name, user.username),
            new(ClaimTypes.Email, user.email)
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
