using System;
using System.Collections.Generic;
using System.Numerics;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.Fireworks;

/// <summary>
/// A particle-based fireworks display: rockets launch from the floor, decelerate under gravity and
/// burst at their apex into a shower of sparks that arc back down. The cube16x FireworksAnimation
/// was only a stub, so this is a fresh implementation.
/// </summary>
public class FireworksAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Fireworks",
        "Rockets launch from the floor and burst into falling sparks.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("SparksPerBurst", "Sparks per burst", AnimationConfigType.Int,
                DefaultValue: 24, MinValue: 4, MaxValue: 80),
            DurationConfig.Descriptor(20.0f),
        ]);

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(30);

    private const float Gravity = 11f; // cube units / s^2, pulling toward -Z

    private readonly Random _random = new();
    private readonly List<Rocket> _rockets = [];
    private readonly List<Spark> _sparks = [];

    private Point3D _size;
    private float _dt;
    private float _nextLaunchTime;
    private int _sparksPerBurst = 24;
    private float _durationSeconds = 20.0f;

    public void Configure(AnimationConfig config)
    {
        if (config.Get<int>("SparksPerBurst") is { } sparks)
            _sparksPerBurst = Math.Max(4, sparks);
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
        _size = animationContext.CubeData.Size;
        _dt = (float)(FrameTime?.TotalSeconds ?? 0.03);
        _rockets.Clear();
        _sparks.Clear();
        _nextLaunchTime = 0f;
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var time = (float)frameContext.ElapsedTimeUs / 1_000_000f;

        if (time >= _nextLaunchTime)
        {
            LaunchRocket();
            _nextLaunchTime = time + 0.6f + (float)_random.NextDouble() * 1.2f;
        }

        UpdateRockets();
        UpdateSparks();
        Render(frameContext.Buffer);
        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }

    private void LaunchRocket()
    {
        var pos = new Vector3(
            _size.X / 2f + Jitter(_size.X * 0.2f),
            _size.Y / 2f + Jitter(_size.Y * 0.2f),
            0f);

        // Aim the apex at roughly 70-95% of the cube height.
        var targetHeight = _size.Z * (0.7f + (float)_random.NextDouble() * 0.25f);
        var vz = MathF.Sqrt(2f * Gravity * targetHeight);
        _rockets.Add(new Rocket { Pos = pos, Vel = new Vector3(Jitter(1.5f), Jitter(1.5f), vz) });
    }

    private void UpdateRockets()
    {
        for (var i = _rockets.Count - 1; i >= 0; i--)
        {
            var r = _rockets[i];
            r.Vel.Z -= Gravity * _dt;
            r.Pos += r.Vel * _dt;

            if (r.Vel.Z <= 0f || r.Pos.Z >= _size.Z - 1)
            {
                Burst(r.Pos);
                _rockets.RemoveAt(i);
            }
            else
            {
                _rockets[i] = r;
            }
        }
    }

    private void Burst(Vector3 center)
    {
        for (var i = 0; i < _sparksPerBurst; i++)
        {
            var speed = 3f + (float)_random.NextDouble() * 4f;
            _sparks.Add(new Spark
            {
                Pos = center,
                Vel = RandomDirection() * speed + new Vector3(0f, 0f, 1.5f),
                Life = 0.8f + (float)_random.NextDouble() * 0.7f,
            });
        }
    }

    private void UpdateSparks()
    {
        for (var i = _sparks.Count - 1; i >= 0; i--)
        {
            var s = _sparks[i];
            s.Vel.Z -= Gravity * _dt;
            s.Pos += s.Vel * _dt;
            s.Life -= _dt;

            if (s.Life <= 0f || s.Pos.Z < 0f)
            {
                _sparks.RemoveAt(i);
            }
            else
            {
                _sparks[i] = s;
            }
        }
    }

    private void Render(ICubeData cube)
    {
        cube.Clear();
        foreach (var rocket in _rockets)
            Plot(cube, rocket.Pos);
        foreach (var spark in _sparks)
            Plot(cube, spark.Pos);
    }

    private void Plot(ICubeData cube, Vector3 pos)
    {
        var x = (int)MathF.Round(pos.X);
        var y = (int)MathF.Round(pos.Y);
        var z = (int)MathF.Round(pos.Z);
        if (x >= 0 && x < _size.X && y >= 0 && y < _size.Y && z >= 0 && z < _size.Z)
        {
            cube.SetLed(new Point3D(x, y, z), true);
        }
    }

    private float Jitter(float amount) => ((float)_random.NextDouble() * 2f - 1f) * amount;

    private Vector3 RandomDirection()
    {
        var z = (float)_random.NextDouble() * 2f - 1f;
        var angle = (float)_random.NextDouble() * MathF.Tau;
        var r = MathF.Sqrt(MathF.Max(0f, 1f - z * z));
        return new Vector3(r * MathF.Cos(angle), r * MathF.Sin(angle), z);
    }

    private struct Rocket
    {
        public Vector3 Pos;
        public Vector3 Vel;
    }

    private struct Spark
    {
        public Vector3 Pos;
        public Vector3 Vel;
        public float Life;
    }
}
