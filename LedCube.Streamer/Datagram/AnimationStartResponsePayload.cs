using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct AnimationStartResponsePayload : IWritableDatagram<AnimationStartResponsePayload>, IReadableDatagram<AnimationStartResponsePayload>
{
    public static int Size => sizeof(UInt32);

    public UInt32 CurrentTicks;
    
    public static void WriteTo(Span<byte> target, in AnimationStartResponsePayload source)
    {
        MemoryMarshal.Write(target[0..], in source.CurrentTicks);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref AnimationStartResponsePayload target)
    {
        target.CurrentTicks = MemoryMarshal.Read<UInt32>(source[0..]);
    }
   
    public override string ToString()
    {
        return $"{nameof(CurrentTicks)}: {CurrentTicks}";
    }
}