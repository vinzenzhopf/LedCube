namespace LedCube.Core.Common.Config;

public record CubeStreamerSettings()
{
    public int Port { get; set; } = 4242;
    public string Hostname { get; set; } = string.Empty;
    public bool SearchPerBroadcast { get; set; } = true;
    public CubeDataProjectionSettings Projection = new();

    /// <summary>
    /// When enabled, every sent/received datagram is logged at Trace level (incl. raw bytes).
    /// Diagnostics only — this runs on the per-frame hot path and impacts streaming performance.
    /// </summary>
    public bool EnableTraceDatagramLogging { get; set; } = false;
}