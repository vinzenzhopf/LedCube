using System;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Common.Model.Orientation;

/// <summary>
/// Serializes a cube while remapping each LED from canonical (animation) space to physical/display
/// space per a <see cref="CubeOrientation"/>. The permutation is precomputed once (cheap per-frame:
/// a bulk serialize + a bit shuffle). The identity orientation reproduces <see cref="ICubeData.Serialize"/>
/// byte-for-byte, so streaming output is unchanged unless a non-identity orientation is configured.
/// </summary>
public sealed class OrientationStreamMapper
{
    private readonly int[] _displayIndex; // canonical linear index -> display linear index
    private readonly byte[] _scratch;

    public Point3D Size { get; }
    public CubeOrientation Orientation { get; }

    public OrientationStreamMapper(Point3D size, CubeOrientation orientation)
    {
        Size = size;
        Orientation = orientation;
        var length = size.X * size.Y * size.Z;
        _displayIndex = new int[length];
        _scratch = new byte[(length + 7) / 8];

        var displaySize = orientation.DisplaySize(size);
        for (var i = 0; i < length; i++)
        {
            var c = new Point3D(i % size.X, (i / size.X) % size.Y, (i / (size.X * size.Y)) % size.Z);
            var d = orientation.ToDisplay(c, size);
            _displayIndex[i] = d.X + d.Y * displaySize.X + d.Z * displaySize.X * displaySize.Y;
        }
    }

    /// <summary>Bit-packs the cube into <paramref name="target"/> in physical/display order.</summary>
    public void Serialize(ICubeData data, Span<byte> target)
    {
        data.Serialize(_scratch);
        target.Clear();
        for (var i = 0; i < _displayIndex.Length; i++)
        {
            if ((_scratch[i >> 3] & (1 << (i & 7))) == 0) continue;
            var di = _displayIndex[i];
            target[di >> 3] |= (byte)(1 << (di & 7));
        }
    }
}
