using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LedCube.Animation.FileFormat.AnimationRaw.Io.Versions.V1;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Common.Io;
using AnimationModel = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;

namespace LedCube.Animation.FileFormat.AnimationRaw.Io;

/// <summary>
/// Writes an <see cref="Animation"/> to a <c>.lcanimraw</c> container, always at the current
/// format version. Identical frame payloads are deduplicated into a single pool entry and
/// keyframe id references are rewritten to match.
/// </summary>
public static class LcAnimRawWriter
{
    public static void Write(Stream stream, AnimationModel animation)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(animation);

        var manifest = animation.Manifest;
        var stride = manifest.LedFormat.BytesPerFrame(manifest.Size);

        // The writer MUST guarantee the same constraints the reader validates.
        AnimationValidator.Validate(manifest, animation.Frames.Count, animation.Keyframes);

        for (var i = 0; i < animation.Frames.Count; i++)
        {
            var length = animation.Frames[i].Data.Length;
            if (length != stride)
            {
                throw new InvalidFileFormatException(
                    $"Frame {i} has {length} bytes but the frame stride for size {manifest.Size} " +
                    $"and format {manifest.LedFormat} is {stride}.");
            }
        }

        var (pool, remap) = Deduplicate(animation.Frames);
        var keyframes = animation.Keyframes.Select(k => new Keyframe(k.At, remap[k.Id])).ToList();

        var manifestBytes = FormatJson.SerializeToUtf8Bytes(BuildDto(manifest, keyframes));

        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);

        ZipEntries.Write(archive, LcAnimRawFormat.ManifestEntry, manifestBytes, CompressionLevel.Optimal);

        var framesBin = new byte[pool.Count * stride];
        for (var i = 0; i < pool.Count; i++)
        {
            pool[i].Data.Span.CopyTo(framesBin.AsSpan(i * stride, stride));
        }

        ZipEntries.Write(archive, LcAnimRawFormat.FramesEntry, framesBin, CompressionLevel.Optimal);

        if (animation.Thumbnail is not null)
        {
            ZipEntries.Write(archive, LcAnimRawFormat.ThumbnailEntry, animation.Thumbnail, CompressionLevel.Optimal);
        }

        foreach (var (name, data) in animation.ExtraEntries)
        {
            if (LcAnimRawFormat.KnownEntries.Contains(name))
            {
                throw new InvalidFileFormatException(
                    $"Extra entry name '{name}' collides with a reserved container entry.");
            }

            ZipEntries.Write(archive, name, data, CompressionLevel.Optimal);
        }
    }

    private static (List<Frame> Pool, int[] Remap) Deduplicate(IReadOnlyList<Frame> frames)
    {
        var pool = new List<Frame>();
        var remap = new int[frames.Count];
        var seen = new Dictionary<byte[], int>(ByteArrayComparer.Instance);

        for (var i = 0; i < frames.Count; i++)
        {
            var key = frames[i].Data.ToArray();
            if (seen.TryGetValue(key, out var existing))
            {
                remap[i] = existing;
            }
            else
            {
                var newIndex = pool.Count;
                seen[key] = newIndex;
                pool.Add(frames[i]);
                remap[i] = newIndex;
            }
        }

        return (pool, remap);
    }

    private static ManifestV1Dto BuildDto(AnimationManifest manifest, List<Keyframe> keyframes) => new()
    {
        FormatVersion = LcAnimRawFormat.CurrentVersion,
        Name = manifest.Name,
        Author = manifest.Author,
        Description = manifest.Description,
        CreatedUtc = manifest.CreatedUtc,
        Size = new SizeDto(manifest.Size.X, manifest.Size.Y, manifest.Size.Z),
        LedFormat = manifest.LedFormat.ToString(),
        FrameCount = manifest.FrameCount,
        FrameTimeUs = manifest.FrameTimeUs,
        SeamlessLoop = manifest.SeamlessLoop,
        Keyframes = keyframes.Select(k => new KeyframeDto { At = k.At, Id = k.Id }).ToList(),
        Extra = manifest.ExtraFields is { Count: > 0 }
            ? new Dictionary<string, System.Text.Json.JsonElement>(manifest.ExtraFields)
            : null,
    };
}
