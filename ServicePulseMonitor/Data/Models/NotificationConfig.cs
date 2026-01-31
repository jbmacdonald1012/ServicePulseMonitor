namespace ServicePulseMonitor.Data.Models;

public class NotificationConfig
{
    public Guid NotificationConfigGuid { get; set; }
    public long RuleId { get; set; }
    public string? Uri { get; set; }
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }

    public AlertRule AlertRule { get; set; } = null!;
}
