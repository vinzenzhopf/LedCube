using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Test.WeirdBuffers;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;

public class NoProjectionTests : TestWithLoggingBase
{
    public NoProjectionTests(ITestOutputHelper output) : base(output)
    {
    }

    public class Bool4CubeProjectionTests(ITestOutputHelper output) : CubeProjectionTestBase<CubeDataBuffer4>(output, new());

    public class Bool16CubeProjectionTests(ITestOutputHelper output) : CubeProjectionTestBase<CubeDataBuffer16>(output, new());

    public class BoolUnequalLongXCubeProjectionTests (ITestOutputHelper output) : CubeProjectionTestBase<CubeDataBuffer4x7x35>(output, new());

    public class BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : CubeProjectionTestBase<CubeDataBuffer4x100x8>(output, new());

    public class BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : CubeProjectionTestBase<CubeDataBuffer66x42x12>(output, new());
}