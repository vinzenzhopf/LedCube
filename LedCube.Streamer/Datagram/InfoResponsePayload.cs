using System;
using System.Runtime.InteropServices;
using System.Text;
using LedCube.Streamer.Datagram.String;

namespace LedCube.Streamer.Datagram;

public struct InfoResponsePayload : IWritableDatagram<InfoResponsePayload>, IReadableDatagram<InfoResponsePayload>
{

    public static int Size => sizeof(UInt32) * 2 + 32 + sizeof(CubeErrorCode) + sizeof(AnimationStatus);
    
    public CString<Ascii32Buffer> Version;
    public UInt32 LastFrameTimeUs;
    public UInt32 CurrentTicks;
    public CubeErrorCode ErrorCode;
    public AnimationStatus Status;
    
    public static void WriteTo(Span<byte> target, in InfoResponsePayload source)
    {
        source.Version.CopyTo(target[0..32]);
        MemoryMarshal.Write(target[32..], in source.LastFrameTimeUs);
        MemoryMarshal.Write(target[36..], in source.CurrentTicks);
        MemoryMarshal.Write(target[40..], in source.ErrorCode);
        MemoryMarshal.Write(target[42..], in source.Status);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref InfoResponsePayload target)
    {
        target.Version.CopyFrom(source[0..32]);
        target.LastFrameTimeUs = MemoryMarshal.Read<UInt32>(source[32..]);
        target.CurrentTicks = MemoryMarshal.Read<UInt32>(source[36..]);
        target.ErrorCode = MemoryMarshal.Read<CubeErrorCode>(source[40..]);
        target.Status = MemoryMarshal.Read<AnimationStatus>(source[42..]);
    }
    public override string ToString()
    {
        return $"{nameof(Version)}: {Version}, {nameof(LastFrameTimeUs)}: {LastFrameTimeUs}, {nameof(CurrentTicks)}: {CurrentTicks}, {nameof(ErrorCode)}: {ErrorCode}, {nameof(Status)}: {Status}";
    }
};