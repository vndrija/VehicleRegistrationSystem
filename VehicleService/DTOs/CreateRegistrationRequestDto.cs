using System;
using Microsoft.AspNetCore.Http;
using VehicleService.Enums;

namespace VehicleService.DTOs;

public class CreateRegistrationRequestDto
{
    public int VehicleId { get; set; }
    public RegistrationRequestType Type { get; set; } = RegistrationRequestType.New;
    public DateTime TechnicalInspectionDate { get; set; }
    public IFormFile InsuranceDoc { get; set; } = null!;
    public IFormFile InspectionDoc { get; set; } = null!;
    public IFormFile? IdentityDoc { get; set; } // Optional for renewal
}
