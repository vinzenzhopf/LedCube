using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct Header
{
    public DatagramType PayloadType;
    public UInt16 PacketCount;

    public static int Size => sizeof(DatagramType) + sizeof(UInt16);
    
    public static Header ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new Header()
        {
            PayloadType = MemoryMarshal.Read<DatagramType>(span[0..]),
            PacketCount = MemoryMarshal.Read<UInt16>(span[2..]),
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(Header header)
    {
        var span = new byte[Size].AsSpan();
        MemoryMarshal.Write(span[0..], ref header.PayloadType);
        MemoryMarshal.Write(span[2..], ref header.PacketCount);
        return span;
    }

    public override string ToString()
    {
        return $"{nameof(PayloadType)}: {PayloadType}, {nameof(PacketCount)}: {PacketCount}";
    }
}