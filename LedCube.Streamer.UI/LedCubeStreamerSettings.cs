using System.Drawing;
using System.Windows.Controls;
using LedCube.Core.Common.Config;
using LedCube.Core.Config;

namespace LedCube.Streamer.UI;

public record LedCubeStreamerSettings : ICubeConfigRepository
{
    public CubeConfig CubeConfig { get; set; } = new();

    public static LedCubeStreamerSettings Default => new LedCubeStreamerSettings()
    {
        CubeConfig = new CubeConfig()
        {
            Name = "LedCube",
            Dimensions = new CubeDimensions() {
                X = 8, 
                Y = 8, 
                Z = 8,
            },
            StreamerSettings = new CubeStreamerSettings()
            {
                Hostname = "",
                Port = 4242,
                SearchPerBroadcast = true,
                Projection = new CubeDataProjectionSettings()
                {
                    Orientation = CartesianOrientation.LeftHanded
                }
            },
            Cube3DDrawingConfig =
            {
                DrawWireframe = true,
                LedType = new LedType()
                {
                    LedDimensions = 5,
                    Shape = LedShape.Zylinder,
                    Tint = Color.Blue
                }
            }
        }
    };
}