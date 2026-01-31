using System.Text.Json;

namespace ServicePulseMonitor.Models;

public class Alert
{
    public long AlertId { get; set; }
    public long ServiceId { get; set; }
    public Guid UserGuid { get; set; }
    public string AlertType { get; set; } = null!;
    public DateTime TriggeredAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public JsonDocument? Message { get; set; }

    public Service Service { get; set; } = null!;
    public User User { get; set; } = null!;
}
