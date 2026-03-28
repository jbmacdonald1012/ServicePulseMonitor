using System.Net.Http.Json;
using ServicePulseMonitor.Common;
using ServicePulseMonitor.Data.DTOs;

namespace ServicePulseMonitor.Tests.Integration;

/// <summary>
/// Integration tests covering MH-6: alert generation on status change and resolution on recovery.
/// </summary>
[TestFixture]
public class AlertLifecycleTests : TestBase
{
    [Test]
    public async Task SubmitUnhealthy_WhenServiceWasHealthy_CreatesStatusChangeAlert()
    {
        // Service starts as Healthy (default CurrentStatus on registration)
        var svc = await RegisterServiceAsync();

        await SubmitHealthCheckAsync(svc.ServiceId, "Unhealthy", responseTimeMs: 0);

        var alertResp = await Client.GetAsync(
            $"/api/alerts?serviceId={svc.ServiceId}&isResolved=false");
        alertResp.EnsureSuccessStatusCode();
        var paged = await alertResp.Content.ReadFromJsonAsync<PagedResult<AlertDto>>();

        Assert.That(paged, Is.Not.Null);
        Assert.That(paged!.TotalCount, Is.EqualTo(1));
        Assert.That(paged.Items.Single().AlertType, Is.EqualTo("StatusChange"));
    }

    [Test]
    public async Task SubmitHealthy_AfterUnhealthy_ResolvesOpenAlert()
    {
        var svc = await RegisterServiceAsync();

        await SubmitHealthCheckAsync(svc.ServiceId, "Unhealthy", responseTimeMs: 0);
        await SubmitHealthCheckAsync(svc.ServiceId, "Healthy", responseTimeMs: 50);

        // Open alerts should be gone
        var openResp = await Client.GetAsync(
            $"/api/alerts?serviceId={svc.ServiceId}&isResolved=false");
        openResp.EnsureSuccessStatusCode();
        var openPaged = await openResp.Content.ReadFromJsonAsync<PagedResult<AlertDto>>();
        Assert.That(openPaged!.TotalCount, Is.EqualTo(0));

        // At least one resolved alert should exist
        var resolvedResp = await Client.GetAsync(
            $"/api/alerts?serviceId={svc.ServiceId}&isResolved=true");
        resolvedResp.EnsureSuccessStatusCode();
        var resolvedPaged = await resolvedResp.Content.ReadFromJsonAsync<PagedResult<AlertDto>>();
        Assert.That(resolvedPaged!.TotalCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task SubmitSameStatus_WhenStatusUnchanged_NoAlertCreated()
    {
        var svc = await RegisterServiceAsync();

        // Service starts Healthy — submitting Healthy again is no status change
        await SubmitHealthCheckAsync(svc.ServiceId, "Healthy");
        await SubmitHealthCheckAsync(svc.ServiceId, "Healthy");

        var alertResp = await Client.GetAsync($"/api/alerts?serviceId={svc.ServiceId}");
        alertResp.EnsureSuccessStatusCode();
        var paged = await alertResp.Content.ReadFromJsonAsync<PagedResult<AlertDto>>();

        Assert.That(paged, Is.Not.Null);
        Assert.That(paged!.TotalCount, Is.EqualTo(0));
    }
}
