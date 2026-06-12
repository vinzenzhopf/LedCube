using System.Numerics;

namespace LedCube.Core.UI.CubeView3D.Mesh;

/// <summary>
/// A triangle mesh ready for GPU upload. Vertices are interleaved as
/// position (3 floats) followed by normal (3 floats); indices are triangle lists.
/// </summary>
public sealed class ObjMesh
{
    public const int FloatsPerVertex = 6;

    public ObjMesh(float[] interleavedVertices, uint[] indices, Vector3 boundsMin, Vector3 boundsMax)
    {
        InterleavedVertices = interleavedVertices;
        Indices = indices;
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
    }

    /// <summary>Interleaved px,py,pz,nx,ny,nz per vertex.</summary>
    public float[] InterleavedVertices { get; }

    public uint[] Indices { get; }

    public int VertexCount => InterleavedVertices.Length / FloatsPerVertex;
    public int IndexCount => Indices.Length;

    /// <summary>Bounding box of the (post-transform) referenced geometry.</summary>
    public Vector3 BoundsMin { get; }
    public Vector3 BoundsMax { get; }
}
