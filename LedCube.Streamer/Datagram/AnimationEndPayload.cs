using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct AnimationEndPayload
{
    public UInt32 CurrentTicks;

    public const int Size = sizeof(UInt32);
    
    public static AnimationEndPayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new AnimationEndPayload()
        {
            CurrentTicks = MemoryMarshal.Read<UInt32>(span[0..]),
        };
    }

    public static ReadOnlyMemory<byte> WriteToMemory(AnimationEndPayload data)
    {   
        Memory<byte> buffer = new byte[Size];
        MemoryMarshal.Write(buffer.Span[0..], in data.CurrentTicks);
        return buffer;
    }

    public static ReadOnlySpan<byte> WriteToSpan(AnimationEndPayload data)
        => WriteToMemory(data).Span;

    public override string ToString()
    {
        return $"{nameof(CurrentTicks)}: {CurrentTicks}";
    }
}