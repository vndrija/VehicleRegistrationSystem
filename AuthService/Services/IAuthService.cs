using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services;

public interface IAuthService
{
    Task<(bool Success, List<string> Errors, LoginResponse? Response)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, List<string> Errors, LoginResponse? Response)> LoginAsync(LoginRequest request);
    Task<(bool Success, List<string> Errors, User? User)> GetUserByIdAsync(int userId);
    Task<(bool Success, List<string> Errors, LoginResponse? Response)> RefreshTokenAsync(string refreshToken);
    Task<(bool Success, string? UserId, string? Role)> ValidateTokenAsync(string token);
}
