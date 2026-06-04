using System;
using System.Numerics;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using SdfOps = LedCube.Sdf.Core.Sdf;

namespace LedCube.Plugins.Animation.BouncingCubes;

/// <summary>
/// Several wireframe cubes that fly around inside the cube, reflecting off the walls and bouncing
/// off each other with elastic (equal-mass) collisions. Rendered as an SDF. Planned in the cube16x
/// roadmap (Bouncing Cubes).
/// </summary>
public class BouncingCubesAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Bouncing Cubes",
        "Several wireframe cubes that bounce off the walls and off each other.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("Count", "Cube count", AnimationConfigType.Int,
                DefaultValue: 3, MinValue: 1, MaxValue: 6),
            new AnimationConfigDescriptor("Size", "Cube half-size", AnimationConfigType.Float,
                DefaultValue: 2.0f, MinValue: 1.0f, MaxValue: 4.0f),
            new AnimationConfigDescriptor("Speed", "Speed", AnimationConfigType.Float,
                DefaultValue: 6.0f, MinValue: 1.0f, MaxValue: 24.0f),
            new AnimationConfigDescriptor("Gravity", "Gravity", AnimationConfigType.Float,
                DefaultValue: 0.0f, MinValue: 0.0f, MaxValue: 30.0f),
            DurationConfig.Descriptor(20.0f),
        ]);

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(30);

    private readonly Random _random = new();

    private int _count = 3;
    private float _halfSize = 2.0f;
    private float _speed = 6.0f;
    private float _gravity;
    private float _durationSeconds = 20.0f;

    private Body[] _cubes = [];
    private Sdf3D _box = SdfOps.Void();
    private Sdf3D _scene = SdfOps.Void();
    private Vector3 _min;
    private Vector3 _max;
    private float _dt;

    public void Configure(AnimationConfig config)
    {
        if (config.Get<int>("Count") is { } count)
            _count = Math.Max(1, count);
        if (config.Get<float>("Size") is { } size)
            _halfSize = size;
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
        _dt = (float)(FrameTime?.TotalSeconds ?? 0.03);
        _min = new Vector3(_halfSize);
        _max = new Vector3(size.X - 1 - _halfSize, size.Y - 1 - _halfSize, size.Z - 1 - _halfSize);

        _box = SdfOps.BoxFrame(new Vector3(_halfSize), 0.3f);
        _cubes = new Body[_count];
        for (var i = 0; i < _count; i++)
        {
            _cubes[i] = new Body { Pos = RandomPosition(), Vel = RandomDirection() * _speed };
        }

        // Reads the live cube positions each evaluation, so no per-frame allocation.
        _scene = (point, time) =>
        {
            var d = float.MaxValue;
            for (var i = 0; i < _cubes.Length; i++)
            {
                d = MathF.Min(d, _box(point - _cubes[i].Pos, time));
            }

            return d;
        };
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        Integrate();
        ResolveCollisions();

        var timeSeconds = (float)frameContext.ElapsedTimeUs / 1_000_000f;
        frameContext.Buffer.Render(_scene, timeSeconds, new SdfRenderOptions { Centered = false, Margin = 0.5f });

        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }

    private void Integrate()
    {
        for (var i = 0; i < _cubes.Length; i++)
        {
            _cubes[i].Vel.Z -= _gravity * _dt;
            _cubes[i].Pos += _cubes[i].Vel * _dt;

            Bounce(ref _cubes[i].Pos.X, ref _cubes[i].Vel.X, _min.X, _max.X);
            Bounce(ref _cubes[i].Pos.Y, ref _cubes[i].Vel.Y, _min.Y, _max.Y);
            Bounce(ref _cubes[i].Pos.Z, ref _cubes[i].Vel.Z, _min.Z, _max.Z);
        }
    }

    private void ResolveCollisions()
    {
        var minDist = 2f * _halfSize; // centres closer than this = faces overlapping
        for (var i = 0; i < _cubes.Length; i++)
        for (var j = i + 1; j < _cubes.Length; j++)
        {
            var delta = _cubes[j].Pos - _cubes[i].Pos;
            var dist = delta.Length();
            if (dist >= minDist)
            {
                continue;
            }

            if (dist < 1e-4f)
            {
                _cubes[j].Pos += new Vector3(0.1f, 0.05f, 0.0f);
                continue;
            }

            var normal = delta / dist;

            // Elastic, equal-mass response: exchange the velocity components along the normal.
            var approach = Vector3.Dot(_cubes[i].Vel - _cubes[j].Vel, normal);
            if (approach > 0f)
            {
                var impulse = approach * normal;
                _cubes[i].Vel -= impulse;
                _cubes[j].Vel += impulse;
            }

            // Push the pair apart so they stop overlapping.
            var separation = normal * ((minDist - dist) / 2f);
            _cubes[i].Pos -= separation;
            _cubes[j].Pos += separation;
        }

        for (var i = 0; i < _cubes.Length; i++)
        {
            _cubes[i].Pos = Vector3.Clamp(_cubes[i].Pos, _min, _max);
        }
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

    private Vector3 RandomPosition()
    {
        return new Vector3(
            Lerp(_min.X, _max.X, (float)_random.NextDouble()),
            Lerp(_min.Y, _max.Y, (float)_random.NextDouble()),
            Lerp(_min.Z, _max.Z, (float)_random.NextDouble()));
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    private Vector3 RandomDirection()
    {
        var z = (float)_random.NextDouble() * 2f - 1f;
        var angle = (float)_random.NextDouble() * MathF.Tau;
        var r = MathF.Sqrt(MathF.Max(0f, 1f - z * z));
        return new Vector3(r * MathF.Cos(angle), r * MathF.Sin(angle), z);
    }

    private struct Body
    {
        public Vector3 Pos;
        public Vector3 Vel;
    }
}
