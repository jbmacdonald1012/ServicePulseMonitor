using System.Text.Json;

namespace ServicePulseMonitor.Data.Models;

public class HealthCheck
{
    public long HealthCheckId { get; set; }
    public long ServiceId { get; set; }
    public string Status { get; set; } = null!;
    public int? ResponseTimeMs { get; set; }
    public DateTime CheckedAt { get; set; }
    public JsonDocument? Details { get; set; }

    public Service Service { get; set; } = null!;
}
