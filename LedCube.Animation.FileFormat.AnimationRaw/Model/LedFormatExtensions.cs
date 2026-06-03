using System;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.AnimationRaw.Model;

public static class LedFormatExtensions
{
    /// <summary>
    /// Number of payload bytes a single frame occupies for the given cube size and encoding
    /// (the <c>frameStride</c>). For <see cref="LedFormat.Binary"/> this is <c>ceil(N / 8)</c>,
    /// for <see cref="LedFormat.Grayscale"/> it is <c>N</c>, and for <see cref="LedFormat.Rgb"/>
    /// it is <c>N * 3</c>, where <c>N = size.X * size.Y * size.Z</c>.
    /// </summary>
    public static int BytesPerFrame(this LedFormat format, Point3D size)
    {
        if (size.X < 1 || size.Y < 1 || size.Z < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "All cube dimensions must be >= 1.");
        }

        var n = checked(size.X * size.Y * size.Z);
        return format switch
        {
            LedFormat.Binary => (n + 7) / 8,
            LedFormat.Grayscale => n,
            LedFormat.Rgb => checked(n * 3),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown LED format."),
        };
    }
}
