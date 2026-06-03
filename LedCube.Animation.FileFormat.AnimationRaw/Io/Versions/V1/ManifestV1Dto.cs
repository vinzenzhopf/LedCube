using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LedCube.Animation.FileFormat.AnimationRaw.Io.Versions.V1;

/// <summary>
/// Frozen JSON shape of the v1 <c>manifest.json</c>. This record is never modified once a
/// later version ships; a new version means a new DTO + deserializer + migration.
/// New optional fields added within v1's lifetime must be marked with their version-of-introduction.
/// </summary>
internal sealed record ManifestV1Dto
{
    public int FormatVersion { get; init; }
    public string? Name { get; init; }
    public string? Author { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? CreatedUtc { get; init; }
    public SizeDto? Size { get; init; }
    public string? LedFormat { get; init; }
    public int FrameCount { get; init; }
    public uint FrameTimeUs { get; init; }
    public bool SeamlessLoop { get; init; }
    public List<KeyframeDto>? Keyframes { get; init; }

    /// <summary>Unknown/forward-compatible manifest fields, preserved for round-tripping.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}
