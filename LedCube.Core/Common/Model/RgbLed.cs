using System.Drawing;

namespace LedCube.Core.Common.Model
{
    public class RgbLed : ILed<Color>
    {
        public Color Value { get; set; }
    }
}