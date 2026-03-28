using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Data;

/// <summary>Entity Framework Core database context for the ServicePulse monitoring system.</summary>
public class ServicePulseDbContext : DbContext
{
    /// <summary>Initialises a new instance with the provided options.</summary>
    public ServicePulseDbContext(DbContextOptions<ServicePulseDbContext> options)
        : base(options)
    {
    }

    /// <summary>Registered services being monitored.</summary>
    public DbSet<Service> Services { get; set; }

    /// <summary>Health check results collected from monitored services.</summary>
    public DbSet<HealthCheck> HealthChecks { get; set; }

    /// <summary>Detected service-to-service dependencies.</summary>
    public DbSet<ServiceDependency> ServiceDependencies { get; set; }

    /// <summary>Alerts raised by the health monitoring pipeline.</summary>
    public DbSet<Alert> Alerts { get; set; }

    /// <summary>Configured alert rules for monitored services.</summary>
    public DbSet<AlertRule> AlertRules { get; set; }

    /// <summary>Notification endpoint configurations associated with alert rules.</summary>
    public DbSet<NotificationConfig> NotificationConfigs { get; set; }

    /// <summary>User accounts in the monitoring system.</summary>
    public DbSet<User> Users { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServicePulseDbContext).Assembly);
    }
}
