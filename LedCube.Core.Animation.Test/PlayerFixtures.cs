using System.Collections.Generic;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;

namespace LedCube.Core.Animation.Test;

internal static class PlayerFixtures
{
    public static AnimationManifest Manifest(
        Point3D size,
        LedFormat format = LedFormat.Binary,
        int frameCount = 10,
        uint frameTimeUs = 1000,
        bool loop = false) => new()
    {
        Name = "Test",
        Size = size,
        LedFormat = format,
        FrameCount = frameCount,
        FrameTimeUs = frameTimeUs,
        SeamlessLoop = loop,
    };

    /// <summary>A bit-packed binary frame of the right stride with the given LED indices turned on.</summary>
    public static byte[] BinaryFrame(Point3D size, params int[] onLeds)
    {
        var data = new byte[LedFormat.Binary.BytesPerFrame(size)];
        foreach (var i in onLeds)
        {
            data[i >> 3] |= (byte)(1 << (i & 7));
        }

        return data;
    }

    public static RawAnimation Build(
        AnimationManifest manifest,
        IReadOnlyList<Frame> frames,
        IReadOnlyList<Keyframe> keyframes) => new(manifest, frames, keyframes);
}
