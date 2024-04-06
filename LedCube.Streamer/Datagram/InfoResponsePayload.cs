using System.Runtime.InteropServices;
using System.Text;

namespace LedCube.Streamer.Datagram;

public struct InfoResponsePayload
{
    /**
     * String size 32
     */
    public string Version;
    public UInt32 LastFrameTimeUs;
    public UInt32 CurrentTicks;
    public CubeErrorCode ErrorCode;
    public AnimationStatus Status;
    
    public const int Size = sizeof(UInt32) * 2 + VersionLength + sizeof(CubeErrorCode) + sizeof(AnimationStatus);
    public const int VersionLength = 32;
    
    public static InfoResponsePayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new InfoResponsePayload()
        {
            Version = Encoding.ASCII.GetString(span[0..32]),
            LastFrameTimeUs = MemoryMarshal.Read<UInt32>(span[32..]),
            CurrentTicks = MemoryMarshal.Read<UInt32>(span[36..]),
            ErrorCode = MemoryMarshal.Read<CubeErrorCode>(span[40..]),
            Status = MemoryMarshal.Read<AnimationStatus>(span[42..]),
        };
    }

    public static ReadOnlyMemory<byte> WriteToMemory(InfoResponsePayload data)
    {
        Memory<byte> buffer = new byte[42];
        Encoding.ASCII.GetBytes(data.Version).AsSpan().CopyTo(buffer.Span[0..]);
        MemoryMarshal.Write(buffer.Span[32..], in data.LastFrameTimeUs);
        MemoryMarshal.Write(buffer.Span[36..], in data.CurrentTicks);
        MemoryMarshal.Write(buffer.Span[40..], in data.ErrorCode);
        MemoryMarshal.Write(buffer.Span[42..], in data.Status);
        return buffer;
    }

    public static ReadOnlySpan<byte> WriteToSpan(InfoResponsePayload data)
        => WriteToMemory(data).Span;

    public override string ToString()
    {
        return $"{nameof(Version)}: {Version}, {nameof(LastFrameTimeUs)}: {LastFrameTimeUs}, {nameof(CurrentTicks)}: {CurrentTicks}, {nameof(ErrorCode)}: {ErrorCode}, {nameof(Status)}: {Status}";
    }
};