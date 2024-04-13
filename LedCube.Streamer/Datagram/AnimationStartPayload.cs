using System;
using System.Runtime.InteropServices;
using System.Text;
using LedCube.Streamer.Datagram.String;

namespace LedCube.Streamer.Datagram;

public struct AnimationStartPayload : IWritableDatagram<AnimationStartPayload>, IReadableDatagram<AnimationStartPayload>
{
    public static int Size => sizeof(UInt32) * 2 + 64;
    
    
    public UInt32 FrameTimeUs;
    public CString<Ascii64Buffer> AnimationName;
    public UInt32 CurrentTicks;
    
    public static void WriteTo(Span<byte> target, in AnimationStartPayload source)
    {
        MemoryMarshal.Write(target[0..], in source.FrameTimeUs);
        source.AnimationName.CopyTo(target.Slice(4,64));
        MemoryMarshal.Write(target[68..], in source.CurrentTicks);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref AnimationStartPayload target)
    {
        target.FrameTimeUs = MemoryMarshal.Read<UInt32>(source[0..]);
        target.AnimationName.CopyFrom(source.Slice(4, 64));
        target.CurrentTicks = MemoryMarshal.Read<UInt32>(source[68..]);
    }

    public override string ToString()
    {
        return $"{nameof(FrameTimeUs)}: {FrameTimeUs}, {nameof(AnimationName)}: {AnimationName}, {nameof(CurrentTicks)}: {CurrentTicks}";
    }
}