using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;

namespace LedCube.Core.Animation.Test;

public class BinaryFrameRendererTests
{
    [Fact]
    public void Render_SetsExactlyTheOnBits()
    {
        var size = new Point3D(4, 4, 4); // N=64, stride=8
        // Indices: 0 -> (0,0,0), 5 -> (1,1,0), 63 -> (3,3,3)
        var frame = new Frame(PlayerFixtures.BinaryFrame(size, 0, 5, 63));
        var cube = new CubeData<CubeDataBuffer16>();

        new BinaryFrameRenderer().Render(frame, size, cube);

        Assert.True(cube.GetLed(new Point3D(0, 0, 0)));
        Assert.True(cube.GetLed(new Point3D(1, 1, 0)));
        Assert.True(cube.GetLed(new Point3D(3, 3, 3)));

        // A couple of LEDs that must remain off.
        Assert.False(cube.GetLed(new Point3D(1, 0, 0)));
        Assert.False(cube.GetLed(new Point3D(2, 2, 2)));
    }

    [Fact]
    public void Render_ClearsPreviousContent()
    {
        var size = new Point3D(4, 4, 4);
        var renderer = new BinaryFrameRenderer();
        var cube = new CubeData<CubeDataBuffer16>();

        renderer.Render(new Frame(PlayerFixtures.BinaryFrame(size, 0)), size, cube);
        Assert.True(cube.GetLed(new Point3D(0, 0, 0)));

        renderer.Render(new Frame(PlayerFixtures.BinaryFrame(size, 5)), size, cube);
        Assert.False(cube.GetLed(new Point3D(0, 0, 0))); // cleared
        Assert.True(cube.GetLed(new Point3D(1, 1, 0)));
    }
}
