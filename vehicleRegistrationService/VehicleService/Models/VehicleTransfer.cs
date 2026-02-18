using System;
using VehicleService.Enums;

namespace VehicleService.Models;

public class VehicleTransfer
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string FromUserId { get; set; } = "";
    public string ToUserId { get; set; } = "";
    public VehicleTransferStatus Status { get; set; } = VehicleTransferStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? RespondedAt { get; set; }

    public Vehicle? Vehicle { get; set; }
}
