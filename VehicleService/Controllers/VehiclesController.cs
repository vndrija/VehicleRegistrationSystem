using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VehicleService.Models;
using VehicleService.DTOs;
using VehicleService.Services;

namespace VehicleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // All endpoints require JWT authentication
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IVehicleService _vehicleService;

        public VehiclesController(AppDbContext db, IVehicleService vehicleService)
        {
            _db = db;
            _vehicleService = vehicleService;
        }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Vehicles.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        return vehicle == null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Vehicle vehicle)
    {
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Vehicle vehicle)
    {
        var existing = await _db.Vehicles.FindAsync(id);
        if (existing == null) return NotFound();

        existing.RegistrationNumber = vehicle.RegistrationNumber;
        existing.ChassisNumber = vehicle.ChassisNumber;
        existing.Make = vehicle.Make;
        existing.Model = vehicle.Model;
        existing.Year = vehicle.Year;
        existing.OwnerName = vehicle.OwnerName;
        existing.ExpirationDate = vehicle.ExpirationDate;
        existing.VehicleType = vehicle.VehicleType;
        existing.EngineNumber = vehicle.EngineNumber;
        existing.Color = vehicle.Color;
        existing.FuelType = vehicle.FuelType;
        existing.EngineCapacity = vehicle.EngineCapacity;
        existing.MaxWeight = vehicle.MaxWeight;
        existing.NumberOfSeats = vehicle.NumberOfSeats;
        existing.RegistrationDate = vehicle.RegistrationDate;
        existing.RegistrationType = vehicle.RegistrationType;
        existing.Status = vehicle.Status;
        existing.LastTechnicalInspectionDate = vehicle.LastTechnicalInspectionDate;
        existing.TechnicalInspectionExpirationDate = vehicle.TechnicalInspectionExpirationDate;
        existing.InsuranceExpirationDate = vehicle.InsuranceExpirationDate;
        existing.InsuranceCompany = vehicle.InsuranceCompany;
        existing.OwnerId = vehicle.OwnerId;
        existing.PreviousOwner = vehicle.PreviousOwner;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        
        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        return NoContent();
    }
    
    // ADVANCED FEATURE #1: Check expiring registrations
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring()
    {
        var threshold = DateTime.Now.AddDays(30);
        var expiring = await _db.Vehicles
            .Where(v => v.ExpirationDate <= threshold && v.ExpirationDate >= DateTime.Now)
            .ToListAsync();
        return Ok(expiring);
    }

    // ADVANCED FEATURE #2: Dashboard statistics
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalVehicles = await _db.Vehicles.CountAsync();
        var activeVehicles = await _db.Vehicles.CountAsync(v => v.Status == "Active");
        var expiringSoon = await _db.Vehicles.CountAsync(v =>
            v.ExpirationDate <= DateTime.Now.AddDays(30) && v.ExpirationDate >= DateTime.Now);
        var inspectionDue = await _db.Vehicles.CountAsync(v =>
            v.TechnicalInspectionExpirationDate != null &&
            v.TechnicalInspectionExpirationDate <= DateTime.Now.AddDays(30));
        var byType = await _db.Vehicles
            .GroupBy(v => v.VehicleType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();
        var byFuelType = await _db.Vehicles
            .GroupBy(v => v.FuelType)
            .Select(g => new { FuelType = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            TotalVehicles = totalVehicles,
            ActiveVehicles = activeVehicles,
            ExpiringSoon = expiringSoon,
            InspectionDue = inspectionDue,
            ByType = byType,
            ByFuelType = byFuelType
        });
    }

    // ADVANCED FEATURE #3: Search with multiple criteria
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string? vehicleType,
        [FromQuery] string? fuelType,
        [FromQuery] string? status,
        [FromQuery] int? minYear,
        [FromQuery] int? maxYear)
    {
        var vehicles = _db.Vehicles.AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            vehicles = vehicles.Where(v =>
                v.RegistrationNumber.Contains(query) ||
                v.Make.Contains(query) ||
                v.Model.Contains(query) ||
                v.ChassisNumber.Contains(query) ||
                v.OwnerName.Contains(query));
        }

        if (!string.IsNullOrEmpty(vehicleType))
            vehicles = vehicles.Where(v => v.VehicleType == vehicleType);

        if (!string.IsNullOrEmpty(fuelType))
            vehicles = vehicles.Where(v => v.FuelType == fuelType);

        if (!string.IsNullOrEmpty(status))
            vehicles = vehicles.Where(v => v.Status == status);

        if (minYear.HasValue)
            vehicles = vehicles.Where(v => v.Year >= minYear.Value);

        if (maxYear.HasValue)
            vehicles = vehicles.Where(v => v.Year <= maxYear.Value);

        return Ok(await vehicles.ToListAsync());
    }

    // ADVANCED FEATURE #4: Renew registration (Issue/Renew License Plate)
    // Издавање регистрационе налепнице - License plate sticker issuance/renewal
    [HttpPost("{id}/renew")]
    public async Task<IActionResult> RenewRegistration(int id, [FromBody] RenewRegistrationRequest request)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound(new { message = "Vehicle not found" });

        // Validation: Check vehicle eligibility
        var validationErrors = new List<string>();

        // Vehicle must be Active
        if (vehicle.Status != "Active")
            validationErrors.Add("Cannot renew registration for inactive vehicle");

        // Vehicle must already be registered in national registry
        if (!vehicle.IsRegisteredInNationalRegistry)
            validationErrors.Add("Vehicle must be registered in the national registry before renewal");

        // 1. Technical validity certificate (not older than 30 days)
        if (!request.HasTechnicalValidityCertificate)
        {
            validationErrors.Add("Registration list with technical validity certificate is required");
        }
        else if (request.TechnicalValidityCertificateDate.HasValue)
        {
            var daysSinceCertificate = (DateTime.Now - request.TechnicalValidityCertificateDate.Value).TotalDays;
            if (daysSinceCertificate > 30)
            {
                validationErrors.Add("Technical validity certificate is older than 30 days");
            }
            if (daysSinceCertificate < 0)
            {
                validationErrors.Add("Technical validity certificate date cannot be in the future");
            }
        }
        else
        {
            validationErrors.Add("Technical validity certificate date is required");
        }

        // 2. Insurance proof
        if (!request.HasInsuranceProof)
        {
            validationErrors.Add("Proof of mandatory vehicle insurance is required");
        }

        // 3. Owner identity proof
        if (!request.HasOwnerIdentityProof)
        {
            validationErrors.Add("Proof of owner identity is required (ID card, passport, business registry)");
        }

        // 4. Payment confirmation
        if (!request.HasPaymentConfirmation)
        {
            validationErrors.Add("Payment confirmation from eGovernment portal is required");
        }

        // Renewal period validation
        var validPeriods = new[] { 3, 6, 12, 24 };
        if (!validPeriods.Contains(request.Months))
        {
            validationErrors.Add("Renewal period must be 3, 6, 12, or 24 months");
        }

        // Return validation errors if any
        if (validationErrors.Any())
        {
            return BadRequest(new
            {
                message = "Registration renewal validation failed",
                errors = validationErrors
            });
        }

        // Calculate new expiration date
        var newExpirationDate = vehicle.ExpirationDate > DateTime.Now
            ? vehicle.ExpirationDate.AddMonths(request.Months)
            : DateTime.Now.AddMonths(request.Months);

        // Update vehicle
        vehicle.ExpirationDate = newExpirationDate;

        // Update document verification flags
        vehicle.HasTechnicalValidityCertificate = request.HasTechnicalValidityCertificate;
        vehicle.TechnicalValidityCertificateDate = request.TechnicalValidityCertificateDate;
        vehicle.HasInsuranceProof = request.HasInsuranceProof;
        vehicle.HasOwnerIdentityProof = request.HasOwnerIdentityProof;
        vehicle.HasPaymentConfirmation = request.HasPaymentConfirmation;

        // Update technical inspection dates based on new certificate
        vehicle.LastTechnicalInspectionDate = request.TechnicalValidityCertificateDate;
        vehicle.TechnicalInspectionExpirationDate = request.TechnicalValidityCertificateDate?.AddMonths(12);

        // Update insurance information if provided
        if (request.InsuranceExpirationDate.HasValue)
        {
            vehicle.InsuranceExpirationDate = request.InsuranceExpirationDate;
        }
        if (!string.IsNullOrEmpty(request.InsuranceCompany))
        {
            vehicle.InsuranceCompany = request.InsuranceCompany;
        }

        await _db.SaveChangesAsync();

        // TODO: Send notification to owner about successful renewal (via NotificationService)
        // TODO: Create audit log entry for renewal

        return Ok(new
        {
            message = "Registration renewed successfully (License plate sticker issued)",
            vehicleId = vehicle.Id,
            registrationNumber = vehicle.RegistrationNumber,
            renewalDate = DateTime.Now,
            newExpirationDate = vehicle.ExpirationDate,
            renewalPeriod = $"{request.Months} months",
            technicalInspectionExpirationDate = vehicle.TechnicalInspectionExpirationDate
        });
    }

    // ADVANCED FEATURE #5: Vehicles needing technical inspection
    [HttpGet("technical-inspection-due")]
    public async Task<IActionResult> GetTechnicalInspectionDue([FromQuery] int days = 30)
    {
        var threshold = DateTime.Now.AddDays(days);
        var vehicles = await _db.Vehicles
            .Where(v => v.TechnicalInspectionExpirationDate != null &&
                       v.TechnicalInspectionExpirationDate <= threshold &&
                       v.TechnicalInspectionExpirationDate >= DateTime.Now &&
                       v.Status == "Active")
            .ToListAsync();
        return Ok(vehicles);
    }

    // ADVANCED FEATURE #6: Transfer ownership
    [HttpPut("{id}/transfer-ownership")]
    public async Task<IActionResult> TransferOwnership(int id, [FromBody] TransferOwnershipRequest request)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        if (vehicle.Status != "Active")
            return BadRequest(new { message = "Cannot transfer inactive vehicle" });

        vehicle.PreviousOwner = vehicle.OwnerName;
        vehicle.OwnerName = request.NewOwnerName;
        vehicle.OwnerId = request.NewOwnerId;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Ownership transferred successfully" });
    }

    // ADVANCED FEATURE #7: Filter by status
    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.Status == status)
            .ToListAsync();
        return Ok(vehicles);
    }

    // ADVANCED FEATURE #8: Deregister vehicle
    [HttpPost("{id}/deregister")]
    public async Task<IActionResult> Deregister(int id, [FromBody] DeregisterRequest? request)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();
        if (vehicle.Status == "Deregistered")
            return BadRequest(new { message = "Vehicle already deregistered" });

        vehicle.Status = "Deregistered";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Vehicle deregistered successfully" });
    }

    // ADVANCED FEATURE #9: Get vehicles by owner
    [HttpGet("by-owner/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerId)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.OwnerId == ownerId)
            .ToListAsync();
        return Ok(vehicles);
    }

    // Get vehicles by owner name (for profile page)
    [HttpGet("owner/{ownerName}")]
    public async Task<IActionResult> GetByOwnerName(string ownerName)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.OwnerName == ownerName)
            .ToListAsync();
        return Ok(new
        {
            message = "Vehicles retrieved successfully",
            data = vehicles
        });
    }

    // ADVANCED FEATURE #10: Update technical inspection
    [HttpPost("{id}/update-inspection")]
    public async Task<IActionResult> UpdateTechnicalInspection(int id, [FromBody] InspectionUpdateRequest request)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle == null) return NotFound();

        vehicle.LastTechnicalInspectionDate = request.InspectionDate;
        vehicle.TechnicalInspectionExpirationDate = request.InspectionDate.AddMonths(request.ValidityMonths);

        await _db.SaveChangesAsync();
        return Ok(new {
            message = "Technical inspection updated successfully",
            expirationDate = vehicle.TechnicalInspectionExpirationDate
        });
    }

    // ADVANCED FEATURE #11: Register vehicle in National Vehicle Registry
    // Based on Serbian eGovernment requirements
    [HttpPost("register")]
    public async Task<IActionResult> RegisterVehicle([FromBody] RegisterVehicleRequest request)
    {
        // Validation: Check all required documents are provided
        var validationErrors = new List<string>();

        // 1. Technical validity certificate (not older than 30 days)
        if (!request.HasTechnicalValidityCertificate)
        {
            validationErrors.Add("Registration list with technical validity certificate is required");
        }
        else if (request.TechnicalValidityCertificateDate.HasValue)
        {
            var daysSinceCertificate = (DateTime.Now - request.TechnicalValidityCertificateDate.Value).TotalDays;
            if (daysSinceCertificate > 30)
            {
                validationErrors.Add("Technical validity certificate is older than 30 days");
            }
        }
        else
        {
            validationErrors.Add("Technical validity certificate date is required");
        }

        // 2. Insurance proof
        if (!request.HasInsuranceProof)
        {
            validationErrors.Add("Proof of mandatory vehicle insurance is required");
        }

        // 3. Ownership proof
        if (!request.HasOwnershipProof)
        {
            validationErrors.Add("Proof of vehicle ownership is required (invoice, contract, inheritance decision, etc.)");
        }
        else if (string.IsNullOrEmpty(request.OwnershipProofType))
        {
            validationErrors.Add("Ownership proof type must be specified");
        }

        // 4. Owner identity proof
        if (!request.HasOwnerIdentityProof)
        {
            validationErrors.Add("Proof of owner identity is required (ID card, passport, business registry)");
        }

        // 5. Payment confirmation
        if (!request.HasPaymentConfirmation)
        {
            validationErrors.Add("Payment confirmation from eGovernment portal is required");
        }

        // Note: Customs duties proof only required for imported vehicles (not enforced here)

        // Basic data validation
        if (string.IsNullOrEmpty(request.RegistrationNumber))
            validationErrors.Add("Registration number is required");

        if (string.IsNullOrEmpty(request.ChassisNumber))
            validationErrors.Add("Chassis number (VIN) is required");

        if (string.IsNullOrEmpty(request.Make) || string.IsNullOrEmpty(request.Model))
            validationErrors.Add("Vehicle make and model are required");

        if (request.Year < 1900 || request.Year > DateTime.Now.Year + 1)
            validationErrors.Add($"Vehicle year must be between 1900 and {DateTime.Now.Year + 1}");

        if (string.IsNullOrEmpty(request.OwnerName) || string.IsNullOrEmpty(request.OwnerId))
            validationErrors.Add("Owner name and ID are required");

        if (request.ExpirationDate <= DateTime.Now)
            validationErrors.Add("Registration expiration date must be in the future");

        if (request.RegistrationDate > DateTime.Now)
            validationErrors.Add("Registration date cannot be in the future");

        // Check for duplicate registration number or chassis number
        var existingByRegNumber = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.RegistrationNumber == request.RegistrationNumber);
        if (existingByRegNumber != null)
            validationErrors.Add($"Vehicle with registration number {request.RegistrationNumber} already exists");

        var existingByChassis = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.ChassisNumber == request.ChassisNumber);
        if (existingByChassis != null)
            validationErrors.Add($"Vehicle with chassis number {request.ChassisNumber} already exists");

        // Return validation errors if any
        if (validationErrors.Any())
        {
            return BadRequest(new
            {
                message = "Vehicle registration validation failed",
                errors = validationErrors
            });
        }

        // Create new vehicle entity
        var vehicle = new Vehicle
        {
            // Basic Information
            RegistrationNumber = request.RegistrationNumber,
            ChassisNumber = request.ChassisNumber,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            OwnerName = request.OwnerName,
            OwnerId = request.OwnerId,
            ExpirationDate = request.ExpirationDate,

            // Vehicle Details
            VehicleType = request.VehicleType,
            EngineNumber = request.EngineNumber,
            Color = request.Color,
            FuelType = request.FuelType,
            EngineCapacity = request.EngineCapacity,
            MaxWeight = request.MaxWeight,
            NumberOfSeats = request.NumberOfSeats,

            // Registration Details
            RegistrationDate = request.RegistrationDate,
            RegistrationType = request.RegistrationType,
            Status = "Active",

            // Insurance
            InsuranceExpirationDate = request.InsuranceExpirationDate,
            InsuranceCompany = request.InsuranceCompany,

            // Document Verification
            HasTechnicalValidityCertificate = request.HasTechnicalValidityCertificate,
            TechnicalValidityCertificateDate = request.TechnicalValidityCertificateDate,
            HasInsuranceProof = request.HasInsuranceProof,
            HasOwnershipProof = request.HasOwnershipProof,
            OwnershipProofType = request.OwnershipProofType,
            HasOwnerIdentityProof = request.HasOwnerIdentityProof,
            HasCustomsDutiesProof = request.HasCustomsDutiesProof,
            HasPaymentConfirmation = request.HasPaymentConfirmation,

            // Mark as registered in national registry
            IsRegisteredInNationalRegistry = true,
            NationalRegistryDate = DateTime.Now,

            // Set technical inspection from the technical validity certificate
            LastTechnicalInspectionDate = request.TechnicalValidityCertificateDate,
            TechnicalInspectionExpirationDate = request.TechnicalValidityCertificateDate?.AddMonths(12)
        };

        // Add to database
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        // TODO: Send notification to owner about successful registration (via NotificationService)
        // TODO: Create audit log entry

        return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, new
        {
            message = "Vehicle successfully registered in National Vehicle Registry",
            vehicleId = vehicle.Id,
            registrationNumber = vehicle.RegistrationNumber,
            nationalRegistryDate = vehicle.NationalRegistryDate,
            expirationDate = vehicle.ExpirationDate,
            vehicle = vehicle
        });
    }

    // ADVANCED FEATURE #12: Temporary vehicle registration
    // Привремена регистрација возила - For temporary import vehicles
    [HttpPost("register-temporary")]
    public async Task<IActionResult> RegisterTemporaryVehicle([FromBody] TemporaryRegistrationRequest request)
    {
        // Validation: Check all required documents are provided
        var validationErrors = new List<string>();

        // 1. Technical validity certificate (not older than 30 days)
        if (!request.HasTechnicalValidityCertificate)
        {
            validationErrors.Add("Registration list with technical validity certificate is required");
        }
        else if (request.TechnicalValidityCertificateDate.HasValue)
        {
            var daysSinceCertificate = (DateTime.Now - request.TechnicalValidityCertificateDate.Value).TotalDays;
            if (daysSinceCertificate > 30)
            {
                validationErrors.Add("Technical validity certificate is older than 30 days");
            }
        }
        else
        {
            validationErrors.Add("Technical validity certificate date is required");
        }

        // 2. Insurance proof
        if (!request.HasInsuranceProof)
        {
            validationErrors.Add("Proof of mandatory vehicle insurance is required");
        }

        // 3. Ownership proof (or basis for temporary use)
        if (!request.HasOwnershipProof)
        {
            validationErrors.Add("Proof of vehicle ownership or basis for temporary use is required");
        }
        else if (string.IsNullOrEmpty(request.OwnershipProofType))
        {
            validationErrors.Add("Ownership proof type must be specified (e.g., Leasing contract, Temporary use agreement)");
        }

        // 4. REQUIRED: Temporary import approval from customs
        if (!request.HasTemporaryImportApproval)
        {
            validationErrors.Add("Temporary import approval from customs is required");
        }
        else
        {
            if (string.IsNullOrEmpty(request.CustomsApprovalNumber))
                validationErrors.Add("Customs approval number is required");

            if (string.IsNullOrEmpty(request.CustomsOffice))
                validationErrors.Add("Issuing customs office is required");

            if (request.CustomsApprovalDate == default)
                validationErrors.Add("Customs approval date is required");
        }

        // 5. Owner identity proof
        if (!request.HasOwnerIdentityProof)
        {
            validationErrors.Add("Proof of owner identity is required (ID card, passport, business registry)");
        }

        // 6. Payment confirmation
        if (!request.HasPaymentConfirmation)
        {
            validationErrors.Add("Payment confirmation from eGovernment portal is required");
        }

        // Temporary registration period validation - Maximum 1 year (12 months)
        if (request.RegistrationPeriodMonths < 1 || request.RegistrationPeriodMonths > 12)
        {
            validationErrors.Add("Temporary registration period must be between 1 and 12 months");
        }

        // Basic data validation
        if (string.IsNullOrEmpty(request.RegistrationNumber))
            validationErrors.Add("Registration number is required");

        if (string.IsNullOrEmpty(request.ChassisNumber))
            validationErrors.Add("Chassis number (VIN) is required");

        if (string.IsNullOrEmpty(request.Make) || string.IsNullOrEmpty(request.Model))
            validationErrors.Add("Vehicle make and model are required");

        if (request.Year < 1900 || request.Year > DateTime.Now.Year + 1)
            validationErrors.Add($"Vehicle year must be between 1900 and {DateTime.Now.Year + 1}");

        if (string.IsNullOrEmpty(request.OwnerName) || string.IsNullOrEmpty(request.OwnerId))
            validationErrors.Add("Owner name and ID are required");

        if (request.RegistrationDate > DateTime.Now)
            validationErrors.Add("Registration date cannot be in the future");

        if (string.IsNullOrEmpty(request.TemporaryUseReason))
            validationErrors.Add("Reason for temporary registration is required");

        // Check for duplicate registration number or chassis number
        var existingByRegNumber = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.RegistrationNumber == request.RegistrationNumber);
        if (existingByRegNumber != null)
            validationErrors.Add($"Vehicle with registration number {request.RegistrationNumber} already exists");

        var existingByChassis = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.ChassisNumber == request.ChassisNumber);
        if (existingByChassis != null)
            validationErrors.Add($"Vehicle with chassis number {request.ChassisNumber} already exists");

        // Return validation errors if any
        if (validationErrors.Any())
        {
            return BadRequest(new
            {
                message = "Temporary vehicle registration validation failed",
                errors = validationErrors
            });
        }

        // Calculate expiration date based on registration period
        var expirationDate = request.RegistrationDate.AddMonths(request.RegistrationPeriodMonths);

        // Create new vehicle entity
        var vehicle = new Vehicle
        {
            // Basic Information
            RegistrationNumber = request.RegistrationNumber,
            ChassisNumber = request.ChassisNumber,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            OwnerName = request.OwnerName,
            OwnerId = request.OwnerId,
            ExpirationDate = expirationDate,

            // Vehicle Details
            VehicleType = request.VehicleType,
            EngineNumber = request.EngineNumber,
            Color = request.Color,
            FuelType = request.FuelType,
            EngineCapacity = request.EngineCapacity,
            MaxWeight = request.MaxWeight,
            NumberOfSeats = request.NumberOfSeats,

            // Registration Details - TEMPORARY
            RegistrationDate = request.RegistrationDate,
            RegistrationType = "Temporary", // Key difference!
            Status = "Active",

            // Insurance
            InsuranceExpirationDate = request.InsuranceExpirationDate,
            InsuranceCompany = request.InsuranceCompany,

            // Document Verification
            HasTechnicalValidityCertificate = request.HasTechnicalValidityCertificate,
            TechnicalValidityCertificateDate = request.TechnicalValidityCertificateDate,
            HasInsuranceProof = request.HasInsuranceProof,
            HasOwnershipProof = request.HasOwnershipProof,
            OwnershipProofType = request.OwnershipProofType,
            HasOwnerIdentityProof = request.HasOwnerIdentityProof,
            HasPaymentConfirmation = request.HasPaymentConfirmation,

            // Temporary Import & Customs Information
            HasTemporaryImportApproval = request.HasTemporaryImportApproval,
            CustomsApprovalNumber = request.CustomsApprovalNumber,
            CustomsApprovalDate = request.CustomsApprovalDate,
            CustomsOffice = request.CustomsOffice,
            TemporaryUseReason = request.TemporaryUseReason,

            // Mark as registered in national registry (temporary)
            IsRegisteredInNationalRegistry = true,
            NationalRegistryDate = DateTime.Now,

            // Set technical inspection from the technical validity certificate
            LastTechnicalInspectionDate = request.TechnicalValidityCertificateDate,
            TechnicalInspectionExpirationDate = request.TechnicalValidityCertificateDate?.AddMonths(12)
        };

        // Add to database
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        // TODO: Send notification to owner about successful temporary registration (via NotificationService)
        // TODO: Create audit log entry

        return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, new
        {
            message = "Vehicle successfully registered with TEMPORARY registration",
            vehicleId = vehicle.Id,
            registrationNumber = vehicle.RegistrationNumber,
            registrationType = vehicle.RegistrationType,
            registrationDate = vehicle.RegistrationDate,
            expirationDate = vehicle.ExpirationDate,
            registrationPeriod = $"{request.RegistrationPeriodMonths} months",
            customsApprovalNumber = vehicle.CustomsApprovalNumber,
            customsOffice = vehicle.CustomsOffice,
            temporaryUseReason = vehicle.TemporaryUseReason,
            vehicle = vehicle
        });
    }

    // ADVANCED FEATURE #13: Change license plates
    // Promena registarskih tablica - Issue new license plates
    [HttpPost("{id}/change-license-plate")]
    public async Task<IActionResult> ChangeLicensePlate(int id, [FromBody] ChangeLicensePlateRequest request)
    {
        var (success, errors, vehicle) = await _vehicleService.ChangeLicensePlateAsync(id, request);

        if (!success)
        {
            return BadRequest(new
            {
                message = "License plate change validation failed",
                errors = errors
            });
        }

        return Ok(new
        {
            message = "License plates changed successfully",
            vehicleId = vehicle!.Id,
            oldRegistrationNumber = "Stored in audit log", // TODO: Return from audit log
            newRegistrationNumber = vehicle.RegistrationNumber,
            reason = request.Reason,
            changeDate = DateTime.Now,
            technicalInspectionExpirationDate = vehicle.TechnicalInspectionExpirationDate,
            vehicle = vehicle
        });
    }
}
}