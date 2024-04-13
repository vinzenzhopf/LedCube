using System;
using System.Runtime.CompilerServices;

namespace LedCube.Streamer.Datagram.String;

[InlineArray(32)]
public struct Ascii32Buffer : IStringBuffer<Ascii32Buffer>
{
    private byte _element0;
    public static Span<byte> GetBuffer(ref Ascii32Buffer self) => self;
    public static ReadOnlySpan<byte> GetReadOnlyBuffer(in Ascii32Buffer self) => self;
}