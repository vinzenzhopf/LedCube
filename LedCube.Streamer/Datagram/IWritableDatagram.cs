using System;

namespace LedCube.Streamer.Datagram;

/// <summary>
/// Only meant as a type-constraint for generic methods!
/// Do not access structs that implement this interface by interface to avoid boxing!
/// </summary>
public interface IWritableDatagram<TSelf> where TSelf : IWritableDatagram<TSelf>
{
    static abstract void WriteTo(Span<byte> target, in TSelf source);

    static abstract int Size { get; }
}