using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;
using ServicePulseMonitor.Features.Services;

namespace ServicePulseMonitor.Tests.Features;

[TestFixture]
public class RegistrationServiceTests
{
    [Test]
    public async Task RegisterServiceAsync_ValidDto_ReturnsServiceDto()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var dto = new CreateServiceDto
        {
            ServiceName = "Test Service",
            BaseUrl = "http://localhost:8080",
            Description = "Test description"
        };

        var result = await service.RegisterServiceAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ServiceId, Is.GreaterThan(0));
        Assert.That(result.ServiceName, Is.EqualTo("Test Service"));
        Assert.That(result.BaseUrl, Is.EqualTo("http://localhost:8080"));
    }

    [Test]
    public void RegisterServiceAsync_DuplicateName_ThrowsException()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        context.Services.Add(new Service
        {
            ServiceName = "Existing Service",
            RegisteredAt = DateTime.UtcNow
        });
        context.SaveChanges();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var dto = new CreateServiceDto
        {
            ServiceName = "Existing Service",
            BaseUrl = "http://localhost:8080"
        };

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.RegisterServiceAsync(dto));
    }

    [Test]
    public async Task GetServiceByIdAsync_ExistingService_ReturnsServiceDto()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var existingService = new Service
        {
            ServiceName = "Test Service",
            BaseUrl = "http://localhost:8080",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };
        context.Services.Add(existingService);
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.GetServiceByIdAsync(existingService.ServiceId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ServiceId, Is.EqualTo(existingService.ServiceId));
        Assert.That(result.ServiceName, Is.EqualTo("Test Service"));
    }

    [Test]
    public async Task GetServiceByIdAsync_NonExistentService_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.GetServiceByIdAsync(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetServiceByNameAsync_ExistingService_ReturnsServiceDto()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var existingService = new Service
        {
            ServiceName = "Test Service",
            BaseUrl = "http://localhost:8080",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };
        context.Services.Add(existingService);
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.GetServiceByNameAsync("Test Service");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ServiceName, Is.EqualTo("Test Service"));
    }

    [Test]
    public async Task ServiceExistsAsync_ExistingService_ReturnsTrue()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        context.Services.Add(new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.ServiceExistsAsync("Test Service");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ServiceExistsAsync_NonExistentService_ReturnsFalse()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.ServiceExistsAsync("NonExistent Service");

        Assert.That(result, Is.False);
    }
}
