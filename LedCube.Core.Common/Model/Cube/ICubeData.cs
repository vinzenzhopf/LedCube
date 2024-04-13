using System;
using LedCube.Core.Common.Model.Cube.Event;

namespace LedCube.Core.Common.Model.Cube;

public interface ICubeData
{
    Point3D Size { get; }
    int Length { get; }
    bool GetLed(Point3D p);
    void SetLed(Point3D p, bool value);
    void Clear();
    event CubeChangedEventHandler? CubeChanged;
    event LedChangedEventHandler<Point3D>? LedChanged;
    void Serialize(Span<byte> target);
}