using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using LedCube.Core.Common.CubeData.Repository;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Orientation;
using LedCube.Core.UI.CubeView3D.Camera;
using LedCube.Core.UI.CubeView3D.Mesh;
using LedCube.Core.UI.CubeView3D.Rendering;
using Silk.NET.OpenGL;

namespace LedCube.Core.UI.CubeView3D.Controls;

/// <summary>
/// Avalonia control that renders the live cube state in 3D using OpenGL (via Silk.NET).
/// Bind <see cref="CubeData"/> to the current <see cref="ICubeData"/>; the control snapshots it
/// each frame and instances one transparent LED mesh per LED, on a podest, with FRONT/first-row
/// floor annotations. Drag to orbit, wheel to zoom.
///
/// Axis convention (matches the cube data / SimpleRotationCubeProjection): +Y up, +Z back
/// (front = -Z), +X is the first-row direction (LED index increments X first).
/// </summary>
public class Cube3DView : OpenGlControlBase, ICustomHitTest
{
    // OpenGlControlBase has no hit-testable background, so without ICustomHitTest pointer events
    // (orbit/zoom) never reach the control. The point is in the control's LOCAL space, so we must
    // bounds-check it — returning true unconditionally claims points outside our area (e.g. over a
    // sibling panel above us), which steals their clicks.
    public bool HitTest(Avalonia.Point point) => new Rect(Bounds.Size).Contains(point);

    public static readonly StyledProperty<ICubeData?> CubeDataProperty =
        AvaloniaProperty.Register<Cube3DView, ICubeData?>(nameof(CubeData));

    /// <summary>
    /// Optional live source. When set, the control reads the repository's CURRENT cube each frame —
    /// needed because the playback service swaps in a new <see cref="ICubeData"/> instance on play,
    /// which a one-time <see cref="CubeData"/> binding would miss. Takes precedence over CubeData.
    /// </summary>
    public static readonly StyledProperty<ICubeRepository?> CubeRepositoryProperty =
        AvaloniaProperty.Register<Cube3DView, ICubeRepository?>(nameof(CubeRepository));

    public static readonly StyledProperty<double> LedScaleProperty =
        AvaloniaProperty.Register<Cube3DView, double>(nameof(LedScale), 0.85);

    public static readonly StyledProperty<Color> LedColorProperty =
        AvaloniaProperty.Register<Cube3DView, Color>(nameof(LedColor), Color.FromRgb(40, 100, 235));

    public static readonly StyledProperty<Color> BackgroundColorProperty =
        AvaloniaProperty.Register<Cube3DView, Color>(nameof(BackgroundColor), Color.FromRgb(18, 18, 24));

    public static readonly StyledProperty<bool> ShowPodestProperty =
        AvaloniaProperty.Register<Cube3DView, bool>(nameof(ShowPodest), true);

    public static readonly StyledProperty<bool> ShowAxisGizmoProperty =
        AvaloniaProperty.Register<Cube3DView, bool>(nameof(ShowAxisGizmo), true);

    public static readonly StyledProperty<bool> ShowLed0ArrowProperty =
        AvaloniaProperty.Register<Cube3DView, bool>(nameof(ShowLed0Arrow), true);

    public static readonly StyledProperty<bool> ShowAnnotationsProperty =
        AvaloniaProperty.Register<Cube3DView, bool>(nameof(ShowAnnotations), true);

    public static readonly StyledProperty<double> TransparencyProperty =
        AvaloniaProperty.Register<Cube3DView, double>(nameof(Transparency), 0.75);

    public static readonly StyledProperty<CubeOrientation> OrientationProperty =
        AvaloniaProperty.Register<Cube3DView, CubeOrientation>(nameof(Orientation), CubeOrientation.Default);

    public ICubeData? CubeData
    {
        get => GetValue(CubeDataProperty);
        set => SetValue(CubeDataProperty, value);
    }

    public ICubeRepository? CubeRepository
    {
        get => GetValue(CubeRepositoryProperty);
        set => SetValue(CubeRepositoryProperty, value);
    }

    public double LedScale
    {
        get => GetValue(LedScaleProperty);
        set => SetValue(LedScaleProperty, value);
    }

