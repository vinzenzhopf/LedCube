using System.Diagnostics;
using System.Numerics;
using LedCube.Core.Common.CubeData.Generator;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using Xunit.Abstractions;

namespace LedCube.Test.Core.SdfTests;

public class SdfTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SdfTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SimpleTests()
    {
        var sut = Sdf.Core.Sdf.Translate(Sdf.Core.Sdf.Sphere(3), new Vector3(8, 8, 8));

        var res = sut(new Vector3(8, 8, 8), 0);
        Assert.True(res < 0);
        Assert.True(sut(new Vector3(9,9,9), 0) < 0);
        Assert.True(sut(new Vector3(8,8,8), 0) < 0);
        Assert.False(sut(new Vector3(2,2,2), 0) < 0);
    }

    [Fact]
    public void DrawCubeTimingTest()
    {
        ICubeData buffer = new CubeData<CubeDataBuffer16>();
        buffer.Clear();
        
        var sut = Sdf.Core.Sdf.Translate(Sdf.Core.Sdf.Sphere(3), new Vector3(8, 8, 8));
        
        var sw = Stopwatch.StartNew();
        using var pos = new PositionGenerator3D(buffer.Size).GetEnumerator();
        while(pos.MoveNext())
        {
            buffer.SetLed(pos.Current, sut(pos.Current, 0) < 0);
        }
        var time = sw.Elapsed;
        _testOutputHelper.WriteLine("Elapsed Time: {0}", time);
    }
}