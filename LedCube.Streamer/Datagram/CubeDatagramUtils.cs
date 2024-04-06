using System.Net.Sockets;
using LedCube.Streamer.UdpCom;

namespace LedCube.Streamer.Datagram;

public static class CubeDatagramUtils
{
    public static ReceivedDatagram ResolveDatagramContent(UdpReceiveResult result)
    {
        var datagram = result.Buffer.AsSpan();

        var header = new CubeDatagramHeader();
        CubeDatagramHeader.ReadFrom(datagram[..CubeDatagramHeader.Size], ref header);
        var data = ParsePayloadData(header.PayloadType, datagram[CubeDatagramHeader.Size..]);
        
        return new ReceivedDatagram(result.RemoteEndPoint, header, data);
    }
    
    private static object? ParsePayloadData(DatagramType type, ReadOnlySpan<byte> payloadSpan)
    {
        return type switch
        {
            DatagramType.Discovery => DatagramExtensions.Read<InfoResponsePayload>(payloadSpan),
            DatagramType.InfoResponse => DatagramExtensions.Read<InfoResponsePayload>(payloadSpan),
            DatagramType.ErrorResponse => DatagramExtensions.Read<InfoResponsePayload>(payloadSpan),
            DatagramType.AnimationStartAck => DatagramExtensions.Read<AnimationStartResponsePayload>(payloadSpan),
            DatagramType.AnimationEndAck => DatagramExtensions.Read<AnimationEndResponsePayload>(payloadSpan),
            DatagramType.FrameDataAck => DatagramExtensions.Read<FrameResponsePayload>(payloadSpan),
            _ => null
        };
    }
}