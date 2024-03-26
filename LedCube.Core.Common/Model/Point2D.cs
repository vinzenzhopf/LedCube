using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace LedCube.Core.Common.Model;

public readonly struct Point2D : IEquatable<Point2D>
{
    public static readonly Point2D Empty = new();

    public int X { get; }

    public int Y { get; }
    
    public Point2D(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
    
    public Point2D(Point2D sz)
    {
        X = sz.X;
        Y = sz.Y;
    }
    
    [Browsable(false)]
    public readonly bool IsEmpty => X == 0 && Y == 0;

    public static Point2D operator +(Point2D pt, Point2D sz) => Add(pt, sz);
    
    public static Point2D operator -(Point2D pt, Point2D sz) => Subtract(pt, sz);
    
    public static bool operator ==(Point2D left, Point2D right) => left.X == right.X && left.Y == right.Y;

    public static bool operator !=(Point2D left, Point2D right) => !(left == right);
    
    public static Point2D Add(Point2D pt, Point2D sz) => new Point2D(unchecked(pt.X + sz.X), unchecked(pt.Y + sz.Y));
    public static Point2D Subtract(Point2D pt, Point2D sz) => new Point2D(unchecked(pt.X - sz.X), unchecked(pt.Y - sz.Y));

    public readonly override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Point2D && Equals((Point2D) obj);
    }

    public readonly bool Equals(Point2D other) => this == other;

    public readonly override int GetHashCode() => HashCode.Combine(X, Y);

    public readonly override string ToString() => $"{{X={X},Y={Y}}}";
}