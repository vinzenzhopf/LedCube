namespace LedCube.Streamer.Datagram.String;

public interface IStringBuffer<TSelf> where TSelf : struct, IStringBuffer<TSelf>
{
    static abstract Span<byte> GetBuffer(ref TSelf self);
    static abstract ReadOnlySpan<byte> GetReadOnlyBuffer(in TSelf self);

}