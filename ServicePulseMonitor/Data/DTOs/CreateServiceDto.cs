using System.ComponentModel.DataAnnotations;

namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Request body for registering a new service.</summary>
public record CreateServiceDto
{
    /// <summary>Unique name for the service (1–255 characters).</summary>
    [Required(ErrorMessage = "Service name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Service name must be between 1 and 255 characters")]
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>Base URL of the service's health endpoint (optional, must be a valid URL if provided).</summary>
    [Url(ErrorMessage = "Base URL must be a valid URL")]
    [StringLength(500, ErrorMessage = "Base URL must not exceed 500 characters")]
    public string? BaseUrl { get; init; }

    /// <summary>Optional human-readable description of the service (max 1000 characters).</summary>
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; init; }
}
