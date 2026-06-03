namespace LedCube.Core.Animation;

/// <summary>
/// Result of advancing a player one step.
/// </summary>
/// <param name="Rendered">True if the target buffer was rewritten (false when the active frame was unchanged).</param>
/// <param name="Finished">True once a non-looping timeline has run past its end.</param>
public readonly record struct PlaybackStep(bool Rendered, bool Finished);
