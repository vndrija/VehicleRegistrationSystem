using System;

namespace VehicleService.DTOs;

public class UpdateVehicleRequest
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

}
