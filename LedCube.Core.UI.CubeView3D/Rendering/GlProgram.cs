using System;
using Silk.NET.OpenGL;

namespace LedCube.Core.UI.CubeView3D.Rendering;

/// <summary>Shared helpers for compiling and linking GLSL programs.</summary>
internal static class GlProgram
{
    public static uint Build(GL gl, string vertexSource, string fragmentSource)
    {
        var vs = Compile(gl, ShaderType.VertexShader, vertexSource);
        var fs = Compile(gl, ShaderType.FragmentShader, fragmentSource);
        var program = gl.CreateProgram();
        gl.AttachShader(program, vs);
        gl.AttachShader(program, fs);
        gl.LinkProgram(program);
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out var ok);
        if (ok == 0)
            throw new InvalidOperationException("Program link failed: " + gl.GetProgramInfoLog(program));
        gl.DetachShader(program, vs);
        gl.DetachShader(program, fs);
        gl.DeleteShader(vs);
        gl.DeleteShader(fs);
        return program;
    }

    private static uint Compile(GL gl, ShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out var ok);
        if (ok == 0)
            throw new InvalidOperationException($"{type} compile failed: " + gl.GetShaderInfoLog(shader));
        return shader;
    }
}
