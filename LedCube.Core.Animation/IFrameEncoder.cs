using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Animation;

/// <summary>
/// Encodes a cube's current LED state into a baked <see cref="Frame"/> payload for one
/// <see cref="LedFormat"/>. The exact inverse of <see cref="IFrameRenderer"/>; used by authoring
/// tools to bake an <see cref="ICubeData"/> into a <c>.lcanimraw</c> frame.
/// </summary>
public interface IFrameEncoder
{
    LedFormat Format { get; }

    /// <summary>
    /// Reads <paramref name="source"/> and produces the packed frame payload. The LED layout
    /// follows the format's index ordering (X fastest), matching the paired <see cref="IFrameRenderer"/>.
    /// </summary>
    Frame Encode(ICubeData source);
}
