using System;
using System.IO;
using LedCube.Core.Common.Model;

namespace LedCube.Core.UI.Services.Library.Model;

public record LibraryAnimationEntry
{
    public required string FilePath { get; init; }
    public required string Name { get; init; }
    public string? Author { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? CreatedUtc { get; init; }
    public Point3D Size { get; init; }
    public int FrameCount { get; init; }
    public uint FrameTimeUs { get; init; }
    public bool SeamlessLoop { get; init; }

    public string FileName => Path.GetFileName(FilePath);

    /// <summary>Total runtime derived from the authored frame period and frame count.</summary>
    public TimeSpan Duration => TimeSpan.FromMicroseconds((double)FrameTimeUs * FrameCount);

    /// <summary>Authored frames per second (0 when the frame period is unknown).</summary>
    public double Fps => FrameTimeUs > 0 ? 1_000_000d / FrameTimeUs : 0d;
}
