using System;
using System.Numerics;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Logging;
using SdfOps = LedCube.Sdf.Core.Sdf;

namespace LedCube.Plugins.Animation.NightCityDrive;

/// <summary>
/// A drive through a night city. The city is a fixed, infinite procedural grid of block clusters
/// (each block is a cluster of hash-generated houses, separated from its neighbours by empty roads).
/// A view transform glued to the car — a position and heading in city space — maps the city into the
/// cube; the car itself stays anchored facing cube +Y. Driving straight advances the position; at an
/// intersection the car may turn a corner, which sweeps the heading 90 degrees while orbiting the
/// position around the turn center, so the whole cityscape rotates about a point beside the car and
/// settles onto the next street. Rendered with voxel-center sampling (see BuildScene).
/// </summary>
public sealed class NightCityDriveAnimation(ILogger<NightCityDriveAnimation> logger)
    : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Night City Drive",
        "A drive through a night city of block clusters, turning corners.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("VehicleSpeed", "Vehicle Speed (px/s)", AnimationConfigType.Float,
                DefaultValue: 4.0f, MinValue: 0.0f, MaxValue: 12.0f,
                Description: "How fast the city streams past, in cube cells per second."),
            new AnimationConfigDescriptor("TurnChance", "Turn chance", AnimationConfigType.Float,
                DefaultValue: 0.4f, MinValue: 0.0f, MaxValue: 1.0f,
                Description: "Probability of turning at each intersection."),
            DurationConfig.Descriptor(0.0f)
        ]);

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(10);

    // City grid, in cells. A block tile is S wide; its outer RoadHalf on every side is empty road, so
    // the buildable core is BW = S - 2*RoadHalf, subdivided into Hs-sized house cells.
    private const float S = 8f;
    private const float RoadHalf = 2f;
    private const float Hs = 2f;
    private const float BW = S - 2f * RoadHalf;
    private const float TurnRadius = 2f;
    private const float Far = 1E6f;
    private const float HalfPi = MathF.PI / 2f;

    private float _vehicleSpeed = 4.0f;
    private float _turnChance = 0.4f;
    private float _durationSeconds;

    private readonly Random _rng = new();
    private Vector3 _size = new(16, 16, 16);
    private float _maxHeight;

    private float _centerX;   // car anchor in cube space (the rotation center of the city)
    private float _carY;

    // View transform: the car's position and heading in city space.
    private Vector2 _pos;
    private float _phi;       // heading angle: 0 = +X, pi/2 = +Y
    private bool _turning;
    private Vector2 _turnCenter;
    private float _turnSign;  // +1 left, -1 right
    private float _turned;    // signed angle turned so far

    private ulong _lastElapsedUs;

    private Sdf3D _carShape = SdfOps.Void();
    private Sdf3D _sdf = SdfOps.Void();

    public void Configure(AnimationConfig config)
    {
        if (config.Get<float>("VehicleSpeed") is { } speed)
            _vehicleSpeed = speed;
        if (config.Get<float>("TurnChance") is { } turn)
            _turnChance = turn;
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
        _size = animationContext.CubeData.Size;
        _maxHeight = _size.Z - 2f;

        _centerX = _size.X / 2f;
        _carY = _size.Y / 2f - 3f; // a bit back, so most of the cube shows the road ahead

        // Start centered on a vertical road (X = multiple of S), heading +Y.
        _pos = new Vector2(0f, 0f);
        _phi = HalfPi;
        _turning = false;
        _turned = 0f;
        _lastElapsedUs = 0;

        _carShape = SdfOps.BoxSize(new Vector3(2f, 3f, 1f)); // 2 (X) x 3 (Y) x 1 (Z), on z-cell 0
        _sdf = BuildScene();
        logger.LogDebug("Night City Drive started on a {X}x{Y}x{Z} cube at {Speed} px/s",
            _size.X, _size.Y, _size.Z, _vehicleSpeed);
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var dt = (frameContext.ElapsedTimeUs - _lastElapsedUs) / 1_000_000f;
        _lastElapsedUs = frameContext.ElapsedTimeUs;
        dt = Math.Clamp(dt, 0f, 0.1f);

        AdvanceCar(_vehicleSpeed * dt);

        var timeSeconds = frameContext.ElapsedTimeUs / 1_000_000f;
        frameContext.Buffer.Render(_sdf, timeSeconds, new SdfRenderOptions { Centered = false, Margin = 0f });
        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }

    // --- Car path -------------------------------------------------------------------------------

    private void AdvanceCar(float move)
    {
        if (move <= 0f)
            return;

        if (_turning)
        {
            StepTurn(move);
            return;
        }

        var heading = new Vector2(MathF.Cos(_phi), MathF.Sin(_phi));
        var alongX = MathF.Abs(heading.X) > 0.5f;
        var dir = alongX ? MathF.Sign(heading.X) : MathF.Sign(heading.Y);

        var before = alongX ? _pos.X : _pos.Y;
        _pos += heading * move;
        var after = alongX ? _pos.X : _pos.Y;

        // The next intersection center is the next multiple of S ahead; the turn starts TurnRadius
        // before it so the arc lands exactly on the perpendicular road centerline.
        var nextCenter = dir > 0f ? (MathF.Floor(before / S) + 1f) * S : (MathF.Ceiling(before / S) - 1f) * S;
        var trigger = nextCenter - dir * TurnRadius;

        var crossed = dir > 0f ? before < trigger && after >= trigger
                               : before > trigger && after <= trigger;
        if (crossed && _rng.NextDouble() < _turnChance)
        {
            // Snap onto the trigger point, then start a 90-degree arc to either side.
            if (alongX) _pos.X = trigger; else _pos.Y = trigger;
            _turnSign = _rng.Next(2) == 0 ? 1f : -1f;
            var normal = _turnSign > 0f ? new Vector2(-heading.Y, heading.X) : new Vector2(heading.Y, -heading.X);
            _turnCenter = _pos + TurnRadius * normal;
            _turned = 0f;
            _turning = true;
        }
    }

    private void StepTurn(float move)
    {
        // Tangential speed equals the driving speed: d(phi) = move / radius.
        var dPhi = _turnSign * (move / TurnRadius);
        var remaining = _turnSign * HalfPi - _turned;
        if (MathF.Abs(dPhi) > MathF.Abs(remaining))
            dPhi = remaining;

        _pos = _turnCenter + Rotate(_pos - _turnCenter, dPhi);
        _phi += dPhi;
        _turned += dPhi;

        if (MathF.Abs(_turned) >= HalfPi - 1e-4f)
        {
            // Lock back onto the grid: square the heading and snap the new cross-axis to a centerline.
            _phi = MathF.Round(_phi / HalfPi) * HalfPi;
            if (MathF.Abs(MathF.Cos(_phi)) > 0.5f) // now heading along X -> snap Y
                _pos.Y = MathF.Round(_pos.Y / S) * S;
            else
                _pos.X = MathF.Round(_pos.X / S) * S;
            _turning = false;
        }
    }

    // --- Scene ----------------------------------------------------------------------------------

    /// <summary>
    /// Unions the car (fixed, facing +Y) with the city sampled through the view transform. The sample
    /// point is shifted by +0.5 so an integer LED index maps to its voxel center; with a 0 render margin
    /// box bounds in plain cell coordinates light exactly the cells they span.
    /// </summary>
    private Sdf3D BuildScene()
    {
        return (p, t) =>
        {
            var q = p + new Vector3(0.5f);
            var car = _carShape(q - new Vector3(_centerX, _carY, 0.5f), t);

            // cube -> city: rotate the cube offset by (phi - 90 deg) and translate by the car position.
            var rel = new Vector2(q.X - _centerX, q.Y - _carY);
            var rot = Rotate(rel, _phi - HalfPi);
            var cityPos = new Vector3(rot.X + _pos.X, rot.Y + _pos.Y, q.Z);

            return MathF.Min(car, CityDist(cityPos));
        };
    }

    private float CityDist(Vector3 c)
    {
        if (c.Z < 0f)
            return Far;

        var bx = MathF.Floor(c.X / S);
        var by = MathF.Floor(c.Y / S);
        var ux = c.X - bx * S - RoadHalf; // position within the buildable core, 0..BW
        var uy = c.Y - by * S - RoadHalf;
        if (ux < 0f || ux > BW || uy < 0f || uy > BW)
            return Far; // road gap between blocks

        var sx = MathF.Floor(ux / Hs);
        var sy = MathF.Floor(uy / Hs);
        var height = HouseHeight((int)bx, (int)by, (int)sx, (int)sy);
        if (height <= 0f)
            return Far; // an empty gap inside the block

        var min = new Vector3(bx * S + RoadHalf + sx * Hs, by * S + RoadHalf + sy * Hs, 0f);
        var max = new Vector3(min.X + Hs, min.Y + Hs, height);
        var center = (min + max) * 0.5f;
        return BoxDist(c - center, (max - min) * 0.5f);
    }

    private float HouseHeight(int bx, int by, int sx, int sy)
    {
        var h = Hash(bx, by, sx, sy);
        if ((h & 0xFFFF) / 65535f < 0.18f)
            return 0f; // ~18% of house cells are empty, giving spaces within the block
        var t = ((h >> 16) & 0xFFFF) / 65535f;
        return 2f + MathF.Floor(t * (_maxHeight - 1f)); // 2 .. _maxHeight cells tall
    }

    // --- Helpers --------------------------------------------------------------------------------

    private static Vector2 Rotate(Vector2 v, float angle)
    {
        var c = MathF.Cos(angle);
        var s = MathF.Sin(angle);
        return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
    }

    private static float BoxDist(Vector3 p, Vector3 half)
    {
        var q = Vector3.Abs(p) - half;
        return Vector3.Max(q, Vector3.Zero).Length() + MathF.Min(MathF.Max(q.X, MathF.Max(q.Y, q.Z)), 0f);
    }

    private static uint Hash(int x, int y, int z, int w)
    {
        unchecked
        {
            var h = 2166136261u;
            h = (h ^ (uint)x) * 16777619u;
            h = (h ^ (uint)y) * 16777619u;
            h = (h ^ (uint)z) * 16777619u;
            h = (h ^ (uint)w) * 16777619u;
            h ^= h >> 13;
            h *= 0x5bd1e995u;
            h ^= h >> 15;
            return h;
        }
    }
}
