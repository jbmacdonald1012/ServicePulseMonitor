using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ServicePulseMonitor.Data;

namespace ServicePulseMonitor.Tests;

/// <summary>Factory for creating isolated in-memory <see cref="ServicePulseDbContext"/> instances for unit tests.</summary>
public static class TestDbContextFactory
{
    /// <summary>Creates a new in-memory context with a unique database name and JSON value converters applied.</summary>
    public static ServicePulseDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ServicePulseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new InMemoryServicePulseDbContext(options);
    }
}
