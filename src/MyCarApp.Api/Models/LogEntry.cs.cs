namespace MyCarApp.Api.Models;

public class LogEntry
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    //public Vehicle Vehicle { get; set; } = null!;

    public DateTime DateTime { get; set; }
    public decimal OdometerKm { get; set; }

    public bool FuelLoaded { get; set; }
    public decimal? FuelLiters { get; set; }
    public decimal? FuelPricePerLiter { get; set; }
    public decimal? FuelTotalPaid { get; set; }
    public string? PetrolStationName { get; set; }
}