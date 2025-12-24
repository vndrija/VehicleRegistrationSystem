using System;

namespace VehicleService.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = "";
    public string ChassisNumber { get; set; } = "";
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public string OwnerName { get; set; } = "";
    public DateTime ExpirationDate { get; set; }

}
