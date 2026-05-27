namespace MyCarApp.Api.Models;

public class ImportBatch
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public string FileName { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public bool IsLatest { get; set; } = true;

    // Navigation
    public Vehicle? Vehicle { get; set; }
    public List<LogEntry> LogEntries { get; set; } = new();
}