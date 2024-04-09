using System;
using System.Numerics;

namespace LedCube.Sdf.Core;

public static class Sdf
{
    public static Sdf3D Sphere(float radius)
        => (position, _) => position.Length() - radius;

    public static Sdf3D Void() => (_, _) => 1E6F;

    public static Sdf3D Box(Vector3 dimensions)
        => (position, _) =>
        {
            var q = Vector3.Abs(position) - dimensions;
            return Vector3.Max(q, Vector3.Zero).Length() + MathF.Min(MathF.Max(q.X, MathF.Max(q.Y, q.Z)), 0);
        };

    public static Sdf3D BoxFrame(Vector3 dimensions, float thickness)
        => (position, _) =>
        {
            position = Vector3.Abs(position) - dimensions;
            var q = Vector3.Abs(position + new Vector3(thickness)) - new Vector3(thickness);
            return MathF.Min(MathF.Min(
                    Vector3.Max(new(position.X, q.Y, q.Z), Vector3.Zero).Length() + MathF.Min(MathF.Max(position.X, MathF.Max(q.Y, q.Z)), 0),
                    Vector3.Max(new(q.X, position.Y, q.Z), Vector3.Zero).Length() + MathF.Min(MathF.Max(q.X, MathF.Max(position.Y, q.Z)), 0)),
                Vector3.Max(new(q.X, q.Y, position.Z), Vector3.Zero).Length() + MathF.Min(MathF.Max(q.X, MathF.Max(q.Y, position.Z)), 0));
        };

    public static Sdf3D Torus(float radiusMajor, float radiusMinor)
        => (position, _) =>
        {
            var q = new Vector2(new Vector2(position.X, position.Z).Length() - radiusMajor, position.Y);
            return q.Length() - radiusMinor;
        };

    public static Sdf3D Octahedron(float size)
        => (position, _) =>
        {
            const float sqrt3Over3 = 0.57735027f;
            position = Vector3.Abs(position);
            return (position.X + position.Y + position.Z - size) * sqrt3Over3;
        };

    public static Sdf3D Plane(Vector3 normal, float height)
    {
        normal = Vector3.Normalize(normal);
        return (position, _) => Vector3.Dot(position, normal) + height;
    }

    public static Sdf3D Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        => (p, _) =>
        {
            var ba = b - a;
            var pa = p - a;
            var cb = c - b;
            var pb = p - b;
            var dc = d - c;
            var pc = p - c;
            var ad = a - d;
            var pd = p - d;
            var nor = Vector3.Cross(ba, ad);

            var eta = MathF.Sign(Vector3.Dot(Vector3.Cross(ba, nor), pa)) +
                      MathF.Sign(Vector3.Dot(Vector3.Cross(cb, nor), pb)) +
                      MathF.Sign(Vector3.Dot(Vector3.Cross(dc, nor), pc)) +
                      MathF.Sign(Vector3.Dot(Vector3.Cross(ad, nor), pd));

            return MathF.Sqrt((eta < 3.0)
                    ? MathF.Min(MathF.Min(MathF.Min(
                                Dot2(ba * Math.Clamp(Vector3.Dot(ba, pa) / Dot2(ba), 0, 1) - pa),
                                Dot2(cb * Math.Clamp(Vector3.Dot(cb, pb) / Dot2(cb), 0, 1) - pb)),
                            Dot2(dc * Math.Clamp(Vector3.Dot(dc, pc) / Dot2(dc), 0, 1) - pc)),
                        Dot2(ad * Math.Clamp(Vector3.Dot(ad, pd) / Dot2(ad), 0, 1) - pd))
                    : Vector3.Dot(nor, pa) * Vector3.Dot(nor, pa) / Vector3.Dot(nor, nor));
        };

    private static float Dot2(Vector3 v) => Vector3.Dot(v, v);

    // Revolute around Y
    public static Sdf3D Revolution(Sdf2D sdf, float offset)
        => (position, time) => sdf(new(new Vector2(position.X, position.Z).Length() - offset, position.Y), time);

    public static Sdf3D Extrusion(Sdf2D sdf, float depth)
        => (position, time) =>
        {
            var d = sdf(new Vector2(position.X, position.Y), time);
            var w = new Vector2(d, MathF.Abs(position.Z) - depth);
            return MathF.Min(MathF.Max(w.X, w.Y), 0) + Vector2.Max(w, Vector2.Zero).Length();
        };

    public static Sdf3D Elongation(Sdf3D sdf, Vector3 offset)
        => (position, time) =>
        {
            var q = Vector3.Abs(position) - offset;
            return sdf(Vector3.Max(q, Vector3.Zero), time) + MathF.Min(MathF.Max(q.X, MathF.Max(q.Y, q.Z)), 0);
        };

    public static Sdf3D Union(Sdf3D a, Sdf3D b)
        => (position, time) => MathF.Min(a(position, time), b(position, time));

    public static Sdf3D Subtraction(Sdf3D a, Sdf3D b)
        => (position, time) => MathF.Max(-a(position, time), b(position, time));

    public static Sdf3D Intersection(Sdf3D a, Sdf3D b)
        => (position, time) => MathF.Max(a(position, time), b(position, time));

    public static Sdf3D Xor(Sdf3D a, Sdf3D b)
        => (position, time) =>
        {
            var d1 = a(position, time);
            var d2 = b(position, time);
            return MathF.Max(MathF.Min(d2, d2), -MathF.Max(d1, d2));
        };

    public static Sdf3D Translate(Sdf3D sdf, Vector3 offset)
        => (position, time) => sdf(position - offset, time);

    public static Sdf3D Rotate(Sdf3D sdf, Quaternion rotation)
    {
        var inverseRotation = Quaternion.Inverse(rotation);
        return (position, time) => sdf(Vector3.Transform(position, inverseRotation), time);
    }

    public static Sdf3D Scale(Sdf3D sdf, float scale)
        => (position, time) => sdf(position / scale, time) * scale;
}