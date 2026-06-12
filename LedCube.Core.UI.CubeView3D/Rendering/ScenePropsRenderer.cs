using System;
using System.Collections.Generic;
using System.Numerics;
using LedCube.Core.UI.CubeView3D.Mesh;
using Silk.NET.OpenGL;

namespace LedCube.Core.UI.CubeView3D.Rendering;

/// <summary>
/// Renders opaque scene props (the podest, via a lit solid shader) and the flat floor decal
/// (a textured quad carrying the FRONT/first-row annotations). Separate from the LED pass so the
/// control can order opaque → decal → transparent LEDs correctly.
/// </summary>
public sealed unsafe class ScenePropsRenderer : IDisposable
{
    private GL? _gl;

    private uint _solidProgram;
    private int _sViewProj, _sModel, _sColor, _sLightDir, _sAmbient;

    private uint _texProgram;
    private int _tViewProj, _tModel, _tTex, _tOpacity;

    private readonly List<Prop> _props = new();
    private bool _hasDecal;
    private uint _decalVao, _decalVbo, _decalEbo, _decalTex;
    private Matrix4x4 _decalModel = Matrix4x4.Identity;

    public Vector3 LightDirection { get; set; } = Vector3.Normalize(new Vector3(-0.4f, -1f, -0.6f));
    public float Ambient { get; set; } = 0.4f;

    public bool IsInitialized => _gl is not null;

    private struct Prop
    {
        public uint Vao, Vbo, Ebo;
        public int IndexCount;
        public Matrix4x4 Model;
        public Vector3 Color;
    }

    public void Initialize(GL gl)
    {
        _gl = gl;
        _solidProgram = GlProgram.Build(gl, Shaders.SolidVertex, Shaders.SolidFragment);
        _sViewProj = gl.GetUniformLocation(_solidProgram, "uViewProj");
        _sModel = gl.GetUniformLocation(_solidProgram, "uModel");
        _sColor = gl.GetUniformLocation(_solidProgram, "uColor");
        _sLightDir = gl.GetUniformLocation(_solidProgram, "uLightDir");
        _sAmbient = gl.GetUniformLocation(_solidProgram, "uAmbient");

        _texProgram = GlProgram.Build(gl, Shaders.TexturedVertex, Shaders.TexturedFragment);
        _tViewProj = gl.GetUniformLocation(_texProgram, "uViewProj");
        _tModel = gl.GetUniformLocation(_texProgram, "uModel");
        _tTex = gl.GetUniformLocation(_texProgram, "uTex");
        _tOpacity = gl.GetUniformLocation(_texProgram, "uOpacity");
    }

    public void AddSolid(ObjMesh mesh, Matrix4x4 model, Vector3 color)
    {
        if (_gl is not { } gl) throw new InvalidOperationException("Initialize first.");
        var vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        var vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (float* p = mesh.InterleavedVertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(mesh.InterleavedVertices.Length * sizeof(float)), p, BufferUsageARB.StaticDraw);

        const uint stride = ObjMesh.FloatsPerVertex * sizeof(float);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

        var ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed (uint* p = mesh.Indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(mesh.Indices.Length * sizeof(uint)), p, BufferUsageARB.StaticDraw);

        gl.BindVertexArray(0);
        _props.Add(new Prop { Vao = vao, Vbo = vbo, Ebo = ebo, IndexCount = mesh.IndexCount, Model = model, Color = color });
    }

    /// <summary>Build the floor decal quad spanning [-E,E] in XZ (at local y=0) and upload the texture.</summary>
    public void SetDecal(byte[] rgba, int texSize, float halfExtent, Matrix4x4 model)
    {
        if (_gl is not { } gl) throw new InvalidOperationException("Initialize first.");
        var e = halfExtent;

        // pos(3) + uv(2); uv maps (-E,-E)->(0,0), (E,E)->(1,1).
        float[] verts =
        {
            -e, 0, -e, 0, 0,
             e, 0, -e, 1, 0,
             e, 0,  e, 1, 1,
            -e, 0,  e, 0, 1,
        };
        uint[] indices = { 0, 1, 2, 0, 2, 3 };

        _decalVao = gl.GenVertexArray();
        gl.BindVertexArray(_decalVao);

        _decalVbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _decalVbo);
        fixed (float* p = verts)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(verts.Length * sizeof(float)), p, BufferUsageARB.StaticDraw);
        const uint stride = 5 * sizeof(float);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

        _decalEbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _decalEbo);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), p, BufferUsageARB.StaticDraw);

        gl.BindVertexArray(0);

        _decalTex = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _decalTex);
        fixed (byte* p = rgba)
            gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, (uint)texSize, (uint)texSize, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, p);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        gl.BindTexture(TextureTarget.Texture2D, 0);

        _decalModel = model;
        _hasDecal = true;
    }

    public void RenderSolids(Matrix4x4 viewProj)
    {
        if (_gl is not { } gl || _props.Count == 0) return;
        gl.Enable(EnableCap.DepthTest);
        gl.DepthMask(true);
        gl.Disable(EnableCap.Blend);
        gl.Disable(EnableCap.CullFace);

        gl.UseProgram(_solidProgram);
        SetMatrix(gl, _sViewProj, viewProj);
        var l = LightDirection;
        gl.Uniform3(_sLightDir, l.X, l.Y, l.Z);
        gl.Uniform1(_sAmbient, Ambient);

        foreach (var prop in _props)
        {
            SetMatrix(gl, _sModel, prop.Model);
            gl.Uniform3(_sColor, prop.Color.X, prop.Color.Y, prop.Color.Z);
            gl.BindVertexArray(prop.Vao);
            gl.DrawElements(PrimitiveType.Triangles, (uint)prop.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
        }
        gl.BindVertexArray(0);
    }

    public void RenderDecal(Matrix4x4 viewProj, float opacity)
    {
        if (_gl is not { } gl || !_hasDecal) return;
        gl.Enable(EnableCap.DepthTest);
        gl.DepthMask(false); // sits on the floor; don't occlude LEDs behind it
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        gl.UseProgram(_texProgram);
        SetMatrix(gl, _tViewProj, viewProj);
        SetMatrix(gl, _tModel, _decalModel);
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, _decalTex);
        gl.Uniform1(_tTex, 0);
        gl.Uniform1(_tOpacity, opacity);

        gl.BindVertexArray(_decalVao);
        gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        gl.BindVertexArray(0);
        gl.DepthMask(true);
    }

    private static void SetMatrix(GL gl, int location, Matrix4x4 m) =>
        gl.UniformMatrix4(location, 1, false, (float*)&m);

    public void Dispose()
    {
        if (_gl is not { } gl) return;
        foreach (var prop in _props)
        {
            gl.DeleteBuffer(prop.Vbo);
            gl.DeleteBuffer(prop.Ebo);
            gl.DeleteVertexArray(prop.Vao);
        }
        _props.Clear();
        if (_hasDecal)
        {
            gl.DeleteBuffer(_decalVbo);
            gl.DeleteBuffer(_decalEbo);
            gl.DeleteVertexArray(_decalVao);
            gl.DeleteTexture(_decalTex);
            _hasDecal = false;
        }
        if (_solidProgram != 0) gl.DeleteProgram(_solidProgram);
        if (_texProgram != 0) gl.DeleteProgram(_texProgram);
        _gl = null;
    }
}
