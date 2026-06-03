using System;
using System.Collections.Generic;
using System.Linq;

namespace LedCube.Animation.FileFormat.AnimationRaw.Model;

/// <summary>
/// The in-memory model of a baked <c>.lcanimraw</c> animation: pure data plus cheap lookups.
/// Anything that computes pixel values lives downstream in <c>LedCube.Core.Animation</c>.
/// </summary>
public sealed class Animation : IEquatable<Animation>
{
    private static readonly IReadOnlyDictionary<string, byte[]> EmptyExtraEntries =
        new Dictionary<string, byte[]>();

    /// <summary>Manifest metadata (size, format, frame timing, ...).</summary>
    public AnimationManifest Manifest { get; }

    /// <summary>Pool of unique frames, indexed by <see cref="Keyframe.Id"/>.</summary>
    public IReadOnlyList<Frame> Frames { get; }

    /// <summary>Keyframe schedule, sorted ascending by <see cref="Keyframe.At"/>.</summary>
    public IReadOnlyList<Keyframe> Keyframes { get; }

    /// <summary>Optional thumbnail image bytes (<c>thumbnail.png</c>), preserved verbatim.</summary>
    public byte[]? Thumbnail { get; }

    /// <summary>
    /// Unknown ZIP entries (e.g. the reserved <c>meta/</c> namespace) preserved verbatim so a
    /// reader that re-saves the file does not lose data. Keyed by full entry name.
    /// </summary>
    public IReadOnlyDictionary<string, byte[]> ExtraEntries { get; }

    public Animation(
        AnimationManifest manifest,
        IReadOnlyList<Frame> frames,
        IReadOnlyList<Keyframe> keyframes,
        byte[]? thumbnail = null,
        IReadOnlyDictionary<string, byte[]>? extraEntries = null)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Frames = frames ?? throw new ArgumentNullException(nameof(frames));
        Keyframes = keyframes ?? throw new ArgumentNullException(nameof(keyframes));
        Thumbnail = thumbnail;
        ExtraEntries = extraEntries ?? EmptyExtraEntries;
    }

    /// <summary>
    /// Returns the index into <see cref="Keyframes"/> of the keyframe active at timeline
    /// position <paramref name="t"/> — the largest keyframe with <c>At &lt;= t</c>.
    /// O(log K) binary search; pure lookup, no rendering.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="t"/> is outside <c>[0, FrameCount)</c>.
    /// </exception>
    public int KeyframeIndexAt(int t)
    {
        if (t < 0 || t >= Manifest.FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(t), t,
                $"Timeline position must be in [0, {Manifest.FrameCount}).");
        }

        int lo = 0, hi = Keyframes.Count - 1, result = 0;
        while (lo <= hi)
        {
            var mid = lo + ((hi - lo) >> 1);
            if (Keyframes[mid].At <= t)
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return result;
    }

    /// <summary>
    /// Convenience cheap-lookup: the pool <see cref="Frame"/> active at timeline position
    /// <paramref name="t"/>. Combines <see cref="KeyframeIndexAt"/> with a pool lookup; no rendering.
    /// </summary>
    public Frame ActiveFrameAt(int t) => Frames[Keyframes[KeyframeIndexAt(t)].Id];

    public bool Equals(Animation? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Manifest.Equals(other.Manifest)
               && Frames.SequenceEqual(other.Frames)
               && Keyframes.SequenceEqual(other.Keyframes)
               && ThumbnailEquals(Thumbnail, other.Thumbnail)
               && ExtraEntriesEqual(ExtraEntries, other.ExtraEntries);
    }

    public override bool Equals(object? obj) => Equals(obj as Animation);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Manifest);
        hash.Add(Frames.Count);
        hash.Add(Keyframes.Count);
        hash.Add(Thumbnail?.Length ?? -1);
        hash.Add(ExtraEntries.Count);
        return hash.ToHashCode();
    }

    private static bool ThumbnailEquals(byte[]? a, byte[]? b)
    {
        if (a is null || b is null)
        {
            return a is null && b is null;
        }

        return a.AsSpan().SequenceEqual(b);
    }

    private static bool ExtraEntriesEqual(
        IReadOnlyDictionary<string, byte[]> a,
        IReadOnlyDictionary<string, byte[]> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (var (key, valueA) in a)
        {
            if (!b.TryGetValue(key, out var valueB) || !valueA.AsSpan().SequenceEqual(valueB))
            {
                return false;
            }
        }

        return true;
    }
}
