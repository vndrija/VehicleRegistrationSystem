namespace NotificationService.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipientEmail, string subject, string message);
}
