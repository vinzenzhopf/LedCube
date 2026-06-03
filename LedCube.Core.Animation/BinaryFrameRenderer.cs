using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Animation;

/// <summary>
/// Renders a <see cref="LedFormat.Binary"/> frame: each LED is one bit (LSB-first bit-packed),
/// mapped onto the cube's on/off LEDs. Assumes <paramref name="sourceSize"/> fits the target
/// (the player enforces the size policy before calling).
/// </summary>
public sealed class BinaryFrameRenderer : IFrameRenderer
{
    public LedFormat Format => LedFormat.Binary;

    public void Render(Frame frame, Point3D sourceSize, ICubeData target)
    {
        target.Clear();

        var data = frame.Data.Span;
        var ledCount = sourceSize.X * sourceSize.Y * sourceSize.Z;
        for (var i = 0; i < ledCount; i++)
        {
            var on = ((data[i >> 3] >> (i & 7)) & 1) != 0;
            if (on)
            {
                target.SetLed(Point3D.CoordinateFromIndex(sourceSize, i), true);
            }
        }
    }
}
