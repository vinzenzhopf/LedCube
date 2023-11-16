using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;

public class NoProjectionTests : TestWithLoggingBase
{
    public NoProjectionTests(ITestOutputHelper output) : base(output)
    {
    }
    
    public class Bool4CubeProjectionTests : CubeProjectionTestBase
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 4, 4)))
        {
        }
    }

    public class Bool16CubeProjectionTests : CubeProjectionTestBase
    {
        public Bool16CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(16, 16, 16)))
        {
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 7, 35)))
        {
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 100, 8)))
        {
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(66, 42, 12)))
        {
        }
    }
}