using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using LedCube.Animation.FileFormat.AnimationRaw.Io.Versions.V1;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Common.Io;
using AnimationModel = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;

namespace LedCube.Animation.FileFormat.AnimationRaw.Io;

/// <summary>
/// Reads a <c>.lcanimraw</c> container from a stream into the current <see cref="Animation"/> model.
/// No format version is exposed to callers; version dispatch is internal.
/// </summary>
public static class LcAnimRawReader
{
    public static AnimationModel Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        var manifestBytes = ZipEntries.ReadRequired(archive, LcAnimRawFormat.ManifestEntry);
        var version = FormatJson.PeekFormatVersion(manifestBytes);

        var (manifest, keyframes) = version switch
        {
            1 => ReadV1.MapManifest(FormatJson.Deserialize<ManifestV1Dto>(manifestBytes)),
            _ => throw new UnsupportedFormatVersionException(version, LcAnimRawFormat.CurrentVersion),
        };

        var framesBytes = ZipEntries.ReadRequired(archive, LcAnimRawFormat.FramesEntry);
        var thumbnail = ZipEntries.ReadOptional(archive, LcAnimRawFormat.ThumbnailEntry);
        var extraEntries = ReadExtraEntries(archive);

        var stride = manifest.LedFormat.BytesPerFrame(manifest.Size);
        var frames = SliceFrames(framesBytes, stride);

        AnimationValidator.Validate(manifest, frames.Count, keyframes);

        return new AnimationModel(
            manifest,
            new ReadOnlyCollection<Frame>(frames),
            new ReadOnlyCollection<Keyframe>(keyframes),
            thumbnail,
            extraEntries);
    }

    /// <summary>
    /// Reads only the manifest metadata (size, format, frame timing, ...) without decompressing
    /// <c>frames.bin</c>. Cheap regardless of file size; useful when a caller needs the authored
    /// frame time or size before committing to a full load. Does not validate the keyframe pool.
    /// </summary>
    public static AnimationManifest ReadManifest(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        var manifestBytes = ZipEntries.ReadRequired(archive, LcAnimRawFormat.ManifestEntry);
        var version = FormatJson.PeekFormatVersion(manifestBytes);

        var (manifest, _) = version switch
        {
            1 => ReadV1.MapManifest(FormatJson.Deserialize<ManifestV1Dto>(manifestBytes)),
            _ => throw new UnsupportedFormatVersionException(version, LcAnimRawFormat.CurrentVersion),
        };

        return manifest;
    }

    private static List<Frame> SliceFrames(byte[] framesBytes, int stride)
    {
        if (stride <= 0)
        {
            throw new InvalidFileFormatException($"Computed frame stride must be positive, but was {stride}.");
        }

        if (framesBytes.Length % stride != 0)
        {
            throw new InvalidFileFormatException(
                $"frames.bin length ({framesBytes.Length}) is not a multiple of the frame stride ({stride}).");
        }

        var count = framesBytes.Length / stride;
        var frames = new List<Frame>(count);
        for (var i = 0; i < count; i++)
        {
            var slice = new byte[stride];
            Array.Copy(framesBytes, i * stride, slice, 0, stride);
            frames.Add(new Frame(slice));
        }

        return frames;
    }

    private static IReadOnlyDictionary<string, byte[]>? ReadExtraEntries(ZipArchive archive)
    {
        Dictionary<string, byte[]>? extras = null;
        foreach (var entry in archive.Entries)
        {
            var name = entry.FullName;
            if (LcAnimRawFormat.KnownEntries.Contains(name) || name.EndsWith('/'))
            {
                continue;
            }

            extras ??= new Dictionary<string, byte[]>(StringComparer.Ordinal);
            extras[name] = ZipEntries.ReadAll(entry);
        }

        return extras;
    }
}
