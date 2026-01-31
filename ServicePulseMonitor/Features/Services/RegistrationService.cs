using Microsoft.EntityFrameworkCore;
using ServicePulseMonitor.Data;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;
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
}
