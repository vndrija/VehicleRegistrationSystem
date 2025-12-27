namespace VehicleService.DTOs;

public class CreateVehicleTransferDto
{
    public int VehicleId { get; set; }
    public string ToUserId { get; set; } = "";
}
