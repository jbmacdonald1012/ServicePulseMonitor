using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http.Json;
using ServicePulseMonitor.Data.DTOs;

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
}
