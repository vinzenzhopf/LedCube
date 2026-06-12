using System.Numerics;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Core.UI.CubeView3D.Rendering;
using Xunit;

namespace LedCube.Test.Core.CubeView3D;

public class CubeInstanceLayoutTests
{
    [Fact]
    public void Length_matches_cube_volume()
    {
        var positions = CubeInstanceLayout.Build(new Point3D(16, 16, 16));
        Assert.Equal(16 * 16 * 16, positions.Length);
    }

    [Fact]
    public void Grid_is_centered_on_origin()
    {
        var size = new Point3D(16, 16, 16);
        var positions = CubeInstanceLayout.Build(size);

        var centroid = Vector3.Zero;
        foreach (var p in positions)
            centroid += p;
        centroid /= positions.Length;

        Assert.True(centroid.Length() < 1e-3f, $"expected centered grid, got centroid {centroid}");
    }

    [Fact]
    public void First_corner_is_at_negative_half_extent()
    {
        var positions = CubeInstanceLayout.Build(new Point3D(16, 16, 16));
        Assert.Equal(new Vector3(-7.5f, -7.5f, -7.5f), positions[0]);
    }

    [Fact]
    public void Position_index_matches_serialize_bit_order()
    {
        // The control derives brightness[i] from Serialize bit i, and places instance i at
        // positions[i]. This verifies those two orderings agree for a known LED.
        var size = new Point3D(16, 16, 16);
        var coord = new Point3D(3, 5, 7);
        var positions = CubeInstanceLayout.Build(size);

        ICubeData cube = new CubeData<CubeDataBuffer16>();
        cube.SetLed(coord, true);
        var packed = new byte[cube.Length / 8];
        cube.Serialize(packed);

        // Find the single set bit.
        var setIndex = -1;
        for (var i = 0; i < cube.Length; i++)
        {
            if ((packed[i >> 3] & (1 << (i & 7))) != 0)
            {
                Assert.Equal(-1, setIndex); // exactly one bit set
                setIndex = i;
            }
        }

        Assert.NotEqual(-1, setIndex);
        var expected = new Vector3(coord.X - 7.5f, coord.Y - 7.5f, coord.Z - 7.5f);
        Assert.Equal(expected, positions[setIndex]);
    }
}
