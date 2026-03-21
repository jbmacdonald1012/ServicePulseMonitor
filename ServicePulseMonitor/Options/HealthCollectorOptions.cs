namespace ServicePulseMonitor.Options;

/// <summary>Configures the background health polling service.</summary>
public class HealthCollectorOptions
{
    /// <summary>Polling interval in seconds. Default: 30.</summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>Per-request HTTP timeout in seconds. Default: 10.</summary>
    public int TimeoutSeconds { get; set; } = 10;
}
