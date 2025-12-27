using Microsoft.EntityFrameworkCore;
using VehicleService.DTOs;
using VehicleService.Models;

namespace VehicleService.Services;

public class VehicleManagementService 
{
    private readonly AppDbContext _db;

    public VehicleManagementService(AppDbContext db)
    {
        _db = db;
    }

    // public async Task<(bool Success, List<string> Errors, Vehicle? Vehicle)> ChangeLicensePlateAsync(int vehicleId, ChangeLicensePlateRequest request)
    // {
    //     var validationErrors = new List<string>();

    //     // Find the vehicle
    //     var vehicle = await _db.Vehicles.FindAsync(vehicleId);
    //     if (vehicle == null)
    //     {
    //         validationErrors.Add("Vehicle not found");
    //         return (false, validationErrors, null);
    //     }

    //     // Validation: Check vehicle eligibility
    //     if (vehicle.Status != "Active")
    //     {
    //         validationErrors.Add("Cannot change license plates for inactive vehicle");
    //     }

    //     if (!vehicle.IsRegisteredInNationalRegistry)
    //     {
    //         validationErrors.Add("Vehicle must be registered in the national registry");
    //     }

    //     // Validate reason is provided
    //     if (string.IsNullOrWhiteSpace(request.Reason))
    //     {
    //         validationErrors.Add("Reason for license plate change is required");
    //     }

    //     // Validate new registration number
    //     if (string.IsNullOrWhiteSpace(request.NewRegistrationNumber))
    //     {
    //         validationErrors.Add("New registration number is required");
    //     }
    //     else
    //     {
    //         // Check if new registration number already exists
    //         var existingVehicle = await _db.Vehicles
    //             .FirstOrDefaultAsync(v => v.RegistrationNumber == request.NewRegistrationNumber && v.Id != vehicleId);
    //         if (existingVehicle != null)
    //         {
    //             validationErrors.Add($"Registration number {request.NewRegistrationNumber} already exists");
    //         }
    //     }

    //     // 1. Technical validity certificate (not older than 30 days)
    //     if (!request.HasTechnicalValidityCertificate)
    //     {
    //         validationErrors.Add("Registration list with technical validity certificate is required");
    //     }
    //     else if (request.TechnicalValidityCertificateDate.HasValue)
    //     {
    //         var daysSinceCertificate = (DateTime.Now - request.TechnicalValidityCertificateDate.Value).TotalDays;
    //         if (daysSinceCertificate > 30)
    //         {
    //             validationErrors.Add("Technical validity certificate is older than 30 days");
    //         }
    //         if (daysSinceCertificate < 0)
    //         {
    //             validationErrors.Add("Technical validity certificate date cannot be in the future");
    //         }
    //     }
    //     else
    //     {
    //         validationErrors.Add("Technical validity certificate date is required");
    //     }

    //     // 2. Insurance proof
    //     if (!request.HasInsuranceProof)
    //     {
    //         validationErrors.Add("Proof of mandatory vehicle insurance is required");
    //     }

    //     // 3. Owner identity proof
    //     if (!request.HasOwnerIdentityProof)
    //     {
    //         validationErrors.Add("Proof of owner identity is required");
    //     }

    //     // 4. Payment confirmation
    //     if (!request.HasPaymentConfirmation)
    //     {
    //         validationErrors.Add("Payment confirmation from eGovernment portal is required");
    //     }

    //     // 5. Previous license plate requirement (unless lost/stolen)
    //     var reasonLower = request.Reason.ToLower();
    //     if (!reasonLower.Contains("lost") && !reasonLower.Contains("stolen") && !request.HasPreviousLicensePlate)
    //     {
    //         validationErrors.Add("Previous license plates must be returned (unless lost or stolen)");
    //     }

    //     // Return validation errors if any
    //     if (validationErrors.Any())
    //     {
    //         return (false, validationErrors, null);
    //     }

    //     // Store old registration number for audit
    //     var oldRegistrationNumber = vehicle.RegistrationNumber;

    //     // Update vehicle with new license plate
    //     vehicle.RegistrationNumber = request.NewRegistrationNumber;

    //     // Update document verification flags
    //     vehicle.HasTechnicalValidityCertificate = request.HasTechnicalValidityCertificate;
    //     vehicle.TechnicalValidityCertificateDate = request.TechnicalValidityCertificateDate;
    //     vehicle.HasInsuranceProof = request.HasInsuranceProof;
    //     vehicle.HasOwnerIdentityProof = request.HasOwnerIdentityProof;
    //     vehicle.HasPaymentConfirmation = request.HasPaymentConfirmation;

    //     // Update technical inspection dates based on new certificate
    //     vehicle.LastTechnicalInspectionDate = request.TechnicalValidityCertificateDate;
    //     vehicle.TechnicalInspectionExpirationDate = request.TechnicalValidityCertificateDate?.AddMonths(12);

    //     // Update insurance information if provided
    //     if (request.InsuranceExpirationDate.HasValue)
    //     {
    //         vehicle.InsuranceExpirationDate = request.InsuranceExpirationDate;
    //     }
    //     if (!string.IsNullOrEmpty(request.InsuranceCompany))
    //     {
    //         vehicle.InsuranceCompany = request.InsuranceCompany;
    //     }

    //     await _db.SaveChangesAsync();

    //     // TODO: Send notification to owner about license plate change (via NotificationService)
    //     // TODO: Create audit log entry (old registration number -> new registration number)

    //     return (true, new List<string>(), vehicle);
    // }
}
