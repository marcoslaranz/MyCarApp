namespace MyCarApp.Client.Models;

public class ImportBatch
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public int RowCount { get; set; }
}