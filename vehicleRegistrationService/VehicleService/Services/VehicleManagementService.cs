using Microsoft.EntityFrameworkCore;
using VehicleService.DTOs;
using VehicleService.Models;

namespace VehicleService.Services;

public class VehicleManagementService
{
    private readonly AppDbContext _db;

    public VehicleManagementService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, List<string> Errors, Vehicle? Vehicle)> ChangeLicensePlateAsync(int vehicleId, ChangeLicensePlateRequest request)
    {
        var validationErrors = new List<string>();

        // Find the vehicle
        var vehicle = await _db.Vehicles.FindAsync(vehicleId);
        if (vehicle == null)
        {
            validationErrors.Add("Vehicle not found");
            return (false, validationErrors, null);
        }

        // Validate new registration number
        if (string.IsNullOrWhiteSpace(request.NewRegistrationNumber))
        {
            validationErrors.Add("New registration number is required");
        }
        else
        {
            // Check if new registration number already exists
            var existingVehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.RegistrationNumber == request.NewRegistrationNumber && v.Id != vehicleId);
            if (existingVehicle != null)
            {
                validationErrors.Add($"Registration number {request.NewRegistrationNumber} already exists");
            }
        }

        // Return validation errors if any
        if (validationErrors.Any())
        {
            return (false, validationErrors, null);
        }

        // Store old registration number for audit
        var oldRegistrationNumber = vehicle.RegistrationNumber;

        // Update vehicle with new license plate
        vehicle.RegistrationNumber = request.NewRegistrationNumber;

        await _db.SaveChangesAsync();

        return (true, new List<string>(), vehicle);
    }
}
