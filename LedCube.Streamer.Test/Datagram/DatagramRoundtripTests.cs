using System;
using LedCube.Streamer.Datagram;
using LedCube.Streamer.Datagram.String;

namespace LedCube.Streamer.Test.Datagram;

/// <summary>
/// Verifies that every wire-protocol struct survives a WriteTo -> ReadFrom round-trip
/// unchanged. A single wrong offset in these structs silently corrupts the UDP protocol,
/// so each field is asserted explicitly.
/// </summary>
public class DatagramRoundtripTests
{
    private static T RoundTrip<T>(in T source) where T : struct, IWritableDatagram<T>, IReadableDatagram<T>
    {
        // Use the production helpers so the test exercises the same Size/Write/Read path the app uses.
        var buffer = DatagramExtensions.ToMemory(in source);
        return DatagramExtensions.Read<T>(buffer.Span);
    }

    [Fact]
    public void CubeDatagramHeader_Roundtrips()
    {
        var source = new CubeDatagramHeader
        {
            PayloadType = DatagramType.FrameDataAck,
            PacketCount = 0xBEEF
        };

        var result = RoundTrip(in source);

        Assert.Equal(DatagramType.FrameDataAck, result.PayloadType);
        Assert.Equal(0xBEEF, result.PacketCount);
    }

    [Fact]
    public void InfoPayload_Roundtrips()
    {
        var source = new InfoPayload { Version = "Streamer v1.2.3" };

        var result = RoundTrip(in source);

        Assert.Equal("Streamer v1.2.3", result.Version);
    }

    [Fact]
    public void InfoResponsePayload_Roundtrips()
    {
        var source = new InfoResponsePayload
        {
            Version = "Cube v4.5",
            LastFrameTimeUs = 16_667,
            CurrentTicks = 123_456_789,
            ErrorCode = CubeErrorCode.FrameOrder,
            Status = AnimationStatus.Running
        };

        var result = RoundTrip(in source);

        Assert.Equal("Cube v4.5", result.Version);
        Assert.Equal(16_667u, result.LastFrameTimeUs);
        Assert.Equal(123_456_789u, result.CurrentTicks);
        Assert.Equal(CubeErrorCode.FrameOrder, result.ErrorCode);
        Assert.Equal(AnimationStatus.Running, result.Status);
    }

    [Fact]
    public void AnimationStartPayload_Roundtrips()
    {
        var source = new AnimationStartPayload
        {
            FrameTimeUs = 33_333,
            AnimationName = "GameOfLife",
            CurrentTicks = 987_654
        };

        var result = RoundTrip(in source);

        Assert.Equal(33_333u, result.FrameTimeUs);
        Assert.Equal("GameOfLife", result.AnimationName);
        Assert.Equal(987_654u, result.CurrentTicks);
    }

    [Fact]
    public void AnimationStartResponsePayload_Roundtrips()
    {
        var source = new AnimationStartResponsePayload { CurrentTicks = 0xDEADBEEF };

        var result = RoundTrip(in source);

        Assert.Equal(0xDEADBEEFu, result.CurrentTicks);
    }

    [Fact]
    public void AnimationEndPayload_Roundtrips()
    {
        var source = new AnimationEndPayload { CurrentTicks = 42 };

        var result = RoundTrip(in source);

        Assert.Equal(42u, result.CurrentTicks);
    }

    [Fact]
    public void AnimationEndResponsePayload_Roundtrips()
    {
        var source = new AnimationEndResponsePayload { CurrentTicks = 7 };

        var result = RoundTrip(in source);

        Assert.Equal(7u, result.CurrentTicks);
    }

    [Fact]
    public void FrameResponsePayload_Roundtrips()
    {
        var source = new FrameResponsePayload
        {
            FrameNumber = 1000,
            LastFrameTimeUs = 20_000,
            CurrentTicks = 55_555,
            ReceivedTicks = 44_444,
            Status = AnimationStatus.FrameDropped
        };

        var result = RoundTrip(in source);

        Assert.Equal(1000u, result.FrameNumber);
        Assert.Equal(20_000u, result.LastFrameTimeUs);
        Assert.Equal(55_555u, result.CurrentTicks);
        Assert.Equal(44_444u, result.ReceivedTicks);
        Assert.Equal(AnimationStatus.FrameDropped, result.Status);
    }

    // FramePayloadData is a fixed 512-byte inline array (one bit per LED for a 16^3 cube).
    private const int FramePayloadDataSize = 512;

    [Fact]
    public void FramePayload_Roundtrips_HeaderFieldsAndData()
    {
        var source = new FramePayload
        {
            FrameNumber = 256,
            FrameTimeUs = 16_000,
            CurrentTicks = 99_999
        };
        // Stamp a recognisable pattern across the 512-byte payload to catch off-by-one copies.
        for (var i = 0; i < FramePayloadDataSize; i++)
        {
            source.Data[i] = (byte)(i & 0xFF);
        }

        var result = RoundTrip(in source);

        Assert.Equal(256u, result.FrameNumber);
        Assert.Equal(16_000u, result.FrameTimeUs);
        Assert.Equal(99_999u, result.CurrentTicks);
        for (var i = 0; i < FramePayloadDataSize; i++)
        {
            Assert.Equal((byte)(i & 0xFF), result.Data[i]);
        }
    }

    [Fact]
    public void ToMemory_ProducesExactDatagramSize()
    {
        var header = new CubeDatagramHeader { PayloadType = DatagramType.Info, PacketCount = 1 };

        var buffer = DatagramExtensions.ToMemory(in header);

        Assert.Equal(CubeDatagramHeader.Size, buffer.Length);
    }

    [Fact]
    public void CString_TruncatesToBufferAndReadsBackTruncated()
    {
        // Ascii32Buffer holds 32 bytes; the last byte is reserved for the null terminator,
        // so at most 31 characters survive the round-trip.
        var longName = new string('A', 50);
        var source = new InfoPayload { Version = longName };

        var result = RoundTrip(in source);

        Assert.Equal(31, ((string)result.Version).Length);
        Assert.Equal(new string('A', 31), result.Version);
    }
}
