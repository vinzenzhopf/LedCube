using LedCube.Core.Common.Model;
using Xunit.Abstractions;

namespace LedCube.Test.Core;

public class CubeDataTests : TestWithLoggingBase
{
    public CubeDataTests(ITestOutputHelper output) : base(output)
    {
    }
    
    public class Bool4CubeDataTests : CubeDataTestsBase
    {
        public Bool4CubeDataTests(ITestOutputHelper output) : 
            base(output, new Point3D(4, 4, 4))
        {
        }
    }

    public class Bool16CubeDataTests : CubeDataTestsBase
    {
        public Bool16CubeDataTests(ITestOutputHelper output) : 
            base(output, new Point3D(16, 16, 16))
        {
        }
    }

    public class BoolUnequalLongXCubeDataTests : CubeDataTestsBase
    {
        public BoolUnequalLongXCubeDataTests(ITestOutputHelper output) : 
            base(output, new Point3D(4, 7, 35))
        {
        }
    }

    public class BoolUnequalLongYCubeDataTests : CubeDataTestsBase
    {
        public BoolUnequalLongYCubeDataTests(ITestOutputHelper output) : 
            base(output, new Point3D(4, 100, 8))
        {
        }
    }

    public class BoolUnequalLongZCubeDataTests : CubeDataTestsBase
    {
        public BoolUnequalLongZCubeDataTests(ITestOutputHelper output) : 
            base(output, new Point3D(66, 42, 12))
        {
        }
    }
}