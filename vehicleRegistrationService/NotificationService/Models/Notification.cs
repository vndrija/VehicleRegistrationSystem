namespace NotificationService.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed
    public string Type { get; set; } = "Email"; // Email, SMS (for future expansion)
}
