namespace VehicleService.DTOs;

public class ReviewRegistrationRequestDto
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}
