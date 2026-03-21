using Microsoft.AspNetCore.SignalR;

namespace ServicePulseMonitor.Hubs;

/// <summary>
/// SignalR hub for real-time service health broadcasts to connected dashboard clients.
/// All messages are pushed server-side; clients do not send messages to this hub.
/// </summary>
public class HealthHub(ILogger<HealthHub> logger) : Hub
{
    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Dashboard client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <inheritdoc/>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Dashboard client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
