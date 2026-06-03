using System;

namespace LedCube.Animation.FileFormat.AnimationRaw.Model;

/// <summary>
/// One entry in the frame pool: the raw, opaque payload bytes for a single rendered frame.
/// The byte layout is governed by the animation's <see cref="LedFormat"/> and cube size;
/// interpreting individual LEDs is a downstream (rendering) concern. Equality is by content.
/// </summary>
public sealed class Frame : IEquatable<Frame>
{
    public ReadOnlyMemory<byte> Data { get; }

    public Frame(ReadOnlyMemory<byte> data)
    {
        Data = data;
    }

    public bool Equals(Frame? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || Data.Span.SequenceEqual(other.Data.Span);
    }

    public override bool Equals(object? obj) => Equals(obj as Frame);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(Data.Span);
        return hash.ToHashCode();
    }

    public override string ToString() => $"Frame({Data.Length} bytes)";
}
