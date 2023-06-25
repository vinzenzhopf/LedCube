using System;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.CubeData;
using LedCube.Core.CubeData.Projections;
using Xunit;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;

public class NonUniformCubeSimpleRotationProjectionTests : TestWithLoggingBase
{
    public NonUniformCubeSimpleRotationProjectionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Theory]
    [InlineData(8,8,8)]
    [InlineData(8,8,16)]
    [InlineData(8,4,16)]
    public void CheckSize(int dimX, int dimY, int dimZ)
    {
        Point3D GetProjectedSize(Point3D baseSize, Orientation3D o)
        {
            var cubeData = new CubeData(baseSize);
            var projection = new SimpleRotationCubeProjection(cubeData, o);
            var sut = (ICubeData) projection;
            return sut.Size;
        };
        var size = new Point3D(dimX, dimY, dimZ);
        Assert.Equal(new Point3D(dimX, dimY, dimZ), GetProjectedSize(size, Orientation3D.Front));
        Assert.Equal(new Point3D(dimX, dimY, dimZ), GetProjectedSize(size, Orientation3D.Back));
        Assert.Equal(new Point3D(dimY, dimX, dimZ), GetProjectedSize(size, Orientation3D.Left));
        Assert.Equal(new Point3D(dimY, dimX, dimZ), GetProjectedSize(size, Orientation3D.Right));
        Assert.Equal(new Point3D(dimX, dimZ, dimY), GetProjectedSize(size, Orientation3D.Top));
        Assert.Equal(new Point3D(dimX, dimZ, dimY), GetProjectedSize(size, Orientation3D.Top));
    }
}