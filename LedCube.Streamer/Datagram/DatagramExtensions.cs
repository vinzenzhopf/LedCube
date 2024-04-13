using System;

namespace LedCube.Streamer.Datagram;

public static class DatagramExtensions
{

    public static Memory<byte> ToMemory<TDatagram>(in TDatagram datagram) where TDatagram : IWritableDatagram<TDatagram>
    {
        Memory<byte> buffer = new byte[TDatagram.Size];
        TDatagram.WriteTo(buffer.Span, datagram);
        return buffer;
    }

    public static TDatagram Read<TDatagram>(ReadOnlySpan<byte> source) where TDatagram : struct, IReadableDatagram<TDatagram>
    {
        var target = new TDatagram();
        TDatagram.ReadFrom(source, ref target);
        return target;
    }
    
}