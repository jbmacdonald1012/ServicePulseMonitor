using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.Models;
using ServicePulseMonitor.Hubs;
using ServicePulseMonitor.Options;

namespace ServicePulseMonitor.Services;

/// <summary>
/// Background service that periodically polls all registered services' <c>/health</c> endpoints,
/// stores health check records, generates alerts on status changes, and broadcasts updates via SignalR.
/// </summary>
public class HealthCollectorService(
    IServiceScopeFactory scopeFactory,
    IHubContext<HealthHub> hubContext,
    IHttpClientFactory httpClientFactory,
    ILogger<HealthCollectorService> logger,
    IOptions<HealthCollectorOptions> options) : BackgroundService
{
    private readonly HealthCollectorOptions _options = options.Value;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "HealthCollectorService started. Interval: {Interval}s, Timeout: {Timeout}s",
            _options.IntervalSeconds, _options.TimeoutSeconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.IntervalSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await CollectHealthChecksAsync(stoppingToken);
    }

    internal async Task CollectHealthChecksAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServicePulseDbContext>();

        var services = await db.Services.ToListAsync(cancellationToken);
        foreach (var service in services)
            await CheckServiceHealthAsync(db, service, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckServiceHealthAsync(
        ServicePulseDbContext db,
        Service service,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        string newStatus;
        int responseTimeMs = 0;
        string details;

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await client.GetAsync(
                $"{service.BaseUrl?.TrimEnd('/')}/health", cancellationToken);
            sw.Stop();
            responseTimeMs = (int)sw.ElapsedMilliseconds;
            newStatus = response.IsSuccessStatusCode ? "Healthy" : "Degraded";
            details = $"HTTP {(int)response.StatusCode}";
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            newStatus = "Unhealthy";
            details = $"Timeout after {_options.TimeoutSeconds}s";
            logger.LogWarning("Service {ServiceName} timed out", service.ServiceName);
        }
        catch (Exception ex)
        {
            sw.Stop();
            newStatus = "Unhealthy";
            details = ex.Message;
            logger.LogWarning(
                "Service {ServiceName} health check failed: {Error}", service.ServiceName, ex.Message);
        }

        db.HealthChecks.Add(new HealthCheck
        {
            ServiceId = service.ServiceId,
            Status = newStatus,
            ResponseTimeMs = responseTimeMs,
            CheckedAt = DateTime.UtcNow,
            Details = JsonDocument.Parse(JsonSerializer.Serialize(new { message = details }))
        });

        if (service.CurrentStatus != newStatus)
        {
            logger.LogInformation(
                "Service {ServiceName} status changed: {Old} → {New}",
                service.ServiceName, service.CurrentStatus, newStatus);

            var isRecovery = newStatus == "Healthy";
            db.Alerts.Add(new Alert
            {
                ServiceId = service.ServiceId,
                AlertType = "StatusChange",
                Message = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    message = $"{service.ServiceName} transitioned from {service.CurrentStatus} to {newStatus}."
                })),
                TriggeredAt = DateTime.UtcNow,
                IsResolved = isRecovery,
                ResolvedAt = isRecovery ? DateTime.UtcNow : null
            });

            if (isRecovery)
            {
                var openAlerts = db.Alerts
                    .Where(a => a.ServiceId == service.ServiceId && !a.IsResolved);
                await foreach (var a in openAlerts.AsAsyncEnumerable())
                {
                    a.IsResolved = true;
                    a.ResolvedAt = DateTime.UtcNow;
                }
            }

            service.CurrentStatus = newStatus;

            await hubContext.Clients.All.SendAsync("ServiceStatusChanged", new
            {
                serviceId = service.ServiceId,
                serviceName = service.ServiceName,
                status = newStatus,
                responseTimeMs,
                timestamp = DateTime.UtcNow
            });
        }

        service.LastSeenAt = DateTime.UtcNow;
    }
}
