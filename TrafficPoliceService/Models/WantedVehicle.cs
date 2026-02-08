namespace TrafficPoliceService.Models;

public class WantedVehicle
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty; // Registarski broj
    public string Make { get; set; } = string.Empty; // Marka vozila
    public string Model { get; set; } = string.Empty; // Model vozila
    public int Year { get; set; } // Godina proizvodnje
    public string Color { get; set; } = string.Empty; // Boja vozila
    public string Reason { get; set; } = string.Empty; // Razlog: Ukradeno, Korišćeno u zločinu, itd.
    public DateTime DateReported { get; set; } // Datum prijave
    public string Status { get; set; } = "Active"; // Status: Aktivno, Rešeno, Povučeno
}
