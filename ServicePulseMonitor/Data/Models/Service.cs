namespace ServicePulseMonitor.Data.Models;

public class Service
{
    public long ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string? BaseUrl { get; set; }
    public string? Description { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public ICollection<HealthCheck> HealthChecks { get; set; } = new List<HealthCheck>();
    public ICollection<ServiceDependency> CalledServices { get; set; } = new List<ServiceDependency>();
    public ICollection<ServiceDependency> CallingServices { get; set; } = new List<ServiceDependency>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
}
