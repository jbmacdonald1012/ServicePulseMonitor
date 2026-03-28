namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Read-only projection of a recorded health check.</summary>
public record HealthCheckDto
{
    /// <summary>Unique identifier of the health check.</summary>
    public long HealthCheckId { get; init; }

    /// <summary>ID of the service this health check belongs to.</summary>
    public long ServiceId { get; init; }

    /// <summary>Name of the service this health check belongs to.</summary>
    public string? ServiceName { get; init; }

    /// <summary>Health status: <c>Healthy</c>, <c>Degraded</c>, or <c>Unhealthy</c>.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Measured response time in milliseconds, or <c>null</c> if the check timed out.</summary>
    public int? ResponseTimeMs { get; init; }

    /// <summary>UTC timestamp when the health check was performed.</summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>Optional structured details about the health check result.</summary>
    public Dictionary<string, object>? Details { get; init; }
}
