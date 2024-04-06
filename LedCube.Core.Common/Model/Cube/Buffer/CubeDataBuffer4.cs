using System.Runtime.CompilerServices;

namespace LedCube.Core.Common.Model.Cube.Buffer;

[InlineArray(Dimension*Dimension*Dimension)]
public struct CubeDataBuffer4 : ICubeDataBuffer<CubeDataBuffer4>
{
    private bool _element0;
    
    private const int Dimension = 4;
    public static Span<bool> GetBuffer(ref CubeDataBuffer4 self) => self;
    public static ReadOnlySpan<bool> GetReadOnlyBuffer(in CubeDataBuffer4 self) => self;
    public static int Length => Dimension * Dimension * Dimension;
    public static int CoordinatesToIndex(Point3D p) => 
        p.X + 
        p.Y * Dimension + 
        p.Z * Dimension * Dimension;

    public static Point3D IndexToCoordinates(int index) => new( 
        index % Dimension,
        (index / Dimension) % Dimension,
        (index / (Dimension * Dimension)) % Dimension
    );

    public static Point3D Size => new(Dimension, Dimension, Dimension);
}