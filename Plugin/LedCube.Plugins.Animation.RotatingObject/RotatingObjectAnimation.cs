using System;
using System.Numerics;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using SdfOps = LedCube.Sdf.Core.Sdf;

namespace LedCube.Plugins.Animation.RotatingObject;

/// <summary>
/// A solid shape (box frame, octahedron, torus or box) that tumbles inside the cube around a
/// diagonal axis, rendered as an SDF. Planned in the cube16x roadmap (Rotating Objects).
/// </summary>
public class RotatingObjectAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Rotating Object",
        "A solid shape that tumbles inside the cube.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("Shape", "Shape", AnimationConfigType.Enum,
                DefaultValue: "Octahedron",
                EnumValues: ["Octahedron", "BoxFrame", "Torus", "Box"]),
            new AnimationConfigDescriptor("RotationSpeed", "Rotation speed", AnimationConfigType.Float,
                DefaultValue: 1.0f, MinValue: 0.0f, MaxValue: 5.0f),
            DurationConfig.Descriptor(15.0f),
        ]);

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(20);

    private string _shape = "Octahedron";
    private float _rotationSpeed = 1.0f;
    private float _durationSeconds = 15.0f;
    private Sdf3D _sdf = SdfOps.Void();

    public void Configure(AnimationConfig config)
    {
        if (config.GetString("Shape") is { } shape)
            _shape = shape;
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
        var half = (Math.Min(size.X, Math.Min(size.Y, size.Z)) - 1) / 2f;

        var shape = _shape switch
        {
            "BoxFrame" => SdfOps.BoxFrame(new Vector3(half * 0.8f), 0.4f),
            "Torus" => SdfOps.Torus(half * 0.7f, half * 0.28f),
            "Box" => SdfOps.Box(new Vector3(half * 0.7f)),
            _ => SdfOps.Octahedron(half),
        };

        return Driver.ConstantAngularVelotcity(shape, new Vector3(1f, 1f, 1f), MathF.Tau * 0.12f * _rotationSpeed);
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var timeSeconds = (float)frameContext.ElapsedTimeUs / 1_000_000f;
        frameContext.Buffer.Render(_sdf, timeSeconds, new SdfRenderOptions { Centered = true, Margin = 0.5f });
        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }
}
