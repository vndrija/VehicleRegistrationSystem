using System;
using VehicleService.Enums;

namespace VehicleService.Models;

public class RegistrationRequest
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string UserId { get; set; } = "";
    public RegistrationRequestStatus Status { get; set; } = RegistrationRequestStatus.Pending;
    public DateTime TechnicalInspectionDate { get; set; }
    public string InsuranceDocPath { get; set; } = "";
    public string InspectionDocPath { get; set; } = "";
    public string IdentityDocPath { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; } // Admin username who reviewed
    public string? RejectionReason { get; set; } // Reason if rejected

    // Navigation property
    public Vehicle? Vehicle { get; set; }
}
