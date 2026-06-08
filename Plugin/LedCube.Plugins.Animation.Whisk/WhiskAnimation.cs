using System;
using System.Numerics;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using SdfOps = LedCube.Sdf.Core.Sdf;

namespace LedCube.Plugins.Animation.Whisk;

/// <summary>
/// Two thin planes intersecting along the vertical axis, forming a cross that spins around it.
/// Rendered as an SDF. Planned in the cube16x roadmap (WhiskAnimation).
/// </summary>
public class WhiskAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Whisk",
        "Two crossed planes spinning around the vertical axis.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("RotationSpeed", "Rotation speed", AnimationConfigType.Float,
                DefaultValue: 1.0f, MinValue: 0.0f, MaxValue: 5.0f),
            DurationConfig.Descriptor(15.0f),
        ],
        FrameTime: TimeSpan.FromMilliseconds(20));

    public TimeSpan? FrameTime => Info.FrameTime;

    private float _rotationSpeed = 1.0f;
    private float _durationSeconds = 15.0f;
    private Sdf3D _sdf = SdfOps.Void();

    public void Configure(AnimationConfig config)
    {
        if (config.Get<float>("RotationSpeed") is { } rotation)
            _rotationSpeed = rotation;
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
        _sdf = BuildSdf(animationContext.CubeData.Size);
    }

    private Sdf3D BuildSdf(Point3D size)
    {
        var reach = Math.Max(size.X, Math.Max(size.Y, size.Z)) / 2f + 1f;

        // Two full-height slabs, each one LED thick on one axis, crossing at the centre.
        var slabAlongY = SdfOps.Box(new Vector3(0.5f, reach, reach));
        var slabAlongX = SdfOps.Box(new Vector3(reach, 0.5f, reach));
        var cross = SdfOps.Union(slabAlongY, slabAlongX);

        return Driver.ConstantAngularVelotcity(cross, Vector3.UnitZ, MathF.Tau * 0.15f * _rotationSpeed);
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var timeSeconds = (float)frameContext.ElapsedTimeUs / 1_000_000f;
        frameContext.Buffer.Render(_sdf, timeSeconds, new SdfRenderOptions { Centered = true, Margin = 0.5f });
        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }
}
