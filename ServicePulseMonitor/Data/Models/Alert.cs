using System.Text.Json;

namespace ServicePulseMonitor.Data.Models;

/// <summary>Represents an alert raised when a monitored service changes health status.</summary>
public class Alert
{
    /// <summary>Unique identifier of the alert.</summary>
    public long AlertId { get; set; }

    /// <summary>ID of the service that triggered this alert.</summary>
    public long ServiceId { get; set; }

    /// <summary>GUID of the user who acknowledged or owns this alert, or <c>null</c> if unacknowledged.</summary>
    public Guid? UserGuid { get; set; }

    /// <summary>Category of the alert (e.g. <c>StatusChange</c>).</summary>
    public string AlertType { get; set; } = null!;

    /// <summary>UTC timestamp when the alert was first raised.</summary>
    public DateTime TriggeredAt { get; set; }

    /// <summary>Whether the alert has been acknowledged by a user.</summary>
    public bool IsAcknowledged { get; set; }

    /// <summary>Whether the alert has been resolved (service returned to Healthy).</summary>
    public bool IsResolved { get; set; }

    /// <summary>UTC timestamp when the alert was resolved, or <c>null</c> if still open.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Optional JSON payload describing the alert details.</summary>
    public JsonDocument? Message { get; set; }

    /// <summary>The service that triggered this alert.</summary>
    public Service Service { get; set; } = null!;

    /// <summary>The user associated with this alert, if any.</summary>
    public User User { get; set; } = null!;
}
