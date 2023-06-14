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

    public static ReadOnlySpan<byte> WriteToSpan(InfoResponsePayload data)
    {
        var span = new byte[42].AsSpan();
        Encoding.ASCII.GetBytes(data.Version).AsSpan().CopyTo(span[0..]);
        MemoryMarshal.Write(span[32..], ref data.LastFrameTimeUs);
        MemoryMarshal.Write(span[36..], ref data.CurrentTicks);
        MemoryMarshal.Write(span[40..], ref data.ErrorCode);
        MemoryMarshal.Write(span[42..], ref data.Status);
        return span;
    }

    public override string ToString()
    {
        return $"{nameof(Version)}: {Version}, {nameof(LastFrameTimeUs)}: {LastFrameTimeUs}, {nameof(CurrentTicks)}: {CurrentTicks}, {nameof(ErrorCode)}: {ErrorCode}, {nameof(Status)}: {Status}";
    }
};