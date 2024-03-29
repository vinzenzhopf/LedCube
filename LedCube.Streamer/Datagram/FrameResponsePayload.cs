﻿using System.Runtime.InteropServices;

namespace LedCube.Streamer.Datagram;

public struct FrameResponsePayload
{
    public UInt32 FrameNumber;
    public UInt32 LastFrameTimeUs;
    public UInt32 CurrentTicks;
    public UInt32 ReceivedTicks;
    public AnimationStatus Status;
    
    public const int Size = sizeof(UInt32) * 4 + sizeof(AnimationStatus);

    public static FrameResponsePayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new FrameResponsePayload()
        {
            FrameNumber = MemoryMarshal.Read<UInt32>(span[0..]),
            LastFrameTimeUs = MemoryMarshal.Read<UInt32>(span[4..]),
            CurrentTicks = MemoryMarshal.Read<UInt32>(span[8..]),
            ReceivedTicks = MemoryMarshal.Read<UInt32>(span[12..]),
            Status = MemoryMarshal.Read<AnimationStatus>(span[16..])
        };
    }

    public static ReadOnlyMemory<byte> WriteToMemory(FrameResponsePayload data)
    {   
        var buffer = new byte[Size].AsMemory();
        MemoryMarshal.Write(buffer.Span[0..], ref data.FrameNumber);
        MemoryMarshal.Write(buffer.Span[4..], ref data.LastFrameTimeUs);
        MemoryMarshal.Write(buffer.Span[8..], ref data.CurrentTicks);
        MemoryMarshal.Write(buffer.Span[12..], ref data.ReceivedTicks);
        MemoryMarshal.Write(buffer.Span[16..], ref data.Status);
        return buffer;
    }

    public static ReadOnlySpan<byte> WriteToSpan(FrameResponsePayload data)
        => WriteToMemory(data).Span;

    public override string ToString()
    {
        return $"{nameof(FrameNumber)}: {FrameNumber}, {nameof(LastFrameTimeUs)}: {LastFrameTimeUs}, {nameof(CurrentTicks)}: {CurrentTicks}, {nameof(ReceivedTicks)}: {ReceivedTicks}, {nameof(Status)}: {Status}";
    }
}