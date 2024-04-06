using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct AnimationStartResponsePayload
{
    public UInt32 CurrentTicks;

    public const int Size = sizeof(UInt32);
    
    public static AnimationStartResponsePayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new AnimationStartResponsePayload()
        {
            CurrentTicks = MemoryMarshal.Read<UInt32>(span[0..]),
        };
    }

    public static ReadOnlyMemory<byte> WriteToMemory(AnimationStartResponsePayload data)
    {   
        Memory<byte> buffer = new byte[Size];
        MemoryMarshal.Write(buffer.Span[0..], in data.CurrentTicks);
        return buffer;
    }

    public static ReadOnlySpan<byte> WriteToSpan(AnimationStartResponsePayload data)
        => WriteToMemory(data).Span;

    public override string ToString()
    {
        return $"{nameof(CurrentTicks)}: {CurrentTicks}";
    }
}