    /// <summary>Color of the (diffuse) LEDs. The real cube uses diffuse blue.</summary>
    public Color LedColor
    {
        get => GetValue(LedColorProperty);
        set => SetValue(LedColorProperty, value);
    }

    public Color BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public bool ShowPodest
    {
        get => GetValue(ShowPodestProperty);
        set => SetValue(ShowPodestProperty, value);
    }

    public bool ShowAnnotations
    {
        get => GetValue(ShowAnnotationsProperty);
        set => SetValue(ShowAnnotationsProperty, value);
    }

    /// <summary>How the logical cube maps onto the physical/displayed cube (LED 0 corner, axes, front).</summary>
    public CubeOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>0 = LEDs fully opaque, 1 = maximally see-through (to view inner LEDs).</summary>
    public double Transparency
    {
        get => GetValue(TransparencyProperty);
        set => SetValue(TransparencyProperty, value);
    }

    /// <summary>Show the RGB axis gizmo (X=red, Y=green, Z=blue) at the base corner.</summary>
    public bool ShowAxisGizmo
    {
        get => GetValue(ShowAxisGizmoProperty);
        set => SetValue(ShowAxisGizmoProperty, value);
    }

    /// <summary>Show the "LED 0" arrow on the floor decal.</summary>
    public bool ShowLed0Arrow
    {
        get => GetValue(ShowLed0ArrowProperty);
        set => SetValue(ShowLed0ArrowProperty, value);
    }

    // Canonical display basis (right-handed): data X -> right (+X), Y -> depth toward the back
    // (Front = -Z, so data +Y advances away from the camera), Z -> up (+Y). LED 0 sits at the
    // front-left-bottom corner (toward the camera) and Z is vertical, matching the cube convention.
    private static readonly CubeOrientation CanonicalDisplay = new()
    {
        AxisX = CubeDirection.Right,
        AxisY = CubeDirection.Front,
        AxisZ = CubeDirection.Up
    };

    private readonly OrbitCamera _camera = new();
    private readonly LedCubeRenderer _renderer = new();
    private readonly ScenePropsRenderer _scene = new();   // podest + floor decal
    private readonly ScenePropsRenderer _gizmo = new();   // RGB axis gizmo (toggled independently)
    private GL? _gl;
    private ObjMesh? _mesh;
    private Point3D _initializedSize;
    private CubeOrientation _initializedOrientation = CubeOrientation.Default;
    private bool _initializedLed0Arrow;
    private byte[] _packed = Array.Empty<byte>();
    private float[] _brightness = Array.Empty<float>();
    private bool _failed;

    private bool _dragging;
    private Avalonia.Point _lastPointer;
    private readonly DispatcherTimer _renderTimer;

    public Cube3DView()
    {
        // OnOpenGlRender runs on the render thread, where RequestNextFrameRendering() is a no-op
        // (it must talk to the compositor from the UI thread). Drive the frame loop from a
        // UI-thread timer instead so live cube updates and camera changes are reflected.
        _renderTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render,
            (_, _) => { RequestNextFrameRendering(); InvalidateVisual(); });
        AttachedToVisualTree += (_, _) => _renderTimer.Start();
        DetachedFromVisualTree += (_, _) => _renderTimer.Stop();
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        try
        {
            _gl = GL.GetApi(name => gl.GetProcAddress(name));
            _mesh = LedMeshProvider.LoadLedMesh();
        }
        catch (Exception ex)
        {
            Fail(ex);
        }
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        _renderer.Dispose();
        _scene.Dispose();
        _gizmo.Dispose();
        _gl = null;
        _initializedSize = Point3D.Empty;
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (_failed || _gl is not { } glApi || _mesh is null)
            return;

