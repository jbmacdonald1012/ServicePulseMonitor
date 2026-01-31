namespace ServicePulseMonitor.Models;

public class AlertRule
{
    public long RuleId { get; set; }
    public long ServiceId { get; set; }
    public string RuleType { get; set; } = null!;
    public decimal? Threshold { get; set; }
    public string? LogType { get; set; }
    public string? NotificationChannel { get; set; }

    public Service Service { get; set; } = null!;
    public ICollection<NotificationConfig> NotificationConfigs { get; set; } = new List<NotificationConfig>();
}
