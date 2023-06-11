using System.Runtime.InteropServices;
using System.Text;

namespace LedCube.Streamer.Datagram;


public enum DatagramType : UInt16
{
    Discovery = 0x10,
    Info = 0x20,
    InfoResponse = 0x21,
    ErrorResponse = 0x22,
    AnimationStart = 0x30,
    AnimationStartAck = 0x31,
    AnimationEnd = 0x32,
    AnimationEndAck = 0x33,
    FrameData = 0x40,
    FrameDataAck = 0x41,
}

public enum ErrorCode : UInt16
{
    Ok = 0x00,
    PackageOrder = 0x10,
    FrameOrder = 0x11,
    UnknownPackage = 0x20
};

public struct Header
{
    public DatagramType PayloadType;
    public UInt16 PacketCount;

    public static int Size => sizeof(DatagramType) + sizeof(UInt16);
    
    public static Header ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new Header()
        {
            PayloadType = MemoryMarshal.Read<DatagramType>(span[0..]),
            PacketCount = MemoryMarshal.Read<UInt16>(span[2..]),
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(Header header)
    {
        var span = new byte[Size].AsSpan();
        MemoryMarshal.Write(span[0..], ref header.PayloadType);
        MemoryMarshal.Write(span[2..], ref header.PacketCount);
        return span;
    }
}

public struct InfoPayload
{
    /**
     * String size 32
     */
    public string Version;
    
    public static InfoPayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new InfoPayload()
        {
            Version = Encoding.ASCII.GetString(span[0..32])
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(InfoPayload data)
    {
        var span = new byte[32].AsSpan();
        Encoding.ASCII.GetBytes(data.Version).AsSpan().CopyTo(span[0..]);
        return span;
    }
};

public struct InfoResponsePayload
{
    /**
     * String size 32
     */
    public string Version;
    public UInt32 MaxFrameTimeUs;
    public UInt32 RuntimeMs;
    public ErrorCode ErrorCode;
    
    public static InfoResponsePayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new InfoResponsePayload()
        {
            Version = Encoding.ASCII.GetString(span[0..32]),
            MaxFrameTimeUs = MemoryMarshal.Read<UInt32>(span[32..]),
            RuntimeMs = MemoryMarshal.Read<UInt32>(span[36..]),
            ErrorCode = MemoryMarshal.Read<ErrorCode>(span[40..]),
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(InfoResponsePayload data)
    {
        var span = new byte[42].AsSpan();
        Encoding.ASCII.GetBytes(data.Version).AsSpan().CopyTo(span[0..]);
        MemoryMarshal.Write(span[32..], ref data.MaxFrameTimeUs);
        MemoryMarshal.Write(span[36..], ref data.RuntimeMs);
        MemoryMarshal.Write(span[40..], ref data.ErrorCode);
        return span;
    }
};

public struct AnimationStartPayload
{
    public UInt32 FrameTimeUs;
    /**
     * String length of 64
     */
    public string AnimationName;
    
    public static AnimationStartPayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new AnimationStartPayload()
        {
            FrameTimeUs = MemoryMarshal.Read<UInt32>(span[0..]),
            AnimationName = Encoding.ASCII.GetString(span.Slice(4, 64)),
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(AnimationStartPayload data)
    {   
        var span = new byte[68].AsSpan();
        MemoryMarshal.Write(span[0..], ref data.FrameTimeUs);
        Encoding.ASCII.GetBytes(data.AnimationName).AsSpan()
            .CopyTo(span.Slice(4,64));
        return span;
    }
}

public struct FramePayload
{
    public UInt32 FrameNumber;
    public UInt32 FrameTimeUs;
    //Lenght: 512 bytes
    public byte[] Data;
    
    public static FramePayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new FramePayload()
        {
            FrameNumber = MemoryMarshal.Read<UInt32>(span[0..]),
            FrameTimeUs = MemoryMarshal.Read<UInt32>(span[4..]),
            Data = span.Slice(8, 512).ToArray(),
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(FramePayload data)
    {   
        var span = new byte[512+8].AsSpan();
        MemoryMarshal.Write(span[0..], ref data.FrameNumber);
        MemoryMarshal.Write(span[4..], ref data.FrameTimeUs);
        data.Data.AsSpan()[0..512].CopyTo(span[8..]);
        return span;
    }
}