        try
        {
            // Prefer the live repository instance (playback swaps the cube on play); fall back to CubeData.
            var data = CubeRepository?.GetCubeData() ?? CubeData;
            if (data is null)
                return;

            EnsureSceneInitialized(glApi, data.Size, data.Length);
            ApplyTunables();
            SnapshotBrightness(data);

            var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
            var pxW = Math.Max(1, (int)(Bounds.Width * scaling));
            var pxH = Math.Max(1, (int)(Bounds.Height * scaling));

            glApi.BindFramebuffer(GLEnum.Framebuffer, (uint)fb);
            glApi.Viewport(0, 0, (uint)pxW, (uint)pxH);

            var bg = ToVector3(BackgroundColor);
            _gl.ClearColor(bg.X, bg.Y, bg.Z, 1f);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthMask(true);
            _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            var viewProj = _camera.GetViewProjection((float)pxW / pxH);

            if (ShowPodest)
                _scene.RenderSolids(viewProj);      // opaque podest (writes depth)
            if (ShowAxisGizmo)
                _gizmo.RenderSolids(viewProj);       // RGB axis gizmo (independent of the podest)
            if (ShowAnnotations)
                _scene.RenderDecal(viewProj, 1f);   // floor decal (blended, no depth write)
            _renderer.Render(viewProj, _brightness); // transparent LEDs (blended, no depth write)

            _gl.Disable(EnableCap.Blend);
            _gl.DepthMask(true);
        }
        catch (Exception ex)
        {
            Fail(ex);
        }
        // Continuous rendering is driven by _renderTimer on the UI thread (see constructor).
    }

    private void EnsureSceneInitialized(GL glApi, Point3D size, int length)
    {
        var orientation = Orientation;
        var led0Arrow = ShowLed0Arrow;
        // ShowAxisGizmo / ShowPodest are gated at render time, so they don't force a rebuild.
        if (_renderer.IsInitialized && _initializedSize == size && _initializedOrientation == orientation
            && _initializedLed0Arrow == led0Arrow)
            return;

        if (_renderer.IsInitialized) _renderer.Dispose();
        if (_scene.IsInitialized) _scene.Dispose();
        if (_gizmo.IsInitialized) _gizmo.Dispose();

        // Compose the data transform (identity / installation) with the fixed canonical display basis.
        var effective = CubeOrientation.Compose(orientation, CanonicalDisplay);
        _renderer.Initialize(glApi, _mesh!, CubeInstanceLayout.Build(size, effective), Log);
        BuildScene(glApi, size, effective, led0Arrow);

        _initializedSize = size;
        _initializedOrientation = orientation;
        _initializedLed0Arrow = led0Arrow;
        _packed = new byte[(length + 7) / 8];
        _brightness = new float[length];

        // Frame the cube: pull the camera back to fit, looking slightly above the base.
        var maxDim = Math.Max(size.X, Math.Max(size.Y, size.Z));
        _camera.Target = Vector3.Zero;
        _camera.MaxDistance = maxDim * 6f;
        _camera.Distance = maxDim * 1.9f;
    }

    /// <summary>Build the podest, floor decal, and RGB axis gizmo. <paramref name="effective"/> already
    /// includes the canonical display basis, so it maps data coords straight to world coords.</summary>
    private void BuildScene(GL glApi, Point3D size, CubeOrientation effective, bool led0Arrow)
    {
        _scene.Initialize(glApi);
        _gizmo.Initialize(glApi);

        var dispSize = effective.DisplaySize(size);
        var footprintHalf = Math.Max(dispSize.X, dispSize.Z) / 2f - 0.5f;
        var floorY = -(dispSize.Y - 1) / 2f - 0.6f; // top of the podest (world Y is up)

        // Podest: matches the real cube proportions — a 38-wide LED array on a 42-wide, 10-tall base.
        var podestW = (Math.Max(dispSize.X, dispSize.Z)) * (42f / 38f);
        var podestH = podestW * (10f / 42f);
        var podest = PrimitiveMeshes.CreateBox(podestW, podestH, podestW);
        _scene.AddSolid(podest, Matrix4x4.CreateTranslation(0f, floorY - podestH / 2f, 0f), new Vector3(0.06f, 0.06f, 0.07f));

        // LED 0 floor position + first-index floor direction (world XZ).
        var cx = (dispSize.X - 1) / 2f;
        var cz = (dispSize.Z - 1) / 2f;
        var led0 = effective.ToDisplay(new Point3D(0, 0, 0), size);
        var led0Next = effective.ToDisplay(new Point3D(1, 0, 0), size); // data +X
        var led0Floor = new Vector2(led0.X - cx, led0.Z - cz);
        var firstDir = new Vector2(led0Next.X - led0.X, led0Next.Z - led0.Z);

        var decalHalf = podestW / 2f;
        var decal = FloorDecalTexture.Create(512, decalHalf, footprintHalf, led0Floor, firstDir, led0Arrow);
        _scene.SetDecal(decal.Rgba, decal.Size, decalHalf, Matrix4x4.CreateTranslation(0f, floorY + 0.05f, 0f));

        BuildAxisGizmo(effective, footprintHalf, floorY); // into _gizmo; rendered only when ShowAxisGizmo
    }

    /// <summary>RGB axis arrows (X=red, Y=green, Z=blue) at the front-left base corner, outside the grid.</summary>
    private void BuildAxisGizmo(CubeOrientation effective, float footprintHalf, float floorY)
    {
        // Front-left base corner (+Z is front), near LED 0's origin, just outside the LED grid.
        var corner = new Vector3(-(footprintHalf + 1.0f), floorY + 0.05f, footprintHalf + 1.0f);
        const float length = 2.6f;
        const float thick = 0.16f;
        var colors = new[] { new Vector3(0.9f, 0.2f, 0.2f), new Vector3(0.2f, 0.85f, 0.25f), new Vector3(0.3f, 0.45f, 1.0f) };

        // small neutral cube anchoring the origin
        _gizmo.AddSolid(PrimitiveMeshes.CreateBox(thick * 2f, thick * 2f, thick * 2f),
            Matrix4x4.CreateTranslation(corner), new Vector3(0.8f, 0.8f, 0.85f));

        for (var axis = 0; axis < 3; axis++)
        {
            var (wAxis, sign) = effective.Output(axis);
            var box = wAxis switch
            {
                0 => PrimitiveMeshes.CreateBox(length, thick, thick),
                1 => PrimitiveMeshes.CreateBox(thick, length, thick),
                _ => PrimitiveMeshes.CreateBox(thick, thick, length)
            };
            var offset = Vector3.Zero;
            switch (wAxis)
            {
                case 0: offset.X = sign * length / 2f; break;
                case 1: offset.Y = sign * length / 2f; break;
                default: offset.Z = sign * length / 2f; break;
            }
            _gizmo.AddSolid(box, Matrix4x4.CreateTranslation(corner + offset), colors[axis]);
        }
    }

    private void SnapshotBrightness(ICubeData data)
    {
        // Serialize gives a coherent bit-packed snapshot (LED i = bit i), matching instance order.
        data.Serialize(_packed);
        for (var i = 0; i < _brightness.Length; i++)
        {
            var on = (_packed[i >> 3] & (1 << (i & 7))) != 0;
            _brightness[i] = on ? 1f : 0f;
        }
    }

    private void ApplyTunables()
    {
        _renderer.LedScale = (float)LedScale;
        var on = ToVector3(LedColor);
        _renderer.OnColor = on;
        _renderer.OffColor = on * 0.30f; // off LEDs are a dim version of the chosen color

        var t = (float)Math.Clamp(Transparency, 0d, 1d);
        _renderer.OffAlpha = Lerp(0.6f, 0.03f, t); // off LEDs fade out as transparency rises
        _renderer.OnAlpha = Lerp(1.0f, 0.6f, t);   // lit LEDs stay fairly solid
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    private static Vector3 ToVector3(Color c) => new(c.R / 255f, c.G / 255f, c.B / 255f);

    private void Fail(Exception ex)
    {
        _failed = true;
        Log($"render failed: {ex}");
    }

    private static void Log(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[Cube3DView] {message}");
        Console.Error.WriteLine($"[Cube3DView] {message}");
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _dragging = true;
        _lastPointer = e.GetPosition(this);
        e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging) return;
        var p = e.GetPosition(this);
        _camera.Orbit((float)(p.X - _lastPointer.X), (float)(p.Y - _lastPointer.Y));
        _lastPointer = p;
        RequestNextFrameRendering();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        e.Pointer.Capture(null);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        _camera.Zoom((float)e.Delta.Y);
        RequestNextFrameRendering();
    }
}
