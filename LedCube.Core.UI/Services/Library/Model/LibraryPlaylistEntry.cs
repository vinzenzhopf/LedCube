using System;
using System.IO;

namespace LedCube.Core.UI.Services.Library.Model;

public record LibraryPlaylistEntry
{
    public required string FilePath { get; init; }
    public string? Name { get; init; }
    public string? Author { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? CreatedUtc { get; init; }

    public string FileName => Path.GetFileName(FilePath);

    /// <summary>The manifest name when present, else the bare file name.</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? FileName : Name;
}
