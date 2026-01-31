namespace ServicePulseMonitor.Data.DTOs;

public class ServiceHealthSummaryDto
{
    public long ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public string? CurrentStatus { get; set; }
    public DateTime? LastCheckAt { get; set; }
    public int TotalHealthChecks { get; set; }
    public int HealthyCount { get; set; }
    public int DegradedCount { get; set; }
    public int UnhealthyCount { get; set; }
    public double? AverageResponseTimeMs { get; set; }
    public double UptimePercentage { get; set; }
}
