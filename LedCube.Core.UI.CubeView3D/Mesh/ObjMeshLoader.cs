using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace LedCube.Core.UI.CubeView3D.Mesh;

/// <summary>
/// Minimal Wavefront OBJ loader. The exported LED meshes are position-only triangle lists
/// (<c>v x y z</c> / <c>f a b c</c>, no <c>vn</c>/<c>vt</c>), so we parse positions and faces,
/// optionally clip/recenter/normalize, then generate smooth per-vertex normals for lighting.
/// </summary>
public static class ObjMeshLoader
{
    public static ObjMesh Load(Stream stream, ObjLoadOptions options)
    {
        using var reader = new StreamReader(stream);
        return Load(reader, options);
    }

    public static ObjMesh Load(TextReader reader, ObjLoadOptions options)
    {
        var positions = new List<Vector3>();
        var triangles = new List<(int a, int b, int c)>();

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var span = line.AsSpan().Trim();
            if (span.IsEmpty || span[0] == '#')
                continue;

            if (span.StartsWith("v ") || span.StartsWith("v\t"))
            {
                positions.Add(ParseVertex(span[2..]));
            }
            else if (span.StartsWith("f ") || span.StartsWith("f\t"))
            {
                AppendFace(span[2..], positions.Count, triangles);
            }
        }

        return Build(positions, triangles, options);
    }

    private static Vector3 ParseVertex(ReadOnlySpan<char> rest)
    {
        Span<float> v = stackalloc float[3];
        var i = 0;
        foreach (var range in SplitWhitespace(rest))
        {
            if (i >= 3) break;
            v[i++] = float.Parse(rest[range], NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        return new Vector3(v[0], v[1], v[2]);
    }

    private static void AppendFace(ReadOnlySpan<char> rest, int vertexCount, List<(int, int, int)> triangles)
    {
        Span<int> idx = stackalloc int[8];
        var count = 0;
        foreach (var range in SplitWhitespace(rest))
        {
            var token = rest[range];
            // token forms: "v", "v/vt", "v/vt/vn", "v//vn" — first component is the position index.
            var slash = token.IndexOf('/');
            var vtok = slash >= 0 ? token[..slash] : token;
            var v = int.Parse(vtok, NumberStyles.Integer, CultureInfo.InvariantCulture);
            // OBJ indices are 1-based; negative indices are relative to the end.
            var resolved = v < 0 ? vertexCount + v : v - 1;
            if (count < idx.Length)
                idx[count++] = resolved;
        }

        // Triangulate as a fan.
        for (var k = 2; k < count; k++)
            triangles.Add((idx[0], idx[k - 1], idx[k]));
    }

    private static ObjMesh Build(List<Vector3> positions, List<(int a, int b, int c)> triangles, ObjLoadOptions options)
    {
        // Clip triangles whose any vertex falls below the local-z cutoff (removes LED legs).
        if (options.ClipMinLocalZ is { } clipZ)
        {
            triangles.RemoveAll(t =>
                positions[t.a].Z < clipZ ||
                positions[t.b].Z < clipZ ||
                positions[t.c].Z < clipZ);
        }

        // Determine which vertices are actually referenced and their bounds.
        var used = new bool[positions.Count];
        var min = new Vector3(float.PositiveInfinity);
        var max = new Vector3(float.NegativeInfinity);
        foreach (var (a, b, c) in triangles)
        {
            MarkUsed(a); MarkUsed(b); MarkUsed(c);
        }

        void MarkUsed(int vi)
        {
            used[vi] = true;
            min = Vector3.Min(min, positions[vi]);
            max = Vector3.Max(max, positions[vi]);
        }

        if (triangles.Count == 0)
            throw new InvalidDataException("OBJ produced no triangles (check clipping options).");

        var center = options.Recenter ? (min + max) * 0.5f : Vector3.Zero;
        var extent = max - min;
        var maxExtent = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z));
        var scale = options.NormalizeToUnit && maxExtent > 0 ? 1f / maxExtent : 1f;

        Vector3 Transform(Vector3 p) => (p - center) * scale;

        // Smooth per-vertex normals: accumulate area-weighted face normals.
        var normals = new Vector3[positions.Count];
        foreach (var (a, b, c) in triangles)
        {
            var p0 = positions[a];
            var p1 = positions[b];
            var p2 = positions[c];
            var n = Vector3.Cross(p1 - p0, p2 - p0); // CCW winding -> outward
            normals[a] += n;
            normals[b] += n;
            normals[c] += n;
        }

        // Remap referenced vertices to a compact 0..N range.
        var remap = new int[positions.Count];
        Array.Fill(remap, -1);
        var verts = new List<float>();
        var nextIndex = 0;
        var tMin = new Vector3(float.PositiveInfinity);
        var tMax = new Vector3(float.NegativeInfinity);
        for (var i = 0; i < positions.Count; i++)
        {
            if (!used[i]) continue;
            remap[i] = nextIndex++;
            var p = Transform(positions[i]);
            var n = normals[i];
            n = n.LengthSquared() > 1e-12f ? Vector3.Normalize(n) : Vector3.UnitZ;
            verts.Add(p.X); verts.Add(p.Y); verts.Add(p.Z);
            verts.Add(n.X); verts.Add(n.Y); verts.Add(n.Z);
            tMin = Vector3.Min(tMin, p);
            tMax = Vector3.Max(tMax, p);
        }

        var indices = new uint[triangles.Count * 3];
        var w = 0;
        foreach (var (a, b, c) in triangles)
        {
            indices[w++] = (uint)remap[a];
            indices[w++] = (uint)remap[b];
            indices[w++] = (uint)remap[c];
        }

        return new ObjMesh(verts.ToArray(), indices, tMin, tMax);
    }

    private static SplitEnumerator SplitWhitespace(ReadOnlySpan<char> span) => new(span);

    /// <summary>Allocation-free whitespace tokenizer yielding ranges into the source span.</summary>
    private ref struct SplitEnumerator
    {
        private readonly ReadOnlySpan<char> _span;
        private int _pos;

        public SplitEnumerator(ReadOnlySpan<char> span)
        {
            _span = span;
            _pos = 0;
        }

        public SplitEnumerator GetEnumerator() => this;

        public Range Current { get; private set; }

        public bool MoveNext()
        {
            while (_pos < _span.Length && char.IsWhiteSpace(_span[_pos]))
                _pos++;
            if (_pos >= _span.Length)
                return false;
            var start = _pos;
            while (_pos < _span.Length && !char.IsWhiteSpace(_span[_pos]))
                _pos++;
            Current = new Range(start, _pos);
            return true;
        }
    }
}
