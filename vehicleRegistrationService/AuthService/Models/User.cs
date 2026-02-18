using System;

namespace AuthService.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "User"; // Default role: "User" or "Admin"
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}
