using System;

namespace VehicleService.Models;

public class VehicleOwnershipHistory
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string OwnerId { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public Vehicle? Vehicle { get; set; }
}
