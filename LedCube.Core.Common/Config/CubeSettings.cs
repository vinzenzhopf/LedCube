using LedCube.Core.Common.Config.Entities;

namespace LedCube.Core.Common.Config;

public record CubeSettings
{
    public string Name { get; init; } = string.Empty;
    public CubeDimensions Dimensions { get; init; } = new();
}
