namespace LedCube.Core.Common.Config;

public readonly record struct CubeDimensions()
{
    public int X { get; init; } = 8;
    public int Y { get; init; } = 8;
    public int Z { get; init; } = 8;
}