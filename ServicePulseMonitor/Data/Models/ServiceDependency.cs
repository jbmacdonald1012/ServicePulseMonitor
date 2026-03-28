namespace ServicePulseMonitor.Data.Models;

/// <summary>Represents a detected HTTP dependency between two services.</summary>
public class ServiceDependency
{
    /// <summary>Unique identifier of this dependency record.</summary>
    public long DependencyId { get; set; }

    /// <summary>ID of the service that makes calls (the caller).</summary>
    public long ServiceId { get; set; }

    /// <summary>ID of the service being called (the callee).</summary>
    public long DependsOnServiceId { get; set; }

    /// <summary>UTC timestamp when this dependency was first detected.</summary>
    public DateTime DiscoveredAt { get; set; }

    /// <summary>The service that makes the calls.</summary>
    public Service Caller { get; set; } = null!;

    /// <summary>The service being called.</summary>
    public Service Callee { get; set; } = null!;
}
