namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Read-only projection of a registered service.</summary>
public record ServiceDto
{
    /// <summary>Unique identifier of the service.</summary>
    public long ServiceId { get; init; }

    /// <summary>Unique name of the service.</summary>
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>Base URL of the service's health endpoint, or <c>null</c> if not configured.</summary>
    public string? BaseUrl { get; init; }

    /// <summary>Optional human-readable description of the service.</summary>
    public string? Description { get; init; }

    /// <summary>UTC timestamp when the service was first registered.</summary>
    public DateTime RegisteredAt { get; init; }

    /// <summary>UTC timestamp of the most recent successful health check, or <c>null</c> if never polled.</summary>
    public DateTime? LastSeenAt { get; init; }
}
