using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthService.DTOs;
using AuthService.Services;
using AuthService.Models;

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _db;

    public AuthController(IAuthService authService, AppDbContext db)
    {
        _authService = authService;
        _db = db;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, errors, response) = await _authService.RegisterAsync(request);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Registration failed",
                errors = errors
            });
        }

        return Ok(new
        {
            message = "User registered successfully",
            data = response
        });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, errors, response) = await _authService.LoginAsync(request);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Login failed",
                errors = errors
            });
        }

        return Ok(new
        {
            message = "Login successful",
            data = response
        });
    }

    // GET /api/auth/validate
    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromHeader(Name = "Authorization")] string? authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new { message = "No token provided" });
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var (success, userId, role) = await _authService.ValidateTokenAsync(token);

        if (!success)
        {
            return Unauthorized(new { message = "Invalid or expired token" });
        }

        return Ok(new
        {
            message = "Token is valid",
            userId = userId,
            role = role
        });
    }

    // POST /api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var (success, errors, response) = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Token refresh failed",
                errors = errors
            });
        }

        return Ok(new
        {
            message = "Token refreshed successfully",
            data = response
        });
    }

    // GET /api/auth/users/{id}
    [HttpGet("users/{id}")]
    [Authorize] // Requires valid JWT token
    public async Task<IActionResult> GetUserById(int id)
    {
        var (success, errors, user) = await _authService.GetUserByIdAsync(id);

        if (!success)
        {
            return NotFound(new
            {
                message = "User not found",
                errors = errors
            });
        }

        return Ok(new
        {
            id = user!.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role,
            createdAt = user.CreatedAt,
            lastLoginAt = user.LastLoginAt,
            isActive = user.IsActive
        });
    }

    // GET /api/auth/users - Get all users (for dropdowns/selections)
    [HttpGet("users")]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .Select(u => new
            {
                id = u.Id,
                username = u.Username,
                email = u.Email,
                role = u.Role
            })
            .ToListAsync();

        return Ok(new
        {
            count = users.Count,
            data = users
        });
    }

    // GET /api/auth/me - Get current user info
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, errors, user) = await _authService.GetUserByIdAsync(userId);

        if (!success)
        {
            return NotFound(new
            {
                message = "User not found",
                errors = errors
            });
        }

        return Ok(new
        {
            id = user!.Id,
            username = user.Username,
            email = user.Email,
            role = user.Role,
            createdAt = user.CreatedAt,
            lastLoginAt = user.LastLoginAt,
            isActive = user.IsActive
        });
    }

    // PUT /api/auth/profile - Update user profile
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, errors, userDto) = await _authService.UpdateProfileAsync(userId, request);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Profile update failed",
                errors = errors
            });
        }

        return Ok(new
        {
            message = "Profile updated successfully",
            data = userDto
        });
    }

    // PUT /api/auth/change-password - Change user password
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var (success, errors, response) = await _authService.ChangePasswordAsync(userId, request);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Password change failed",
                errors = errors
            });
        }

        return Ok(new
        {
            message = "Password changed successfully",
            data = response
        });
    }
}
