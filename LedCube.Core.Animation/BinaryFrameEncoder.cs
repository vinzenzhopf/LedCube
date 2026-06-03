using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Animation;

/// <summary>
/// Encodes a <see cref="LedFormat.Binary"/> frame: each LED becomes one bit (LSB-first bit-packed)
/// using the same index ordering as <see cref="BinaryFrameRenderer"/>, so encode∘render round-trips
/// exactly through the real playback path.
/// </summary>
public sealed class BinaryFrameEncoder : IFrameEncoder
{
    public LedFormat Format => LedFormat.Binary;

    public Frame Encode(ICubeData source)
    {
        var size = source.Size;
        var data = new byte[Format.BytesPerFrame(size)];

        var ledCount = size.X * size.Y * size.Z;
        for (var i = 0; i < ledCount; i++)
        {
            if (source.GetLed(Point3D.CoordinateFromIndex(size, i)))
            {
                data[i >> 3] |= (byte)(1 << (i & 7));
            }
        }

        return new Frame(data);
    }
}
