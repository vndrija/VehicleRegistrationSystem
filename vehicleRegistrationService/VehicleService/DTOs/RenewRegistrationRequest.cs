namespace VehicleService.DTOs;

public class RenewRegistrationRequest
{
    // Renewal period
    public int Months { get; set; } = 12; // Default 12 months

    // Document Verification Flags (required for renewal)
    public bool HasTechnicalValidityCertificate { get; set; }
    public DateTime? TechnicalValidityCertificateDate { get; set; }
    public bool HasInsuranceProof { get; set; }
    public bool HasOwnerIdentityProof { get; set; }
    public bool HasPaymentConfirmation { get; set; }

    // Optional: Updated insurance information
    public DateTime? InsuranceExpirationDate { get; set; }
    public string? InsuranceCompany { get; set; }
}
