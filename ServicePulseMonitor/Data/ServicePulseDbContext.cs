using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Models;

namespace ServicePulseMonitor.Data;

public class ServicePulseDbContext : DbContext
{
    public ServicePulseDbContext(DbContextOptions<ServicePulseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services { get; set; }
    public DbSet<HealthCheck> HealthChecks { get; set; }
    public DbSet<ServiceDependency> ServiceDependencies { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<AlertRule> AlertRules { get; set; }
    public DbSet<NotificationConfig> NotificationConfigs { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServicePulseDbContext).Assembly);
    }
}
