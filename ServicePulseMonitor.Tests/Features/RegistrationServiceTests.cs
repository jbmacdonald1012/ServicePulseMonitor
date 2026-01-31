using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;
using ServicePulseMonitor.Features.Services;
using ServicePulseMonitor.Common;

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

    [Test]
    public async Task GetAllServicesAsync_MultipleServices_ReturnsPagedResult()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        for (int i = 1; i <= 5; i++)
        {
            context.Services.Add(new Service
            {
                ServiceName = $"Service {i}",
                RegisteredAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.GetAllServicesAsync(pageNumber: 1, pageSize: 3);

        Assert.That(result.Items.Count(), Is.EqualTo(3));
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(3));
        Assert.That(result.TotalPages, Is.EqualTo(2));
        Assert.That(result.HasNext, Is.True);
        Assert.That(result.HasPrevious, Is.False);
    }

    [Test]
    public async Task UpdateServiceAsync_ValidUpdate_ReturnsUpdatedDto()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var existingService = new Service
        {
            ServiceName = "Original Name",
            BaseUrl = "http://localhost:8080",
            RegisteredAt = DateTime.UtcNow
        };
        context.Services.Add(existingService);
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var updateDto = new UpdateServiceDto
        {
            ServiceName = "Updated Name",
            BaseUrl = "http://localhost:9999",
            Description = "Updated description"
        };

        var result = await service.UpdateServiceAsync(existingService.ServiceId, updateDto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ServiceName, Is.EqualTo("Updated Name"));
        Assert.That(result.BaseUrl, Is.EqualTo("http://localhost:9999"));
        Assert.That(result.Description, Is.EqualTo("Updated description"));
    }

    [Test]
    public async Task UpdateServiceAsync_NonExistentService_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var updateDto = new UpdateServiceDto
        {
            ServiceName = "Updated Name",
            BaseUrl = "http://localhost:9999"
        };

        var result = await service.UpdateServiceAsync(999, updateDto);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void UpdateServiceAsync_DuplicateName_ThrowsException()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        context.Services.Add(new Service
        {
            ServiceName = "Service 1",
            RegisteredAt = DateTime.UtcNow
        });
        context.Services.Add(new Service
        {
            ServiceName = "Service 2",
            RegisteredAt = DateTime.UtcNow
        });
        context.SaveChanges();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var serviceToUpdate = context.Services.First(s => s.ServiceName == "Service 2");
        var updateDto = new UpdateServiceDto
        {
            ServiceName = "Service 1",
            BaseUrl = "http://localhost:8080"
        };

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.UpdateServiceAsync(serviceToUpdate.ServiceId, updateDto));
    }

    [Test]
    public async Task DeleteServiceAsync_ExistingService_ReturnsTrue()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var existingService = new Service
        {
            ServiceName = "Service to Delete",
            RegisteredAt = DateTime.UtcNow
        };
        context.Services.Add(existingService);
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.DeleteServiceAsync(existingService.ServiceId);

        Assert.That(result, Is.True);

        var deletedService = await context.Services.FindAsync(existingService.ServiceId);
        Assert.That(deletedService, Is.Null);
    }

    [Test]
    public async Task DeleteServiceAsync_NonExistentService_ReturnsFalse()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.DeleteServiceAsync(999);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SearchServicesByNameAsync_MatchingServices_ReturnsResults()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        context.Services.Add(new Service { ServiceName = "User Service", RegisteredAt = DateTime.UtcNow });
        context.Services.Add(new Service { ServiceName = "User Authentication", RegisteredAt = DateTime.UtcNow });
        context.Services.Add(new Service { ServiceName = "Order Service", RegisteredAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var results = (await service.SearchServicesByNameAsync("User")).ToList();

        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results.All(s => s.ServiceName.Contains("User")), Is.True);
    }

    [Test]
    public async Task SearchServicesByNameAsync_NoMatches_ReturnsEmpty()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        context.Services.Add(new Service { ServiceName = "User Service", RegisteredAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var results = await service.SearchServicesByNameAsync("NonExistent");

        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task GetServiceHealthSummaryAsync_WithHealthChecks_ReturnsCorrectStats()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var testService = new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow
        };
        context.Services.Add(testService);
        await context.SaveChangesAsync();

        context.HealthChecks.Add(new HealthCheck
        {
            ServiceId = testService.ServiceId,
            Status = "Healthy",
            ResponseTimeMs = 50,
            CheckedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        context.HealthChecks.Add(new HealthCheck
        {
            ServiceId = testService.ServiceId,
            Status = "Healthy",
            ResponseTimeMs = 60,
            CheckedAt = DateTime.UtcNow.AddMinutes(-3)
        });
        context.HealthChecks.Add(new HealthCheck
        {
            ServiceId = testService.ServiceId,
            Status = "Degraded",
            ResponseTimeMs = 200,
            CheckedAt = DateTime.UtcNow.AddMinutes(-1)
        });
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.GetServiceHealthSummaryAsync(testService.ServiceId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ServiceId, Is.EqualTo(testService.ServiceId));
        Assert.That(result.ServiceName, Is.EqualTo("Test Service"));
        Assert.That(result.TotalHealthChecks, Is.EqualTo(3));
        Assert.That(result.HealthyCount, Is.EqualTo(2));
        Assert.That(result.DegradedCount, Is.EqualTo(1));
        Assert.That(result.UnhealthyCount, Is.EqualTo(0));
        Assert.That(result.CurrentStatus, Is.EqualTo("Degraded"));
        Assert.That(result.AverageResponseTimeMs, Is.EqualTo(103.33).Within(0.01));
        Assert.That(result.UptimePercentage, Is.EqualTo(66.67).Within(0.01));
    }

    [Test]
    public async Task GetServiceHealthSummaryAsync_NoHealthChecks_ReturnsZeroStats()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var testService = new Service
        {
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow
        };
        context.Services.Add(testService);
        await context.SaveChangesAsync();

        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.GetServiceHealthSummaryAsync(testService.ServiceId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalHealthChecks, Is.EqualTo(0));
        Assert.That(result.CurrentStatus, Is.Null);
        Assert.That(result.UptimePercentage, Is.EqualTo(0));
    }

    [Test]
    public async Task GetServiceHealthSummaryAsync_NonExistentService_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext();
        var logger = NullLogger<RegistrationService>.Instance;
        var service = new RegistrationService(context, logger);

        var result = await service.GetServiceHealthSummaryAsync(999);

        Assert.That(result, Is.Null);
    }
}
