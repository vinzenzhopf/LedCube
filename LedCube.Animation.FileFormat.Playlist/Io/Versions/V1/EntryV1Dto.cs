using System.Collections.Generic;

namespace LedCube.Animation.FileFormat.Playlist.Io.Versions.V1;

/// <summary>Frozen JSON shape of a single <c>entries</c> element. Original v1.</summary>
internal sealed record EntryV1Dto
{
    public string? Id { get; init; }
    public string? InstanceName { get; init; }
    public string? TypeName { get; init; }
    public int RepeatCount { get; init; } = 1;
    public uint? FrameTimeUsOverride { get; init; }

    /// <summary>
    /// Config values as JSON primitives. Deserializes to <see cref="System.Text.Json.JsonElement"/>
    /// per value (object target); normalized to CLR primitives by the mapper.
    /// </summary>
    public Dictionary<string, object?>? Config { get; init; }
}
