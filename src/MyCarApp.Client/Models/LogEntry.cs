namespace MyCarApp.Client.Models;

public class LogEntry
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Now;
    public decimal OdometerKm { get; set; }
    public bool FuelLoaded { get; set; }
    public decimal? FuelLiters { get; set; }
    public decimal? FuelPricePerLiter { get; set; }
    public decimal? FuelTotalPaid { get; set; }
    public string? PetrolStationName { get; set; }
}