namespace ServicePulseMonitor.Data.DTOs;

public class HealthCheckDto
{
    public long HealthCheckId { get; set; }
    public long ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ResponseTimeMs { get; set; }
    public DateTime CheckedAt { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}
