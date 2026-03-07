namespace ServicePulseMonitor.Data.DTOs;

public record HealthCheckDto
{
    public long HealthCheckId { get; init; }
    public long ServiceId { get; init; }
    public string? ServiceName { get; init; }
    public string Status { get; init; } = string.Empty;
    public int? ResponseTimeMs { get; init; }
    public DateTime CheckedAt { get; init; }
    public Dictionary<string, object>? Details { get; init; }
}
