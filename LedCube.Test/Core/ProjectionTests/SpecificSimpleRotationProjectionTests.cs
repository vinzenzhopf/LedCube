using System.Security.Cryptography;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.CubeData;
using LedCube.Core.CubeData.Projections;
using Xunit;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;

public class SpecificSimpleRotationToFrontProjectionTests : TestWithLoggingBase
{
    public SpecificSimpleRotationToFrontProjectionTests(ITestOutputHelper output) : base(output)
    {
    }

    public const int Size = 4;
    
    [Theory]
    [InlineData(Orientation3D.Front, 0, 0, 0, 0, 0, 0)]
    [InlineData(Orientation3D.Front, 1, 1, 1, 1, 1, 1)]
    
    //Base Point 0,0,1 in all directions
    [InlineData(Orientation3D.Top,   0, 0, 1, 0, 0, 1)]
    [InlineData(Orientation3D.Bottom, 0, 0, Size-1-1, 0, 0, 1)]
    [InlineData(Orientation3D.Front, 0, 1, 0, 0, 0, 1)]
    [InlineData(Orientation3D.Back, 0, Size-1-1, 0, 0, 0, 1)]
    [InlineData(Orientation3D.Right, 0, 1, 0, 0, 0, 1)]
    [InlineData(Orientation3D.Left, 0, 1, 0, 0, 0, 1)]
    public void CheckPointProjection(Orientation3D o, int projX, int projY, int projZ, int baseX, int baseY, int baseZ)
    {
        var projectedPoint = new Point3D(projX, projY, projZ);
        var basePoint = new Point3D(baseX, baseY, baseZ);

        var cubeData = new CubeData(new Point3D(Size, Size, Size));
        var projection = new SimpleRotationCubeProjection(cubeData, o);
        var sut = (ICubeData) projection;

        sut.SetLed(projectedPoint, true);
        Assert.True(cubeData.GetLed(basePoint));
    }
    
}
