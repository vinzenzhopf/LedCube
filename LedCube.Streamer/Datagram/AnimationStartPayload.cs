using System.Runtime.InteropServices;
using System.Text;

namespace LedCube.Streamer.Datagram;

public struct AnimationStartPayload : IWritableDatagram<AnimationStartPayload>, IReadableDatagram<AnimationStartPayload>
{
    private const int AnimationNameLength = 64;
    public static int Size => sizeof(UInt32) * 2 + AnimationNameLength;
    
    
    public UInt32 FrameTimeUs;
    /**
     * String length of 63
     */
    public string AnimationName;
    public UInt32 CurrentTicks;
    
    public static void WriteTo(Span<byte> target, in AnimationStartPayload source)
    {
        MemoryMarshal.Write(target[0..], in source.FrameTimeUs);
        Encoding.ASCII.TryGetBytes(source.AnimationName, target.Slice(4,64), out _);
        MemoryMarshal.Write(target[68..], in source.CurrentTicks);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref AnimationStartPayload target)
    {
        target.FrameTimeUs = MemoryMarshal.Read<UInt32>(source[0..]);
        target.AnimationName = Encoding.ASCII.GetString(source.Slice(4, 64));
        target.CurrentTicks = MemoryMarshal.Read<UInt32>(source[68..]);
    }

    public override string ToString()
    {
        return $"{nameof(FrameTimeUs)}: {FrameTimeUs}, {nameof(AnimationName)}: {AnimationName}, {nameof(CurrentTicks)}: {CurrentTicks}";
    }
}