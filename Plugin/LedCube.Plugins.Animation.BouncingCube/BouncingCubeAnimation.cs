using System;
using System.Numerics;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using SdfOps = LedCube.Sdf.Core.Sdf;

namespace LedCube.Plugins.Animation.BouncingCube;

/// <summary>
/// A wireframe cube that breathes (grows and shrinks) while tumbling, rendered as a signed distance
/// field. A modern reinterpretation of the cube16x BouncingCubeAnimation using the SDF engine.
/// </summary>
public class BouncingCubeAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Bouncing Cube",
        "A wireframe cube that breathes and tumbles, rendered via SDF.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("RotationSpeed", "Rotation speed", AnimationConfigType.Float,
                DefaultValue: 1.0f, MinValue: 0.0f, MaxValue: 5.0f),
            new AnimationConfigDescriptor("BreatheSpeed", "Breathe speed", AnimationConfigType.Float,
                DefaultValue: 1.0f, MinValue: 0.0f, MaxValue: 5.0f),
            DurationConfig.Descriptor(16.0f),
        ]);

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(20);

    private float _rotationSpeed = 1.0f;
    private float _breatheSpeed = 1.0f;
    private float _durationSeconds = 16.0f;
    private Sdf3D _sdf = SdfOps.Void();

    public void Configure(AnimationConfig config)
    {
        if (config.Get<float>("RotationSpeed") is { } rotation)
            _rotationSpeed = rotation;
        if (config.Get<float>("BreatheSpeed") is { } breathe)
            _breatheSpeed = breathe;
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
        _sdf = BuildSdf(animationContext.CubeData.Size);
    }

    private Sdf3D BuildSdf(Point3D size)
    {
        var maxHalf = (Math.Min(size.X, Math.Min(size.Y, size.Z)) - 1) / 2.0f;
        var breatheSpeed = _breatheSpeed;

        // A unit wireframe cube, scaled over time so it breathes between ~25% and ~95% of the cube.
        var unitFrame = SdfOps.BoxFrame(Vector3.One, 0.08f);
        Sdf3D breathing = (position, time) =>
        {
            var k = maxHalf * (0.6f + 0.35f * MathF.Sin(time * breatheSpeed * 1.2f));
            k = MathF.Max(k, 0.5f);
            return unitFrame(position / k, time) * k;
        };

        return Driver.ConstantAngularVelotcity(breathing, new Vector3(1f, 1f, 0.5f), MathF.Tau * 0.1f * _rotationSpeed);
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var timeSeconds = (float)frameContext.ElapsedTimeUs / 1_000_000f;
        frameContext.Buffer.Render(_sdf, timeSeconds, new SdfRenderOptions { Centered = true, Margin = 0.5f });
        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }
}
