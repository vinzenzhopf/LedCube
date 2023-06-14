using System.Text;

namespace LedCube.Streamer.Datagram;

public struct InfoPayload
{
    /**
     * String size 32
     */
    public string Version;
    
    public const int Size = VersionLength;
    public const int VersionLength = 32;
    
    public static InfoPayload ReadFromSpan(ReadOnlySpan<byte> span)
    {
        return new InfoPayload()
        {
            Version = Encoding.ASCII.GetString(span[0..32])
        };
    }

    public static ReadOnlySpan<byte> WriteToSpan(InfoPayload data)
    {
        var span = new byte[Size].AsSpan();
        Encoding.ASCII.GetBytes(data.Version).AsSpan().CopyTo(span[0..]);
        return span;
    }

    public override string ToString()
    {
        return $"{nameof(Version)}: {Version}";
    }
};