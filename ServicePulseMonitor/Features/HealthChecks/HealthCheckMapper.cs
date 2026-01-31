using System.Text.Json;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Data.Models;

namespace ServicePulseMonitor.Features.HealthChecks;

public static class HealthCheckMapper
{
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

    public static IEnumerable<HealthCheckDto> ToDtoList(IEnumerable<HealthCheck> entities)
    {
        return entities.Select(ToDto);
    }
}
