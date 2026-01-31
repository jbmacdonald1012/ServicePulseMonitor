using System.ComponentModel.DataAnnotations;

namespace ServicePulseMonitor.Data.DTOs;

public class CreateHealthCheckDto
{
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(Healthy|Degraded|Unhealthy)$",
        ErrorMessage = "Status must be 'Healthy', 'Degraded', or 'Unhealthy'")]
    public string Status { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Response time must be non-negative")]
    public int? ResponseTimeMs { get; set; }

    public Dictionary<string, object>? Details { get; set; }
}
