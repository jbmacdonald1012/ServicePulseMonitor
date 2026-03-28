using System.ComponentModel.DataAnnotations;

namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Request body for updating an existing service registration.</summary>
public record UpdateServiceDto
{
    /// <summary>New unique name for the service (1–255 characters).</summary>
    [Required(ErrorMessage = "Service name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Service name must be between 1 and 255 characters")]
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>New base URL of the service's health endpoint (optional, must be a valid URL if provided).</summary>
    [Url(ErrorMessage = "Base URL must be a valid URL")]
    [StringLength(500, ErrorMessage = "Base URL must not exceed 500 characters")]
    public string? BaseUrl { get; init; }

    /// <summary>Updated description of the service (max 1000 characters, optional).</summary>
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; init; }
}
