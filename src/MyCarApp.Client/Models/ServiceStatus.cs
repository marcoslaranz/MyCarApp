namespace MyCarApp.Client.Models;

public class ServiceStatus
{
    public ServiceItem Item { get; set; } = null!;
    public string Status { get; set; } = "ok"; // "ok", "dueSoon", "overdue"
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusColor { get; set; } = "#2e7d32";
    public string StatusEmoji { get; set; } = "🟢";
}

public static class ServiceStatusCalculator
{
    private const int WarningKm = 500;
    private const int WarningMonths = 1;

    public static ServiceStatus Calculate(ServiceItem item, decimal currentOdometerKm)
    {
        var status = new ServiceStatus { Item = item };
        bool isOverdue = false;
        bool isDueSoon = false;
        var reasons = new List<string>();

        // Check KM interval
        if (item.IntervalKm.HasValue && item.LastServiceKm.HasValue)
        {
            var kmSince = currentOdometerKm - item.LastServiceKm.Value;
            var kmRemaining = item.IntervalKm.Value - kmSince;

            if (kmRemaining <= 0)
            {
                isOverdue = true;
                reasons.Add($"{Math.Abs((int)kmRemaining):N0} km overdue");
            }
            else if (kmRemaining <= WarningKm)
            {
                isDueSoon = true;
                reasons.Add($"{(int)kmRemaining:N0} km remaining");
            }
        }

        // Check time interval
        if (item.IntervalMonths.HasValue && item.LastServiceDate.HasValue)
        {
            var monthsSince = (DateTime.Today.Year - item.LastServiceDate.Value.Year) * 12
                + DateTime.Today.Month - item.LastServiceDate.Value.Month;
            var monthsRemaining = item.IntervalMonths.Value - monthsSince;

            if (monthsRemaining <= 0)
            {
                isOverdue = true;
                reasons.Add($"{Math.Abs(monthsRemaining)} month(s) overdue");
            }
            else if (monthsRemaining <= WarningMonths)
            {
                isDueSoon = true;
                reasons.Add($"{monthsRemaining} month(s) remaining");
            }
        }

        if (isOverdue)
        {
            status.Status = "overdue";
            status.StatusLabel = string.Join(", ", reasons);
            status.StatusColor = "#c62828";
            status.StatusEmoji = "🔴";
        }
        else if (isDueSoon)
        {
            status.Status = "dueSoon";
            status.StatusLabel = string.Join(", ", reasons);
            status.StatusColor = "#e65100";
            status.StatusEmoji = "🟡";
        }
        else
        {
            status.Status = "ok";
            status.StatusLabel = "OK";
            status.StatusColor = "#2e7d32";
            status.StatusEmoji = "🟢";
        }

        return status;
    }
}