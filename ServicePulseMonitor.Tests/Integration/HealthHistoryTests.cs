using System.Net.Http.Json;
using ServicePulseMonitor.Data.DTOs;

namespace ServicePulseMonitor.Tests.Integration;

/// <summary>
/// Integration tests covering MH-2: health history storage and time-range filtering.
/// </summary>
[TestFixture]
public class HealthHistoryTests : TestBase
{
    [Test]
    public async Task GetHealthChecks_FutureFrom_ReturnsEmptyList()
    {
        var svc = await RegisterServiceAsync();
        await SubmitHealthCheckAsync(svc.ServiceId, "Healthy");

        // A 'from' timestamp in the future means no checks can match
        var futureFrom = Uri.EscapeDataString(DateTime.UtcNow.AddHours(1).ToString("O"));
        var response = await Client.GetAsync(
            $"/api/services/{svc.ServiceId}/healthchecks?from={futureFrom}&limit=100");
        response.EnsureSuccessStatusCode();

        var checks = await response.Content.ReadFromJsonAsync<List<HealthCheckDto>>();
        Assert.That(checks, Is.Not.Null);
        Assert.That(checks!, Is.Empty);
    }

    [Test]
    public async Task GetHealthChecks_PastTo_ReturnsEmptyList()
    {
        var svc = await RegisterServiceAsync();
        await SubmitHealthCheckAsync(svc.ServiceId, "Healthy");

        // A 'to' timestamp in the past means no checks can match
        var pastTo = Uri.EscapeDataString(DateTime.UtcNow.AddHours(-1).ToString("O"));
        var response = await Client.GetAsync(
            $"/api/services/{svc.ServiceId}/healthchecks?to={pastTo}&limit=100");
        response.EnsureSuccessStatusCode();

        var checks = await response.Content.ReadFromJsonAsync<List<HealthCheckDto>>();
        Assert.That(checks, Is.Not.Null);
        Assert.That(checks!, Is.Empty);
    }

    [Test]
    public async Task GetHealthChecks_NoFilter_ReturnsAllSubmittedChecks()
    {
        var svc = await RegisterServiceAsync();
        await SubmitHealthCheckAsync(svc.ServiceId, "Healthy");
        await SubmitHealthCheckAsync(svc.ServiceId, "Degraded");

        var response = await Client.GetAsync(
            $"/api/services/{svc.ServiceId}/healthchecks?limit=100");
        response.EnsureSuccessStatusCode();

        var checks = await response.Content.ReadFromJsonAsync<List<HealthCheckDto>>();
        Assert.That(checks, Is.Not.Null);
        Assert.That(checks!.Count, Is.EqualTo(2));
    }
}
