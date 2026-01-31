namespace ServicePulseMonitor.Models;

public class ServiceDependency
{
    public long DependencyId { get; set; }
    public long ServiceId { get; set; }
    public long DependsOnServiceId { get; set; }
    public DateTime DiscoveredAt { get; set; }

    public Service Caller { get; set; } = null!;
    public Service Callee { get; set; } = null!;
}
