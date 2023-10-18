using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct FramePayload
{
    public UInt32 FrameNumber;
    public UInt32 FrameTimeUs;
    public UInt32 CurrentTicks;
    //Lenght: 512 bytes
    public byte[] Data;
    
    public const int Size = sizeof(UInt32) * 3 + DataSize;
    public const int DataSize = 512;
    
    public static FramePayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new FramePayload()
        {
            FrameNumber = MemoryMarshal.Read<UInt32>(span[0..]),
            FrameTimeUs = MemoryMarshal.Read<UInt32>(span[4..]),
            CurrentTicks = MemoryMarshal.Read<UInt32>(span[8..]),
            Data = span.Slice(12, DataSize).ToArray(),
        };
    }

    public static ReadOnlyMemory<byte> WriteToMemory(FramePayload data)
    {   
        var buffer = new byte[Size].AsMemory();
        MemoryMarshal.Write(buffer.Span[0..], ref data.FrameNumber);
        MemoryMarshal.Write(buffer.Span[4..], ref data.FrameTimeUs);
        MemoryMarshal.Write(buffer.Span[8..], ref data.CurrentTicks);
        data.Data.AsSpan()[0..DataSize].CopyTo(buffer.Span[12..]);
        return buffer;
    }

    public static ReadOnlySpan<byte> WriteToSpan(FramePayload data)
        => WriteToMemory(data).Span;

    public override string ToString()
    {
        return $"{nameof(FrameNumber)}: {FrameNumber}, {nameof(FrameTimeUs)}: {FrameTimeUs}, {nameof(CurrentTicks)}: {CurrentTicks}, {nameof(Data)}: {Data}";
    }
}