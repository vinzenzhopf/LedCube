using System.Drawing;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Config.Entities;

namespace LedCube.Animator.Settings;

public class LedCubeAnimatorSettings
{
    public CubeSettings Cube { get; init; } = new();
    public Cube3DDrawingConfig Display { get; init; } = new();
    public AnimatorUIState UIState { get; init; } = new();

    public static LedCubeAnimatorSettings Default { get; } = new()
    {
        Cube = new CubeSettings
        {
            Name = "Cube",
            Dimensions = new CubeDimensions { X = 16, Y = 16, Z = 16 }
        },
        Display = new Cube3DDrawingConfig
        {
            DrawWireframe = false,
            LedType = new LedType
            {
                LedDimensions = 5,
                Shape = LedShape.Led,
                Tint = Color.Yellow
            }
        }
    };
}
