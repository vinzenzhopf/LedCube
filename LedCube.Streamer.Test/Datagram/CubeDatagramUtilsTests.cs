using System;
using System.Net;
using System.Net.Sockets;
using LedCube.Streamer.Datagram;

namespace LedCube.Streamer.Test.Datagram;

/// <summary>
/// Covers <see cref="CubeDatagramUtils.ResolveDatagramContent"/>: header parsing and the
/// payload-type dispatch switch that turns a raw datagram into a typed payload.
/// </summary>
public class CubeDatagramUtilsTests
{
    private static readonly IPEndPoint Remote = new(IPAddress.Loopback, 4242);

    private static UdpReceiveResult BuildDatagram<TPayload>(DatagramType type, ushort packetCount, in TPayload payload)
        where TPayload : struct, IWritableDatagram<TPayload>
    {
        var buffer = new byte[CubeDatagramHeader.Size + TPayload.Size];
        var header = new CubeDatagramHeader { PayloadType = type, PacketCount = packetCount };
        CubeDatagramHeader.WriteTo(buffer, in header);
        TPayload.WriteTo(buffer.AsSpan(CubeDatagramHeader.Size), in payload);
        return new UdpReceiveResult(buffer, Remote);
    }

    [Fact]
    public void Resolve_ParsesHeader()
    {
        var datagram = BuildDatagram(DatagramType.InfoResponse, 0x1234, new InfoResponsePayload { Version = "v1" });

        var result = CubeDatagramUtils.ResolveDatagramContent(datagram);

        Assert.Equal(DatagramType.InfoResponse, result.Header.PayloadType);
        Assert.Equal(0x1234, result.Header.PacketCount);
        Assert.Equal(Remote, result.Remote);
    }

    [Fact]
    public void Resolve_InfoResponse_ParsesPayload()
    {
        var payload = new InfoResponsePayload
        {
            Version = "Cube v2",
            LastFrameTimeUs = 1234,
            Status = AnimationStatus.Running,
            ErrorCode = CubeErrorCode.Ok
        };
        var datagram = BuildDatagram(DatagramType.InfoResponse, 1, in payload);

        var result = CubeDatagramUtils.ResolveDatagramContent(datagram);

        var parsed = Assert.IsType<InfoResponsePayload>(result.Payload);
        Assert.Equal("Cube v2", parsed.Version);
        Assert.Equal(1234u, parsed.LastFrameTimeUs);
        Assert.Equal(AnimationStatus.Running, parsed.Status);
    }

    [Fact]
    public void Resolve_AnimationStartAck_ParsesPayload()
    {
        var datagram = BuildDatagram(DatagramType.AnimationStartAck, 2,
            new AnimationStartResponsePayload { CurrentTicks = 5555 });

        var result = CubeDatagramUtils.ResolveDatagramContent(datagram);

        var parsed = Assert.IsType<AnimationStartResponsePayload>(result.Payload);
        Assert.Equal(5555u, parsed.CurrentTicks);
    }

    [Fact]
    public void Resolve_AnimationEndAck_ParsesPayload()
    {
        var datagram = BuildDatagram(DatagramType.AnimationEndAck, 3,
            new AnimationEndResponsePayload { CurrentTicks = 6666 });

        var result = CubeDatagramUtils.ResolveDatagramContent(datagram);

        var parsed = Assert.IsType<AnimationEndResponsePayload>(result.Payload);
        Assert.Equal(6666u, parsed.CurrentTicks);
    }

    [Fact]
    public void Resolve_FrameDataAck_ParsesPayload()
    {
        var payload = new FrameResponsePayload { FrameNumber = 77, Status = AnimationStatus.FrameRedrawn };
        var datagram = BuildDatagram(DatagramType.FrameDataAck, 4, in payload);

        var result = CubeDatagramUtils.ResolveDatagramContent(datagram);

        var parsed = Assert.IsType<FrameResponsePayload>(result.Payload);
        Assert.Equal(77u, parsed.FrameNumber);
        Assert.Equal(AnimationStatus.FrameRedrawn, parsed.Status);
    }

    [Fact]
    public void Resolve_UnmappedType_ReturnsNullPayload()
    {
        // FrameData (a request type) has no entry in the parse switch -> null payload.
        var datagram = BuildDatagram(DatagramType.AnimationStart, 5,
            new AnimationStartPayload { FrameTimeUs = 1000, AnimationName = "X" });

        var result = CubeDatagramUtils.ResolveDatagramContent(datagram);

        Assert.Null(result.Payload);
        Assert.Equal(DatagramType.AnimationStart, result.Header.PayloadType);
    }
}
