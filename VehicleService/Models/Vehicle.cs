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

    // Vehicle Details
    public string VehicleType { get; set; } = ""; // Car, Motorcycle, Truck, Bus
    public string EngineNumber { get; set; } = "";
    public string Color { get; set; } = "";
    public string FuelType { get; set; } = ""; // Petrol, Diesel, Electric, Hybrid
    public int EngineCapacity { get; set; } // in CC
    public int MaxWeight { get; set; } // in KG
    public int NumberOfSeats { get; set; }

    // Registration Details
    public DateTime RegistrationDate { get; set; } // First registration date
    public string RegistrationType { get; set; } = ""; // Permanent, Temporary
    public string Status { get; set; } = "Active"; // Active, Suspended, Deregistered

    // Technical Inspection
    public DateTime? LastTechnicalInspectionDate { get; set; }
    public DateTime? TechnicalInspectionExpirationDate { get; set; }

    // Insurance
    public DateTime? InsuranceExpirationDate { get; set; }
    public string InsuranceCompany { get; set; } = "";

    // Ownership
    public string OwnerId { get; set; } = ""; // Foreign key to User in AuthService
    public string PreviousOwner { get; set; } = "";

    // Document Verification (for National Vehicle Register Entry)
    public bool HasTechnicalValidityCertificate { get; set; } = false; // Registration list with technical proof (not older than 30 days)
    public DateTime? TechnicalValidityCertificateDate { get; set; } // Date when certificate was issued
    public bool HasInsuranceProof { get; set; } = false; // Proof of mandatory vehicle insurance
    public bool HasOwnershipProof { get; set; } = false; // Traffic permit, invoice, contract, inheritance decision, etc.
    public string OwnershipProofType { get; set; } = ""; // Type of ownership document
    public bool HasOwnerIdentityProof { get; set; } = false; // ID card, passport, business registry
    public bool HasCustomsDutiesProof { get; set; } = false; // Proof of paid customs duties (if applicable)
    public bool HasPaymentConfirmation { get; set; } = false; // Payment slip from eGovernment portal
    public bool IsRegisteredInNationalRegistry { get; set; } = false; // Final registration status
    public DateTime? NationalRegistryDate { get; set; } // Date when vehicle was registered in national registry

    // Temporary Registration & Customs (for Temporary Import Vehicles)
    public bool HasTemporaryImportApproval { get; set; } = false; // Approval from customs for temporary import
    public string CustomsApprovalNumber { get; set; } = ""; // Temporary import approval number
    public DateTime? CustomsApprovalDate { get; set; } // Date of customs approval
    public string CustomsOffice { get; set; } = ""; // Customs office that issued approval
    public string TemporaryUseReason { get; set; } = ""; // Reason for temporary registration (e.g., "Leasing", "Business")

}
