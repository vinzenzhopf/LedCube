namespace LedCube.Streamer.Datagram.String;

internal interface IStringBuffer<TSelf> where TSelf : IStringBuffer<TSelf>
{
    static abstract Span<byte> GetBuffer(ref TSelf self);
    static abstract ReadOnlySpan<byte> GetReadOnlyBuffer(in TSelf self);

}