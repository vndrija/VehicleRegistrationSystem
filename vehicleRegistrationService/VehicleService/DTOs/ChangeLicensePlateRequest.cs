namespace VehicleService.DTOs;

public class ChangeLicensePlateRequest
{
    // New Registration Details
    public string NewRegistrationNumber { get; set; } = "";
    public string Reason { get; set; } = ""; // e.g., "Damaged", "Lost", "Stolen", "Owner preference"

    // Document Verification Flags (required for change)
    public bool HasTechnicalValidityCertificate { get; set; }
    public DateTime? TechnicalValidityCertificateDate { get; set; }
    public bool HasInsuranceProof { get; set; }
    public bool HasOwnerIdentityProof { get; set; }
    public bool HasPaymentConfirmation { get; set; }
    public bool HasPreviousLicensePlate { get; set; } // Old plates must be returned (if not lost/stolen)

    // Optional: Updated insurance information
    public DateTime? InsuranceExpirationDate { get; set; }
    public string? InsuranceCompany { get; set; }
}
