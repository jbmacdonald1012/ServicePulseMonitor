namespace ServicePulseMonitor.Data.Models;

/// <summary>Represents a monitored service registered with the system.</summary>
public class Service
{
    /// <summary>Unique identifier of the service.</summary>
    public long ServiceId { get; set; }

    /// <summary>Unique name of the service.</summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>Base URL of the service's health endpoint, or <c>null</c> if not configured.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Optional human-readable description.</summary>
    public string? Description { get; set; }

    /// <summary>UTC timestamp when the service was registered.</summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>UTC timestamp of the most recent successful health poll, or <c>null</c> if never polled.</summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>Current health status: <c>Healthy</c>, <c>Degraded</c>, or <c>Unhealthy</c>.</summary>
    public string CurrentStatus { get; set; } = "Healthy";

    /// <summary>Health check records associated with this service.</summary>
    public ICollection<HealthCheck> HealthChecks { get; set; } = new List<HealthCheck>();

    /// <summary>Dependencies this service calls (outbound edges).</summary>
    public ICollection<ServiceDependency> CalledServices { get; set; } = new List<ServiceDependency>();

    /// <summary>Dependencies that call this service (inbound edges).</summary>
    public ICollection<ServiceDependency> CallingServices { get; set; } = new List<ServiceDependency>();

    /// <summary>Alerts raised for this service.</summary>
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    /// <summary>Alert rules configured for this service.</summary>
    public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
}
