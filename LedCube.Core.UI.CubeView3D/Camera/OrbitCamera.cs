using System;
using System.Numerics;

namespace LedCube.Core.UI.CubeView3D.Camera;

/// <summary>
/// Arcball-style orbit camera looking at a fixed target. Yaw/pitch orbit the target,
/// distance zooms. Builds right-handed View and Projection matrices for
/// <c>System.Numerics</c> (row-vector convention: transform = Model * View * Projection).
/// </summary>
public sealed class OrbitCamera
{
    private float _yaw = 0.7f;     // radians around world Y — view the front (+Z) face, slightly right
    private float _pitch = 0.45f; // radians, above the horizon
    private float _distance = 3.0f;

    public Vector3 Target { get; set; } = Vector3.Zero;
    public float FieldOfView { get; set; } = MathF.PI / 4f; // 45°
    public float NearPlane { get; set; } = 0.05f;
    public float FarPlane { get; set; } = 100f;

    public float MinPitch { get; set; } = -1.5f;
    public float MaxPitch { get; set; } = 1.5f;
    public float MinDistance { get; set; } = 0.5f;
    public float MaxDistance { get; set; } = 50f;

    public float RotateSpeed { get; set; } = 0.01f;
    public float ZoomSpeed { get; set; } = 0.1f;

    public float Distance
    {
        get => _distance;
        set => _distance = Math.Clamp(value, MinDistance, MaxDistance);
    }

    /// <summary>Orbit by pixel deltas (e.g. mouse drag).</summary>
    public void Orbit(float deltaXPixels, float deltaYPixels)
    {
        _yaw -= deltaXPixels * RotateSpeed;
        _pitch = Math.Clamp(_pitch + deltaYPixels * RotateSpeed, MinPitch, MaxPitch);
    }

    /// <summary>Zoom by wheel notches (positive = zoom in).</summary>
    public void Zoom(float delta)
    {
        // Multiplicative so zoom feels even across the distance range.
        Distance = _distance * MathF.Exp(-delta * ZoomSpeed);
    }

    public Vector3 Position
    {
        get
        {
            var cosPitch = MathF.Cos(_pitch);
            var dir = new Vector3(
                cosPitch * MathF.Sin(_yaw),
                MathF.Sin(_pitch),
                cosPitch * MathF.Cos(_yaw));
            return Target + dir * _distance;
        }
    }

    public Matrix4x4 GetViewMatrix() =>
        Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);

    public Matrix4x4 GetProjectionMatrix(float aspect) =>
        Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, MathF.Max(aspect, 0.0001f), NearPlane, FarPlane);

    public Matrix4x4 GetViewProjection(float aspect) =>
        GetViewMatrix() * GetProjectionMatrix(aspect);
}
