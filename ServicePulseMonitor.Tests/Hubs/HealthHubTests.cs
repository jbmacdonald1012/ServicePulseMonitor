using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using ServicePulseMonitor.Hubs;

namespace ServicePulseMonitor.Tests.Hubs;

[TestFixture]
public class HealthHubTests
{
    private Mock<ILogger<HealthHub>> _mockLogger = null!;
    private Mock<HubCallerContext> _mockContext = null!;
    private HealthHub _hub = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<HealthHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");

        _hub = new HealthHub(_mockLogger.Object)
        {
            Context = _mockContext.Object
        };
    }

    [TearDown]
    public void TearDown() => _hub.Dispose();

    [Test]
    public async Task OnConnectedAsync_LogsConnectionId()
    {
        await _hub.OnConnectedAsync();

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("test-connection-id")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task OnDisconnectedAsync_LogsConnectionId()
    {
        await _hub.OnDisconnectedAsync(null);

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("test-connection-id")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
