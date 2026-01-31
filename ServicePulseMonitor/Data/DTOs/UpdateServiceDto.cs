using System.ComponentModel.DataAnnotations;

namespace ServicePulseMonitor.Data.DTOs;

public class UpdateServiceDto
{
    [Required(ErrorMessage = "Service name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Service name must be between 1 and 255 characters")]
    public string ServiceName { get; set; } = string.Empty;

    [Url(ErrorMessage = "Base URL must be a valid URL")]
    [StringLength(500, ErrorMessage = "Base URL must not exceed 500 characters")]
    public string? BaseUrl { get; set; }

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; set; }
}
