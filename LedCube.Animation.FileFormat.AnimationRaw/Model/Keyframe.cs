namespace LedCube.Animation.FileFormat.AnimationRaw.Model;

/// <summary>
/// A single scheduling entry: the pool frame <paramref name="Id"/> becomes active at
/// timeline position <paramref name="At"/> and holds until the next keyframe.
/// </summary>
/// <param name="At">Timeline position (0-based frame index) at which this frame becomes active.</param>
/// <param name="Id">Index into the frame pool (<see cref="Animation.Frames"/>).</param>
public readonly record struct Keyframe(int At, int Id);
