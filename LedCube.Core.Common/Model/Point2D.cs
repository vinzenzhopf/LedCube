using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace LedCube.Core.Common.Model;

public readonly struct Point2D : IEquatable<Point2D>
{
    public static readonly Point2D Empty = default;

    public readonly int X;
    public readonly int Y;
    
    public Point2D(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
    
    [Browsable(false)]
    public bool IsEmpty => X is 0 && Y is 0;
    public ReadOnlySpan<int> AsSpan() => MemoryMarshal.CreateReadOnlySpan(in X, 3);
    
    public static explicit operator Point2D(Vector2 vector) => new Point2D((int) vector.X, (int) vector.Y);
    public static implicit operator Vector2(Point2D point) => new Vector2(point.X, point.Y);
    
    public static Point2D operator +(Point2D pt, Point2D sz) => Add(pt, sz);
    
    public static Point2D operator -(Point2D pt, Point2D sz) => Subtract(pt, sz);
    
    public static bool operator ==(Point2D left, Point2D right) => left.X == right.X && left.Y == right.Y;

    public static bool operator !=(Point2D left, Point2D right) => !(left == right);
    
    public static Point2D Add(Point2D pt, Point2D sz) => new Point2D(unchecked(pt.X + sz.X), unchecked(pt.Y + sz.Y));
    public static Point2D Subtract(Point2D pt, Point2D sz) => new Point2D(unchecked(pt.X - sz.X), unchecked(pt.Y - sz.Y));

    public readonly override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Point2D point && Equals(point);
    }

    public readonly bool Equals(Point2D other) => this == other;

    public readonly override int GetHashCode() => HashCode.Combine(X, Y);

    public readonly override string ToString() => $"{{X={X},Y={Y}}}";
}