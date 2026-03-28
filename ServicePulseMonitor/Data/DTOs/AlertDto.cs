namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Projection of an <c>Alert</c> entity for API responses.</summary>
public record AlertDto
{
    /// <summary>Unique identifier of the alert.</summary>
    public long AlertId { get; init; }

    /// <summary>ID of the service that triggered this alert.</summary>
    public long ServiceId { get; init; }

    /// <summary>Name of the service that triggered this alert.</summary>
    public string? ServiceName { get; init; }

    /// <summary>Category of the alert (e.g. <c>StatusChange</c>).</summary>
    public string AlertType { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the alert was first raised.</summary>
    public DateTime TriggeredAt { get; init; }

    /// <summary>Whether the alert has been acknowledged by a user.</summary>
    public bool IsAcknowledged { get; init; }

    /// <summary>Whether the alert has been resolved.</summary>
    public bool IsResolved { get; init; }

    /// <summary>UTC timestamp when the alert was resolved, or <c>null</c> if still open.</summary>
    public DateTime? ResolvedAt { get; init; }

    /// <summary>Raw JSON text of the alert message payload, or <c>null</c> if no message was recorded.</summary>
    public string? Message { get; init; }
}
