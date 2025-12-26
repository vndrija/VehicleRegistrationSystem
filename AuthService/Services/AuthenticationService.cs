using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.DTOs;
using AuthService.Models;
using BCrypt.Net;

namespace AuthService.Services;

public class AuthenticationService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expireMinutes;
    private readonly int _refreshTokenExpireDays;

    public AuthenticationService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;

        var jwtSettings = configuration.GetSection("Jwt");
        _secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        _audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        _expireMinutes = int.Parse(jwtSettings["ExpireMinutes"] ?? "60");
        _refreshTokenExpireDays = int.Parse(jwtSettings["RefreshTokenExpireDays"] ?? "7");
    }

    public async Task<(bool Success, List<string> Errors, LoginResponse? Response)> RegisterAsync(RegisterRequest request)
    {
        var errors = new List<string>();

        // Validation
        if (string.IsNullOrWhiteSpace(request.Username))
            errors.Add("Username is required");
        else if (request.Username.Length < 3)
            errors.Add("Username must be at least 3 characters");

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required");
        else if (request.Password.Length < 6)
            errors.Add("Password must be at least 6 characters");

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required");
        else if (!request.Email.Contains("@"))
            errors.Add("Invalid email format");

        // Check if username already exists
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
            errors.Add("Username already exists");

        // Check if email already exists
        var existingEmail = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingEmail != null)
            errors.Add("Email already exists");

        if (errors.Any())
            return (false, errors, null);

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Email = request.Email,
            Role = request.Role == "Admin" ? "Admin" : "User", // Only allow User or Admin
            CreatedAt = DateTime.Now,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Generate tokens
        var (token, refreshToken, expiresAt) = GenerateTokens(user);

        var response = new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        };

        return (true, new List<string>(), response);
    }

    public async Task<(bool Success, List<string> Errors, LoginResponse? Response)> LoginAsync(LoginRequest request)
    {
        var errors = new List<string>();

        // Find user
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            errors.Add("Invalid username or password");
            return (false, errors, null);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            errors.Add("User account is inactive");
            return (false, errors, null);
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            errors.Add("Invalid username or password");
            return (false, errors, null);
        }

        // Update last login
        user.LastLoginAt = DateTime.Now;
        await _db.SaveChangesAsync();

        // Generate tokens
        var (token, refreshToken, expiresAt) = GenerateTokens(user);

        var response = new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        };

        return (true, new List<string>(), response);
    }

    public async Task<(bool Success, List<string> Errors, User? User)> GetUserByIdAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, new List<string> { "User not found" }, null);
        }

        return (true, new List<string>(), user);
    }

    public async Task<(bool Success, List<string> Errors, LoginResponse? Response)> RefreshTokenAsync(string refreshToken)
    {
        // For simplicity, we'll decode the refresh token and validate it
        // In production, you'd store refresh tokens in DB with expiration

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return (false, new List<string> { "Invalid refresh token" }, null);
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                return (false, new List<string> { "User not found or inactive" }, null);
            }

            // Generate new tokens
            var (token, newRefreshToken, expiresAt) = GenerateTokens(user);

            var response = new LoginResponse
            {
                Token = token,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            };

            return (true, new List<string>(), response);
        }
        catch (Exception)
        {
            return (false, new List<string> { "Invalid or expired refresh token" }, null);
        }
    }

    public async Task<(bool Success, string? UserId, string? Role)> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return (false, null, null);
            }

            return (true, userIdClaim, roleClaim);
        }
        catch
        {
            return (false, null, null);
        }
    }

    private (string Token, string RefreshToken, DateTime ExpiresAt) GenerateTokens(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var expiresAt = DateTime.UtcNow.AddMinutes(_expireMinutes);

        // Access token claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Refresh token (longer expiration)
        var refreshTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(_refreshTokenExpireDays),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var refreshTokenObj = tokenHandler.CreateToken(refreshTokenDescriptor);
        var refreshToken = tokenHandler.WriteToken(refreshTokenObj);

        return (tokenString, refreshToken, expiresAt);
    }
}
