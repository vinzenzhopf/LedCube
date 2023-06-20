using System.Drawing;
using System.Windows.Controls;
using LedCube.Core.Common.Config;

namespace LedCube.Streamer.UI;

public record LedCubeStreamerSettings
{
    public CubeConfig CubeConfig { get; set; } = new();

    public static LedCubeStreamerSettings Default => new LedCubeStreamerSettings()
    {
        CubeConfig = new CubeConfig()
        {
            Dimensions = new CubeDimensions() {
                X = 8, 
                Y = 8, 
                Z = 8, 
                Orientation = EuclideanOrientation.RightHanded
            },
            DrawWireframe = true,
            LedType = new LedType()
            {
                LedDimensions = 5,
                Shape = LedShape.Zylinder,
                Tint = Color.Blue
            },
            Name = "LedCube"
        }
    };
}