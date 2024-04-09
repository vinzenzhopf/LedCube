using System.Diagnostics;
using System.Numerics;
using System.Text;
using LedCube.Core.Common.CubeData.Generator;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Sdf.Core;
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
    
    [Fact]
    public void TestSdfFrame()
    {
        ICubeData buffer = new CubeData<CubeDataBuffer16>();
        buffer.Clear();
        
        var sut = Sdf.Core.Sdf.BoxFrame(new Vector3(4, 4, 4), 0.25f);
        
        
        buffer.Render(sut, 0, new SdfRenderOptions(){Centered = true, Margin = 0.499f});
        var sb = new StringBuilder();
        sb.AppendCubeData(buffer);
        _testOutputHelper.WriteLine(sb.ToString());
    }
}

public static class SdfTestOutputExtensions
{
    public static StringBuilder AppendCubeData(this StringBuilder sb, ICubeData cubeData)
    {
        const int stackCount = 4;
        for (var z = 0; z < cubeData.Size.Z; z+=stackCount)
        {
            sb.AppendStackedPlaneData(cubeData, z, stackCount);
        }
        return sb;
    }

    public static StringBuilder AppendStackedPlaneData(this StringBuilder sb, ICubeData cubeData, int planeIndex, int planeCount = 1)
    {
        //Header
        for (var i = 0; i < planeCount; i++)
        {
            sb.Append($"Plane {planeIndex+i:D2} Data:");
            var missing = 3 + 2 * cubeData.Size.X - 14;
            for (var s = 0; s < missing; s++)
            {
                sb.Append(' ');
            }
            sb.Append("\t\t");
        }
        sb.Append(" \n");
        for (var i = 0; i < planeCount; i++)
        {
            sb.AppendPlaneHeader(cubeData.Size.X);
        }
        sb.Append(" \n");
        
        
        for (var y = 0; y < cubeData.Size.Y; y++)
        {
            for (var i = 0; i < planeCount; i++)
            {
                sb.Append(NumberToSingleChar(y));
                for (var x = 0; x < cubeData.Size.X; x++)
                {
                    sb.Append(' ').Append(cubeData.GetLed(new Point3D(x, y, planeIndex + i)) ? 'X' : ' ');
                }
                sb.Append(' ');
                sb.Append(NumberToSingleChar(y));
                sb.Append("\t\t");    
            }
            sb.Append('\n');
        }
        
        //Footer Row
        for (var i = 0; i < planeCount; i++)
        {
            sb.AppendPlaneHeader(cubeData.Size.X);
        }
        sb.Append(" \n");
        return sb;
    }

    private static StringBuilder AppendPlaneHeader(this StringBuilder sb, int size)
    {
        sb.Append(' ');
        for (var x = 0; x < size; x++)
        {
            sb.Append(' ').Append(NumberToSingleChar(x));
        }
        sb.Append("  ");
        sb.Append("\t\t");
        return sb;
    }
    
    private static char NumberToSingleChar(int index)
    {
        const string map = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        return (index < 0 || index > map.Length) ? '-' : map[index];
    } 
}