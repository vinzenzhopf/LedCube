using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Test.WeirdBuffers;
using Xunit.Abstractions;

namespace LedCube.Test.Core;

public class CubeDataTests : TestWithLoggingBase
{
    public CubeDataTests(ITestOutputHelper output) : base(output)
    {
    }

    public class Bool4CubeDataTests(ITestOutputHelper output) : CubeDataTestsBase<CubeDataBuffer4>(output);
    public class Bool16CubeDataTests(ITestOutputHelper output)  : CubeDataTestsBase<CubeDataBuffer4>(output);
    public class BoolUnequalLongXCubeDataTests(ITestOutputHelper output) : CubeDataTestsBase<CubeDataBuffer4x7x35>(output);
    public class BoolUnequalLongYCubeDataTests(ITestOutputHelper output) : CubeDataTestsBase<CubeDataBuffer4x100x8>(output);
    public class BoolUnequalLongZCubeDataTests(ITestOutputHelper output) : CubeDataTestsBase<CubeDataBuffer4x7x35>(output);
}