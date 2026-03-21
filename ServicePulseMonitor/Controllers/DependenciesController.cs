using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Controllers;

/// <summary>Manages service-to-service HTTP dependency detection and graph queries.</summary>
[ApiController]
[Route("api/dependencies")]
public class DependenciesController(
    ServicePulseDbContext db,
    ILogger<DependenciesController> logger) : ControllerBase
{
    /// <summary>
    /// Records a detected dependency between two services. Called automatically by the
    /// client library's <c>DependencyDetectionHandler</c>. Idempotent — repeated calls for
    /// the same pair are silently ignored.
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> RecordDependency([FromBody] RecordDependencyRequest request)
    {
        var source = await db.Services
            .FirstOrDefaultAsync(s => s.ServiceName == request.ServiceName);

        if (source is null)
        {
            logger.LogWarning("Dependency report: source service '{Name}' not registered", request.ServiceName);
            return NotFound($"Source service '{request.ServiceName}' is not registered.");
        }

        var target = await db.Services.FirstOrDefaultAsync(s =>
            (s.BaseUrl != null && s.BaseUrl.StartsWith(request.DependsOnUrl)) ||
            s.ServiceName == request.DependsOnServiceName);

        if (target is null)
        {
            logger.LogWarning(
                "Dependency report: target service matching url '{Url}' or name '{Name}' not registered",
                request.DependsOnUrl, request.DependsOnServiceName);
            return NotFound($"Target service matching '{request.DependsOnUrl}' is not registered.");
        }

        var exists = await db.ServiceDependencies
            .AnyAsync(d => d.ServiceId == source.ServiceId && d.DependsOnServiceId == target.ServiceId);

        if (!exists)
        {
            db.ServiceDependencies.Add(new ServiceDependency
            {
                ServiceId = source.ServiceId,
                DependsOnServiceId = target.ServiceId,
                DiscoveredAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            logger.LogInformation(
                "Recorded dependency: {Source} → {Target}", source.ServiceName, target.ServiceName);
        }

        return Ok();
    }

    /// <summary>Returns the full service dependency graph as a flat list of edges.</summary>
    [HttpGet]
    public async Task<IActionResult> GetDependencyGraph()
    {
        var deps = await db.ServiceDependencies
            .Include(d => d.Caller)
            .Include(d => d.Callee)
            .Select(d => new DependencyGraphItemDto
            {
                SourceId = d.ServiceId,
                SourceName = d.Caller.ServiceName,
                TargetId = d.DependsOnServiceId,
                TargetName = d.Callee.ServiceName,
                DiscoveredAt = d.DiscoveredAt
            })
            .ToListAsync();

        return Ok(deps);
    }
}
