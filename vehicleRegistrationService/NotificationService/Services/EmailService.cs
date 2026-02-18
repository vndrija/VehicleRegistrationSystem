using MailKit.Net.Smtp;
using MimeKit;

namespace NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string message)
    {
        try
        {
            _logger.LogInformation("=== Starting email send process ===");
            _logger.LogInformation("Recipient: {Email}", recipientEmail);
            _logger.LogInformation("Subject: {Subject}", subject);

            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = _configuration["Email:SmtpPort"];
            var username = _configuration["Email:Username"];
            var senderEmail = _configuration["Email:SenderEmail"];

            _logger.LogInformation("SMTP Config: Host={Host}, Port={Port}, Username={Username}",
                smtpHost, smtpPort, username);

            if (string.IsNullOrEmpty(smtpHost))
            {
                _logger.LogWarning("SMTP Host is not configured. Email will only be logged, not sent.");
                _logger.LogInformation("Email would be sent to: {Email}", recipientEmail);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Message: {Message}", message);
                return true;
            }

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(
                _configuration["Email:SenderName"] ?? "eUprava System",
                senderEmail ?? "noreply@euprava.rs"
            ));
            emailMessage.To.Add(new MailboxAddress("", recipientEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #333;'>eUprava Notification</h2>
                        <div style='padding: 20px; background-color: #f5f5f5; border-radius: 5px;'>
                            {message}
                        </div>
                        <hr style='margin-top: 20px;'/>
                        <p style='color: #666; font-size: 12px;'>
                            This is an automated notification from eUprava Vehicle Registration System.
                        </p>
                    </body>
                    </html>"
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            _logger.LogInformation("Connecting to SMTP server: {Host}:{Port}", smtpHost, smtpPort);

            using var client = new SmtpClient();

            await client.ConnectAsync(
                smtpHost,
                int.Parse(smtpPort ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            _logger.LogInformation("Connected to SMTP server. Authenticating...");

            await client.AuthenticateAsync(username, _configuration["Email:Password"]);

            _logger.LogInformation("Authenticated. Sending email...");

            await client.SendAsync(emailMessage);

            _logger.LogInformation("Email sent. Disconnecting...");

            await client.DisconnectAsync(true);

            _logger.LogInformation("✅ Email sent successfully to {Email}", recipientEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {Email}. Error: {Message}", recipientEmail, ex.Message);
            return false;
        }
    }
}
