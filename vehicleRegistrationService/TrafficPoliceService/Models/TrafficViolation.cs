namespace TrafficPoliceService.Models;

public class TrafficViolation
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty; // Registarski broj vozila
    public string OwnerName { get; set; } = string.Empty; // Ime vlasnika
    public string ViolationType { get; set; } = string.Empty; // Tip povrede: Prebrza vožnja, Istekla inspekcija, itd.
    public string Location { get; set; } = string.Empty; // Lokacija povrede
    public DateTime DateOfViolation { get; set; } // Datum povrede
    public decimal FineAmount { get; set; } // Iznos kazne u RSD
    public string Status { get; set; } = "Pending"; // Status: Na čekanju, Plaćeno, Sporeno
}
