using System.Drawing;

namespace LedCube.Core.Common.Model.Cube;

public class RgbLed : ILed<Color>
{
    public Color Value { get; set; }
    public static ILed<Color> Create()
    {
        return new RgbLed();
    }
}