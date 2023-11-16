namespace LedCube.Core.Config;

public record CubeStreamerSettings()
{
    public int Port { get; set; } = 4242;
    public string Hostname { get; set; } = string.Empty;
    public bool SearchPerBroadcast { get; set; } = true;
    public CubeDataProjectionSettings Projection = new();
}