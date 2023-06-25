using System.Security.Cryptography;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.CubeData;
using LedCube.Core.CubeData.Projections;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;

public class SpecificSimpleRotationProjectionTests : TestWithLoggingBase
{
    public SpecificSimpleRotationProjectionTests(ITestOutputHelper output) : base(output)
    {
    }

    public const int Size = 16;
    public const int Max = Size - 1;
    
    [Theory]
    [InlineData(Orientation3D.Front, 1, 1, 1, 1, 1, 1)]
    [InlineData(Orientation3D.Back, 0, 0, Max, Max, Max, Max)]
    //Base Point 1,2,3 in all directions projected
    //Front, Left, Back and Right are rotated around the Z-Axis.
    //Top and Bottom are rotated around the X-Axis.
    [InlineData(Orientation3D.Front,  1, 2, 3, 1, 2, 3)]
    [InlineData(Orientation3D.Left,   1, 2, 3, Max-2, 1, 3)]
    [InlineData(Orientation3D.Back,   1, 2, 3, Max-1, Max-2, 3)]
    [InlineData(Orientation3D.Right,  1, 2, 3, 2, Max-1, 3)]
    [InlineData(Orientation3D.Top,    1, 2, 3, 1, Max-3, 2)]
    [InlineData(Orientation3D.Bottom, 1, 2, 3, 1, 3, Max-2)]
    
    //Base Point 0,0,0 in all directions projected
    [InlineData(Orientation3D.Front,  0, 0, 0, 0, 0, 0)]
    [InlineData(Orientation3D.Left,   0, 0, 0, Max, 0, 0)]
    [InlineData(Orientation3D.Back,   0, 0, 0, Max, Max, 0)]
    [InlineData(Orientation3D.Right,  0, 0, 0, 0, Max, 0)]
    [InlineData(Orientation3D.Top,    0, 0, 0, 0, Max, 0)]
    [InlineData(Orientation3D.Bottom, 0, 0, 0, 0, 0, Max)]
    
    //Base Point Max,Max,Max in all directions projected
    [InlineData(Orientation3D.Front,  Max, Max, Max, Max, Max, Max)]
    [InlineData(Orientation3D.Left,   Max, Max, Max, 0, Max, Max)]
    [InlineData(Orientation3D.Back,   Max, Max, Max, 0, 0, Max)]
    [InlineData(Orientation3D.Right,  Max, Max, Max, Max, 0, Max)]
    [InlineData(Orientation3D.Top,    Max, Max, Max, Max, 0, Max)]
    [InlineData(Orientation3D.Bottom, Max, Max, Max, Max, Max, 0)]
    public void CheckPointProjection(Orientation3D o, int baseX, int baseY, int baseZ, int projX, int projY, int projZ)
    {
        var projectedPoint = new Point3D(projX, projY, projZ);
        var basePoint = new Point3D(baseX, baseY, baseZ);

        var cubeData = new CubeData(new Point3D(Size, Size, Size));
        var projection = new SimpleRotationCubeProjection(cubeData, o);
        var sut = (ICubeData) projection;
        
        var eventTriggered = false;
        void OnLocalChange(Point3D p, bool value)
        {
            Assert.Equal(basePoint, p);
            eventTriggered = true;
        }
        cubeData.LedChanged += OnLocalChange;

        sut.SetLed(projectedPoint, true);
        Assert.True(cubeData.GetLed(basePoint));
        Assert.True(eventTriggered);
    }

