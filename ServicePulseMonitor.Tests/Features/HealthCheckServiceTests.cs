using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;
using ServicePulseMonitor.Features.HealthChecks;

namespace ServicePulseMonitor.Tests.Features;

[TestFixture]
public class HealthCheckServiceTests
{
    [Test]
    public async Task SubmitHealthCheckAsync_ValidDto_ReturnsHealthCheckDto()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };
        context.Services.Add(service);
        await context.SaveChangesAsync();

        var logger = NullLogger<HealthCheckService>.Instance;
        var healthCheckService = new HealthCheckService(context, logger);

        var dto = new CreateHealthCheckDto
        {
            Status = "Healthy",
            ResponseTimeMs = 150
        };

        var result = await healthCheckService.SubmitHealthCheckAsync(service.ServiceId, dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.HealthCheckId, Is.GreaterThan(0));
        Assert.That(result.ServiceId, Is.EqualTo(service.ServiceId));
        Assert.That(result.Status, Is.EqualTo("Healthy"));
        Assert.That(result.ResponseTimeMs, Is.EqualTo(150));
        Assert.That(result.ServiceName, Is.EqualTo("Test Service"));
    }

    [Test]
    public void SubmitHealthCheckAsync_NonExistentService_ThrowsException()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = NullLogger<HealthCheckService>.Instance;
        var healthCheckService = new HealthCheckService(context, logger);

        var dto = new CreateHealthCheckDto
        {
            Status = "Healthy",
            ResponseTimeMs = 150
        };

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await healthCheckService.SubmitHealthCheckAsync(999, dto));
    }

    [Test]
    public async Task SubmitHealthCheckAsync_UpdatesServiceLastSeenAt()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow.AddHours(-1)
        };
        context.Services.Add(service);
        await context.SaveChangesAsync();

        var originalLastSeenAt = service.LastSeenAt;

        var logger = NullLogger<HealthCheckService>.Instance;
        var healthCheckService = new HealthCheckService(context, logger);

        var dto = new CreateHealthCheckDto
        {
            Status = "Healthy"
        };

        await healthCheckService.SubmitHealthCheckAsync(service.ServiceId, dto);

        var updatedService = await context.Services.FindAsync(service.ServiceId);
        Assert.That(updatedService!.LastSeenAt, Is.GreaterThan(originalLastSeenAt));
    }

    [Test]
    public async Task GetHealthCheckByIdAsync_ExistingHealthCheck_ReturnsDto()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };
        context.Services.Add(service);
        await context.SaveChangesAsync();

        var healthCheck = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Healthy",
            ResponseTimeMs = 100,
            CheckedAt = DateTime.UtcNow
        };
        context.HealthChecks.Add(healthCheck);
        await context.SaveChangesAsync();

        var logger = NullLogger<HealthCheckService>.Instance;
        var healthCheckService = new HealthCheckService(context, logger);

        var result = await healthCheckService.GetHealthCheckByIdAsync(healthCheck.HealthCheckId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.HealthCheckId, Is.EqualTo(healthCheck.HealthCheckId));
        Assert.That(result.Status, Is.EqualTo("Healthy"));
        Assert.That(result.ServiceName, Is.EqualTo("Test Service"));
    }

    [Test]
    public async Task GetHealthCheckByIdAsync_NonExistentHealthCheck_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = NullLogger<HealthCheckService>.Instance;
        var healthCheckService = new HealthCheckService(context, logger);

        var result = await healthCheckService.GetHealthCheckByIdAsync(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetHealthChecksByServiceIdAsync_ReturnsOrderedList()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };
        context.Services.Add(service);
        await context.SaveChangesAsync();

        var healthCheck1 = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Healthy",
            CheckedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        var healthCheck2 = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Degraded",
            CheckedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var healthCheck3 = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Healthy",
            CheckedAt = DateTime.UtcNow
        };
        context.HealthChecks.AddRange(healthCheck1, healthCheck2, healthCheck3);
        await context.SaveChangesAsync();

        var logger = NullLogger<HealthCheckService>.Instance;
        var healthCheckService = new HealthCheckService(context, logger);

        var results = await healthCheckService.GetHealthChecksByServiceIdAsync(service.ServiceId);
        var resultList = results.ToList();

        Assert.That(resultList, Has.Count.EqualTo(3));
        Assert.That(resultList[0].HealthCheckId, Is.EqualTo(healthCheck3.HealthCheckId));
        Assert.That(resultList[1].HealthCheckId, Is.EqualTo(healthCheck2.HealthCheckId));
        Assert.That(resultList[2].HealthCheckId, Is.EqualTo(healthCheck1.HealthCheckId));
    }

    [Test]
    public async Task GetLatestHealthCheckAsync_ReturnsNewest()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };
        context.Services.Add(service);
        await context.SaveChangesAsync();

        var healthCheck1 = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Healthy",
            CheckedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        var healthCheck2 = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Degraded",
            CheckedAt = DateTime.UtcNow
        };
        context.HealthChecks.AddRange(healthCheck1, healthCheck2);
        await context.SaveChangesAsync();

        var logger = NullLogger<HealthCheckService>.Instance;
        var healthCheckService = new HealthCheckService(context, logger);

        var result = await healthCheckService.GetLatestHealthCheckAsync(service.ServiceId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.HealthCheckId, Is.EqualTo(healthCheck2.HealthCheckId));
        Assert.That(result.Status, Is.EqualTo("Degraded"));
    }

    [Test]
    public async Task GetHealthChecksByStatusAsync_FiltersCorrectly()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };
        context.Services.Add(service);
        await context.SaveChangesAsync();

        var healthCheck1 = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Healthy",
            CheckedAt = DateTime.UtcNow
        };
        var healthCheck2 = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Unhealthy",
            CheckedAt = DateTime.UtcNow
        };
        var healthCheck3 = new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = "Healthy",
            CheckedAt = DateTime.UtcNow
        };
        context.HealthChecks.AddRange(healthCheck1, healthCheck2, healthCheck3);
        await context.SaveChangesAsync();

        var logger = NullLogger<HealthCheckService>.Instance;
        var healthCheckService = new HealthCheckService(context, logger);

        var results = await healthCheckService.GetHealthChecksByStatusAsync("Healthy");
        var resultList = results.ToList();

        Assert.That(resultList, Has.Count.EqualTo(2));
        Assert.That(resultList.All(hc => hc.Status == "Healthy"), Is.True);
    }
}
