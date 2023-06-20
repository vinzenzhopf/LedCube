using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Common.Model
{
    public class Led : ILed<bool>
    {
        public bool Value { get; set; }
    }
}