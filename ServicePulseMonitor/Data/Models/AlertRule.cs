namespace ServicePulseMonitor.Data.Models;

/// <summary>Defines a condition that triggers an alert for a monitored service.</summary>
public class AlertRule
{
    /// <summary>Unique identifier of the alert rule.</summary>
    public long RuleId { get; set; }

    /// <summary>ID of the service this rule applies to.</summary>
    public long ServiceId { get; set; }

    /// <summary>Category of condition that triggers the rule (e.g. <c>ResponseTime</c>, <c>StatusChange</c>).</summary>
    public string RuleType { get; set; } = null!;

    /// <summary>Numeric threshold value for threshold-based rules, or <c>null</c> if not applicable.</summary>
    public decimal? Threshold { get; set; }

    /// <summary>Log type filter for log-based rules, or <c>null</c> if not applicable.</summary>
    public string? LogType { get; set; }

    /// <summary>Notification channel to use when this rule fires, or <c>null</c> if not configured.</summary>
    public string? NotificationChannel { get; set; }

    /// <summary>The service this rule belongs to.</summary>
    public Service Service { get; set; } = null!;

    /// <summary>Notification configurations associated with this rule.</summary>
    public ICollection<NotificationConfig> NotificationConfigs { get; set; } = new List<NotificationConfig>();
}
