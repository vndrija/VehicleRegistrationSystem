namespace VehicleService.DTOs;

public class RegisterVehicleRequest
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

    // Registration Details
    public DateTime RegistrationDate { get; set; }
    public string RegistrationType { get; set; } = ""; // Permanent or Temporary
    public DateTime ExpirationDate { get; set; }

    // Insurance
    public DateTime? InsuranceExpirationDate { get; set; }
    public string InsuranceCompany { get; set; } = "";

    // Document Verification Flags
    public bool HasTechnicalValidityCertificate { get; set; }
    public DateTime? TechnicalValidityCertificateDate { get; set; }
    public bool HasInsuranceProof { get; set; }
    public bool HasOwnershipProof { get; set; }
    public string OwnershipProofType { get; set; } = ""; // e.g., "Invoice", "Contract", "Inheritance", "Court Decision"
    public bool HasOwnerIdentityProof { get; set; }
    public bool HasCustomsDutiesProof { get; set; } // Required for imported vehicles
    public bool HasPaymentConfirmation { get; set; }
}
