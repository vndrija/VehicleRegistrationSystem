using Microsoft.EntityFrameworkCore;
using VehicleService.Models;
using VehicleService.Enums;

namespace VehicleService.Services;

/// <summary>
/// Background service that runs periodically to send expiration warnings for vehicles expiring soon.
/// Runs once per day at 8 AM.
/// </summary>
public class ExpirationNotificationBackgroundService : BackgroundService
{
    private readonly ILogger<ExpirationNotificationBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _runTime = new TimeSpan(8, 0, 0); // Run at 8 AM
    private readonly int _expirationWarningDays = 30; // Warn 30 days before expiration

    public ExpirationNotificationBackgroundService(
        ILogger<ExpirationNotificationBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expiration Notification Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate next run time - 1 minute from now for testing
                var now = DateTime.Now;
                var nextRun = now.AddMinutes(1);

                var delay = nextRun - now;
                _logger.LogInformation("Next expiration check scheduled for {NextRun} (in {Delay})", nextRun, delay);

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await CheckAndNotifyExpiringVehicles();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Expiration Notification Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Expiration Notification Background Service");
                // Wait 5 minutes before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task CheckAndNotifyExpiringVehicles()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                // Find vehicles expiring within the warning period
                var expirationThreshold = DateTime.Now.AddDays(_expirationWarningDays);
                var expiringVehicles = await db.Vehicles
                    .Where(v => v.ExpirationDate <= expirationThreshold
                             && v.ExpirationDate > DateTime.Now
                             && v.Status == VehicleStatus.Registered) // Only registered vehicles
                    .OrderBy(v => v.ExpirationDate)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} vehicles expiring within {Days} days", expiringVehicles.Count, _expirationWarningDays);

                if (expiringVehicles.Count == 0)
                {
                    _logger.LogInformation("No vehicles found expiring soon. No notifications sent.");
                    return;
                }

                var successCount = 0;
                var failureCount = 0;

                foreach (var vehicle in expiringVehicles)
                {
                    try
                    {
                        var daysUntilExpiration = (vehicle.ExpirationDate - DateTime.Now).Days;
                        var success = await SendExpirationNotification(vehicle, daysUntilExpiration, httpClientFactory);

                        if (success)
                            successCount++;
                        else
                            failureCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending notification for vehicle {VehicleId}", vehicle.Id);
                        failureCount++;
                    }
                }

                _logger.LogInformation(
                    "Expiration notification batch complete: {SuccessCount} sent, {FailureCount} failed",
                    successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and notifying expiring vehicles");
            }
        }
    }

    private async Task<bool> SendExpirationNotification(Vehicle vehicle, int daysUntilExpiration, IHttpClientFactory httpClientFactory)
    {
        try
        {
            // Step 1: Get service account JWT token from AuthService
            var serviceToken = await GetServiceAccountToken(httpClientFactory);
            if (string.IsNullOrEmpty(serviceToken))
            {
                _logger.LogWarning("Failed to get service account token for vehicle {VehicleId}", vehicle.Id);
                return false;
            }

            // Step 2: Get user email from AuthService using service account token
            var authClient = httpClientFactory.CreateClient("AuthService");
            authClient.DefaultRequestHeaders.Remove("Authorization");
            authClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceToken}");

            var userResponse = await authClient.GetAsync($"/api/auth/users/{vehicle.OwnerId}");

            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user details for vehicle {VehicleId}, owner {OwnerId}",
                    vehicle.Id, vehicle.OwnerId);
                return false;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var responseObj = System.Text.Json.JsonDocument.Parse(userJson);
            var email = responseObj.RootElement.GetProperty("email").GetString();

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("User {OwnerId} has no email address", vehicle.OwnerId);
                return false;
            }

            // Step 3: Send notification via NotificationService
            var notificationClient = httpClientFactory.CreateClient("NotificationService");
            var notificationRequest = new
            {
                userId = int.Parse(vehicle.OwnerId),
                recipientEmail = email,
                subject = "Vehicle Registration Expiring Soon",
                message = $"Your vehicle registration for {vehicle.RegistrationNumber} will expire on {vehicle.ExpirationDate:yyyy-MM-dd}. " +
                         $"That's in {daysUntilExpiration} days. Please renew your registration to avoid penalties."
            };

            var notificationResponse = await notificationClient.PostAsJsonAsync(
                "/api/notification/send",
                notificationRequest
            );

            if (notificationResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Expiration notification sent to {Email} for vehicle {VehicleId} ({RegistrationNumber})",
                    email, vehicle.Id, vehicle.RegistrationNumber);
                return true;
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send expiration notification to {Email} for vehicle {VehicleId}. Status: {Status}",
                    email, vehicle.Id, notificationResponse.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending expiration notification for vehicle {VehicleId}", vehicle.Id);
            return false;
        }
    }

    private async Task<string?> GetServiceAccountToken(IHttpClientFactory httpClientFactory)
    {
        try
        {
            var authClient = httpClientFactory.CreateClient("AuthService");
            var loginRequest = new
            {
                username = "system_service",
                password = "SystemService@2026"
            };

            var response = await authClient.PostAsJsonAsync("/api/auth/login", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to authenticate service account. Status: {Status}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var responseObj = System.Text.Json.JsonDocument.Parse(json);
            var token = responseObj.RootElement.GetProperty("data").GetProperty("token").GetString();

            _logger.LogInformation("Service account authenticated successfully");
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service account token");
            return null;
        }
    }
}
