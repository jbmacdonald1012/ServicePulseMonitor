namespace ServicePulseMonitor.Data.DTOs;

public record ServiceHealthSummaryDto
{
    public long ServiceId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string? BaseUrl { get; init; }
    public string? CurrentStatus { get; init; }
    public DateTime? LastCheckAt { get; init; }
    public int TotalHealthChecks { get; init; }
    public int HealthyCount { get; init; }
    public int DegradedCount { get; init; }
    public int UnhealthyCount { get; init; }
    public double? AverageResponseTimeMs { get; init; }
    public double UptimePercentage { get; init; }
}
