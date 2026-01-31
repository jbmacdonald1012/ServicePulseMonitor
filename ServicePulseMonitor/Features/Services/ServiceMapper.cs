using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Features.Services;

public static class ServiceMapper
{
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

    public static ServiceDto ToDto(Service entity)
    {
        return new ServiceDto
        {
            ServiceId = entity.ServiceId,
            ServiceName = entity.ServiceName,
            BaseUrl = entity.BaseUrl,
            Description = entity.Description,
            RegisteredAt = entity.RegisteredAt,
            LastSeenAt = entity.LastSeenAt
        };
    }

    public static IEnumerable<ServiceDto> ToDtoList(IEnumerable<Service> entities)
    {
        return entities.Select(ToDto);
    }
}
