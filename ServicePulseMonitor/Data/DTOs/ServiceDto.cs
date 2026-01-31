namespace ServicePulseMonitor.Data.DTOs;

public class ServiceDto
{
    public long ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public string? Description { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
}
