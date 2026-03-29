namespace LedCube.Core.Common.Config.Entities;

public readonly record struct CubeDimensions(int X, int Y, int Z)
{
    public CubeDimensions() : this(8, 8, 8)
    {
    }
}