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

    public static ReadOnlySpan<byte> WriteToSpan(AnimationEndPayload data)
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