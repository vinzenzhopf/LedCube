using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LedCube.Animation.FileFormat.AnimationRaw.Io;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.Test.Fixtures;

/// <summary>Small builders for assembling <see cref="RawAnimation"/> models and round-tripping them.</summary>
internal static class AnimationBuilder
{
    public static byte[] FrameBytes(int stride, byte fill)
    {
        var bytes = new byte[stride];
        Array.Fill(bytes, fill);
        return bytes;
    }

    /// <summary>Deterministic, content-distinct frame payload keyed by <paramref name="seed"/>.</summary>
    public static byte[] PatternBytes(int stride, int seed)
    {
        var bytes = new byte[stride];
        for (var i = 0; i < stride; i++)
        {
            bytes[i] = (byte)(((i * 31) + (seed * 17) + 7) & 0xFF);
        }

        return bytes;
    }

    public static Frame Frame(int stride, int seed) => new(PatternBytes(stride, seed));

    public static AnimationManifest Manifest(
        Point3D? size = null,
        LedFormat ledFormat = LedFormat.Binary,
        int frameCount = 30,
        uint frameTimeUs = 20000,
        bool loop = false,
        string name = "Test Animation",
        string? author = null,
        string? description = null,
        DateTimeOffset? createdUtc = null,
        IReadOnlyDictionary<string, JsonElement>? extraFields = null) => new()
    {
        Name = name,
        Author = author,
        Description = description,
        CreatedUtc = createdUtc,
        Size = size ?? new Point3D(16, 16, 16),
        LedFormat = ledFormat,
        FrameCount = frameCount,
        FrameTimeUs = frameTimeUs,
        Loop = loop,
        ExtraFields = extraFields,
    };

    public static RawAnimation Static(LedFormat ledFormat, Point3D size, int frameCount = 30, byte fill = 0xA5)
    {
        var stride = ledFormat.BytesPerFrame(size);
        return new RawAnimation(
            Manifest(size, ledFormat, frameCount),
            new List<Frame> { new(FrameBytes(stride, fill)) },
            new List<Keyframe> { new(0, 0) });
    }

    public static RawAnimation RoundTrip(RawAnimation animation)
    {
        using var ms = new MemoryStream();
        LcAnimRawWriter.Write(ms, animation);
        ms.Position = 0;
        return LcAnimRawReader.Read(ms);
    }

    public static byte[] WriteToBytes(RawAnimation animation)
    {
        using var ms = new MemoryStream();
        LcAnimRawWriter.Write(ms, animation);
        return ms.ToArray();
    }

    public static RawAnimation ReadFromBytes(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return LcAnimRawReader.Read(ms);
    }
}
