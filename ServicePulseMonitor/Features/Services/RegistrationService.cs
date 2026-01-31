using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;
using ServicePulseMonitor.Common;
using Microsoft.Extensions.Logging;

namespace ServicePulseMonitor.Features.Services;

public class RegistrationService : IRegistrationService
{
    private readonly ServicePulseDbContext _context;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(ServicePulseDbContext context, ILogger<RegistrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceDto> RegisterServiceAsync(CreateServiceDto dto)
    {
        var exists = await ServiceExistsAsync(dto.ServiceName);
        if (exists)
        {
            _logger.LogWarning("Attempted to register duplicate service: {ServiceName}", dto.ServiceName);
            throw new InvalidOperationException($"Service with name '{dto.ServiceName}' already exists");
        }

        var service = ServiceMapper.ToEntity(dto);

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Service registered: {ServiceName} (ID: {ServiceId})",
            service.ServiceName, service.ServiceId);

        return ServiceMapper.ToDto(service);
    }

    public async Task<ServiceDto?> GetServiceByIdAsync(long serviceId)
    {
        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

        return service is null ? null : ServiceMapper.ToDto(service);
    }

    public async Task<ServiceDto?> GetServiceByNameAsync(string serviceName)
    {
        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServiceName == serviceName);

        return service is null ? null : ServiceMapper.ToDto(service);
    }

    public async Task<bool> ServiceExistsAsync(string serviceName)
    {
        return await _context.Services
            .AnyAsync(s => s.ServiceName == serviceName);
    }

    public async Task<PagedResult<ServiceDto>> GetAllServicesAsync(int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Services.AsNoTracking();

        var totalCount = await query.CountAsync();

        var services = await query
            .OrderBy(s => s.ServiceName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ServiceDto>
        {
            Items = ServiceMapper.ToDtoList(services),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ServiceDto?> UpdateServiceAsync(long serviceId, UpdateServiceDto dto)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service == null)
        {
            _logger.LogWarning("Attempted to update non-existent service: {ServiceId}", serviceId);
            return null;
        }

        if (service.ServiceName != dto.ServiceName)
        {
            var nameExists = await ServiceExistsAsync(dto.ServiceName);
            if (nameExists)
            {
                _logger.LogWarning("Attempted to update service {ServiceId} with duplicate name: {ServiceName}",
                    serviceId, dto.ServiceName);
                throw new InvalidOperationException($"Service with name '{dto.ServiceName}' already exists");
            }
        }

        service.ServiceName = dto.ServiceName;
        service.BaseUrl = dto.BaseUrl;
        service.Description = dto.Description;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Service updated: {ServiceName} (ID: {ServiceId})",
            service.ServiceName, serviceId);

        return ServiceMapper.ToDto(service);
    }

    public async Task<bool> DeleteServiceAsync(long serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service == null)
        {
            _logger.LogWarning("Attempted to delete non-existent service: {ServiceId}", serviceId);
            return false;
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Service deleted: {ServiceName} (ID: {ServiceId})",
            service.ServiceName, serviceId);

        return true;
    }

    public async Task<IEnumerable<ServiceDto>> SearchServicesByNameAsync(string query)
    {
        var services = await _context.Services
            .Where(s => s.ServiceName.Contains(query))
            .AsNoTracking()
            .OrderBy(s => s.ServiceName)
            .ToListAsync();

        return ServiceMapper.ToDtoList(services);
    }

    public async Task<ServiceHealthSummaryDto?> GetServiceHealthSummaryAsync(long serviceId)
    {
        var service = await _context.Services
            .Include(s => s.HealthChecks)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

        if (service == null)
        {
            return null;
        }

        var healthChecks = service.HealthChecks.ToList();
        var latestCheck = healthChecks.OrderByDescending(hc => hc.CheckedAt).FirstOrDefault();

        var healthyCount = healthChecks.Count(hc => hc.Status == "Healthy");
        var degradedCount = healthChecks.Count(hc => hc.Status == "Degraded");
        var unhealthyCount = healthChecks.Count(hc => hc.Status == "Unhealthy");

        var avgResponseTime = healthChecks
            .Where(hc => hc.ResponseTimeMs.HasValue)
            .Average(hc => (double?)hc.ResponseTimeMs);

        var uptimePercentage = healthChecks.Count > 0
            ? (double)healthyCount / healthChecks.Count * 100
            : 0;

        return new ServiceHealthSummaryDto
        {
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
            BaseUrl = service.BaseUrl,
            CurrentStatus = latestCheck?.Status,
            LastCheckAt = latestCheck?.CheckedAt,
            TotalHealthChecks = healthChecks.Count,
            HealthyCount = healthyCount,
            DegradedCount = degradedCount,
            UnhealthyCount = unhealthyCount,
            AverageResponseTimeMs = avgResponseTime,
            UptimePercentage = Math.Round(uptimePercentage, 2)
        };
    }
}
