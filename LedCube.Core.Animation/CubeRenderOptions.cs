using LedCube.Core.Common.Model;

namespace LedCube.Core.Animation;

/// <summary>
/// Options controlling how a <see cref="RawAnimationPlayer"/> renders frames onto a cube.
/// </summary>
/// <param name="TargetSize">Size of the cube the animation is being played on.</param>
/// <param name="SizeMismatch">Behaviour when the animation's authored size differs from <paramref name="TargetSize"/>.</param>
public sealed record CubeRenderOptions(
    Point3D TargetSize,
    SizeMismatchBehavior SizeMismatch = SizeMismatchBehavior.Reject);
