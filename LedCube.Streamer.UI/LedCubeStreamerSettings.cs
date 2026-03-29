using LedCube.Core.Common.Config;
using LedCube.Core.Common.Config.Entities;
using LedCube.Streamer.UI.Settings;

namespace LedCube.Streamer.UI;

public record LedCubeStreamerSettings
{
    public CubeSettings Cube { get; init; } = new();
    public CubeStreamerSettings Connection { get; init; } = new();
    public StreamerUIState UIState { get; init; } = new();

    public static LedCubeStreamerSettings Default => new()
    {
        Cube = new CubeSettings
        {
            Name = "LedCube",
            Dimensions = new CubeDimensions { X = 8, Y = 8, Z = 8 }
        },
        Connection = new CubeStreamerSettings
        {
            Hostname = "",
            Port = 4242,
            SearchPerBroadcast = true,
            Projection = new CubeDataProjectionSettings
            {
                Orientation = CartesianOrientation.LeftHanded
            }
        }
    };
}
