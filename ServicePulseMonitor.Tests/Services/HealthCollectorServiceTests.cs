using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OptionsFactory = Microsoft.Extensions.Options.Options;
using Moq;
using Moq.Protected;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.Models;
using ServicePulseMonitor.Hubs;
using ServicePulseMonitor.Options;
using ServicePulseMonitor.Services;

namespace ServicePulseMonitor.Tests.Services;

[TestFixture]
public class HealthCollectorServiceTests
{
    private Mock<IServiceScopeFactory> _mockScopeFactory = null!;
    private Mock<IServiceScope> _mockScope = null!;
    private Mock<IServiceProvider> _mockServiceProvider = null!;
    private Mock<IHubContext<HealthHub>> _mockHubContext = null!;
    private Mock<IHubClients> _mockClients = null!;
    private Mock<IClientProxy> _mockAllClients = null!;
    private Mock<HttpMessageHandler> _mockHandler = null!;
    private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
    private ServicePulseDbContext _db = null!;
    private HealthCollectorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _db = TestDbContextFactory.CreateInMemoryContext();

        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ServicePulseDbContext)))
            .Returns(_db);

        _mockScope = new Mock<IServiceScope>();
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);

        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);

        _mockAllClients = new Mock<IClientProxy>();
        _mockClients = new Mock<IHubClients>();
        _mockClients.Setup(c => c.All).Returns(_mockAllClients.Object);

        _mockHubContext = new Mock<IHubContext<HealthHub>>();
        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);

        _mockHandler = new Mock<HttpMessageHandler>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(_mockHandler.Object));
    }

    [TearDown]
    public void TearDown()
    {
        _service?.Dispose();
        _db.Dispose();
    }

    private HealthCollectorService BuildService(int intervalSeconds = 30, int timeoutSeconds = 5)
    {
        var options = OptionsFactory.Create(new HealthCollectorOptions
        {
            IntervalSeconds = intervalSeconds,
            TimeoutSeconds = timeoutSeconds
        });

        return new HealthCollectorService(
            _mockScopeFactory.Object,
            _mockHubContext.Object,
            _mockHttpClientFactory.Object,
            NullLogger<HealthCollectorService>.Instance,
            options);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode)
    {
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode));
    }

    private void SetupHttpTimeout()
    {
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout"));
    }

    private void SetupHttpException(Exception ex)
    {
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(ex);
    }

    private async Task<Service> SeedServiceAsync(string status = "Healthy")
    {
        var svc = new Service
        {
            ServiceName = "TestService",
            BaseUrl = "http://localhost:5000",
            RegisteredAt = DateTime.UtcNow,
            CurrentStatus = status
        };
        _db.Services.Add(svc);
        await _db.SaveChangesAsync();
        return svc;
    }

    // ── Health check records ─────────────────────────────────────────────────

    [Test]
    public async Task CollectHealthChecks_SuccessfulResponse_StoresHealthyRecord()
    {
        await SeedServiceAsync();
        SetupHttpResponse(HttpStatusCode.OK);
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        var record = _db.HealthChecks.Single();
        Assert.That(record.Status, Is.EqualTo("Healthy"));
        Assert.That(record.ResponseTimeMs, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task CollectHealthChecks_Non2xxResponse_StoresDegradedRecord()
    {
        await SeedServiceAsync();
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable);
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        var record = _db.HealthChecks.Single();
        Assert.That(record.Status, Is.EqualTo("Degraded"));
    }

    [Test]
    public async Task CollectHealthChecks_Timeout_StoresUnhealthyRecord()
    {
        await SeedServiceAsync();
        SetupHttpTimeout();
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        var record = _db.HealthChecks.Single();
        Assert.That(record.Status, Is.EqualTo("Unhealthy"));
    }

    [Test]
    public async Task CollectHealthChecks_HttpException_StoresUnhealthyRecord()
    {
        await SeedServiceAsync();
        SetupHttpException(new HttpRequestException("Connection refused"));
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        var record = _db.HealthChecks.Single();
        Assert.That(record.Status, Is.EqualTo("Unhealthy"));
    }

    // ── Status change detection ──────────────────────────────────────────────

    [Test]
    public async Task CollectHealthChecks_StatusChanges_CreatesAlert()
    {
        await SeedServiceAsync(status: "Healthy");
        SetupHttpTimeout(); // will return Unhealthy
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        var alert = _db.Alerts.Single();
        Assert.That(alert.AlertType, Is.EqualTo("StatusChange"));
        Assert.That(alert.IsResolved, Is.False);
        Assert.That(alert.UserGuid, Is.Null);
    }

    [Test]
    public async Task CollectHealthChecks_StatusChanges_UpdatesServiceCurrentStatus()
    {
        var svc = await SeedServiceAsync(status: "Healthy");
        SetupHttpTimeout();
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        var updated = _db.Services.Single(s => s.ServiceId == svc.ServiceId);
        Assert.That(updated.CurrentStatus, Is.EqualTo("Unhealthy"));
    }

    [Test]
    public async Task CollectHealthChecks_NoStatusChange_NoAlertCreated()
    {
        await SeedServiceAsync(status: "Healthy");
        SetupHttpResponse(HttpStatusCode.OK); // still Healthy
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        Assert.That(_db.Alerts.Count(), Is.EqualTo(0));
    }

    // ── Alert resolution ─────────────────────────────────────────────────────

    [Test]
    public async Task CollectHealthChecks_RecoveryToHealthy_ResolvesOpenAlerts()
    {
        var svc = await SeedServiceAsync(status: "Unhealthy");
        // Seed an existing open alert
        _db.Alerts.Add(new Alert
        {
            ServiceId = svc.ServiceId,
            AlertType = "StatusChange",
            TriggeredAt = DateTime.UtcNow.AddMinutes(-5),
            IsResolved = false
        });
        await _db.SaveChangesAsync();

        SetupHttpResponse(HttpStatusCode.OK); // recovers to Healthy
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        // All alerts (pre-existing + new recovery alert) should be resolved
        var openAlerts = _db.Alerts.Where(a => !a.IsResolved).ToList();
        Assert.That(openAlerts, Is.Empty);
        Assert.That(_db.Alerts.All(a => a.ResolvedAt.HasValue), Is.True);
    }

    // ── Multi-service polling ────────────────────────────────────────────────

    [Test]
    public async Task CollectHealthChecks_MultipleServices_ChecksAll()
    {
        _db.Services.AddRange(
            new Service { ServiceName = "Svc1", BaseUrl = "http://svc1", RegisteredAt = DateTime.UtcNow, CurrentStatus = "Healthy" },
            new Service { ServiceName = "Svc2", BaseUrl = "http://svc2", RegisteredAt = DateTime.UtcNow, CurrentStatus = "Healthy" });
        await _db.SaveChangesAsync();

        SetupHttpResponse(HttpStatusCode.OK);
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        Assert.That(_db.HealthChecks.Count(), Is.EqualTo(2));
    }

    // ── SignalR broadcast ────────────────────────────────────────────────────

    [Test]
    public async Task CollectHealthChecks_StatusChanges_BroadcastsSignalREvent()
    {
        await SeedServiceAsync(status: "Healthy");
        SetupHttpTimeout(); // causes status change Healthy → Unhealthy
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        _mockAllClients.Verify(
            c => c.SendCoreAsync(
                "ServiceStatusChanged",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CollectHealthChecks_StatusChangesToUnhealthy_BroadcastsAlertGenerated()
    {
        await SeedServiceAsync(status: "Healthy");
        SetupHttpTimeout(); // Healthy → Unhealthy
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        _mockAllClients.Verify(
            c => c.SendCoreAsync(
                "AlertGenerated",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CollectHealthChecks_RecoveryToHealthy_DoesNotBroadcastAlertGenerated()
    {
        await SeedServiceAsync(status: "Unhealthy");
        SetupHttpResponse(HttpStatusCode.OK); // Unhealthy → Healthy
        _service = BuildService();

        await _service.CollectHealthChecksAsync(CancellationToken.None);

        _mockAllClients.Verify(
            c => c.SendCoreAsync(
                "AlertGenerated",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
