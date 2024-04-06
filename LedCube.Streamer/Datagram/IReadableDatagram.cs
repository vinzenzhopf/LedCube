namespace LedCube.Streamer.Datagram;

/// <summary>
/// Only meant as a type-constraint for generic methods!
/// Do not access structs that implement this interface by interface to avoid boxing!
/// </summary>
public interface IReadableDatagram<TSelf> where TSelf : IReadableDatagram<TSelf>
{

    static abstract void ReadFrom(ReadOnlySpan<byte> source, ref TSelf target);

    static abstract int Size { get; }

}