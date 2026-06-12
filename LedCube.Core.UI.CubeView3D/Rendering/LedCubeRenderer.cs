using System;
using System.Numerics;
using LedCube.Core.UI.CubeView3D.Mesh;
using Silk.NET.OpenGL;

namespace LedCube.Core.UI.CubeView3D.Rendering;

/// <summary>
/// Owns all OpenGL resources for the cube preview and is the only type that touches GL.
/// Renders a single LED mesh instanced once per LED; per-frame the only upload is a small
/// brightness buffer (one float per LED). All public state (colors, scale, light) is tunable.
/// </summary>
public sealed unsafe class LedCubeRenderer : IDisposable
{
    // Local LED model long-axis is +z; rotate it to world +y ("up") for all instances.
    private static readonly Matrix4x4 ModelRotation = Matrix4x4.CreateRotationX(-MathF.PI / 2f);

    private GL? _gl;
    private uint _program;
    private uint _vao;
    private uint _meshVbo;
    private uint _ebo;
    private uint _instancePosVbo;
    private uint _brightnessVbo;
    private int _indexCount;
    private int _instanceCount;

    private int _uViewProj, _uModel, _uLightDir, _uOnColor, _uOffColor, _uAmbient, _uOnAlpha, _uOffAlpha;

    // Tunables
    public float LedScale { get; set; } = 0.85f;
    public Vector3 LightDirection { get; set; } = Vector3.Normalize(new Vector3(-0.4f, -1f, -0.6f));
    public Vector3 OnColor { get; set; } = new(0.20f, 0.45f, 1.0f); // diffuse blue
    public Vector3 OffColor { get; set; } = new(0.10f, 0.14f, 0.22f);
    public float Ambient { get; set; } = 0.30f;
    public float OnAlpha { get; set; } = 0.92f;
    public float OffAlpha { get; set; } = 0.12f; // low -> off LEDs are see-through

    public bool IsInitialized => _gl is not null;

    public void Initialize(GL gl, ObjMesh mesh, ReadOnlySpan<Vector3> instancePositions, Action<string>? log = null)
    {
        _gl = gl;
        log?.Invoke($"GL_VERSION: {gl.GetStringS(StringName.Version)}");
        log?.Invoke($"GLSL: {gl.GetStringS(StringName.ShadingLanguageVersion)}");

        _program = GlProgram.Build(gl, Shaders.LedVertex, Shaders.LedFragment);
        _uViewProj = gl.GetUniformLocation(_program, "uViewProj");
        _uModel = gl.GetUniformLocation(_program, "uModel");
        _uLightDir = gl.GetUniformLocation(_program, "uLightDir");
        _uOnColor = gl.GetUniformLocation(_program, "uOnColor");
        _uOffColor = gl.GetUniformLocation(_program, "uOffColor");
        _uAmbient = gl.GetUniformLocation(_program, "uAmbient");
        _uOnAlpha = gl.GetUniformLocation(_program, "uOnAlpha");
        _uOffAlpha = gl.GetUniformLocation(_program, "uOffAlpha");

        _indexCount = mesh.IndexCount;
        _instanceCount = instancePositions.Length;

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        // Mesh vertex buffer: interleaved pos(3) + normal(3).
        _meshVbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _meshVbo);
        fixed (float* p = mesh.InterleavedVertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(mesh.InterleavedVertices.Length * sizeof(float)), p, BufferUsageARB.StaticDraw);

        const uint stride = ObjMesh.FloatsPerVertex * sizeof(float);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

        // Index buffer.
        _ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* p = mesh.Indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(mesh.Indices.Length * sizeof(uint)), p, BufferUsageARB.StaticDraw);

        // Per-instance positions (static).
        _instancePosVbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instancePosVbo);
        fixed (Vector3* p = instancePositions)
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(instancePositions.Length * sizeof(Vector3)), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vector3), (void*)0);
        gl.VertexAttribDivisor(2, 1);

        // Per-instance brightness (dynamic, updated each frame).
        _brightnessVbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _brightnessVbo);
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_instanceCount * sizeof(float)), null, BufferUsageARB.DynamicDraw);
        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, sizeof(float), (void*)0);
        gl.VertexAttribDivisor(3, 1);

        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    /// <summary>
    /// Draws all LEDs with alpha blending (depth test on, depth write off) so inner LEDs show through.
    /// The caller owns clearing and viewport; opaque scene props should be drawn first.
    /// </summary>
    public void Render(Matrix4x4 viewProj, ReadOnlySpan<float> brightness)
    {
        if (_gl is not { } gl) return;
        if (brightness.Length != _instanceCount)
            throw new ArgumentException($"brightness length {brightness.Length} != instance count {_instanceCount}", nameof(brightness));

        gl.Enable(EnableCap.DepthTest);
        gl.DepthMask(false);            // transparent: don't let LEDs occlude each other in the depth buffer
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.Disable(EnableCap.CullFace); // LED meshes aren't guaranteed watertight/consistently wound

        gl.UseProgram(_program);

        var model = Matrix4x4.CreateScale(LedScale) * ModelRotation;
        SetMatrix(gl, _uModel, model);
        SetMatrix(gl, _uViewProj, viewProj);
        var l = LightDirection;
        gl.Uniform3(_uLightDir, l.X, l.Y, l.Z);
        gl.Uniform3(_uOnColor, OnColor.X, OnColor.Y, OnColor.Z);
        gl.Uniform3(_uOffColor, OffColor.X, OffColor.Y, OffColor.Z);
        gl.Uniform1(_uAmbient, Ambient);
        gl.Uniform1(_uOnAlpha, OnAlpha);
        gl.Uniform1(_uOffAlpha, OffAlpha);

        gl.BindVertexArray(_vao);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _brightnessVbo);
        fixed (float* p = brightness)
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(brightness.Length * sizeof(float)), p);

        gl.DrawElementsInstanced(PrimitiveType.Triangles, (uint)_indexCount,
            DrawElementsType.UnsignedInt, (void*)0, (uint)_instanceCount);

        gl.BindVertexArray(0);
        gl.DepthMask(true);
    }

    private static void SetMatrix(GL gl, int location, Matrix4x4 m)
    {
        // Upload raw (no transpose); GLSL multiplies on the left, cancelling the convention flip.
        gl.UniformMatrix4(location, 1, false, (float*)&m);
    }

    public void Dispose()
    {
        if (_gl is not { } gl) return;
        if (_brightnessVbo != 0) gl.DeleteBuffer(_brightnessVbo);
        if (_instancePosVbo != 0) gl.DeleteBuffer(_instancePosVbo);
        if (_ebo != 0) gl.DeleteBuffer(_ebo);
        if (_meshVbo != 0) gl.DeleteBuffer(_meshVbo);
        if (_vao != 0) gl.DeleteVertexArray(_vao);
        if (_program != 0) gl.DeleteProgram(_program);
        _gl = null;
    }
}
