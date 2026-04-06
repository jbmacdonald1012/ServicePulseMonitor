using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Common;

namespace ServicePulseMonitor.Features.Services;

/// <summary>Manages registration and lifecycle of monitored services.</summary>
public interface IRegistrationService
{
    /// <summary>Registers a new service. Throws <see cref="InvalidOperationException"/> if the name is already taken.</summary>
    Task<ServiceDto> RegisterServiceAsync(CreateServiceDto dto);

    /// <summary>Returns a service by its ID, or <c>null</c> if not found.</summary>
    Task<ServiceDto?> GetServiceByIdAsync(long serviceId);

    /// <summary>Returns a service by its exact name, or <c>null</c> if not found.</summary>
    Task<ServiceDto?> GetServiceByNameAsync(string serviceName);

    /// <summary>Returns <c>true</c> if a service with the given name is already registered.</summary>
    Task<bool> ServiceExistsAsync(string serviceName);

    /// <summary>Returns a paginated, name-ordered list of all registered services.</summary>
    Task<PagedResult<ServiceDto>> GetAllServicesAsync(int pageNumber = 1, int pageSize = 20);

    /// <summary>Updates a service's details. Returns <c>null</c> if not found. Throws <see cref="InvalidOperationException"/> on duplicate name.</summary>
    Task<ServiceDto?> UpdateServiceAsync(long serviceId, UpdateServiceDto dto);

    /// <summary>Deletes a service by its ID. Returns <c>false</c> if not found.</summary>
    Task<bool> DeleteServiceAsync(long serviceId);

    /// <summary>Returns all services whose names contain <paramref name="query"/>, ordered alphabetically.</summary>
    Task<IEnumerable<ServiceDto>> SearchServicesByNameAsync(string query);

    /// <summary>Returns aggregated health statistics for a service, or <c>null</c> if not found.</summary>
    Task<ServiceHealthSummaryDto?> GetServiceHealthSummaryAsync(long serviceId);

    /// <summary>
    /// Registers the service if it does not already exist; otherwise updates its <c>BaseUrl</c> and
    /// <c>Description</c> in-place. Used by the self-registration path so that services can re-register
    /// after a restart without being rejected with a conflict.
    /// </summary>
    Task<ServiceDto> UpsertServiceAsync(CreateServiceDto dto);
}
