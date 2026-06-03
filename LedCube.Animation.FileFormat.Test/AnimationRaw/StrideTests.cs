using System;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.Test.AnimationRaw;

public class StrideTests
{
    [Theory]
    [InlineData(16, 16, 16, 512)]   // ceil(4096 / 8)
    [InlineData(8, 8, 8, 64)]       // ceil(512 / 8)
    [InlineData(5, 1, 1, 1)]        // ceil(5 / 8) rounds up
    [InlineData(8, 1, 1, 1)]        // exactly one byte
    [InlineData(9, 1, 1, 2)]        // ceil(9 / 8)
    public void BytesPerFrame_Binary(int x, int y, int z, int expected)
    {
        Assert.Equal(expected, LedFormat.Binary.BytesPerFrame(new Point3D(x, y, z)));
    }

    [Fact]
    public void BytesPerFrame_Grayscale_IsN()
    {
        Assert.Equal(512, LedFormat.Grayscale.BytesPerFrame(new Point3D(8, 8, 8)));
    }

    [Fact]
    public void BytesPerFrame_Rgb_IsThreeN()
    {
        Assert.Equal(512 * 3, LedFormat.Rgb.BytesPerFrame(new Point3D(8, 8, 8)));
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    public void BytesPerFrame_RejectsNonPositiveDimensions(int x, int y, int z)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LedFormat.Binary.BytesPerFrame(new Point3D(x, y, z)));
    }
}
