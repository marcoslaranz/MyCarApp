namespace MyCarApp.Api.Models;

public class ServiceLog
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime ServiceDate { get; set; }
    public decimal OdometerKm { get; set; }
    public string? Notes { get; set; }

    public ICollection<ServiceLogItem> ServiceLogItems { get; set; } = new List<ServiceLogItem>();
    public ICollection<ServiceDocument> ServiceDocuments { get; set; } = new List<ServiceDocument>();
}