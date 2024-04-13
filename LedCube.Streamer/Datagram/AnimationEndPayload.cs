using System;
using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct AnimationEndPayload : IWritableDatagram<AnimationEndPayload>, IReadableDatagram<AnimationEndPayload>
{
    public static int Size => sizeof(UInt32);
    
    public UInt32 CurrentTicks;

    public static void WriteTo(Span<byte> target, in AnimationEndPayload source)
    {
        MemoryMarshal.Write(target[0..], in source.CurrentTicks);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref AnimationEndPayload target)
    {
        target.CurrentTicks = MemoryMarshal.Read<UInt32>(source[0..]);
    }

    public override string ToString()
    {
        return $"{nameof(CurrentTicks)}: {CurrentTicks}";
    }
}