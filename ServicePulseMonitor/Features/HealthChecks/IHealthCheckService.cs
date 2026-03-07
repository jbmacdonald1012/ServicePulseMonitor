using ServicePulseMonitor.Common;
using ServicePulseMonitor.Data.DTOs;

namespace ServicePulseMonitor.Features.HealthChecks;

public interface IHealthCheckService
{
    Task<HealthCheckDto> SubmitHealthCheckAsync(long serviceId, CreateHealthCheckDto dto);
    Task<HealthCheckDto?> GetHealthCheckByIdAsync(long healthCheckId);
    Task<IEnumerable<HealthCheckDto>> GetHealthChecksByServiceIdAsync(long serviceId, int limit = 10);
    Task<HealthCheckDto?> GetLatestHealthCheckAsync(long serviceId);
    Task<IEnumerable<HealthCheckDto>> GetHealthChecksByStatusAsync(string status, int limit = 50);
    Task<PagedResult<HealthCheckDto>> GetAllHealthChecksAsync(int pageNumber = 1, int pageSize = 20);
}
