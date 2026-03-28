using ServicePulseMonitor.Common;
using ServicePulseMonitor.Data.DTOs;

namespace ServicePulseMonitor.Features.HealthChecks;

/// <summary>Manages recording and querying of health check results for monitored services.</summary>
public interface IHealthCheckService
{
    /// <summary>Submits a health check result for a service, updating its status and generating alerts if the status changed.</summary>
    Task<HealthCheckDto> SubmitHealthCheckAsync(long serviceId, CreateHealthCheckDto dto);

    /// <summary>Returns a health check by its ID, or <c>null</c> if not found.</summary>
    Task<HealthCheckDto?> GetHealthCheckByIdAsync(long healthCheckId);

    /// <summary>Returns the most recent health checks for a service, optionally filtered by time range.</summary>
    Task<IEnumerable<HealthCheckDto>> GetHealthChecksByServiceIdAsync(long serviceId, int limit = 10, DateTime? from = null, DateTime? to = null);

    /// <summary>Returns the single most recent health check for a service, or <c>null</c> if none exist.</summary>
    Task<HealthCheckDto?> GetLatestHealthCheckAsync(long serviceId);

    /// <summary>Returns the most recent health checks with a given status across all services.</summary>
    Task<IEnumerable<HealthCheckDto>> GetHealthChecksByStatusAsync(string status, int limit = 50);

    /// <summary>Returns a paginated list of all health checks ordered by <c>CheckedAt</c> descending.</summary>
    Task<PagedResult<HealthCheckDto>> GetAllHealthChecksAsync(int pageNumber = 1, int pageSize = 20);
}
