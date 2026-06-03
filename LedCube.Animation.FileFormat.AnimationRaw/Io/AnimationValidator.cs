using System.Collections.Generic;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Animation.FileFormat.Common.Exceptions;

namespace LedCube.Animation.FileFormat.AnimationRaw.Io;

/// <summary>
/// Enforces the keyframe/pool constraints the reader MUST validate and the writer MUST guarantee.
/// </summary>
internal static class AnimationValidator
{
    public static void Validate(AnimationManifest manifest, int poolSize, IReadOnlyList<Keyframe> keyframes)
    {
        // Constraint 6: frameCount >= 1.
        if (manifest.FrameCount < 1)
        {
            throw new InvalidFileFormatException($"frameCount must be >= 1, but was {manifest.FrameCount}.");
        }

        // Constraint 1: keyframes is non-empty.
        if (keyframes.Count == 0)
        {
            throw new InvalidFileFormatException("keyframes must be non-empty.");
        }

        // Constraint 2: keyframes[0].at == 0.
        if (keyframes[0].At != 0)
        {
            throw new InvalidFileFormatException(
                $"The first keyframe must have at == 0, but was {keyframes[0].At}.");
        }

        for (var i = 0; i < keyframes.Count; i++)
        {
            var kf = keyframes[i];

            // Constraint 3: strictly ascending by at (no duplicates).
            if (i > 0 && kf.At <= keyframes[i - 1].At)
            {
                throw new InvalidFileFormatException(
                    "keyframes must be strictly ascending by 'at' (no duplicates): " +
                    $"index {i - 1} has at={keyframes[i - 1].At}, index {i} has at={kf.At}.");
            }

            // Constraint 4: every at in [0, frameCount).
            if (kf.At < 0 || kf.At >= manifest.FrameCount)
            {
                throw new InvalidFileFormatException(
                    $"keyframe 'at' ({kf.At}) is outside [0, frameCount={manifest.FrameCount}).");
            }

            // Constraint 5: every id in [0, poolSize).
            if (kf.Id < 0 || kf.Id >= poolSize)
            {
                throw new InvalidFileFormatException(
                    $"keyframe 'id' ({kf.Id}) is outside the frame pool [0, {poolSize}).");
            }
        }
    }
}
