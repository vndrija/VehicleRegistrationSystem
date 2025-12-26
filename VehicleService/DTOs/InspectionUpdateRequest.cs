namespace VehicleService.DTOs;

public class InspectionUpdateRequest
{
    public DateTime InspectionDate { get; set; }
    public int ValidityMonths { get; set; } = 12;
}
