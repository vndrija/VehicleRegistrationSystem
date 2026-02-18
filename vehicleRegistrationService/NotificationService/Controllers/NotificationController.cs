using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        AppDbContext context,
        IEmailService emailService,
        ILogger<NotificationController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    // DTO for sending notification
    public class SendNotificationRequest
    {
        public int UserId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // POST: api/notification/send
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            // Create notification record
            var notification = new Notification
            {
                UserId = request.UserId,
                RecipientEmail = request.RecipientEmail,
                Subject = request.Subject,
                Message = request.Message,
                SentDate = DateTime.UtcNow,
                Status = "Pending",
                Type = "Email"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send email
            var emailSent = await _emailService.SendEmailAsync(
                request.RecipientEmail,
                request.Subject,
                request.Message
            );

            // Update status
            notification.Status = emailSent ? "Sent" : "Failed";
            await _context.SaveChangesAsync();

            if (emailSent)
            {
                _logger.LogInformation(
                    "Notification sent successfully to user {UserId} at {Email}",
                    request.UserId,
                    request.RecipientEmail
                );
                return Ok(new { success = true, message = "Notification sent successfully", notificationId = notification.Id });
            }

            _logger.LogWarning(
                "Failed to send notification to user {UserId} at {Email}",
                request.UserId,
                request.RecipientEmail
            );
            return StatusCode(500, new { success = false, message = "Failed to send notification" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", request.UserId);
            return StatusCode(500, new { success = false, message = "An error occurred while sending notification" });
        }
    }

    // GET: api/notification/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(int userId)
    {
        try
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentDate)
                .ToListAsync();

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving notifications" });
        }
    }

    // GET: api/notification/history
    [HttpGet("history")]
    public async Task<IActionResult> GetNotificationHistory()
    {
        try
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.SentDate)
                .Take(100) // Limit to last 100 notifications
                .ToListAsync();

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification history");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving notification history" });
        }
    }

    // GET: api/notification/pending
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingNotifications()
    {
        try
        {
            var pendingNotifications = await _context.Notifications
                .Where(n => n.Status == "Pending" || n.Status == "Failed")
                .OrderBy(n => n.SentDate)
                .ToListAsync();

            return Ok(pendingNotifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending notifications");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving pending notifications" });
        }
    }
}
