using LedCube.Core.Common.CubeData.Projections;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;

public class SimpleRotationToFrontProjectionTests
{
    public class Bool4CubeProjectionTests : CubeProjectionTestBase
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 4, 4)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Front);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 7, 35)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Front);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 100, 8)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Front);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(66, 42, 12)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Front);
        }
    }
}

public class SimpleRotationToRearProjectionTests
{
    public class Bool4CubeProjectionTests : CubeProjectionTestBase
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 4, 4)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Back);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 7, 35)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Back);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 100, 8)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Back);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(66, 42, 12)))
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
    
    public class Bool4CubeProjectionTests : CubeProjectionTestBase
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 4, 4)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Left);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 7, 35)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Left);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 100, 8)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Left);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(66, 42, 12)))
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
    
    public class Bool4CubeProjectionTests : CubeProjectionTestBase
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 4, 4)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Right);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 7, 35)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Right);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 100, 8)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Right);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(66, 42, 12)))
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
    
    public class Bool4CubeProjectionTests : CubeProjectionTestBase
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 4, 4)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Top);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 7, 35)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Top);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 100, 8)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Top);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(66, 42, 12)))
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
    
    public class Bool4CubeProjectionTests : CubeProjectionTestBase
    {
        public Bool4CubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 4, 4)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Bottom);
        }
    }

    public class BoolUnequalLongXCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongXCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 7, 35)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Bottom);
        }
    }

    public class BoolUnequalLongYCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongYCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(4, 100, 8)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Bottom);
        }
    }

    public class BoolUnequalLongZCubeProjectionTests : CubeProjectionTestBase
    {
        public BoolUnequalLongZCubeProjectionTests(ITestOutputHelper output) : 
            base(output, new CubeData(new Point3D(66, 42, 12)))
        {
            Sut = new SimpleRotationCubeProjection(CubeData, Orientation3D.Bottom);
        }
    }
}