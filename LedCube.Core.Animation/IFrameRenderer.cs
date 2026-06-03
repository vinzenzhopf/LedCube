using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Animation;

/// <summary>
/// Decodes a baked <see cref="Frame"/>'s payload for one <see cref="LedFormat"/> and writes it to a cube.
/// One implementation per format; this is the seam where Grayscale/RGB support plugs in later.
/// </summary>
public interface IFrameRenderer
{
    LedFormat Format { get; }

    /// <summary>
    /// Writes the decoded frame into <paramref name="target"/>. <paramref name="sourceSize"/> is the
    /// animation's authored size; the LED layout follows the format's index ordering (X fastest).
    /// </summary>
    void Render(Frame frame, Point3D sourceSize, ICubeData target);
}
