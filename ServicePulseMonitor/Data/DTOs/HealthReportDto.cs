using System.ComponentModel.DataAnnotations;

namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Health report payload pushed by a monitored service on each reporting interval.</summary>
public record HealthReportDto
{
    /// <summary>The name of the service submitting this report.</summary>
    [Required]
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>Aggregated health status: <c>Healthy</c>, <c>Degraded</c>, or <c>Unhealthy</c>.</summary>
    [Required]
    [RegularExpression("^(Healthy|Degraded|Unhealthy)$",
        ErrorMessage = "Status must be 'Healthy', 'Degraded', or 'Unhealthy'")]
    public string Status { get; init; } = string.Empty;

    /// <summary>Total time in milliseconds taken to evaluate all health checks.</summary>
    [Range(0, long.MaxValue)]
    public long ResponseTimeMs { get; init; }

    /// <summary>UTC timestamp at which the health checks were evaluated.</summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>Individual health check results that make up this report.</summary>
    public List<HealthCheckResultDto>? Checks { get; init; }
}

/// <summary>Result of a single named health check within a <see cref="HealthReportDto"/>.</summary>
public record HealthCheckResultDto
{
    /// <summary>Name of the health check (e.g. <c>self</c>, <c>user-service</c>).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Status of this individual check.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Optional human-readable description or error message.</summary>
    public string? Description { get; init; }
}
