using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ServicePulseMonitor.Controllers;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Features.Services;
using ServicePulseMonitor.Common;

namespace ServicePulseMonitor.Tests.Controllers;

[TestFixture]
public class ServicesControllerTests
{
    private Mock<IRegistrationService> _mockRegistrationService = null!;
    private ServicesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRegistrationService = new Mock<IRegistrationService>();
        var logger = NullLogger<ServicesController>.Instance;
        _controller = new ServicesController(_mockRegistrationService.Object, logger);
    }

    [Test]
    public async Task RegisterService_ValidDto_ReturnsCreatedResult()
    {
        var dto = new CreateServiceDto
        {
            ServiceName = "Test Service",
            BaseUrl = "http://localhost:8080",
            Description = "Test"
        };
        var expectedResult = new ServiceDto
        {
            ServiceId = 1,
            ServiceName = dto.ServiceName,
            BaseUrl = dto.BaseUrl,
            Description = dto.Description,
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };

        _mockRegistrationService
            .Setup(s => s.RegisterServiceAsync(It.IsAny<CreateServiceDto>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.RegisterService(dto);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult!.StatusCode, Is.EqualTo(201));
        Assert.That(createdResult.Value, Is.EqualTo(expectedResult));
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(_controller.GetServiceById)));
    }

    [Test]
    public async Task RegisterService_DuplicateName_ReturnsConflict()
    {
        var dto = new CreateServiceDto
        {
            ServiceName = "Duplicate Service",
            BaseUrl = "http://localhost:8080"
        };

        _mockRegistrationService
            .Setup(s => s.RegisterServiceAsync(It.IsAny<CreateServiceDto>()))
            .ThrowsAsync(new InvalidOperationException("Service already exists"));

        var result = await _controller.RegisterService(dto);

        Assert.That(result.Result, Is.InstanceOf<ConflictObjectResult>());
        var conflictResult = result.Result as ConflictObjectResult;
        Assert.That(conflictResult!.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task GetAllServices_ValidPagination_ReturnsPagedResult()
    {
        var pagedResult = new PagedResult<ServiceDto>
        {
            Items = new List<ServiceDto>
            {
                new ServiceDto { ServiceId = 1, ServiceName = "Service 1", RegisteredAt = DateTime.UtcNow },
                new ServiceDto { ServiceId = 2, ServiceName = "Service 2", RegisteredAt = DateTime.UtcNow }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20
        };

        _mockRegistrationService
            .Setup(s => s.GetAllServicesAsync(1, 20))
            .ReturnsAsync(pagedResult);

        var result = await _controller.GetAllServices(pageNumber: 1, pageSize: 20);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(pagedResult));
    }

    [Test]
    public async Task GetAllServices_InvalidPageNumber_ReturnsBadRequest()
    {
        var result = await _controller.GetAllServices(pageNumber: 0, pageSize: 20);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetAllServices_InvalidPageSize_ReturnsBadRequest()
    {
        var result = await _controller.GetAllServices(pageNumber: 1, pageSize: 101);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetServiceById_ExistingService_ReturnsOk()
    {
        var expectedDto = new ServiceDto
        {
            ServiceId = 1,
            ServiceName = "Test Service",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };

        _mockRegistrationService
            .Setup(s => s.GetServiceByIdAsync(1))
            .ReturnsAsync(expectedDto);

        var result = await _controller.GetServiceById(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expectedDto));
    }

    [Test]
    public async Task GetServiceById_NonExistentService_ReturnsNotFound()
    {
        _mockRegistrationService
            .Setup(s => s.GetServiceByIdAsync(9999))
            .ReturnsAsync((ServiceDto?)null);

        var result = await _controller.GetServiceById(9999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateService_ValidUpdate_ReturnsOk()
    {
        var updateDto = new UpdateServiceDto
        {
            ServiceName = "Updated Service",
            BaseUrl = "http://localhost:9999"
        };

        var expectedResult = new ServiceDto
        {
            ServiceId = 1,
            ServiceName = "Updated Service",
            BaseUrl = "http://localhost:9999",
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };

        _mockRegistrationService
            .Setup(s => s.UpdateServiceAsync(1, It.IsAny<UpdateServiceDto>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.UpdateService(1, updateDto);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task UpdateService_NonExistentService_ReturnsNotFound()
    {
        var updateDto = new UpdateServiceDto
        {
            ServiceName = "Updated Service",
            BaseUrl = "http://localhost:9999"
        };

        _mockRegistrationService
            .Setup(s => s.UpdateServiceAsync(9999, It.IsAny<UpdateServiceDto>()))
            .ReturnsAsync((ServiceDto?)null);

        var result = await _controller.UpdateService(9999, updateDto);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateService_DuplicateName_ReturnsConflict()
    {
        var updateDto = new UpdateServiceDto
        {
            ServiceName = "Duplicate Service",
            BaseUrl = "http://localhost:9999"
        };

        _mockRegistrationService
            .Setup(s => s.UpdateServiceAsync(1, It.IsAny<UpdateServiceDto>()))
            .ThrowsAsync(new InvalidOperationException("Service already exists"));

        var result = await _controller.UpdateService(1, updateDto);

        Assert.That(result.Result, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test]
    public async Task DeleteService_ExistingService_ReturnsNoContent()
    {
        _mockRegistrationService
            .Setup(s => s.DeleteServiceAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.DeleteService(1);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteService_NonExistentService_ReturnsNotFound()
    {
        _mockRegistrationService
            .Setup(s => s.DeleteServiceAsync(9999))
            .ReturnsAsync(false);

        var result = await _controller.DeleteService(9999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task SearchServices_ValidQuery_ReturnsOk()
    {
        var expectedResults = new List<ServiceDto>
        {
            new ServiceDto { ServiceId = 1, ServiceName = "User Service", RegisteredAt = DateTime.UtcNow },
            new ServiceDto { ServiceId = 2, ServiceName = "User Auth", RegisteredAt = DateTime.UtcNow }
        };

        _mockRegistrationService
            .Setup(s => s.SearchServicesByNameAsync("User"))
            .ReturnsAsync(expectedResults);

        var result = await _controller.SearchServices("User");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expectedResults));
    }

    [Test]
    public async Task SearchServices_EmptyQuery_ReturnsBadRequest()
    {
        var result = await _controller.SearchServices("");

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetServiceHealthSummary_ExistingService_ReturnsOk()
    {
        var expectedSummary = new ServiceHealthSummaryDto
        {
            ServiceId = 1,
            ServiceName = "Test Service",
            TotalHealthChecks = 10,
            HealthyCount = 8,
            DegradedCount = 2,
            UnhealthyCount = 0,
            UptimePercentage = 80.0
        };

        _mockRegistrationService
            .Setup(s => s.GetServiceHealthSummaryAsync(1))
            .ReturnsAsync(expectedSummary);

        var result = await _controller.GetServiceHealthSummary(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expectedSummary));
    }

    [Test]
    public async Task GetServiceHealthSummary_NonExistentService_ReturnsNotFound()
    {
        _mockRegistrationService
            .Setup(s => s.GetServiceHealthSummaryAsync(9999))
            .ReturnsAsync((ServiceHealthSummaryDto?)null);

        var result = await _controller.GetServiceHealthSummary(9999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }
}
