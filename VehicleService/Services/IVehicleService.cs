using VehicleService.DTOs;
using VehicleService.Models;

namespace VehicleService.Services;

public interface IVehicleService
{
    Task<(bool Success, List<string> Errors, Vehicle? Vehicle)> ChangeLicensePlateAsync(int vehicleId, ChangeLicensePlateRequest request);
}
