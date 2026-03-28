using System.Text.Json;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Features.HealthChecks;

/// <summary>Maps between <see cref="HealthCheck"/> entities and their DTO representations.</summary>
public static class HealthCheckMapper
{
    /// <summary>Creates a new <see cref="HealthCheck"/> entity from a health check submission.</summary>
    public static HealthCheck ToEntity(long serviceId, CreateHealthCheckDto dto)
    {
        return new HealthCheck
        {
            ServiceId = serviceId,
            Status = dto.Status,
            ResponseTimeMs = dto.ResponseTimeMs,
            CheckedAt = DateTime.UtcNow,
            Details = dto.Details switch
            {
                not null => JsonSerializer.SerializeToDocument(dto.Details),
                _ => null
            }
        };
    }

    /// <summary>Projects a <see cref="HealthCheck"/> entity to a <see cref="HealthCheckDto"/>.</summary>
    public static HealthCheckDto ToDto(HealthCheck entity)
    {
        return new HealthCheckDto
        {
            HealthCheckId = entity.HealthCheckId,
            ServiceId = entity.ServiceId,
            ServiceName = entity.Service?.ServiceName,
            Status = entity.Status,
            ResponseTimeMs = entity.ResponseTimeMs,
            CheckedAt = entity.CheckedAt,
            Details = entity.Details switch
            {
                not null => JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Details.RootElement),
                _ => null
            }
        };
    }

    /// <summary>Projects a sequence of <see cref="HealthCheck"/> entities to <see cref="HealthCheckDto"/> instances.</summary>
    public static IEnumerable<HealthCheckDto> ToDtoList(IEnumerable<HealthCheck> entities)
    {
        return entities.Select(ToDto);
    }
}
