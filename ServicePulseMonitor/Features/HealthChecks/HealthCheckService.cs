using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Common;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;
using Microsoft.Extensions.Logging;

namespace ServicePulseMonitor.Features.HealthChecks;

public class HealthCheckService(ServicePulseDbContext context, ILogger<HealthCheckService> logger)
    : IHealthCheckService
{
    public async Task<HealthCheckDto> SubmitHealthCheckAsync(long serviceId, CreateHealthCheckDto dto)
    {
        var service = await context.Services.FindAsync(serviceId);
        if (service is null)
        {
            logger.LogWarning("Health check submitted for non-existent service: {ServiceId}", serviceId);
            throw new InvalidOperationException($"Service with ID {serviceId} not found");
        }

        var healthCheck = HealthCheckMapper.ToEntity(serviceId, dto);

        context.HealthChecks.Add(healthCheck);

        service.LastSeenAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Health check submitted for service {ServiceName} (ID: {ServiceId}): Status={Status}",
            service.ServiceName, serviceId, healthCheck.Status);

        healthCheck.Service = service;
        return HealthCheckMapper.ToDto(healthCheck);
    }

    public async Task<HealthCheckDto?> GetHealthCheckByIdAsync(long healthCheckId)
    {
        var healthCheck = await context.HealthChecks
            .Include(hc => hc.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(hc => hc.HealthCheckId == healthCheckId);

        return healthCheck is null ? null : HealthCheckMapper.ToDto(healthCheck);
    }

    public async Task<IEnumerable<HealthCheckDto>> GetHealthChecksByServiceIdAsync(long serviceId, int limit = 10)
    {
        var healthChecks = await context.HealthChecks
            .Include(hc => hc.Service)
            .Where(hc => hc.ServiceId == serviceId)
            .OrderByDescending(hc => hc.CheckedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return HealthCheckMapper.ToDtoList(healthChecks);
    }

    public async Task<HealthCheckDto?> GetLatestHealthCheckAsync(long serviceId)
    {
        var healthCheck = await context.HealthChecks
            .Include(hc => hc.Service)
            .Where(hc => hc.ServiceId == serviceId)
            .OrderByDescending(hc => hc.CheckedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return healthCheck is null ? null : HealthCheckMapper.ToDto(healthCheck);
    }

    public async Task<IEnumerable<HealthCheckDto>> GetHealthChecksByStatusAsync(string status, int limit = 50)
    {
        var healthChecks = await context.HealthChecks
            .Include(hc => hc.Service)
            .Where(hc => hc.Status == status)
            .OrderByDescending(hc => hc.CheckedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return HealthCheckMapper.ToDtoList(healthChecks);
    }

    public async Task<PagedResult<HealthCheckDto>> GetAllHealthChecksAsync(int pageNumber = 1, int pageSize = 20)
    {
        var query = context.HealthChecks
            .Include(hc => hc.Service)
            .OrderByDescending(hc => hc.CheckedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync();
        var healthChecks = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<HealthCheckDto>
        {
            Items = HealthCheckMapper.ToDtoList(healthChecks),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
