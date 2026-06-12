using System;
using System.IO;
using System.Numerics;
using LedCube.Core.UI.CubeView3D.Mesh;
using Xunit;

namespace LedCube.Test.Core.CubeView3D;

public class ObjMeshLoaderTests
{
    private static ObjMesh LoadString(string obj, ObjLoadOptions options)
    {
        using var reader = new StringReader(obj);
        return ObjMeshLoader.Load(reader, options);
    }

    private const string Tetrahedron =
        "v 0 0 0\n" +
        "v 1 0 0\n" +
        "v 0 1 0\n" +
        "v 0 0 1\n" +
        "f 1 2 3\n" +
        "f 1 2 4\n" +
        "f 1 3 4\n" +
        "f 2 3 4\n";

    [Fact]
    public void Loads_positions_and_faces()
    {
        var mesh = LoadString(Tetrahedron, new ObjLoadOptions(NormalizeToUnit: false, Recenter: false));
        Assert.Equal(4, mesh.VertexCount);
        Assert.Equal(12, mesh.IndexCount); // 4 triangles
    }

    [Fact]
    public void Generates_unit_length_normals()
    {
        var mesh = LoadString(Tetrahedron, ObjLoadOptions.Default);
        for (var v = 0; v < mesh.VertexCount; v++)
        {
            var baseIdx = v * ObjMesh.FloatsPerVertex;
            var n = new Vector3(
                mesh.InterleavedVertices[baseIdx + 3],
                mesh.InterleavedVertices[baseIdx + 4],
                mesh.InterleavedVertices[baseIdx + 5]);
            Assert.Equal(1f, n.Length(), 3);
        }
    }

    [Fact]
    public void Normalizes_to_unit_extent_and_centers()
    {
        var mesh = LoadString(Tetrahedron, new ObjLoadOptions(NormalizeToUnit: true, Recenter: true));
        var extent = mesh.BoundsMax - mesh.BoundsMin;
        var maxExtent = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z));
        Assert.Equal(1f, maxExtent, 3);
        var center = (mesh.BoundsMin + mesh.BoundsMax) * 0.5f;
        Assert.True(center.Length() < 1e-3f, $"expected centered, got {center}");
    }

    [Fact]
    public void Triangulates_quads_as_fans()
    {
        const string quad =
            "v 0 0 0\nv 1 0 0\nv 1 1 0\nv 0 1 0\nf 1 2 3 4\n";
        var mesh = LoadString(quad, new ObjLoadOptions(NormalizeToUnit: false, Recenter: false));
        Assert.Equal(6, mesh.IndexCount); // quad -> 2 triangles
    }

    [Fact]
    public void Clips_triangles_below_local_z()
    {
        const string twoLayers =
            "v 0 0 0\nv 1 0 0\nv 0 1 0\n" +   // triangle at z=0
            "v 0 0 5\nv 1 0 5\nv 0 1 5\n" +   // triangle at z=5
            "f 1 2 3\nf 4 5 6\n";
        var mesh = LoadString(twoLayers, new ObjLoadOptions(ClipMinLocalZ: 1f, NormalizeToUnit: false, Recenter: false));
        Assert.Equal(3, mesh.IndexCount);  // only the upper triangle survives
        Assert.Equal(3, mesh.VertexCount); // only its 3 vertices are referenced
    }

    [Fact]
    public void Parses_face_tokens_with_slashes()
    {
        const string withSlashes =
            "v 0 0 0\nv 1 0 0\nv 0 1 0\nf 1/1/1 2/2/2 3/3/3\n";
        var mesh = LoadString(withSlashes, new ObjLoadOptions(NormalizeToUnit: false, Recenter: false));
        Assert.Equal(3, mesh.IndexCount);
    }

    [Fact]
    public void Embedded_led_mesh_loads()
    {
        var mesh = LedMeshProvider.LoadLedMesh();
        Assert.True(mesh.IndexCount > 0);
        Assert.Equal(0, mesh.IndexCount % 3);
        // Legs clipped + normalized: the body/dome fits within a unit cube.
        var extent = mesh.BoundsMax - mesh.BoundsMin;
        var maxExtent = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z));
        Assert.True(maxExtent <= 1f + 1e-3f, $"expected normalized extent, got {maxExtent}");
    }
}
