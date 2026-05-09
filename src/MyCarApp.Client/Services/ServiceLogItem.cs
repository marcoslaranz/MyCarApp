namespace MyCarApp.Client.Models;

public class ServiceLogItem
{
    public int Id { get; set; }
    public int ServiceLogId { get; set; }
    public int ServiceItemId { get; set; }
    public bool Done { get; set; }
}