    [Theory]
    [InlineData(Orientation3D.Front, 1, 1, 1, 1, 1, 1)]
    [InlineData(Orientation3D.Back, 0, 0, Max, Max, Max, Max)]
    //Base Point 1,2,3 in all directions projected
    //Front, Left, Back and Right are rotated around the Z-Axis.
    //Top and Bottom are rotated around the X-Axis.
    [InlineData(Orientation3D.Front,  1, 2, 3, 1, 2, 3)]
    [InlineData(Orientation3D.Left,   1, 2, 3, Max-2, 1, 3)]
    [InlineData(Orientation3D.Back,   1, 2, 3, Max-1, Max-2, 3)]
    [InlineData(Orientation3D.Right,  1, 2, 3, 2, Max-1, 3)]
    [InlineData(Orientation3D.Top,    1, 2, 3, 1, Max-3, 2)]
    [InlineData(Orientation3D.Bottom, 1, 2, 3, 1, 3, Max-2)]
    
    //Base Point 0,0,0 in all directions projected
    [InlineData(Orientation3D.Front,  0, 0, 0, 0, 0, 0)]
    [InlineData(Orientation3D.Left,   0, 0, 0, Max, 0, 0)]
    [InlineData(Orientation3D.Back,   0, 0, 0, Max, Max, 0)]
    [InlineData(Orientation3D.Right,  0, 0, 0, 0, Max, 0)]
    [InlineData(Orientation3D.Top,    0, 0, 0, 0, Max, 0)]
    [InlineData(Orientation3D.Bottom, 0, 0, 0, 0, 0, Max)]
    
    //Base Point Max,Max,Max in all directions projected
    [InlineData(Orientation3D.Front,  Max, Max, Max, Max, Max, Max)]
    [InlineData(Orientation3D.Left,   Max, Max, Max, 0, Max, Max)]
    [InlineData(Orientation3D.Back,   Max, Max, Max, 0, 0, Max)]
    [InlineData(Orientation3D.Right,  Max, Max, Max, Max, 0, Max)]
    [InlineData(Orientation3D.Top,    Max, Max, Max, Max, 0, Max)]
    [InlineData(Orientation3D.Bottom, Max, Max, Max, Max, Max, 0)]
    public void CheckPointBackProjection(Orientation3D o, int baseX, int baseY, int baseZ, int projX, int projY, int projZ)
    {
        var projectedPoint = new Point3D(projX, projY, projZ);
        var basePoint = new Point3D(baseX, baseY, baseZ);

        var cubeData = new CubeData(new Point3D(Size, Size, Size));
        var eventTriggered = false;

        var projection = new SimpleRotationCubeProjection(cubeData, o);
        var sut = (ICubeData) projection;
        void OnLocalChange(Point3D p, bool value)
        {
            Assert.Equal(projectedPoint, p);
            eventTriggered = true;
        }
        sut.LedChanged += OnLocalChange;

        cubeData.SetLed(basePoint, true);
        Assert.True(eventTriggered);
    }
    
    [Theory]
    [InlineData(Orientation3D.Front, 1, 1, 1, 1, 1, 1)]
    [InlineData(Orientation3D.Back, 0, 0, Max, Max, Max, Max)]
    
    //Base Point 1,2,3 in all directions projected
    //Front, Left, Back and Right are rotated around the Z-Axis.
    //Top and Bottom are rotated around the X-Axis.
    [InlineData(Orientation3D.Front,  1, 2, 3, 1, 2, 3)]
    [InlineData(Orientation3D.Left,   1, 2, 3, Max-2, 1, 3)]
    [InlineData(Orientation3D.Back,   1, 2, 3, Max-1, Max-2, 3)]
    [InlineData(Orientation3D.Right,  1, 2, 3, 2, Max-1, 3)]
    [InlineData(Orientation3D.Top,    1, 2, 3, 1, Max-3, 2)]
    [InlineData(Orientation3D.Bottom, 1, 2, 3, 1, 3, Max-2)]
    
