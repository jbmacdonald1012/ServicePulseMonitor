using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Data;
using System.Text.Json;

namespace ServicePulseMonitor.Tests;

public static class TestDbContextFactory
{
    public static ServicePulseDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ServicePulseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new TestServicePulseDbContext(options);
        return context;
    }

    private class TestServicePulseDbContext : ServicePulseDbContext
    {
        public TestServicePulseDbContext(DbContextOptions<ServicePulseDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServicePulseMonitor.Data.Models.HealthCheck>()
                .Property(h => h.Details)
                .HasConversion(
                    v => v != null ? v.RootElement.GetRawText() : null,
                    v => v != null ? JsonDocument.Parse(v, default) : null);

            modelBuilder.Entity<ServicePulseMonitor.Data.Models.Alert>()
                .Property(a => a.Message)
                .HasConversion(
                    v => v != null ? v.RootElement.GetRawText() : null,
                    v => v != null ? JsonDocument.Parse(v, default) : null);
        }
    }
}
