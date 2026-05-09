namespace MyCarApp.Client.Models;

public class ServiceItem
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? LastServiceDate { get; set; }
    public decimal? LastServiceKm { get; set; }
    public int? IntervalKm { get; set; }
    public int? IntervalMonths { get; set; }
    public string? Notes { get; set; }
}