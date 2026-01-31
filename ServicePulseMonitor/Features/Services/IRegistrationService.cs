using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Common;

namespace ServicePulseMonitor.Features.Services;

public interface IRegistrationService
{
    Task<ServiceDto> RegisterServiceAsync(CreateServiceDto dto);
    Task<ServiceDto?> GetServiceByIdAsync(long serviceId);
    Task<ServiceDto?> GetServiceByNameAsync(string serviceName);
    Task<bool> ServiceExistsAsync(string serviceName);
    Task<PagedResult<ServiceDto>> GetAllServicesAsync(int pageNumber = 1, int pageSize = 20);
    Task<ServiceDto?> UpdateServiceAsync(long serviceId, UpdateServiceDto dto);
    Task<bool> DeleteServiceAsync(long serviceId);
    Task<IEnumerable<ServiceDto>> SearchServicesByNameAsync(string query);
    Task<ServiceHealthSummaryDto?> GetServiceHealthSummaryAsync(long serviceId);
}
