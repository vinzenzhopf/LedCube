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

    public static ReadOnlySpan<byte> WriteToSpan(AnimationStartResponsePayload data)
    {   
        var span = new byte[Size].AsSpan();
        MemoryMarshal.Write(span[0..], ref data.CurrentTicks);
        return span;
    }

    public override string ToString()
    {
        return $"{nameof(CurrentTicks)}: {CurrentTicks}";
    }
}