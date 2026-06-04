using System;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using Xunit.Abstractions;

namespace LedCube.Test.Core;

/// <summary>
/// Covers <see cref="CubeData{TBuffer}.Serialize"/> — the 1-bit-per-LED packing that feeds the
/// UDP wire format. Uses the 4x4x4 buffer (64 LEDs = 8 bytes) so exact bytes can be asserted.
/// Bit layout: led index i -> byte i/8, bit i%8 (LSB first). For the 4-cube the flat index is
/// X + Y*4 + Z*16.
/// </summary>
public class CubeDataSerializeTests : TestWithLoggingBase
{
    private const int ByteCount = 64 / 8;

    public CubeDataSerializeTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Serialize_EmptyCube_AllZero()
    {
        var sut = new CubeData<CubeDataBuffer4>();
        Span<byte> target = stackalloc byte[ByteCount];
        target.Fill(0xFF); // ensure Serialize clears bytes it owns

        sut.Serialize(target);

        foreach (var b in target)
        {
            Assert.Equal(0, b);
        }
    }

    [Theory]
    // point ,                expected byte index, expected bit mask
    [InlineData(0, 0, 0, 0, 0b0000_0001)] // index 0
    [InlineData(1, 0, 0, 0, 0b0000_0010)] // index 1
    [InlineData(3, 0, 0, 0, 0b0000_1000)] // index 3
    [InlineData(0, 1, 0, 0, 0b0001_0000)] // index 4
    [InlineData(3, 3, 3, 7, 0b1000_0000)] // index 63
    [InlineData(0, 0, 1, 2, 0b0000_0001)] // index 16
    public void Serialize_SingleLed_SetsExpectedBit(int x, int y, int z, int expectedByte, byte expectedMask)
    {
        var sut = new CubeData<CubeDataBuffer4>();
        sut.SetLed(new Point3D(x, y, z), true);

        Span<byte> target = stackalloc byte[ByteCount];
        sut.Serialize(target);

        for (var i = 0; i < ByteCount; i++)
        {
            var expected = i == expectedByte ? expectedMask : (byte)0;
            Assert.Equal(expected, target[i]);
        }
    }

    [Fact]
    public void Serialize_MultipleLeds_OrsBitsTogether()
    {
        var sut = new CubeData<CubeDataBuffer4>();
        sut.SetLed(new Point3D(0, 0, 0), true); // index 0  -> byte0 bit0
        sut.SetLed(new Point3D(1, 0, 0), true); // index 1  -> byte0 bit1
        sut.SetLed(new Point3D(3, 3, 3), true); // index 63 -> byte7 bit7

        Span<byte> target = stackalloc byte[ByteCount];
        sut.Serialize(target);

        Assert.Equal(0b0000_0011, target[0]);
        Assert.Equal(0b1000_0000, target[7]);
        for (var i = 1; i < 7; i++)
        {
            Assert.Equal(0, target[i]);
        }
    }

    [Fact]
    public void Serialize_AllLedsOn_AllBitsSet()
    {
        var sut = new CubeData<CubeDataBuffer4>();
        for (var z = 0; z < 4; z++)
        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            sut.SetLed(new Point3D(x, y, z), true);

        Span<byte> target = stackalloc byte[ByteCount];
        sut.Serialize(target);

        foreach (var b in target)
        {
            Assert.Equal(0xFF, b);
        }
    }

    [Fact]
    public void Serialize_TargetTooSmall_Throws()
    {
        var sut = new CubeData<CubeDataBuffer4>();
        var tooSmall = new byte[ByteCount - 1];

        Assert.Throws<ArgumentException>(() => sut.Serialize(tooSmall));
    }

    [Fact]
    public void Serialize_LargerTarget_LeavesTrailingBytesUntouched()
    {
        var sut = new CubeData<CubeDataBuffer4>();
        sut.SetLed(new Point3D(0, 0, 0), true);

        var target = new byte[ByteCount + 4];
        Array.Fill(target, (byte)0xAA);

        sut.Serialize(target);

        Assert.Equal(0b0000_0001, target[0]);
        // Bytes beyond the cube's own range are not written by Serialize.
        for (var i = ByteCount; i < target.Length; i++)
        {
            Assert.Equal(0xAA, target[i]);
        }
    }
}
