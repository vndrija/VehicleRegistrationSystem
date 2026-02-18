namespace TrafficPoliceService.Models;

public class PoliceVehicleReport
{
    public string RegistrationNumber { get; set; } = string.Empty; // Registarski broj vozila
    public bool IsWanted { get; set; } // Da li je vozilo traženo
    public string? WantedReason { get; set; } // Razlog ako je traženo
    public List<TrafficViolation> ActiveViolations { get; set; } = new(); // Aktivne povrede
    public int ViolationCount { get; set; } // Broj svih povreda
    public decimal OutstandingFines { get; set; } // Neplaćene kazne u RSD
    public string Status { get; set; } = "Clear"; // Status: Čisto, Upozorenje, Traženo
    public DateTime CheckedAt { get; set; } = DateTime.Now; // Vreme provere
}
