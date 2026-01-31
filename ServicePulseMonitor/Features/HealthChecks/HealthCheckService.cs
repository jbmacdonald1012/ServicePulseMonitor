using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;
using Microsoft.Extensions.Logging;

namespace ServicePulseMonitor.Features.HealthChecks;

public class HealthCheckService : IHealthCheckService
{
    private readonly ServicePulseDbContext _context;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(ServicePulseDbContext context, ILogger<HealthCheckService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckDto> SubmitHealthCheckAsync(long serviceId, CreateHealthCheckDto dto)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service is null)
        {
            _logger.LogWarning("Health check submitted for non-existent service: {ServiceId}", serviceId);
            throw new InvalidOperationException($"Service with ID {serviceId} not found");
        }

        var healthCheck = HealthCheckMapper.ToEntity(serviceId, dto);

        _context.HealthChecks.Add(healthCheck);

        service.LastSeenAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Health check submitted for service {ServiceName} (ID: {ServiceId}): Status={Status}",
            service.ServiceName, serviceId, healthCheck.Status);

        healthCheck.Service = service;
        return HealthCheckMapper.ToDto(healthCheck);
    }

    public async Task<HealthCheckDto?> GetHealthCheckByIdAsync(long healthCheckId)
    {
        var healthCheck = await _context.HealthChecks
            .Include(hc => hc.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(hc => hc.HealthCheckId == healthCheckId);

        return healthCheck is null ? null : HealthCheckMapper.ToDto(healthCheck);
    }

    public async Task<IEnumerable<HealthCheckDto>> GetHealthChecksByServiceIdAsync(long serviceId, int limit = 10)
    {
        var healthChecks = await _context.HealthChecks
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
        var healthCheck = await _context.HealthChecks
            .Include(hc => hc.Service)
            .Where(hc => hc.ServiceId == serviceId)
            .OrderByDescending(hc => hc.CheckedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return healthCheck is null ? null : HealthCheckMapper.ToDto(healthCheck);
    }

    public async Task<IEnumerable<HealthCheckDto>> GetHealthChecksByStatusAsync(string status, int limit = 50)
    {
        var healthChecks = await _context.HealthChecks
            .Include(hc => hc.Service)
            .Where(hc => hc.Status == status)
            .OrderByDescending(hc => hc.CheckedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return HealthCheckMapper.ToDtoList(healthChecks);
    }

    public async Task<IEnumerable<HealthCheckDto>> GetAllHealthChecksAsync(int limit = 20, int offset = 0)
    {
        var healthChecks = await _context.HealthChecks
            .Include(hc => hc.Service)
            .OrderByDescending(hc => hc.CheckedAt)
            .Skip(offset)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return HealthCheckMapper.ToDtoList(healthChecks);
    }
}
