using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VehicleService.Models;
using VehicleService.DTOs;
using VehicleService.Services;
using VehicleService.Enums;

namespace VehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All endpoints require JWT authentication
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<VehiclesController> _logger;
        //private readonly IVehicleService _vehicleService;

        public VehiclesController(AppDbContext db, IHttpClientFactory httpClientFactory, ILogger<VehiclesController> logger)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            //_vehicleService = vehicleService;
        }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Vehicles.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        return vehicle == null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVehicleRequest vehicle)
    {
        if (!User?.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { message = "Authentication required" });
        }

        // Try common claim names for user id and name
        var ownerId = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("id")?.Value;

        var ownerName = User.FindFirst("name")?.Value
                        ?? User.Identity?.Name
                        ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        var newVehicle = new Vehicle
        {
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            OwnerId = ownerId,
            OwnerName = ownerName,
            ExpirationDate = DateTime.Now.AddYears(1), // Default 1 year expiration
            Status = VehicleStatus.Unregistered // Explicitly set as Unregistered
        };

        _db.Vehicles.Add(newVehicle);
        await _db.SaveChangesAsync();

        // Create initial ownership history
        var initialOwnership = new VehicleOwnershipHistory
        {
            VehicleId = newVehicle.Id,
            OwnerId = ownerId,
            OwnerName = ownerName,
            FromDate = DateTime.Now,
            ToDate = null // Current owner, so no end date
        };
        _db.VehicleOwnershipHistory.Add(initialOwnership);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Vehicle created successfully",
            data = newVehicle
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateVehicleRequest vehicle)
    {
        var existing = await _db.Vehicles.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Make = vehicle.Make;
        existing.Model = vehicle.Model;
        existing.Year = vehicle.Year;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Vehicle updated successfully",
            data = existing
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        
        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("owner/{ownerName}")]
    public async Task<IActionResult> GetByOwnerName(string ownerName)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.OwnerName == ownerName)
            .ToListAsync();
        return Ok(new
        {
            message = "Vehicles retrieved successfully",
            data = vehicles
        });
    }

    [HttpGet("owner-id/{ownerId}")]
    public async Task<IActionResult> GetByOwnerId(string ownerId)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.OwnerId == ownerId)
            .ToListAsync();
        return Ok(new
        {
            message = "Vehicles retrieved successfully",
            data = vehicles
        });
    }

    [HttpGet("{id}/ownership-history")]
    public async Task<IActionResult> GetOwnershipHistory(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        var history = await _db.VehicleOwnershipHistory
            .Where(h => h.VehicleId == id)
            .OrderByDescending(h => h.FromDate)
            .ToListAsync();

        return Ok(new
        {
            message = "Ownership history retrieved successfully",
            data = history
        });
    }

    // GET: api/vehicles/expiring?days=30
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringVehicles([FromQuery] int days = 30)
    {
        var expirationThreshold = DateTime.Now.AddDays(days);
        var expiringVehicles = await _db.Vehicles
            .Where(v => v.ExpirationDate <= expirationThreshold && v.ExpirationDate >= DateTime.Now)
            .Where(v => v.Status == VehicleStatus.Registered)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync();

        return Ok(new
        {
            message = $"Found {expiringVehicles.Count} vehicles expiring within {days} days",
            data = expiringVehicles
        });
    }

    // POST: api/vehicles/{id}/renew?months=12
    [HttpPost("{id}/renew")]
    public async Task<IActionResult> RenewRegistration(int id, [FromQuery] int months = 12)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        if (vehicle.Status != VehicleStatus.Registered)
        {
            return BadRequest(new { message = "Only registered vehicles can be renewed" });
        }

        // Extend expiration date
        var oldExpirationDate = vehicle.ExpirationDate;
        if (vehicle.ExpirationDate > DateTime.Now)
        {
            vehicle.ExpirationDate = vehicle.ExpirationDate.AddMonths(months);
        }
        else
        {
            vehicle.ExpirationDate = DateTime.Now.AddMonths(months);
        }

        await _db.SaveChangesAsync();

        // Get JWT token from request
        var token = Request.Headers["Authorization"].ToString();

        // Send notification to owner
        await SendNotificationToOwner(
            vehicle.OwnerId,
            vehicle.OwnerName,
            "Vehicle Registration Renewed",
            $"Your vehicle registration for {vehicle.RegistrationNumber} has been renewed. " +
            $"New expiration date: {vehicle.ExpirationDate:yyyy-MM-dd}. " +
            $"Previous expiration date: {oldExpirationDate:yyyy-MM-dd}.",
            token
        );

        return Ok(new
        {
            message = "Registration renewed successfully",
            data = new
            {
                vehicleId = vehicle.Id,
                registrationNumber = vehicle.RegistrationNumber,
                oldExpirationDate = oldExpirationDate,
                newExpirationDate = vehicle.ExpirationDate,
                monthsAdded = months
            }
        });
    }

    // POST: api/vehicles/{id}/notify-expiration
    [HttpPost("{id}/notify-expiration")]
    public async Task<IActionResult> NotifyExpiration(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        var daysUntilExpiration = (vehicle.ExpirationDate - DateTime.Now).Days;

        // Get JWT token from request
        var token = Request.Headers["Authorization"].ToString();

        await SendNotificationToOwner(
            vehicle.OwnerId,
            vehicle.OwnerName,
            "Vehicle Registration Expiring Soon",
            $"Your vehicle registration for {vehicle.RegistrationNumber} will expire on {vehicle.ExpirationDate:yyyy-MM-dd}. " +
            $"That's in {daysUntilExpiration} days. Please renew your registration to avoid penalties.",
            token
        );

        return Ok(new
        {
            message = "Expiration notification sent successfully",
            data = new
            {
                vehicleId = vehicle.Id,
                registrationNumber = vehicle.RegistrationNumber,
                expirationDate = vehicle.ExpirationDate,
                daysUntilExpiration = daysUntilExpiration
            }
        });
    }

    // POST: api/vehicles/notify-all-expiring?days=30
    [HttpPost("notify-all-expiring")]
    public async Task<IActionResult> NotifyAllExpiring([FromQuery] int days = 30)
    {
        var expirationThreshold = DateTime.Now.AddDays(days);
        var expiringVehicles = await _db.Vehicles
            .Where(v => v.ExpirationDate <= expirationThreshold && v.ExpirationDate >= DateTime.Now)
            .Where(v => v.Status == VehicleStatus.Registered)
            .ToListAsync();

        var successCount = 0;
        var failureCount = 0;

        // Get JWT token from request
        var token = Request.Headers["Authorization"].ToString();

        foreach (var vehicle in expiringVehicles)
        {
            var daysUntilExpiration = (vehicle.ExpirationDate - DateTime.Now).Days;
            var success = await SendNotificationToOwner(
                vehicle.OwnerId,
                vehicle.OwnerName,
                "Vehicle Registration Expiring Soon",
                $"Your vehicle registration for {vehicle.RegistrationNumber} will expire on {vehicle.ExpirationDate:yyyy-MM-dd}. " +
                $"That's in {daysUntilExpiration} days. Please renew your registration to avoid penalties.",
                token
            );

            if (success)
                successCount++;
            else
                failureCount++;
        }

        return Ok(new
        {
            message = $"Processed {expiringVehicles.Count} expiring vehicles",
            data = new
            {
                totalVehicles = expiringVehicles.Count,
                notificationsSent = successCount,
                notificationsFailed = failureCount
            }
        });
    }

    // Helper method to send notification to owner via NotificationService
    private async Task<bool> SendNotificationToOwner(string ownerId, string ownerName, string subject, string message, string authorizationToken)
    {
        try
        {
            // First, get the owner's email from AuthService
            var authClient = _httpClientFactory.CreateClient("AuthService");

            // Add Authorization header
            if (!string.IsNullOrEmpty(authorizationToken))
            {
                authClient.DefaultRequestHeaders.Remove("Authorization");
                authClient.DefaultRequestHeaders.Add("Authorization", authorizationToken);
            }

            var userResponse = await authClient.GetAsync($"/api/auth/users/{ownerId}");

            if (!userResponse.IsSuccessStatusCode)
            {
                var errorContent = await userResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to get user details for ownerId: {OwnerId}. Status: {Status}. Response: {Response}",
                    ownerId, userResponse.StatusCode, errorContent);
                return false;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();

            // The response is wrapped in { message, id, username, email, role, ... }
            var responseObj = System.Text.Json.JsonDocument.Parse(userJson);
            var email = responseObj.RootElement.GetProperty("email").GetString();

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("User {OwnerId} has no email address", ownerId);
                return false;
            }

            _logger.LogInformation("Retrieved email {Email} for user {OwnerId}", email, ownerId);

            // Now send notification via NotificationService
            var notificationClient = _httpClientFactory.CreateClient("NotificationService");
            var notificationRequest = new
            {
                userId = int.Parse(ownerId),
                recipientEmail = email,
                subject = subject,
                message = message
            };

            var notificationResponse = await notificationClient.PostAsJsonAsync(
                "/api/notification/send",
                notificationRequest
            );

            if (notificationResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Notification sent successfully to {Email} for user {OwnerId}", email, ownerId);
                return true;
            }
            else
            {
                var errorContent = await notificationResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to send notification to {Email}. Status: {Status}. Response: {Response}",
                    email, notificationResponse.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to owner {OwnerId}", ownerId);
            return false;
        }
    }

}
}