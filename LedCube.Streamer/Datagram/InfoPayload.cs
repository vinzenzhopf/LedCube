using System;
using System.Text;
using LedCube.Streamer.Datagram.String;

namespace LedCube.Streamer.Datagram;

public struct InfoPayload : IWritableDatagram<InfoPayload>, IReadableDatagram<InfoPayload>
{
    public static int Size => 32;
    
    public CString<Ascii32Buffer> Version;

    public static void WriteTo(Span<byte> target, in InfoPayload source)
    {
        source.Version.CopyTo(target);
    }

    public static void ReadFrom(ReadOnlySpan<byte> source, ref InfoPayload target)
    {
        target.Version.CopyFrom(source[0..32]);
    }
    public override string ToString()
    {
        return $"{nameof(Version)}: {Version}";
    }
    
}