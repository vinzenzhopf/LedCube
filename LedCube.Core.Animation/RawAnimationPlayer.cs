using System;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using RawAnimation = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;

namespace LedCube.Core.Animation;

/// <summary>
/// Drives playback of a baked <see cref="RawAnimation"/> onto an <see cref="ICubeData"/>: turns an
/// elapsed time into a timeline position, resolves the active pool frame, and renders it. Pure
/// player logic with no plugin/host dependencies, so it is fully unit-testable.
/// </summary>
public sealed class RawAnimationPlayer
{
    private readonly RawAnimation _animation;
    private readonly IFrameRenderer _renderer;
    private int _lastRenderedPoolId = -1;

    /// <summary>Authored frame period.</summary>
    public TimeSpan FrameTime { get; }

    public int FrameCount => _animation.Manifest.FrameCount;
    public bool Loop => _animation.Manifest.Loop;
    public Point3D Size => _animation.Manifest.Size;
    public LedFormat Format => _animation.Manifest.LedFormat;

    /// <exception cref="NotSupportedException">
    /// The animation's LED format is unsupported (v1: anything but Binary), or its size does not
    /// satisfy <see cref="CubeRenderOptions.SizeMismatch"/>.
    /// </exception>
    public RawAnimationPlayer(RawAnimation animation, CubeRenderOptions options)
    {
        _animation = animation ?? throw new ArgumentNullException(nameof(animation));
        ArgumentNullException.ThrowIfNull(options);

        _renderer = SelectRenderer(animation.Manifest.LedFormat);
        ValidateSize(animation.Manifest.Size, options);

        FrameTime = TimeSpan.FromMicroseconds(animation.Manifest.FrameTimeUs);
    }

    private static IFrameRenderer SelectRenderer(LedFormat format) => format switch
    {
        LedFormat.Binary => new BinaryFrameRenderer(),
        _ => throw new NotSupportedException(
            $"LED format '{format}' is not supported yet; v1 plays Binary animations only."),
    };

    private static void ValidateSize(Point3D sourceSize, CubeRenderOptions options)
    {
        if (sourceSize == options.TargetSize)
        {
            return;
        }

        switch (options.SizeMismatch)
        {
            case SizeMismatchBehavior.Reject:
                throw new NotSupportedException(
                    $"Animation size {sourceSize} does not match the cube size {options.TargetSize}.");
            default:
                throw new NotSupportedException(
                    $"Size-mismatch behaviour '{options.SizeMismatch}' is not implemented.");
        }
    }

    /// <summary>Invalidates the render cache; call when (re)starting playback or swapping the target buffer.</summary>
    public void Reset() => _lastRenderedPoolId = -1;

    /// <summary>
    /// Timeline position (0-based frame index) for the given elapsed animation time. Looping wraps;
    /// a non-looping timeline past its end holds the last frame (<c>FrameCount - 1</c>).
    /// </summary>
    public int TimelinePositionAt(double elapsedUs)
    {
        var frameTimeUs = _animation.Manifest.FrameTimeUs;
        if (frameTimeUs == 0 || elapsedUs <= 0)
        {
            return 0;
        }

        var pos = (long)(elapsedUs / frameTimeUs);
        var count = FrameCount;
        if (pos < count)
        {
            return (int)pos;
        }

        return Loop ? (int)(pos % count) : count - 1;
    }

    /// <summary>True once a non-looping timeline has run past its end.</summary>
    public bool IsFinishedAt(double elapsedUs)
    {
        if (Loop)
        {
            return false;
        }

        var frameTimeUs = _animation.Manifest.FrameTimeUs;
        return frameTimeUs != 0 && elapsedUs / frameTimeUs >= FrameCount;
    }

    /// <summary>
    /// Renders the frame active at timeline position <paramref name="t"/>. Returns false (and does
    /// nothing) when the active pool frame is unchanged since the last render — so holding a frame
    /// across many ticks is free.
    /// </summary>
    public bool RenderAt(int t, ICubeData target)
    {
        ArgumentNullException.ThrowIfNull(target);

        var poolId = _animation.Keyframes[_animation.KeyframeIndexAt(t)].Id;
        if (poolId == _lastRenderedPoolId)
        {
            return false;
        }

        _renderer.Render(_animation.Frames[poolId], _animation.Manifest.Size, target);
        _lastRenderedPoolId = poolId;
        return true;
    }

    /// <summary>Selects and renders the frame for the given elapsed time.</summary>
    public PlaybackStep Advance(double elapsedUs, ICubeData target)
    {
        var rendered = RenderAt(TimelinePositionAt(elapsedUs), target);
        return new PlaybackStep(rendered, IsFinishedAt(elapsedUs));
    }
}
