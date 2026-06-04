namespace LedCube.PluginBase;

/// <summary>
/// Helper for the common "play for a fixed time, then finish" config knob. An animation exposes
/// <see cref="Descriptor"/> in its <see cref="FrameGeneratorInfo"/>, reads the value in
/// <see cref="IFrameGenerator.Configure"/> via <see cref="Read"/>, and ends a frame with
/// <see cref="IsFinished"/> so a playlist can advance. A duration of 0 means "run until stopped".
/// </summary>
public static class DurationConfig
{
    public const string Key = "DurationSeconds";

    public static AnimationConfigDescriptor Descriptor(float defaultSeconds) => new(
        Key, "Duration (s)", AnimationConfigType.Float,
        DefaultValue: defaultSeconds, MinValue: 0.0f, MaxValue: 600.0f,
        Description: "How long to play before finishing. 0 = run until stopped.");

    public static float Read(AnimationConfig config, float current)
        => config.Get<float>(Key) is { } value ? value : current;

    public static bool IsFinished(FrameContext frameContext, float durationSeconds)
        => durationSeconds > 0f && frameContext.ElapsedTimeUs >= (ulong)(durationSeconds * 1_000_000f);
}
