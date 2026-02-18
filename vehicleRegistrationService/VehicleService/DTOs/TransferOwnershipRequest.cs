namespace VehicleService.DTOs;

public class TransferOwnershipRequest
{
    public string NewOwnerName { get; set; } = "";
    public string NewOwnerId { get; set; } = "";
}
