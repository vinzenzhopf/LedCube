using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube.Buffer;

namespace LedCube.Test.WeirdBuffers;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[InlineArray(DimensionZ*DimensionY*DimensionX)]
public struct CubeDataBuffer4x100x8 : ICubeDataBuffer<CubeDataBuffer4x100x8>
{
    private bool _element0;
    
    private const int DimensionX = 4;
    private const int DimensionY = 100;
    private const int DimensionZ = 8;
    public static Span<bool> GetBuffer(ref CubeDataBuffer4x100x8 self) => self;
    public static ReadOnlySpan<bool> GetReadOnlyBuffer(in CubeDataBuffer4x100x8 self) => self;
    public static int Length => DimensionZ * DimensionY * DimensionX;
    public static int CoordinatesToIndex(Point3D p) => 
        p.X + 
        p.Y * DimensionX + 
        p.Z * DimensionX * DimensionY;

    public static Point3D IndexToCoordinates(int index) => new( 
        index % DimensionX,
        (index / DimensionX) % DimensionY,
        (index / (DimensionX * DimensionY)) % DimensionZ
    );

    public static Point3D Size => new(DimensionX, DimensionY, DimensionZ);
}