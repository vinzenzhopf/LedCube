using System;
using System.Collections.Generic;
using LedCube.Animation.FileFormat.Common.Io;

namespace LedCube.Animation.FileFormat.AnimationRaw.Io;

/// <summary>
/// Constants describing the <c>.lcanimraw</c> container layout and version.
/// </summary>
public static class LcAnimRawFormat
{
    /// <summary>The format version this build reads and writes.</summary>
    public const int CurrentVersion = 1;

    public const string ManifestEntry = ContainerEntryNames.Manifest;
    public const string FramesEntry = "frames.bin";
    public const string ThumbnailEntry = "thumbnail.png";

    /// <summary>Entries the reader interprets directly; everything else is preserved as an extra entry.</summary>
    public static readonly IReadOnlySet<string> KnownEntries =
        new HashSet<string>(StringComparer.Ordinal) { ManifestEntry, FramesEntry, ThumbnailEntry };
}
