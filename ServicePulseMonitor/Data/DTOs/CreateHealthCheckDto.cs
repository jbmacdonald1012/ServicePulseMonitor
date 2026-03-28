using System.ComponentModel.DataAnnotations;

namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Request body for submitting a health check result.</summary>
public record CreateHealthCheckDto
{
    /// <summary>Health status of the service: <c>Healthy</c>, <c>Degraded</c>, or <c>Unhealthy</c>.</summary>
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(Healthy|Degraded|Unhealthy)$",
        ErrorMessage = "Status must be 'Healthy', 'Degraded', or 'Unhealthy'")]
    public string Status { get; init; } = string.Empty;

    /// <summary>Measured response time in milliseconds (must be non-negative, or <c>null</c> if unavailable).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "Response time must be non-negative")]
    public int? ResponseTimeMs { get; init; }

    /// <summary>Optional structured details about the health check (e.g. component statuses).</summary>
    public Dictionary<string, object>? Details { get; init; }
}
