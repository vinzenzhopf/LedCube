namespace LedCube.Core.Animation;

/// <summary>
/// What a player does when an animation's authored size differs from the target cube size.
/// </summary>
public enum SizeMismatchBehavior
{
    /// <summary>Refuse to play a differently-sized animation. The only mode supported in v1.</summary>
    Reject,

    // Reserved for later: Center (place the authored frame centered, clip overflow),
    // Scale (nearest-neighbour resample). Not implemented yet.
}
