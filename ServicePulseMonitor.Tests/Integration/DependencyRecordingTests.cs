using System.Net;
using System.Net.Http.Json;
using ServicePulseMonitor.Data.DTOs;

namespace ServicePulseMonitor.Tests.Integration;

/// <summary>
/// Integration tests covering MH-4: dependency recording, idempotency, and graph retrieval.
/// </summary>
[TestFixture]
public class DependencyRecordingTests : TestBase
{
    private async Task<(ServiceDto Source, ServiceDto Target)> SeedTwoServicesAsync()
    {
        var source = await RegisterServiceAsync(baseUrl: $"http://localhost:{Random.Shared.Next(20000, 30000)}");
        var target = await RegisterServiceAsync(baseUrl: $"http://localhost:{Random.Shared.Next(30001, 40000)}");
        return (source, target);
    }

    [Test]
    public async Task PostDependency_ViaAlias_RecordsDependency()
    {
        var (source, target) = await SeedTwoServicesAsync();

        var response = await Client.PostAsJsonAsync("/api/dependencies",
            new RecordDependencyRequest
            {
                ServiceName = source.ServiceName,
                DependsOnUrl = target.BaseUrl!
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var graph = await Client.GetFromJsonAsync<List<DependencyGraphItemDto>>("/api/dependencies");
        Assert.That(graph, Is.Not.Null);
        Assert.That(graph!.Any(d => d.SourceId == source.ServiceId && d.TargetId == target.ServiceId), Is.True);
    }

    [Test]
    public async Task PostDependency_Twice_IsIdempotent()
    {
        var (source, target) = await SeedTwoServicesAsync();

        var payload = new RecordDependencyRequest
        {
            ServiceName = source.ServiceName,
            DependsOnUrl = target.BaseUrl!
        };

        await Client.PostAsJsonAsync("/api/dependencies", payload);
        await Client.PostAsJsonAsync("/api/dependencies", payload);

        var graph = await Client.GetFromJsonAsync<List<DependencyGraphItemDto>>("/api/dependencies");
        var count = graph!.Count(d => d.SourceId == source.ServiceId && d.TargetId == target.ServiceId);
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetDependencyGraph_ContainsCorrectSourceAndTargetNames()
    {
        var (source, target) = await SeedTwoServicesAsync();

        await Client.PostAsJsonAsync("/api/dependencies",
            new RecordDependencyRequest
            {
                ServiceName = source.ServiceName,
                DependsOnUrl = target.BaseUrl!
            });

        var graph = await Client.GetFromJsonAsync<List<DependencyGraphItemDto>>("/api/dependencies");
        var edge = graph!.FirstOrDefault(d =>
            d.SourceId == source.ServiceId && d.TargetId == target.ServiceId);

        Assert.That(edge, Is.Not.Null);
        Assert.That(edge!.SourceName, Is.EqualTo(source.ServiceName));
        Assert.That(edge.TargetName, Is.EqualTo(target.ServiceName));
    }
}
