using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.Models;
using System.Text.Json;

namespace ServicePulseMonitor.Tests;

/// <summary>
/// A <see cref="ServicePulseDbContext"/> subclass that applies <see cref="JsonDocument"/> value
/// conversions required by the EF Core InMemory provider, which does not use database column types.
/// </summary>
internal class InMemoryServicePulseDbContext(DbContextOptions<ServicePulseDbContext> options)
    : ServicePulseDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HealthCheck>()
            .Property(h => h.Details)
            .HasConversion(
                v => v != null ? v.RootElement.GetRawText() : null,
                v => v != null ? JsonDocument.Parse(v, default) : null);

        modelBuilder.Entity<Alert>()
            .Property(a => a.Message)
            .HasConversion(
                v => v != null ? v.RootElement.GetRawText() : null,
                v => v != null ? JsonDocument.Parse(v, default) : null);
    }
}
