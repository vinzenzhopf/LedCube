using System.Drawing;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Common.Model
{
    public class RgbLed : ILed<Color>
    {
        public Color Value { get; set; }
    }
}