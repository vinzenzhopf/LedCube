using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct AnimationEndResponsePayload : IWritableDatagram<AnimationEndResponsePayload>, IReadableDatagram<AnimationEndResponsePayload>
{
    public static int Size => sizeof(UInt32);
    
    public UInt32 CurrentTicks;
    
    public static void WriteTo(Span<byte> target, in AnimationEndResponsePayload source)
    {
        MemoryMarshal.Write(target[0..], in source.CurrentTicks);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref AnimationEndResponsePayload target)
    {
        target.CurrentTicks = MemoryMarshal.Read<UInt32>(source[0..]);
    }

    public override string ToString()
    {
        return $"{nameof(CurrentTicks)}: {CurrentTicks}";
    }
}