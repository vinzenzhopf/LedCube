using System;
using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct FramePayload : IWritableDatagram<FramePayload>, IReadableDatagram<FramePayload>
{
    public static int Size => sizeof(UInt32) * 3 + FramePayloadData.Size;

    public UInt32 FrameNumber;
    public UInt32 FrameTimeUs;
    public UInt32 CurrentTicks;
    //Lenght: 512 bytes
    public FramePayloadData Data;

    public static void WriteTo(Span<byte> target, in FramePayload source)
    {
        MemoryMarshal.Write(target[0..], in source.FrameNumber);
        MemoryMarshal.Write(target[4..], in source.FrameTimeUs);
        MemoryMarshal.Write(target[8..], in source.CurrentTicks);
        ((ReadOnlySpan<byte>) source.Data).CopyTo(target[12..]);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref FramePayload target)
    {
        target.FrameNumber = MemoryMarshal.Read<UInt32>(source[0..]);
        target.FrameTimeUs = MemoryMarshal.Read<UInt32>(source[4..]);
        target.CurrentTicks = MemoryMarshal.Read<UInt32>(source[8..]);
        source.Slice(12, FramePayloadData.Size).CopyTo(target.Data);
    }

    public override string ToString()
    {
        return $"{nameof(FrameNumber)}: {FrameNumber}, {nameof(FrameTimeUs)}: {FrameTimeUs}, {nameof(CurrentTicks)}: {CurrentTicks}, {nameof(Data)}: {Data}";
    }
}