namespace ServicePulseMonitor.Data.DTOs;

/// <summary>Request body sent by the client library's DependencyDetectionHandler.</summary>
public record RecordDependencyRequest
{
    /// <summary>The name of the service that has this dependency.</summary>
    public string ServiceName { get; init; } = string.Empty;

    /// <summary>The base URL (scheme + authority) of the dependency target.</summary>
    public string DependsOnUrl { get; init; } = string.Empty;

    /// <summary>Optional hostname of the service being depended upon (used as fallback for lookup).</summary>
    public string? DependsOnServiceName { get; init; }
}
