namespace LedCube.Core.Common.Config;

public record CubeDimensions
{
    public int X { get; set; } = 8;
    public int Y { get; set; } = 8;
    public int Z { get; set; } = 8;
}