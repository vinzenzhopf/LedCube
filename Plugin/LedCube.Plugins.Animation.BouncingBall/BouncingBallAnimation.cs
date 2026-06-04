using System;
using System.Numerics;
using LedCube.PluginBase;
using LedCube.Sdf.Core;

namespace LedCube.Plugins.Animation.BouncingBall;

/// <summary>
/// A solid ball (an SDF sphere) that bounces around inside the cube, reflecting off the walls, with
/// optional gravity. Planned in the cube16x roadmap (BouncingBall).
/// </summary>
public class BouncingBallAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Bouncing Ball",
        "A ball that bounces around inside the cube.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("Radius", "Ball radius", AnimationConfigType.Float,
                DefaultValue: 2.5f, MinValue: 1.0f, MaxValue: 6.0f),
            new AnimationConfigDescriptor("Speed", "Speed", AnimationConfigType.Float,
                DefaultValue: 8.0f, MinValue: 1.0f, MaxValue: 30.0f),
            new AnimationConfigDescriptor("Gravity", "Gravity", AnimationConfigType.Float,
                DefaultValue: 0.0f, MinValue: 0.0f, MaxValue: 30.0f),
            DurationConfig.Descriptor(20.0f),
        ]);

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(20);

    private readonly Random _random = new();
    private readonly Sdf3D _ball;

    private float _radius = 2.5f;
    private float _speed = 8.0f;
    private float _gravity;
    private float _durationSeconds = 20.0f;

    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _min;
    private Vector3 _max;
    private float _dt;

    public BouncingBallAnimation()
    {
        // Reads the live position/radius each evaluation, so no per-frame allocation.
        _ball = (point, _) => (point - _position).Length() - _radius;
    }

    public void Configure(AnimationConfig config)
    {
        if (config.Get<float>("Radius") is { } radius)
            _radius = radius;
        if (config.Get<float>("Speed") is { } speed)
            _speed = speed;
        if (config.Get<float>("Gravity") is { } gravity)
            _gravity = gravity;
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext)
    {
        var cube = animationContext.CubeData;
        cube.Clear();

        var size = cube.Size;
        _dt = (float)(FrameTime?.TotalSeconds ?? 0.02);
        _min = new Vector3(_radius);
        _max = new Vector3(size.X - 1 - _radius, size.Y - 1 - _radius, size.Z - 1 - _radius);
        _position = new Vector3((size.X - 1) / 2f, (size.Y - 1) / 2f, (size.Z - 1) / 2f);
        _velocity = RandomDirection() * _speed;
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        _velocity.Z -= _gravity * _dt;
        _position += _velocity * _dt;

        Bounce(ref _position.X, ref _velocity.X, _min.X, _max.X);
        Bounce(ref _position.Y, ref _velocity.Y, _min.Y, _max.Y);
        Bounce(ref _position.Z, ref _velocity.Z, _min.Z, _max.Z);

        var timeSeconds = (float)frameContext.ElapsedTimeUs / 1_000_000f;
        frameContext.Buffer.Render(_ball, timeSeconds, new SdfRenderOptions { Centered = false, Margin = 0.5f });

        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }

    private static void Bounce(ref float position, ref float velocity, float min, float max)
    {
        if (max < min)
        {
            position = (min + max) / 2f;
            velocity = 0f;
            return;
        }

        if (position < min)
        {
            position = min;
            velocity = MathF.Abs(velocity);
        }
        else if (position > max)
        {
            position = max;
            velocity = -MathF.Abs(velocity);
        }
    }

    private Vector3 RandomDirection()
    {
        var z = (float)_random.NextDouble() * 2f - 1f;
        var angle = (float)_random.NextDouble() * MathF.Tau;
        var r = MathF.Sqrt(MathF.Max(0f, 1f - z * z));
        return new Vector3(r * MathF.Cos(angle), r * MathF.Sin(angle), z);
    }
}
