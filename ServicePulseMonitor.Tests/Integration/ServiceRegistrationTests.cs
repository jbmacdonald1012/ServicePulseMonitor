using System.Net;
using System.Net.Http.Json;
using ServicePulseMonitor.Common;
using ServicePulseMonitor.Data.DTOs;

namespace ServicePulseMonitor.Tests.Integration;

/// <summary>
/// Integration tests covering MH-1: service registration, listing, and deduplication.
/// </summary>
[TestFixture]
public class ServiceRegistrationTests : TestBase
{
    [Test]
    public async Task RegisterService_ValidRequest_Returns201WithBody()
    {
        var name = $"RegSvc-{Guid.NewGuid():N}";

        var response = await Client.PostAsJsonAsync("/api/services",
            new CreateServiceDto { ServiceName = name, BaseUrl = "http://localhost:19100" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var result = await response.Content.ReadFromJsonAsync<ServiceDto>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ServiceName, Is.EqualTo(name));
        Assert.That(result.ServiceId, Is.GreaterThan(0));
    }

    [Test]
    public async Task RegisterService_AppearsInPagedList()
    {
        var svc = await RegisterServiceAsync();

        var listResponse = await Client.GetAsync("/api/services?pageNumber=1&pageSize=100");
        listResponse.EnsureSuccessStatusCode();
        var paged = await listResponse.Content.ReadFromJsonAsync<PagedResult<ServiceDto>>();

        Assert.That(paged, Is.Not.Null);
        Assert.That(paged!.Items.Any(s => s.ServiceId == svc.ServiceId), Is.True);
    }

    [Test]
    public async Task RegisterService_DuplicateName_Returns409()
    {
        var name = $"DupSvc-{Guid.NewGuid():N}";
        var dto = new CreateServiceDto { ServiceName = name, BaseUrl = "http://localhost:19101" };

        var first = await Client.PostAsJsonAsync("/api/services", dto);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var second = await Client.PostAsJsonAsync("/api/services", dto);
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}
