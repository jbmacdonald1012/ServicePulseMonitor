namespace ServicePulseMonitor.Data.Models;

/// <summary>Notification endpoint configuration associated with an alert rule.</summary>
public class NotificationConfig
{
    /// <summary>Unique identifier of this notification configuration.</summary>
    public Guid NotificationConfigGuid { get; set; }

    /// <summary>ID of the alert rule this configuration belongs to.</summary>
    public long RuleId { get; set; }

    /// <summary>URI of the notification endpoint (e.g. a webhook URL), or <c>null</c> if not configured.</summary>
    public string? Uri { get; set; }

    /// <summary>Human-readable display name for this notification channel, or <c>null</c> if not set.</summary>
    public string? DisplayName { get; set; }

    /// <summary>UTC timestamp when this configuration was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>The alert rule this configuration belongs to.</summary>
    public AlertRule AlertRule { get; set; } = null!;
}
