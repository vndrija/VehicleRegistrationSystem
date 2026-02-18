namespace VehicleService.DTOs;

public class TemporaryRegistrationRequest
{
    // Basic Vehicle Information
    public string RegistrationNumber { get; set; } = "";
    public string ChassisNumber { get; set; } = "";
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public string OwnerName { get; set; } = "";
    public string OwnerId { get; set; } = "";

    // Vehicle Details
    public string VehicleType { get; set; } = "";
    public string EngineNumber { get; set; } = "";
    public string Color { get; set; } = "";
    public string FuelType { get; set; } = "";
    public int EngineCapacity { get; set; }
    public int MaxWeight { get; set; }
    public int NumberOfSeats { get; set; }

    // Temporary Registration Details
    public DateTime RegistrationDate { get; set; }
    public int RegistrationPeriodMonths { get; set; } // Max 12 months

    // Customs and Import Information
    public string CustomsApprovalNumber { get; set; } = ""; // Temporary import approval number
    public DateTime CustomsApprovalDate { get; set; }
    public string CustomsOffice { get; set; } = ""; // Issuing customs office
    public string TemporaryUseReason { get; set; } = ""; // e.g., "Leasing", "Foreign business", etc.

    // Insurance
    public DateTime? InsuranceExpirationDate { get; set; }
    public string InsuranceCompany { get; set; } = "";

    // Document Verification Flags
    public bool HasTechnicalValidityCertificate { get; set; }
    public DateTime? TechnicalValidityCertificateDate { get; set; }
    public bool HasInsuranceProof { get; set; }
    public bool HasOwnershipProof { get; set; }
    public string OwnershipProofType { get; set; } = ""; // e.g., "Temporary use agreement", "Leasing contract"
    public bool HasTemporaryImportApproval { get; set; } // Required!
    public bool HasOwnerIdentityProof { get; set; }
    public bool HasPaymentConfirmation { get; set; }
}
