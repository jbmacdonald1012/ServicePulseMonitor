using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http.Json;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Common;

namespace ServicePulseMonitor.Tests.Integration;

[TestFixture]
public class ServicesEndpointTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task PostService_ValidDto_ReturnsCreated()
    {
        var dto = new CreateServiceDto
        {
            ServiceName = $"Integration Test Service {Guid.NewGuid()}",
            BaseUrl = "http://localhost:9000",
            Description = "Integration test"
        };

        var response = await _client.PostAsJsonAsync("/api/services", dto);

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

        var result = await response.Content.ReadFromJsonAsync<ServiceDto>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ServiceName, Is.EqualTo(dto.ServiceName));
    }

    [Test]
    public async Task PostService_DuplicateName_ReturnsConflict()
    {
        var uniqueName = $"Duplicate Test Service {Guid.NewGuid()}";
        var dto = new CreateServiceDto
        {
            ServiceName = uniqueName,
            BaseUrl = "http://localhost:9000"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/services", dto);
        Assert.That(firstResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

        var secondResponse = await _client.PostAsJsonAsync("/api/services", dto);
        Assert.That(secondResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Conflict));
    }

    [Test]
    public async Task GetService_ExistingService_ReturnsOk()
    {
        var dto = new CreateServiceDto
        {
            ServiceName = $"Get Test Service {Guid.NewGuid()}",
            BaseUrl = "http://localhost:9000"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/services", dto);
        var createdService = await createResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var getResponse = await _client.GetAsync($"/api/services/{createdService!.ServiceId}");

        Assert.That(getResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var result = await getResponse.Content.ReadFromJsonAsync<ServiceDto>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ServiceId, Is.EqualTo(createdService.ServiceId));
    }

    [Test]
    public async Task GetService_NonExistentService_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/services/999999");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetAllServices_ReturnsPagedResult()
    {
        var response = await _client.GetAsync("/api/services?pageNumber=1&pageSize=20");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ServiceDto>>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Is.Not.Null);
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(20));
    }

    [Test]
    public async Task GetAllServices_InvalidPageNumber_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/services?pageNumber=0&pageSize=20");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateService_ValidUpdate_ReturnsOk()
    {
        var createDto = new CreateServiceDto
        {
            ServiceName = $"Update Test Service {Guid.NewGuid()}",
            BaseUrl = "http://localhost:9000",
            Description = "Original description"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/services", createDto);
        var createdService = await createResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var updateDto = new UpdateServiceDto
        {
            ServiceName = createdService!.ServiceName,
            BaseUrl = "http://localhost:9999",
            Description = "Updated description"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/services/{createdService.ServiceId}", updateDto);

        Assert.That(updateResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var result = await updateResponse.Content.ReadFromJsonAsync<ServiceDto>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.BaseUrl, Is.EqualTo("http://localhost:9999"));
        Assert.That(result.Description, Is.EqualTo("Updated description"));
    }

    [Test]
    public async Task UpdateService_NonExistentService_ReturnsNotFound()
    {
        var updateDto = new UpdateServiceDto
        {
            ServiceName = "Non-existent Service",
            BaseUrl = "http://localhost:9999"
        };

        var response = await _client.PutAsJsonAsync("/api/services/999999", updateDto);

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteService_ExistingService_ReturnsNoContent()
    {
        var createDto = new CreateServiceDto
        {
            ServiceName = $"Delete Test Service {Guid.NewGuid()}",
            BaseUrl = "http://localhost:9000"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/services", createDto);
        var createdService = await createResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/services/{createdService!.ServiceId}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

        var getResponse = await _client.GetAsync($"/api/services/{createdService.ServiceId}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteService_NonExistentService_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/services/999999");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task SearchServices_ValidQuery_ReturnsResults()
    {
        var uniquePrefix = $"Search Test {Guid.NewGuid().ToString().Substring(0, 8)}";
        var createDto1 = new CreateServiceDto
        {
            ServiceName = $"{uniquePrefix} Service 1",
            BaseUrl = "http://localhost:9000"
        };
        var createDto2 = new CreateServiceDto
        {
            ServiceName = $"{uniquePrefix} Service 2",
            BaseUrl = "http://localhost:9001"
        };

        await _client.PostAsJsonAsync("/api/services", createDto1);
        await _client.PostAsJsonAsync("/api/services", createDto2);

        var response = await _client.GetAsync($"/api/services/search?q={uniquePrefix}");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var results = await response.Content.ReadFromJsonAsync<List<ServiceDto>>();
        Assert.That(results, Is.Not.Null);
        Assert.That(results!.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task SearchServices_EmptyQuery_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/services/search?q=");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetServiceHealthSummary_ServiceWithHealthChecks_ReturnsOk()
    {
        var createDto = new CreateServiceDto
        {
            ServiceName = $"Health Summary Test {Guid.NewGuid()}",
            BaseUrl = "http://localhost:9000"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/services", createDto);
        var createdService = await createResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var healthCheckDto = new CreateHealthCheckDto
        {
            Status = "Healthy",
            ResponseTimeMs = 50
        };

        await _client.PostAsJsonAsync($"/api/services/{createdService!.ServiceId}/healthchecks", healthCheckDto);

        var response = await _client.GetAsync($"/api/services/{createdService.ServiceId}/health");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<ServiceHealthSummaryDto>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ServiceId, Is.EqualTo(createdService.ServiceId));
        Assert.That(result.TotalHealthChecks, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetServiceHealthSummary_NonExistentService_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/services/999999/health");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }
}
