using System.ComponentModel.DataAnnotations;

namespace ServicePulseMonitor.Data.DTOs;

public record CreateHealthCheckDto
{
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(Healthy|Degraded|Unhealthy)$",
        ErrorMessage = "Status must be 'Healthy', 'Degraded', or 'Unhealthy'")]
    public string Status { get; init; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Response time must be non-negative")]
    public int? ResponseTimeMs { get; init; }

    public Dictionary<string, object>? Details { get; init; }
}
