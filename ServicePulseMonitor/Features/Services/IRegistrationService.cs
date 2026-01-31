using ServicePulseMonitor.Data.DTOs;

namespace ServicePulseMonitor.Features.Services;

public interface IRegistrationService
{
    Task<ServiceDto> RegisterServiceAsync(CreateServiceDto dto);
    Task<ServiceDto?> GetServiceByIdAsync(long serviceId);
    Task<ServiceDto?> GetServiceByNameAsync(string serviceName);
    Task<bool> ServiceExistsAsync(string serviceName);
}
