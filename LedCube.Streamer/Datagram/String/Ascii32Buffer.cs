using System.Runtime.CompilerServices;
using System.Text;

namespace LedCube.Streamer.Datagram;


public struct CString<TBuffer> where TBuffer : struct
{
    private TBuffer _buffer;
    
    public static implicit operator string(in CString<TBuffer> ascii32)
    {
        ReadOnlySpan<byte> buffer = ascii32;
        var terminal = buffer.IndexOf(byte.MinValue);
        if (terminal is -1)
        {
            return string.Empty;
        }
        return Encoding.ASCII.GetString(buffer[..terminal]);
    } 
    
    public static implicit operator CString<TBuffer>(string str)
    {
        CString<TBuffer> buffer = default;
        Encoding.ASCII.TryGetBytes(str, buffer[..^1], out _);
        return buffer;
    } 
}

[InlineArray(32)]
public struct Ascii32Buffer : IStringBuffer<Ascii32Buffer>
{
    private byte _element0;

    public static Span<byte> GetBuffer(ref Ascii32Buffer self) => self;

    public static ReadOnlySpan<byte> GetReadOnlyBuffer(in Ascii32Buffer self) => self;
}

[InlineArray(64)]
public struct Ascii64Buffer : IStringBuffer<Ascii64Buffer>
{
    private byte _element0;

    public static Span<byte> GetBuffer(ref Ascii64Buffer self) => self;

    public static ReadOnlySpan<byte> GetReadOnlyBuffer(in Ascii64Buffer self) => self;
}

internal interface IStringBuffer<TSelf> where TSelf : IStringBuffer<TSelf>
{
    static abstract Span<byte> GetBuffer(ref TSelf self);
    static abstract ReadOnlySpan<byte> GetReadOnlyBuffer(in TSelf self);

}