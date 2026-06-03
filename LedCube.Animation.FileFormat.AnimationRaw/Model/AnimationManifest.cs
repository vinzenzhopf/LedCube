using System;
using System.Collections.Generic;
using System.Text.Json;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.AnimationRaw.Model;

/// <summary>
/// The deserialized <c>manifest.json</c> of a baked animation, excluding the keyframe
/// schedule (which is exposed via <see cref="Animation.Keyframes"/>). Always represents the
/// current format version; no <c>formatVersion</c> is exposed to callers.
/// </summary>
public sealed record AnimationManifest
{
    /// <summary>Display name. Not unique, not used for lookup.</summary>
    public required string Name { get; init; }

    /// <summary>Optional free-text author.</summary>
    public string? Author { get; init; }

    /// <summary>Optional free-text description.</summary>
    public string? Description { get; init; }

    /// <summary>Optional UTC creation timestamp.</summary>
    public DateTimeOffset? CreatedUtc { get; init; }

    /// <summary>Authored cube size; all dimensions are &gt;= 1. Not restricted to 16³.</summary>
    public required Point3D Size { get; init; }

    /// <summary>Per-LED encoding of the frame payloads.</summary>
    public required LedFormat LedFormat { get; init; }

    /// <summary>Timeline length in frames (not the number of unique pool frames).</summary>
    public required int FrameCount { get; init; }

    /// <summary>Authoring frame period in microseconds. A player may override it.</summary>
    public required uint FrameTimeUs { get; init; }

    /// <summary>
    /// Authoring hint: the animation is a seamless loop (its last frame flows back into the first).
    /// Informational only — playback repetition is controlled by the player/playlist, not this flag.
    /// Defaults to false.
    /// </summary>
    public bool SeamlessLoop { get; init; }

    /// <summary>
    /// Unknown manifest fields preserved verbatim for round-tripping newer files through
    /// this reader. Null when the manifest had no unrecognized fields.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement>? ExtraFields { get; init; }
}
