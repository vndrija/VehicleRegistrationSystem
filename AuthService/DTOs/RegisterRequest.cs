namespace AuthService.DTOs;

public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "User"; // Default to "User", can be "Admin"
}
