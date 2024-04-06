using System.Text;

namespace LedCube.Streamer.Datagram.String;

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