using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Features.Services;

/// <summary>Maps between <see cref="Service"/> entities and their DTO representations.</summary>
public static class ServiceMapper
{
    /// <summary>Creates a new <see cref="Service"/> entity from a registration request.</summary>
    public static Service ToEntity(CreateServiceDto dto)
    {
        return new Service
        {
            ServiceName = dto.ServiceName,
            BaseUrl = dto.BaseUrl,
            Description = dto.Description,
            RegisteredAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow
        };
    }

    /// <summary>Projects a <see cref="Service"/> entity to a <see cref="ServiceDto"/>.</summary>
    public static ServiceDto ToDto(Service entity)
    {
        return new ServiceDto
        {
            ServiceId = entity.ServiceId,
            ServiceName = entity.ServiceName,
            BaseUrl = entity.BaseUrl,
            Description = entity.Description,
            RegisteredAt = entity.RegisteredAt,
            LastSeenAt = entity.LastSeenAt,
            CurrentStatus = entity.CurrentStatus
        };
    }

    /// <summary>Projects a sequence of <see cref="Service"/> entities to <see cref="ServiceDto"/> instances.</summary>
    public static IEnumerable<ServiceDto> ToDtoList(IEnumerable<Service> entities)
    {
        return entities.Select(ToDto);
    }
}
