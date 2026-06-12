using System.Numerics;
using LedCube.Core.UI.CubeView3D.Mesh;

namespace LedCube.Core.UI.CubeView3D.Rendering;

/// <summary>Procedural primitive meshes (interleaved pos+normal) for scene props like the podest.</summary>
public static class PrimitiveMeshes
{
    /// <summary>
    /// Axis-aligned box centered at the origin with the given full extents. Flat per-face normals
    /// (dimensions are baked in so non-uniform sizing doesn't distort lighting).
    /// </summary>
    public static ObjMesh CreateBox(float width, float height, float depth)
    {
        var hx = width * 0.5f;
        var hy = height * 0.5f;
        var hz = depth * 0.5f;

        // 6 faces, each: 4 verts (pos+normal), 2 triangles.
        var faces = new (Vector3 normal, Vector3 a, Vector3 b, Vector3 c, Vector3 d)[]
        {
            // +X
            (new(1,0,0),  new(hx,-hy,-hz), new(hx,-hy,hz), new(hx,hy,hz), new(hx,hy,-hz)),
            // -X
            (new(-1,0,0), new(-hx,-hy,hz), new(-hx,-hy,-hz), new(-hx,hy,-hz), new(-hx,hy,hz)),
            // +Y (top)
            (new(0,1,0),  new(-hx,hy,-hz), new(hx,hy,-hz), new(hx,hy,hz), new(-hx,hy,hz)),
            // -Y (bottom)
            (new(0,-1,0), new(-hx,-hy,hz), new(hx,-hy,hz), new(hx,-hy,-hz), new(-hx,-hy,-hz)),
            // +Z
            (new(0,0,1),  new(hx,-hy,hz), new(-hx,-hy,hz), new(-hx,hy,hz), new(hx,hy,hz)),
            // -Z
            (new(0,0,-1), new(-hx,-hy,-hz), new(hx,-hy,-hz), new(hx,hy,-hz), new(-hx,hy,-hz)),
        };

        var verts = new float[faces.Length * 4 * ObjMesh.FloatsPerVertex];
        var indices = new uint[faces.Length * 6];
        var v = 0;
        var i = 0;
        uint baseIndex = 0;
        foreach (var (n, a, b, c, d) in faces)
        {
            foreach (var p in new[] { a, b, c, d })
            {
                verts[v++] = p.X; verts[v++] = p.Y; verts[v++] = p.Z;
                verts[v++] = n.X; verts[v++] = n.Y; verts[v++] = n.Z;
            }
            indices[i++] = baseIndex; indices[i++] = baseIndex + 1; indices[i++] = baseIndex + 2;
            indices[i++] = baseIndex; indices[i++] = baseIndex + 2; indices[i++] = baseIndex + 3;
            baseIndex += 4;
        }

        return new ObjMesh(verts, indices, new Vector3(-hx, -hy, -hz), new Vector3(hx, hy, hz));
    }
}
