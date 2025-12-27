using System;
using Microsoft.AspNetCore.Http;

namespace VehicleService.DTOs;

public class CreateRegistrationRequestDto
{
    public int VehicleId { get; set; }
    public DateTime TechnicalInspectionDate { get; set; }
    public IFormFile InsuranceDoc { get; set; } = null!;
    public IFormFile InspectionDoc { get; set; } = null!;
    public IFormFile IdentityDoc { get; set; } = null!;
}
