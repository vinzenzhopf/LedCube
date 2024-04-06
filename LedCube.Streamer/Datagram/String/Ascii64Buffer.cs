using System.Runtime.CompilerServices;

namespace LedCube.Streamer.Datagram.String;

[InlineArray(64)]
public struct Ascii64Buffer : IStringBuffer<Ascii64Buffer>
{
    private byte _element0;

    public static Span<byte> GetBuffer(ref Ascii64Buffer self) => self;

    public static ReadOnlySpan<byte> GetReadOnlyBuffer(in Ascii64Buffer self) => self;
}