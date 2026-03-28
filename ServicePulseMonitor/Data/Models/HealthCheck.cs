using System.Text.Json;

namespace ServicePulseMonitor.Data.Models;

/// <summary>Records the result of a single health check poll for a service.</summary>
public class HealthCheck
{
    /// <summary>Unique identifier of this health check record.</summary>
    public long HealthCheckId { get; set; }

    /// <summary>ID of the service this check belongs to.</summary>
    public long ServiceId { get; set; }

    /// <summary>Health status: <c>Healthy</c>, <c>Degraded</c>, or <c>Unhealthy</c>.</summary>
    public string Status { get; set; } = null!;

    /// <summary>Measured response time in milliseconds, or <c>null</c> if the check timed out.</summary>
    public int? ResponseTimeMs { get; set; }

    /// <summary>UTC timestamp when this health check was recorded.</summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>Optional structured JSON details from the service's <c>/health</c> response.</summary>
    public JsonDocument? Details { get; set; }

    /// <summary>The service this check belongs to.</summary>
    public Service Service { get; set; } = null!;
}
