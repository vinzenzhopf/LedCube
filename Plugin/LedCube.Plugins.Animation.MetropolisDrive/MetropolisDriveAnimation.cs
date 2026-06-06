using System;
using System.Numerics;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Logging;
using SdfOps = LedCube.Sdf.Core.Sdf;

namespace LedCube.Plugins.Animation.MetropolisDrive;

/// <summary>
/// A drive down a metropolis speedway. The cube is a fixed world frame: X = road width
/// (left/right), Y = travel direction (depth), Z = up. The car stays anchored in the middle of the
/// road and only bounces / sways / rotates a little; the scenery is what moves. The road is a flat
/// surface flanked by raised curbs; buildings stream past on both sides as a recycling pool conveyor
/// along +Y. Everything is composed as one signed-distance field and rasterized once per frame.
/// </summary>
public sealed class MetropolisDriveAnimation(ILogger<MetropolisDriveAnimation> logger)
    : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Metropolis Drive",
        "A drive down a busy metropolis speedway.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("VehicleSpeed", "Vehicle Speed (px/s)", AnimationConfigType.Float,
                DefaultValue: 12.0f, MinValue: 1.0f, MaxValue: 32.0f,
                Description: "How fast the scenery streams past, in cube cells per second."),
            DurationConfig.Descriptor(0.0f)
        ]);

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(10);

    private const int BuildingsPerSide = 8;
    private const float Far = 1E6f;

    private float _vehicleSpeed = 6.0f;
    private float _durationSeconds;

    private readonly Random _rng = new();
    private Vector3 _size = new(16, 16, 16);

    // Road geometry in cell coordinates, derived from the cube size in Start().
    private float _centerX;     // middle of the road, where the car sits
    private float _edgeLeftX;   // left curb column
    private float _edgeRightX;  // right curb column
    private float _midY;        // Y center of the cube
    private float _carCenterY;  // car sits back from the middle, toward the near face

    // Building pool: indices [0, BuildingsPerSide) are the left lane, the rest the right lane.
    private Building[] _buildings = [];
    private float _frontierLeft;   // far edge (max Y) currently occupied on each lane; new buildings spawn beyond it
    private float _frontierRight;

    // Elapsed-time bookkeeping for the per-frame delta that drives the building conveyor.
    private ulong _lastElapsedUs;

    // Static geometry, composed once with the SDF library; the car's per-frame transform is applied
    // at sample time in CarDist.
    private Sdf3D _carShape = SdfOps.Void();
    private Sdf3D _road = SdfOps.Void();

    private Sdf3D _sdf = SdfOps.Void();

    public void Configure(AnimationConfig config)
    {
        if (config.Get<float>("VehicleSpeed") is { } speed)
            _vehicleSpeed = speed;
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
        _size = animationContext.CubeData.Size;

        // In voxel-center coordinates the cube's middle is at face size/2, and box bounds are plain
        // cell indices (see BuildScene for the +0.5 sampling shift).
        _centerX = _size.X / 2f;
        _edgeLeftX = 4f;            // road starts here; the 4-wide left curb fills cells 0..3
        _edgeRightX = _size.X - 4f; // road ends here; the 4-wide right curb fills the last 4 cells
        _midY = _size.Y / 2f;
        _carCenterY = _midY + 3f;   // sit a bit back from centre, toward the far face

        _lastElapsedUs = 0;

        // Seed both lanes by laying buildings end to end from just past the far face inward, so the
        // visible depth is full and a queue is already waiting below the near face.
        _buildings = new Building[BuildingsPerSide * 2];
        _frontierLeft = _size.Y + 2f;
        _frontierRight = _size.Y + 2f;
        for (var i = 0; i < BuildingsPerSide; i++)
            SpawnBuilding(ref _buildings[i], side: 0, ref _frontierLeft);
        for (var i = 0; i < BuildingsPerSide; i++)
            SpawnBuilding(ref _buildings[BuildingsPerSide + i], side: 1, ref _frontierRight);

        _carShape = BuildCarShape();
        _road = BuildRoad();
        _sdf = BuildScene();
        logger.LogDebug("Metropolis Drive started on a {X}x{Y}x{Z} cube at {Speed} px/s",
            _size.X, _size.Y, _size.Z, _vehicleSpeed);
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var dt = (frameContext.ElapsedTimeUs - _lastElapsedUs) / 1_000_000f;
        _lastElapsedUs = frameContext.ElapsedTimeUs;
        dt = Math.Clamp(dt, 0f, 0.1f); // ignore huge gaps after a pause/scrub

        var move = _vehicleSpeed * dt;

        // Advance the conveyor: every building (and each lane's spawn frontier) slides toward the far
        // face. A building that has fully passed the far face is re-emitted at the near frontier.
        _frontierLeft += move;
        _frontierRight += move;
        for (var i = 0; i < _buildings.Length; i++)
        {
            ref var b = ref _buildings[i];
            b.Y += move;
            if (b.Y - b.HalfY > _size.Y + 1f)
            {
                if (b.Side == 0) SpawnBuilding(ref b, 0, ref _frontierLeft);
                else SpawnBuilding(ref b, 1, ref _frontierRight);
            }
        }

        var timeSeconds = frameContext.ElapsedTimeUs / 1_000_000f;
        frameContext.Buffer.Render(_sdf, timeSeconds, new SdfRenderOptions { Centered = false, Margin = 0f });
        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }

    /// <summary>
    /// Unions the three layers each frame. The sample point is shifted by +0.5 so an integer LED index
    /// maps to the center of its unit voxel; together with a 0 render margin, box bounds expressed in
    /// plain cell coordinates light exactly the cells they span.
    /// </summary>
    private Sdf3D BuildScene()
    {
        return (p, t) =>
        {
            var q = p + new Vector3(0.5f); // LED index -> voxel-center world position
            return MathF.Min(BuildingsDist(q), MathF.Min(CarDist(q, t), _road(q, t)));
        };
    }

    // --- Buildings -----------------------------------------------------------------------------

    private void SpawnBuilding(ref Building b, int side, ref float frontier)
    {
        var width = _rng.Next(1, 5);           // 1..4 cells against the wall
        var depth = _rng.Next(1, 5);           // 1..4 cells along travel
        var height = _rng.Next(3, (int)_size.Z); // 3..size-1 cells tall (skyline)
        var gap = _rng.Next(0, 4);             // 0..3 cell gap before this building

        var halfX = width / 2f;
        var centerX = side == 0
            ? width / 2f                  // left lane occupies cells 0..width-1
            : _size.X - width / 2f;       // right lane occupies cells size-width..size-1

        var halfY = depth / 2f;
        var centerY = frontier - gap - halfY;
        frontier = centerY - halfY;

        b = new Building
        {
            Side = side,
            X = centerX,
            HalfX = halfX,
            Y = centerY,
            HalfY = halfY,
            Height = height
        };
    }

    private float BuildingsDist(Vector3 p)
    {
        var d = Far;
        var buildings = _buildings;
        for (var i = 0; i < buildings.Length; i++)
        {
            var b = buildings[i];
            var halfZ = b.Height / 2f; // base on the floor: cells 0..Height-1
            var local = p - new Vector3(b.X, b.Y, halfZ);
            d = MathF.Min(d, BoxDist(local, new Vector3(b.HalfX, b.HalfY, halfZ)));
        }
        return d;
    }

    // --- Car -----------------------------------------------------------------------------------

    private float CarDist(Vector3 p, float t)
    {
        // Small idle motion layered on top of the fixed anchor: a quick bounce plus a slow lateral sway.
        var bounceZ = 0; // 0.30f * MathF.Sin(t * 7.5f) + 0.15f * MathF.Sin(t * 3.1f);
        var swayX = 0;//0.40f * MathF.Sin(t * 1.7f);

        // A few degrees of body movement: roll about the travel axis (Y), pitch about X, slight yaw about Z.
        var roll = 0;//0.07f * MathF.Sin(t * 2.1f);
        var pitch = 0;//0.05f * MathF.Sin(t * 6.3f);
        var yaw = 0;//0.04f * MathF.Sin(t * 1.3f);

        var center = new Vector3(_centerX + swayX, _carCenterY, 3f + bounceZ);
        var rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        var local = Vector3.Transform(p - center, Quaternion.Inverse(rotation));
        return _carShape(local, t);
    }

    // Car authored about its geometric center (z-face 3.0), wheels resting on the curb level (z-cell 1).
    // 4 (X) wide, 6 (Y) long, 4 (Z) tall: wheels on z-cell 1, body on z-cells 2-3, cabin on z-cell 4.
    // Sizes are the actual cell counts; odd-sized parts are offset to a half-cell so they land on whole cells.
    private static Sdf3D BuildCarShape()
    {
        // Two full-width wheel strips, 1 tall, at the front and back ends (z-cell 1).
        var wheelFL = SdfOps.Translate(SdfOps.BoxSize(new Vector3(1f, 2f, 1f)), new Vector3(-1.5f, 2f, -1.5f));
        var wheelFR = SdfOps.Translate(SdfOps.BoxSize(new Vector3(1f, 2f, 1f)), new Vector3(1.5f, 2f, -1.5f));
        var wheelBL = SdfOps.Translate(SdfOps.BoxSize(new Vector3(1f, 2f, 1f)), new Vector3(-1.5f, -2f, -1.5f));
        var wheelBR = SdfOps.Translate(SdfOps.BoxSize(new Vector3(1f, 2f, 1f)), new Vector3(1.5f, -2f, -1.5f));
        // Body: full 4x6 footprint, 2 tall (z-cells 2-3).
        var body = SdfOps.BoxSize(new Vector3(4f, 6f, 2f));
        // Cabin: 4 wide, 3 long, 1 tall on top (z-cell 4), nudged back half a cell.
        var cabin = SdfOps.Translate(SdfOps.BoxSize(new Vector3(4f, 3f, 1f)), new Vector3(0f, -0.5f, 1.5f));
        return SdfOps.UnionAll(wheelFL, wheelFR, wheelBL, wheelBR, body, cabin);
    }

    // --- Road ----------------------------------------------------------------------------------

    // Road surface is 1 layer at z-cell 0 in the middle; the curbs are 4-wide, 1-tall raised sidewalks
    // at z-cell 1 along each edge. All span the full depth.
    private Sdf3D BuildRoad()
    {
        var surface = SdfOps.BoxBounds(new Vector3(0f, 0f, 0f), _size with {Z = 1f});
        var leftCurb = SdfOps.BoxBounds(new Vector3(0f, 0f, 1f), new Vector3(_edgeLeftX, _size.Y, 2f));
        var rightCurb = SdfOps.BoxBounds(new Vector3(_edgeRightX, 0f, 1f), new Vector3(_size.X, _size.Y, 2f));
        return SdfOps.UnionAll(surface, leftCurb, rightCurb);
    }

    // --- Helpers -------------------------------------------------------------------------------

    private static float BoxDist(Vector3 p, Vector3 half)
    {
        var q = Vector3.Abs(p) - half;
        return Vector3.Max(q, Vector3.Zero).Length() + MathF.Min(MathF.Max(q.X, MathF.Max(q.Y, q.Z)), 0f);
    }

    private struct Building
    {
        public int Side;     // 0 = left lane, 1 = right lane
        public float X;      // center X (fixed to its wall)
        public float HalfX;  // half width
        public float Y;      // center Y (scrolls toward the viewer)
        public float HalfY;  // half depth
        public int Height;   // cells tall, base on the floor
    }
}
