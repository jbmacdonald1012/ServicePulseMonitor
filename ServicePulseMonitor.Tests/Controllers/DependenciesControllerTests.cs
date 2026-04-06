using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ServicePulseMonitor.Controllers;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;
using ServicePulseMonitor.Hubs;

namespace ServicePulseMonitor.Tests.Controllers;

[TestFixture]
public class DependenciesControllerTests
{
    private DependenciesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        _controller = new DependenciesController(context, CreateHubContextMock(), NullLogger<DependenciesController>.Instance);
    }

    private static IHubContext<HealthHub> CreateHubContextMock()
    {
        var mockClients = new Mock<IHubClients>();
        var mockProxy = new Mock<IClientProxy>();
        mockProxy
            .Setup(p => p.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockClients.Setup(c => c.All).Returns(mockProxy.Object);

        var mockHub = new Mock<IHubContext<HealthHub>>();
        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
        return mockHub.Object;
    }

    private async Task SeedServicesAsync(params (string name, string? baseUrl)[] services)
    {
        // Resolve the context from the controller via a helper — seed directly through DB
        var context = TestDbContextFactory.CreateInMemoryContext();
        _controller = new DependenciesController(context, CreateHubContextMock(), NullLogger<DependenciesController>.Instance);

        foreach (var (name, baseUrl) in services)
        {
            context.Services.Add(new Service
            {
                ServiceName = name,
                BaseUrl = baseUrl,
                RegisteredAt = DateTime.UtcNow,
                CurrentStatus = "Healthy"
            });
        }
        await context.SaveChangesAsync();
    }

    // ── POST /api/dependencies/report ────────────────────────────────────────

    [Test]
    public async Task RecordDependency_BothServicesExist_ReturnsOkAndInsertsRow()
    {
        await SeedServicesAsync(("OrderService", "http://order"), ("UserService", "http://user:5001"));

        var result = await _controller.RecordDependency(new RecordDependencyRequest
        {
            ServiceName = "OrderService",
            DependsOnUrl = "http://user:5001"
        });

        Assert.That(result, Is.InstanceOf<OkResult>());

        // Verify row inserted — re-query through the controller's GET
        var getResult = await _controller.GetDependencyGraph() as OkObjectResult;
        var graph = getResult!.Value as IEnumerable<DependencyGraphItemDto>;
        Assert.That(graph!.Count(), Is.EqualTo(1));
        Assert.That(graph!.Single().SourceName, Is.EqualTo("OrderService"));
        Assert.That(graph!.Single().TargetName, Is.EqualTo("UserService"));
    }

    [Test]
    public async Task RecordDependency_DuplicateRequest_ReturnsOkWithNoNewRow()
    {
        await SeedServicesAsync(("OrderService", "http://order"), ("UserService", "http://user:5001"));

        var request = new RecordDependencyRequest
        {
            ServiceName = "OrderService",
            DependsOnUrl = "http://user:5001"
        };

        await _controller.RecordDependency(request);
        var result = await _controller.RecordDependency(request); // second call

        Assert.That(result, Is.InstanceOf<OkResult>());

        var getResult = await _controller.GetDependencyGraph() as OkObjectResult;
        var graph = getResult!.Value as IEnumerable<DependencyGraphItemDto>;
        Assert.That(graph!.Count(), Is.EqualTo(1)); // still only one row
    }

    [Test]
    public async Task RecordDependency_UnknownSourceService_Returns404()
    {
        await SeedServicesAsync(("UserService", "http://user:5001"));

        var result = await _controller.RecordDependency(new RecordDependencyRequest
        {
            ServiceName = "UnknownService",
            DependsOnUrl = "http://user:5001"
        });

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task RecordDependency_UnknownTargetService_Returns404()
    {
        await SeedServicesAsync(("OrderService", "http://order"));

        var result = await _controller.RecordDependency(new RecordDependencyRequest
        {
            ServiceName = "OrderService",
            DependsOnUrl = "http://unknown:9999"
        });

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task RecordDependency_MatchesByBaseUrl_ReturnsOk()
    {
        await SeedServicesAsync(("OrderService", "http://order"), ("UserService", "http://user:5001"));

        var result = await _controller.RecordDependency(new RecordDependencyRequest
        {
            ServiceName = "OrderService",
            DependsOnUrl = "http://user:5001" // matches UserService.BaseUrl exactly
        });

        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task RecordDependency_MatchesByServiceNameFallback_ReturnsOk()
    {
        await SeedServicesAsync(("OrderService", "http://order"), ("UserService", null));

        var result = await _controller.RecordDependency(new RecordDependencyRequest
        {
            ServiceName = "OrderService",
            DependsOnUrl = "http://does-not-match",
            DependsOnServiceName = "UserService" // fallback match
        });

        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    // ── GET /api/dependencies ────────────────────────────────────────────────

    [Test]
    public async Task GetDependencyGraph_NoData_ReturnsEmptyList()
    {
        var result = await _controller.GetDependencyGraph() as OkObjectResult;
        var graph = result!.Value as IEnumerable<DependencyGraphItemDto>;

        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(graph, Is.Empty);
    }

    [Test]
    public async Task GetDependencyGraph_WithDependencies_ReturnsCorrectGraph()
    {
        await SeedServicesAsync(
            ("OrderService", "http://order"),
            ("UserService", "http://user:5001"),
            ("NotificationService", "http://notify"));

        await _controller.RecordDependency(new RecordDependencyRequest
        {
            ServiceName = "OrderService",
            DependsOnUrl = "http://user:5001"
        });
        await _controller.RecordDependency(new RecordDependencyRequest
        {
            ServiceName = "NotificationService",
            DependsOnUrl = "http://order"
        });

        var result = await _controller.GetDependencyGraph() as OkObjectResult;
        var graph = (result!.Value as IEnumerable<DependencyGraphItemDto>)!.ToList();

        Assert.That(graph.Count, Is.EqualTo(2));
        Assert.That(graph.Any(d => d.SourceName == "OrderService" && d.TargetName == "UserService"), Is.True);
        Assert.That(graph.Any(d => d.SourceName == "NotificationService" && d.TargetName == "OrderService"), Is.True);
    }
}
