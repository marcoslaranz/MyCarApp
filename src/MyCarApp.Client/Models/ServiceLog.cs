namespace MyCarApp.Client.Models;

public class ServiceLog
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime ServiceDate { get; set; }
    public decimal OdometerKm { get; set; }
    public string? Notes { get; set; }
    public List<ServiceLogItem> ServiceLogItems { get; set; } = new();
    public List<ServiceDocument> ServiceDocuments { get; set; } = new();
}