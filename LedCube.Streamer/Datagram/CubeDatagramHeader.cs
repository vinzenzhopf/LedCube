using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct CubeDatagramHeader
{
    public DatagramType PayloadType;
    public UInt16 PacketCount;

    public static int Size => sizeof(DatagramType) + sizeof(UInt16);
    
    public static CubeDatagramHeader ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new CubeDatagramHeader()
        {
            PayloadType = MemoryMarshal.Read<DatagramType>(span[0..]),
            PacketCount = MemoryMarshal.Read<UInt16>(span[2..]),
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(CubeDatagramHeader cubeDatagramHeader)
    {
        var span = new byte[Size].AsSpan();
        MemoryMarshal.Write(span[0..], ref cubeDatagramHeader.PayloadType);
        MemoryMarshal.Write(span[2..], ref cubeDatagramHeader.PacketCount);
        return span;
    }

    public override string ToString()
    {
        return $"{nameof(PayloadType)}: {PayloadType}, {nameof(PacketCount)}: {PacketCount}";
    }
}