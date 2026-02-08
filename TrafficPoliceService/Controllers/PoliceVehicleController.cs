using Microsoft.AspNetCore.Mvc;
using TrafficPoliceService.Models;

namespace TrafficPoliceService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PoliceVehicleController : ControllerBase
{
    // Hardcoded database of wanted vehicles
    private static readonly List<WantedVehicle> WantedVehicles = new()
    {
        // Your vehicles - can be added to wanted list for testing
    };

    // Hardcoded database of traffic violations
    private static readonly List<TrafficViolation> TrafficViolations = new()
    {
        new TrafficViolation
        {
            Id = 1,
            RegistrationNumber = "PA456CD",
            OwnerName = "user1",
            ViolationType = "Prebrza vožnja (Prekoračenje dozvoljene brzine od 50km/h)",
            Location = "Bulevar Kralja Aleksandra, Centar Beograda",
            DateOfViolation = DateTime.Now.AddDays(-15),
            FineAmount = 5000,
            Status = "Pending"
        },
        new TrafficViolation
        {
            Id = 2,
            RegistrationNumber = "PA456CD",
            OwnerName = "user1",
            ViolationType = "Parkiranje u zabranjenoj zoni",
            Location = "Trg Republike, Beograd",
            DateOfViolation = DateTime.Now.AddDays(-8),
            FineAmount = 3000,
            Status = "Pending"
        },
        new TrafficViolation
        {
            Id = 3,
            RegistrationNumber = "PA456CD",
            OwnerName = "user1",
            ViolationType = "Istekla tehnička inspekcija",
            Location = "Kontrolna tačka MUP-a, Novosadski put",
            DateOfViolation = DateTime.Now.AddDays(-20),
            FineAmount = 10000,
            Status = "Paid"
        }
    };

    /// <summary>
    /// Proverite da li je vozilo traženo ili ima prometne povrede
    /// Poziva se iz VehicleService da proveri status vozila sa policijom
    /// </summary>
    [HttpGet("check-vehicle/{registrationNumber}")]
    public IActionResult CheckVehicle(string registrationNumber)
    {
        var wanted = WantedVehicles.FirstOrDefault(v =>
            v.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase));

        var violations = TrafficViolations
            .Where(v => v.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var report = new PoliceVehicleReport
        {
            RegistrationNumber = registrationNumber,
            IsWanted = wanted != null && wanted.Status == "Active",
            WantedReason = wanted?.Reason,
            ActiveViolations = violations.Where(v => v.Status == "Pending").ToList(),
            ViolationCount = violations.Count,
            OutstandingFines = violations
                .Where(v => v.Status == "Pending")
                .Sum(v => v.FineAmount),
            Status = wanted?.Status == "Active" ? "Wanted" :
                     violations.Any(v => v.Status == "Pending") ? "Alert" : "Clear"
        };

        return Ok(new
        {
            message = "Vehicle check completed",
            data = report
        });
    }

    /// <summary>
    /// Dobijte sve tražena vozila u sistemu
    /// Krajnja tačka za policijske službenike
    /// </summary>
    [HttpGet("wanted-vehicles")]
    public IActionResult GetWantedVehicles()
    {
        var activeWanted = WantedVehicles
            .Where(v => v.Status == "Active")
            .OrderByDescending(v => v.DateReported)
            .ToList();

        return Ok(new
        {
            message = "Tražena vozila preuzeta",
            count = activeWanted.Count,
            data = activeWanted
        });
    }

    /// <summary>
    /// Dobijte prometne povrede za određeno vozilo
    /// </summary>
    [HttpGet("violations/{registrationNumber}")]
    public IActionResult GetViolations(string registrationNumber)
    {
        var violations = TrafficViolations
            .Where(v => v.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(v => v.DateOfViolation)
            .ToList();

        return Ok(new
        {
            message = "Prometne povrede preuzete",
            count = violations.Count,
            data = violations
        });
    }

    /// <summary>
    /// Prijavite vozilo policiji (iz VehicleService)
    /// VehicleService šalje podatke vozila kada je potrebno
    /// </summary>
    [HttpPost("report-vehicle")]
    public IActionResult ReportVehicle([FromBody] VehicleReportRequest request)
    {
        if (string.IsNullOrEmpty(request.RegistrationNumber))
        {
            return BadRequest(new { message = "Registarski broj je obavezan" });
        }

        // Check if vehicle is already in our system
        var existing = WantedVehicles.FirstOrDefault(v =>
            v.RegistrationNumber.Equals(request.RegistrationNumber, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            return Ok(new
            {
                message = "Vozilo je već registrovano u bazi podataka policije",
                data = new
                {
                    registrationNumber = request.RegistrationNumber,
                    owner = request.OwnerName,
                    status = existing.Status,
                    reason = existing.Reason,
                    recordedAt = existing.DateReported
                }
            });
        }

        // Log incoming report
        return Ok(new
        {
            message = "Prijava vozila je primljena i zabeležena",
            data = new
            {
                registrationNumber = request.RegistrationNumber,
                ownerName = request.OwnerName,
                make = request.Make,
                model = request.Model,
                expirationDate = request.ExpirationDate,
                receivedAt = DateTime.Now,
                status = "Zabeleženo"
            }
        });
    }

    /// <summary>
    /// Dodajte novo traženo vozilo u sistem
    /// Krajnja tačka za policijske službenike
    /// </summary>
    [HttpPost("wanted-vehicles")]
    public IActionResult AddWantedVehicle([FromBody] CreateWantedVehicleRequest request)
    {
        var newVehicle = new WantedVehicle
        {
            Id = WantedVehicles.Count + 1,
            RegistrationNumber = request.RegistrationNumber,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Color = request.Color,
            Reason = request.Reason,
            DateReported = DateTime.Now,
            Status = "Active"
        };

        WantedVehicles.Add(newVehicle);

        return Ok(new
        {
            message = "Vozilo je dodano na listu traženih vozila",
            data = newVehicle
        });
    }

    /// <summary>
    /// Zabeležite prometnu povredu
    /// Krajnja tačka za policijske službenike
    /// </summary>
    [HttpPost("violations")]
    public IActionResult RecordViolation([FromBody] CreateViolationRequest request)
    {
        var violation = new TrafficViolation
        {
            Id = TrafficViolations.Count + 1,
            RegistrationNumber = request.RegistrationNumber,
            OwnerName = request.OwnerName,
            ViolationType = request.ViolationType,
            Location = request.Location,
            DateOfViolation = DateTime.Now,
            FineAmount = request.FineAmount,
            Status = "Pending"
        };

        TrafficViolations.Add(violation);

        return Ok(new
        {
            message = "Prometna povreda je zabeležena",
            data = violation
        });
    }
}

// Request DTOs
public class VehicleReportRequest
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
}

public class CreateWantedVehicleRequest
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class CreateViolationRequest
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal FineAmount { get; set; }
}
