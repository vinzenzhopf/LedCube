using System.Text;

namespace LedCube.Streamer.Datagram;

public struct InfoPayload : IWritableDatagram<InfoPayload>, IReadableDatagram<InfoPayload>
{
    public static int Size => VersionLength;
    private const int VersionLength = 32;
    
    /**
     * String size 32
     */
    public string Version;

    public static void WriteTo(Span<byte> target, in InfoPayload source)
    {
        Encoding.ASCII.TryGetBytes(source.Version, target, out _);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref InfoPayload target)
    {
        target.Version = Encoding.ASCII.GetString(source[0..32]);
    }
    public override string ToString()
    {
        return $"{nameof(Version)}: {Version}";
    }
    
}