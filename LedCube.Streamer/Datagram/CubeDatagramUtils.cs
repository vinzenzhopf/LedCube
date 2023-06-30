using System.Net.Sockets;
using LedCube.Streamer.Service;

namespace LedCube.Streamer.Datagram;

public static class CubeDatagramUtils
{
    public static ReceivedDatagram ResolveDatagramContent(this UdpReceiveResult result)
    {
        var datagram = result.Buffer.AsSpan();
        
        var header = CubeDatagramHeader.ReadFromSpan(datagram[..CubeDatagramHeader.Size]);
        var data = ParsePayloadData(header.PayloadType, datagram[CubeDatagramHeader.Size..]);
        
        return new ReceivedDatagram(result.RemoteEndPoint, header, data);
    }
    
    public static object? ParsePayloadData(DatagramType type, ReadOnlySpan<byte> payloadSpan)
    {
        return type switch
        {
            DatagramType.Discovery => InfoResponsePayload.ReadFromSpan(payloadSpan),
            DatagramType.InfoResponse => InfoResponsePayload.ReadFromSpan(payloadSpan),
            DatagramType.ErrorResponse => InfoResponsePayload.ReadFromSpan(payloadSpan),
            DatagramType.AnimationStartAck => AnimationStartResponsePayload.ReadFromSpan(payloadSpan),
            DatagramType.AnimationEndAck => AnimationEndResponsePayload.ReadFromSpan(payloadSpan),
            DatagramType.FrameDataAck => FrameResponsePayload.ReadFromSpan(payloadSpan),
            _ => null
        };
    }
}