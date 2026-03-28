namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Aggregated health statistics for a single service.</summary>
public record ServiceHealthSummaryDto
{
    /// <summary>Unique identifier of the service.</summary>
    public long ServiceId { get; init; }

    /// <summary>Name of the service.</summary>
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>Base URL of the service, or <c>null</c> if not configured.</summary>
    public string? BaseUrl { get; init; }

    /// <summary>Current health status derived from the most recent check, or <c>null</c> if never checked.</summary>
    public string? CurrentStatus { get; init; }

    /// <summary>UTC timestamp of the most recent health check, or <c>null</c> if never checked.</summary>
    public DateTime? LastCheckAt { get; init; }

    /// <summary>Total number of health checks recorded for this service.</summary>
    public int TotalHealthChecks { get; init; }

    /// <summary>Number of <c>Healthy</c> health checks.</summary>
    public int HealthyCount { get; init; }

    /// <summary>Number of <c>Degraded</c> health checks.</summary>
    public int DegradedCount { get; init; }

    /// <summary>Number of <c>Unhealthy</c> health checks.</summary>
    public int UnhealthyCount { get; init; }

    /// <summary>Average response time in milliseconds across all checks that returned a value, or <c>null</c> if none.</summary>
    public double? AverageResponseTimeMs { get; init; }

    /// <summary>Percentage of checks with <c>Healthy</c> status, rounded to two decimal places.</summary>
    public double UptimePercentage { get; init; }
}
