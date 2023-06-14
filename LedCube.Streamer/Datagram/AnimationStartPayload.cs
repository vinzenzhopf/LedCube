using System.Runtime.InteropServices;
using System.Text;

namespace LedCube.Streamer.Datagram;

public struct AnimationStartPayload
{
    public UInt32 FrameTimeUs;
    /**
     * String length of 63
     */
    public string AnimationName;
    public UInt32 CurrentTicks;
    
    public const int Size = sizeof(UInt32) * 2 + AnimationNameLength;
    public const int AnimationNameLength = 64;
    
    public static AnimationStartPayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new AnimationStartPayload()
        {
            FrameTimeUs = MemoryMarshal.Read<UInt32>(span[0..]),
            AnimationName = Encoding.ASCII.GetString(span.Slice(4, 64)),
            CurrentTicks = MemoryMarshal.Read<UInt32>(span[68..]),
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(AnimationStartPayload data)
    {   
        var span = new byte[Size].AsSpan();
        MemoryMarshal.Write(span[0..], ref data.FrameTimeUs);
        Encoding.ASCII.GetBytes(data.AnimationName).AsSpan()
            .CopyTo(span.Slice(4,64));
        MemoryMarshal.Write(span[68..], ref data.CurrentTicks);
        return span;
    }

    public override string ToString()
    {
        return $"{nameof(FrameTimeUs)}: {FrameTimeUs}, {nameof(AnimationName)}: {AnimationName}, {nameof(CurrentTicks)}: {CurrentTicks}";
    }
}