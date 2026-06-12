using LedCube.Core.Common.Config;
using LedCube.Core.Common.Config.Entities;
using LedCube.Core.Common.Model;
using LedCube.Streamer.UI.Settings;

namespace LedCube.Streamer.UI;

public record LedCubeStreamerSettings
{
    public CubeSettings Cube { get; init; } = new();
    public CubeStreamerSettings Connection { get; init; } = new();
    public LastConnectionSettings LastConnection { get; init; } = new();
    public StreamerUIState UIState { get; init; } = new();
    public KeyboardControlConfig KeyboardControl { get; init; } = new();
    public LibrarySettings Library { get; init; } = new();

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
                // Canonical animation space is right-handed with LED 0 at front-bottom-left (identity).
                // This physical cube is left-handed (same LED-0 corner) and installed turned so its
                // real right side faces front — change these in Settings → Cube orientation.
                HardwareFront = Orientation3D.Front,
                Orientation = CartesianOrientation.LeftHanded,
                InstallationFront = Orientation3D.Right
            }
        }
    };
}
