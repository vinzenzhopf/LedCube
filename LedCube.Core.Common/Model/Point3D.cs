using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace LedCube.Core.Common.Model;

public readonly struct Point3D : IEquatable<Point3D>
{
    public static readonly Point3D Empty = default;

    public readonly int X;
    public readonly int Y;
    public readonly int Z;
    
    public Point3D(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    [Browsable(false)]
    public bool IsEmpty => X is 0 && Y is 0 && Z is 0;
    public ReadOnlySpan<int> AsSpan() => MemoryMarshal.CreateReadOnlySpan(in X, 3);

    /// <summary>
    /// Returns the Product of all the points Elements.
    /// </summary>
    public int ElementProduct => X * Y * Z;

    public static explicit operator Point3D(Vector3 vector) => new Point3D((int) vector.X, (int) vector.Y, (int) vector.Z);
    public static implicit operator Vector3(Point3D point) => new Vector3(point.X, point.Y, point.Z);
    
    public static Point3D operator +(Point3D pt, Point3D sz) => Add(pt, sz);
    public static Point3D operator +(Point3D pt, int sz) => Add(pt, sz);
    
    public static Point3D operator -(Point3D pt, Point3D sz) => Subtract(pt, sz);
    
    public static Point3D operator -(Point3D pt, int sz) => Subtract(pt, sz);
    
    public static bool operator ==(Point3D left, Point3D right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;

    public static bool operator !=(Point3D left, Point3D right) => !(left == right);

    public static bool operator >=(Point3D left, Point3D right) =>
        left.X >= right.X || left.Y >= right.Y || left.Z >= right.Z; 
    
    public static bool operator <=(Point3D left, Point3D right) =>
        left.X <= right.X && left.Y <= right.Y && left.Z <= right.Z;
    
    public static bool operator >(Point3D left, Point3D right) =>
        left.X > right.X || left.Y > right.Y || left.Z > right.Z; 
    
    public static bool operator <(Point3D left, Point3D right) =>
        left.X < right.X && left.Y < right.Y && left.Z < right.Z;

    /// <summary>
    /// Checks if the given point p lays within the points min (including) and max (excluding).
    /// </summary>
    /// <param name="p">Point p to check for</param>
    /// <param name="min">Minimum bounds (including)</param>
    /// <param name="max">Maximum bounds (excluding)</param>
    /// <returns></returns>
    public static bool CheckBounds(Point3D p, Point3D min, Point3D max) =>
        p.X >= min.X && p.X < max.X &&
        p.Y >= min.Y && p.Y < max.Y &&
        p.Z >= min.Z && p.Z < max.Z;

    public static Point3D Add(Point3D pt, Point3D sz) => new Point3D(unchecked(pt.X + sz.X), unchecked(pt.Y + sz.Y), unchecked(pt.Z + sz.Z));
    public static Point3D Add(Point3D pt, int sz) => new Point3D(unchecked(pt.X + sz), unchecked(pt.Y + sz), unchecked(pt.Z + sz));
    public static Point3D Subtract(Point3D pt, Point3D sz) => new Point3D(unchecked(pt.X - sz.X), unchecked(pt.Y - sz.Y), unchecked(pt.Z - sz.Z));
    public static Point3D Subtract(Point3D pt, int sz) => new Point3D(unchecked(pt.X - sz), unchecked(pt.Y - sz), unchecked(pt.Z - sz));
    public static Point3D CoordinateFromIndex(Point3D size, int index) => new(
        index % size.X, (index / size.X) % size.Y, (index / (size.X * size.Y)) % size.Z
    );
    
    public static int IndexFromCoordinate(Point3D size, Point3D p) => 
        p.X + p.Y * size.X + p.Z * size.X * size.Y;
    
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Point3D point && Equals(point);
    }

    public bool Equals(Point3D other) => this == other;

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override string ToString() => $"{{X={X},Y={Y},Z={Z}}}";
    
}
