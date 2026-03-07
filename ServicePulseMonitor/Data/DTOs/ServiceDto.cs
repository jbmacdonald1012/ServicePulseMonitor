namespace ServicePulseMonitor.Data.DTOs;

public record ServiceDto
{
    public long ServiceId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string? BaseUrl { get; init; }
    public string? Description { get; init; }
    public DateTime RegisteredAt { get; init; }
    public DateTime? LastSeenAt { get; init; }
}
