using System.Drawing;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Config.Config;

namespace LedCube.Animator.Settings;

public class LedCubeAnimatorSettings : ICubeConfigRepository
{
    public LedCubeAnimatorSettings()
    {
    }
    
    public LedCubeAnimatorSettings(LedCubeAnimatorSettings other)
    {
    }

    public CubeConfig CubeConfig { get; init; }

    public static LedCubeAnimatorSettings Default { get; } = new()
    {
        CubeConfig = new CubeConfig(){
            Cube3DDrawingConfig = new Cube3DDrawingConfig()
            {
                DrawWireframe = false,
                LedType = new LedType()
                {
                    LedDimensions = 5,
                    Shape = LedShape.Led,
                    Tint = Color.Yellow
                }
            },
            Dimensions = new CubeDimensions()
            {
                X = 16, Y = 16, Z = 16
            },
            Name = "Cube",
            StreamerSettings = new CubeStreamerSettings()
            {
                Hostname = "",
                Port = 6666,
                Projection = new CubeDataProjectionSettings()
                {
                    Orientation = CartesianOrientation.LeftHanded
                },
                SearchPerBroadcast = false
            }
        }
    };
}