    //Base Point 0,0,0 in all directions projected
    [InlineData(Orientation3D.Front,  0, 0, 0, 0, 0, 0)]
    [InlineData(Orientation3D.Left,   0, 0, 0, Max, 0, 0)]
    [InlineData(Orientation3D.Back,   0, 0, 0, Max, Max, 0)]
    [InlineData(Orientation3D.Right,  0, 0, 0, 0, Max, 0)]
    [InlineData(Orientation3D.Top,    0, 0, 0, 0, Max, 0)]
    [InlineData(Orientation3D.Bottom, 0, 0, 0, 0, 0, Max)]
    
    //Base Point Max,Max,Max in all directions projected
    [InlineData(Orientation3D.Front,  Max, Max, Max, Max, Max, Max)]
    [InlineData(Orientation3D.Left,   Max, Max, Max, 0, Max, Max)]
    [InlineData(Orientation3D.Back,   Max, Max, Max, 0, 0, Max)]
    [InlineData(Orientation3D.Right,  Max, Max, Max, Max, 0, Max)]
    [InlineData(Orientation3D.Top,    Max, Max, Max, Max, 0, Max)]
    [InlineData(Orientation3D.Bottom, Max, Max, Max, Max, Max, 0)]
    public void CheckPointTwoWayProjection(Orientation3D o, int baseX, int baseY, int baseZ, int projX, int projY, int projZ)
    {
        var projectedPoint = new Point3D(projX, projY, projZ);
        var basePoint = new Point3D(baseX, baseY, baseZ);

        var cubeData = new CubeData(new Point3D(Size, Size, Size));
        var projection = new SimpleRotationCubeProjection(cubeData, o);
        var sut = (ICubeData) projection;
        
        var eventTriggered = false;
        void OnLocalChange(Point3D p, bool value)
        {
            Assert.Equal(projectedPoint, p);
            eventTriggered = true;
        }
        sut.LedChanged += OnLocalChange;
        
        sut.SetLed(projectedPoint, true);
        Assert.True(eventTriggered);
    }

    [Theory]
    [InlineData(Orientation3D.Front)]
    [InlineData(Orientation3D.Left)]
    [InlineData(Orientation3D.Back)]
    [InlineData(Orientation3D.Right)]
    [InlineData(Orientation3D.Top)]
    [InlineData(Orientation3D.Bottom)]
    public void CheckAllPointsTwoWayProjection(Orientation3D o)
    {
        var cubeData = new CubeData(new Point3D(Size, Size, Size));
        var projection = new SimpleRotationCubeProjection(cubeData, o);
        var sut = (ICubeData) projection;
        
        for (var z = 0; z < Size; z++)
        for (var y = 0; y < Size; y++)
        for (var x = 0; x < Size; x++)
        {
            var projectedPoint = new Point3D(x, y, z);
            var eventTriggered = false;
            void OnLocalChange(Point3D p, bool value)
            {
                Assert.Equal(projectedPoint, p);
                eventTriggered = true;
            }
            sut.LedChanged += OnLocalChange;
            sut.SetLed(projectedPoint, true);
            Assert.True(eventTriggered);
            sut.LedChanged -= OnLocalChange;
        }
    }
    
    [Theory]
    [InlineData(Orientation3D.Front)]
    [InlineData(Orientation3D.Left)]
    [InlineData(Orientation3D.Back)]
    [InlineData(Orientation3D.Right)]
    [InlineData(Orientation3D.Top)]
    [InlineData(Orientation3D.Bottom)]
    public void CheckAllPointsAreSet(Orientation3D o)
    {
        var cubeData = new CubeData(new Point3D(Size, Size, Size));
        var projection = new SimpleRotationCubeProjection(cubeData, o);
        var sut = (ICubeData) projection;
        
        for (var z = 0; z < Size; z++)
        for (var y = 0; y < Size; y++)
        for (var x = 0; x < Size; x++)
        {
            var projectedPoint = new Point3D(x, y, z);
            sut.SetLed(projectedPoint, true);
        }

        for(var i = 0; i < cubeData.Size.ElementProduct; i++)
        {
            Assert.True(cubeData.GetLedIndex(i));
        }
    }
}
