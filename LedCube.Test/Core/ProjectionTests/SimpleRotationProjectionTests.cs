using LedCube.Core.Common.CubeData.Projections;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Test.WeirdBuffers;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;

public class SimpleRotationToFrontProjectionTests
{
    public class Bool4CubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4>
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Front);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x7x35>
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Front);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x100x8>
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Front);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer66x42x12>
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Front);
        }
    }
}

public class SimpleRotationToRearProjectionTests
{
    public class Bool4CubeProjectionTests  : CubeProjectionTestBase<CubeDataBuffer4>
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Back);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x7x35>
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Back);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests  : CubeProjectionTestBase<CubeDataBuffer4x100x8>
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Back);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer66x42x12>
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Back);
        }
    }
}

public class SimpleRotationToLeftProjectionTests : TestWithLoggingBase
{
    public SimpleRotationToLeftProjectionTests(ITestOutputHelper output) : base(output)
    {
    }
    
    public class Bool4CubeProjectionTests  : CubeProjectionTestBase<CubeDataBuffer4>
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Left);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests  : CubeProjectionTestBase<CubeDataBuffer4x7x35>
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Left);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x100x8>
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Left);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer66x42x12>
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Left);
        }
    }
}

public class SimpleRotationToRightProjectionTests : TestWithLoggingBase
{
    public SimpleRotationToRightProjectionTests(ITestOutputHelper output) : base(output)
    {
    }
    
    public class Bool4CubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4>
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Right);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x7x35>
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Right);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x100x8>
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Right);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer66x42x12>
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Right);
        }
    }
}

public class SimpleRotationToTopProjectionTests : TestWithLoggingBase
{
    public SimpleRotationToTopProjectionTests(ITestOutputHelper output) : base(output)
    {
    }
    
    public class Bool4CubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4>
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Top);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x7x35>
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Top);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x100x8>
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Top);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer66x42x12>
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Top);
        }
    }
}

public class SimpleRotationToBottomProjectionTests : TestWithLoggingBase
{
    public SimpleRotationToBottomProjectionTests(ITestOutputHelper output) : base(output)
    {
    }
    
    public class Bool4CubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4>
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Bottom);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x7x35>
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Bottom);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer4x100x8>
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output)  : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Bottom);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase<CubeDataBuffer66x42x12>
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new())
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Bottom);
        }
    }
}