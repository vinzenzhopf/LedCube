using LedCube.Core.Common.CubeData.Projections;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Test.WeirdBuffers;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;

public class NonUniformCubeSimpleRotationProjectionTests : TestWithLoggingBase
{
    public NonUniformCubeSimpleRotationProjectionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Theory]
    [MemberData(nameof(CheckSizeCases))]
    public void CheckSize(ICubeData cubeData)
    {
        var (dimX, dimY, dimZ) = cubeData.Size;
        Assert.Equal(new Point3D(dimX, dimY, dimZ), GetProjectedSize(Orientation3D.Front));
        Assert.Equal(new Point3D(dimX, dimY, dimZ), GetProjectedSize(Orientation3D.Back));
        Assert.Equal(new Point3D(dimY, dimX, dimZ), GetProjectedSize(Orientation3D.Left));
        Assert.Equal(new Point3D(dimY, dimX, dimZ), GetProjectedSize(Orientation3D.Right));
        Assert.Equal(new Point3D(dimX, dimZ, dimY), GetProjectedSize(Orientation3D.Top));
        Assert.Equal(new Point3D(dimX, dimZ, dimY), GetProjectedSize(Orientation3D.Top));
        return;

        Point3D GetProjectedSize(Orientation3D o)
        {
            var projection = new SimpleRotationCubeProjection(cubeData, o);
            var sut = (ICubeData) projection;
            return sut.Size;
        }
    }

    public static TheoryData<ICubeData> CheckSizeCases() => new()
    {
        new CubeData<CubeDataBuffer8>(),
        new CubeData<CubeDataBuffer4x7x35>(),
        new CubeData<CubeDataBuffer4x100x8>(),
        new CubeData<CubeDataBuffer66x42x12>()
    };
}