namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Represents one edge in the service dependency graph.</summary>
public record DependencyGraphItemDto
{
    /// <summary>ID of the calling service.</summary>
    public long SourceId { get; init; }

    /// <summary>Name of the calling service.</summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>ID of the service being called.</summary>
    public long TargetId { get; init; }

    /// <summary>Name of the service being called.</summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>UTC timestamp when this dependency was first detected.</summary>
    public DateTime DiscoveredAt { get; init; }
}
