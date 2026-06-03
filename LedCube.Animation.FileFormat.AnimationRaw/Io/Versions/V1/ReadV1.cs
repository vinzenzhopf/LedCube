using System.Collections.Generic;
using System.Linq;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.AnimationRaw.Io.Versions.V1;

/// <summary>
/// Maps the frozen <see cref="ManifestV1Dto"/> onto the current in-memory model. Because v1 is
/// the current version there is no migration hop yet; when v2 lands a
/// <c>MigrateV1ToV2</c> pure function slots in between this mapper and the current model.
/// </summary>
internal static class ReadV1
{
    public static (AnimationManifest Manifest, List<Keyframe> Keyframes) MapManifest(ManifestV1Dto dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
        {
            throw new InvalidFileFormatException("Manifest field 'name' is required and must be non-empty.");
        }

        if (dto.Size is null)
        {
            throw new InvalidFileFormatException("Manifest field 'size' is required.");
        }

        var size = new Point3D(dto.Size.X, dto.Size.Y, dto.Size.Z);
        if (size.X < 1 || size.Y < 1 || size.Z < 1)
        {
            throw new InvalidFileFormatException("Manifest 'size' dimensions must all be >= 1.");
        }

        var ledFormat = ParseLedFormat(dto.LedFormat);

        if (dto.Keyframes is null)
        {
            throw new InvalidFileFormatException("Manifest field 'keyframes' is required.");
        }

        var keyframes = dto.Keyframes.Select(k => new Keyframe(k.At, k.Id)).ToList();

        var manifest = new AnimationManifest
        {
            Name = dto.Name,
            Author = dto.Author,
            Description = dto.Description,
            CreatedUtc = dto.CreatedUtc,
            Size = size,
            LedFormat = ledFormat,
            FrameCount = dto.FrameCount,
            FrameTimeUs = dto.FrameTimeUs,
            Loop = dto.Loop,
            ExtraFields = dto.Extra is { Count: > 0 } ? dto.Extra : null,
        };

        return (manifest, keyframes);
    }

    private static LedFormat ParseLedFormat(string? value) => value switch
    {
        "Binary" => LedFormat.Binary,
        "Grayscale" => LedFormat.Grayscale,
        "Rgb" => LedFormat.Rgb,
        null => throw new InvalidFileFormatException("Manifest field 'ledFormat' is required."),
        _ => throw new InvalidFileFormatException(
            $"Unknown ledFormat '{value}'. Expected one of: Binary, Grayscale, Rgb."),
    };
}
