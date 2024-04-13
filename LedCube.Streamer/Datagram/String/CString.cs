using System;
using System.Text;

namespace LedCube.Streamer.Datagram.String;

public struct CString<TBuffer> where TBuffer : struct, IStringBuffer<TBuffer>
{
    private TBuffer _buffer;
    
    public static implicit operator string(in CString<TBuffer> ascii32)
    {
        var buffer = TBuffer.GetReadOnlyBuffer(ascii32._buffer);
        var terminal = buffer.IndexOf(byte.MinValue);
        // Avoid Buffer overrun 
        if (terminal is -1)
        {
            return string.Empty;
        }
        return Encoding.ASCII.GetString(buffer[..terminal]);
    } 
    
    public static implicit operator CString<TBuffer>(string str)
    {
        var buffer = new CString<TBuffer>();
        Encoding.ASCII.TryGetBytes(str, TBuffer.GetBuffer(ref buffer._buffer!)[..^1], out _);
        return buffer;
    }
    
    
    public readonly void CopyTo(Span<byte> target) => TBuffer.GetReadOnlyBuffer(in _buffer).CopyTo(target);
    public void CopyFrom(ReadOnlySpan<byte> source) => source.CopyTo(TBuffer.GetBuffer(ref _buffer));
}