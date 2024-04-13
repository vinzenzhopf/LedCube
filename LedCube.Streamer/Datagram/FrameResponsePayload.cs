using System;
using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct FrameResponsePayload : IWritableDatagram<FrameResponsePayload>, IReadableDatagram<FrameResponsePayload>
{ 
    public static int Size => sizeof(UInt32) * 4 + sizeof(AnimationStatus);

    public UInt32 FrameNumber;
    public UInt32 LastFrameTimeUs;
    public UInt32 CurrentTicks;
    public UInt32 ReceivedTicks;
    public AnimationStatus Status;
    
    public static void WriteTo(Span<byte> target, in FrameResponsePayload source)
    {
        MemoryMarshal.Write(target[0..], in source.FrameNumber);
        MemoryMarshal.Write(target[4..], in source.LastFrameTimeUs);
        MemoryMarshal.Write(target[8..], in source.CurrentTicks);
        MemoryMarshal.Write(target[12..], in source.ReceivedTicks);
        MemoryMarshal.Write(target[16..], in source.Status);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref FrameResponsePayload target)
    {
        target.FrameNumber = MemoryMarshal.Read<UInt32>(source[0..]);
        target.LastFrameTimeUs = MemoryMarshal.Read<UInt32>(source[4..]);
        target.CurrentTicks = MemoryMarshal.Read<UInt32>(source[8..]);
        target.ReceivedTicks = MemoryMarshal.Read<UInt32>(source[12..]);
        target.Status = MemoryMarshal.Read<AnimationStatus>(source[16..]);
    }

    public override string ToString()
    {
        return $"{nameof(FrameNumber)}: {FrameNumber}, {nameof(LastFrameTimeUs)}: {LastFrameTimeUs}, {nameof(CurrentTicks)}: {CurrentTicks}, {nameof(ReceivedTicks)}: {ReceivedTicks}, {nameof(Status)}: {Status}";
    }
}