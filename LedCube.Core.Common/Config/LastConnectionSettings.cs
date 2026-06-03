namespace LedCube.Core.Common.Config;

/// Live, auto-saved connection state: the last used host/port and broadcast-search
/// adapter. Kept separate from the user-configured <see cref="CubeStreamerSettings"/>
/// defaults so transient runtime changes don't overwrite them.
public record LastConnectionSettings
{
    public string Hostname { get; init; } = string.Empty;
    public int Port { get; init; } = 4242;
    public string? AdapterAddress { get; init; }
}
