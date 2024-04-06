namespace LedCube.Core.Common.Model.Cube.Buffer;

public interface ICubeDataBuffer<TSelf> where TSelf : struct, ICubeDataBuffer<TSelf>
{
    static abstract Span<bool> GetBuffer(ref TSelf self);
    static abstract ReadOnlySpan<bool> GetReadOnlyBuffer(in TSelf self);
    static abstract int Length { get; }
    static abstract Point3D Size { get; }
    static abstract int CoordinatesToIndex(Point3D p);
    static abstract Point3D IndexToCoordinates(int index);
}