using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct CubeDatagramHeader : IWritableDatagram<CubeDatagramHeader>, IReadableDatagram<CubeDatagramHeader>
{

    public static int Size => sizeof(DatagramType) + sizeof(UInt16);

    public DatagramType PayloadType;
    public UInt16 PacketCount;

    public static void WriteTo(Span<byte> target, in CubeDatagramHeader source)
    {
        MemoryMarshal.Write(target[0..], in source.PayloadType);
        MemoryMarshal.Write(target[2..], in source.PacketCount);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref CubeDatagramHeader target)
    {
        target.PayloadType = MemoryMarshal.Read<DatagramType>(source[0..]);
        target.PacketCount = MemoryMarshal.Read<UInt16>(source[2..]);
    }

    public override string ToString()
    {
        return $"{nameof(PayloadType)}: {PayloadType}, {nameof(PacketCount)}: {PacketCount}";
    }
}