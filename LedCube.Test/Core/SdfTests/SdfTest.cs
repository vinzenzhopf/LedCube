using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using LedCube.Core.Common.CubeData.Generator;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Sdf.Core;
using Pastel;
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
        const float margin = 0.49f;
        var size = new Point3D(16, 16, 16);
        var sdf = Sdf.Core.Sdf.BoxFrame(new Vector3(4, 4, 4), 0.5f);
        
    
        Vector3 center = size - 1;
        center /= 2;
        sdf = Sdf.Core.Sdf.Translate(sdf, center);
        
        var sb = new StringBuilder();
        sb.AppendSdfPlot(size, sdf, margin, 0);
        _testOutputHelper.WriteLine(sb.ToString());
    }
}

public static class SdfTestOutputExtensions
{
    public static StringBuilder AppendSdfPlot(this StringBuilder sb, Point3D size, Sdf3D sdf, float margin, float time)
    {
        const int stackCount = 4;
        for (var z = 0; z < size.Z; z += stackCount)
        {
            //Header
            for (var i = 0; i < stackCount; i++)
            {
                sb.Append($"Plane {z+i:D2} Data:");
                var missing = 3 + 2 * size.X - 14;
                for (var s = 0; s < missing; s++)
                {
                    sb.Append(' ');
                }
                sb.Append("\t\t");
            }
            sb.Append(" \n");
            for (var i = 0; i < stackCount; i++)
            {
                sb.AppendPlaneHeader(size.X);
            }
            sb.Append(" \n");
        
            //Rows
            for (var y = 0; y < size.Y; y++)
            {
                for (var i = 0; i < stackCount; i++)
                {
                    //One Plane Row
                    sb.Append(NumberToSingleChar(y));
                    for (var x = 0; x < size.X; x++)
                    {
                        //Each LED
                        var result = sdf(new Point3D(x, y, z + i), time);
                        var ledState = result <= margin;
                        sb.Append((" "+(ledState ? "X" : " "))
                            .PastelBg(MapColorToValue(result, margin)));
                    }
                    sb.Append(' ');
                    sb.Append(NumberToSingleChar(y));
                    sb.Append("\t\t");    
                }
                sb.Append('\n');
            }
        
            //Footer Row
            for (var i = 0; i < stackCount; i++)
            {
                sb.AppendPlaneHeader(size.X);
            }
            sb.Append(" \n");
        }
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
    
    private static Color MapColorToValue(float value, float margin)
    {
        var v = value - margin;
        v /= 5;
        v = (v > 1) ? 1 : (v < -1) ? -1 : v;
        return v > 0 ? 
            Color.White.Lerp(Color.DarkBlue, v) : 
            Color.Gold.Lerp(Color.White, v*-1);
    }
    
    public static Color Lerp(this Color s, Color t, float k)
    {
        var bk = (1 - k);
        var a = s.A * bk + t.A * k;
        var r = s.R * bk + t.R * k;
        var g = s.G * bk + t.G * k;
        var b = s.B * bk + t.B * k;
        return Color.FromArgb((int) a, (int) r, (int) g, (int) b);
    }
}