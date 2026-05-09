namespace MyCarApp.Client.Models;

public class ServiceDocument
{
    public int Id { get; set; }
    public int ServiceLogId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}