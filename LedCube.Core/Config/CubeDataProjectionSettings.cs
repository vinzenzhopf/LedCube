using LedCube.Core.Common.Config;

namespace LedCube.Core.Config;

public record CubeDataProjectionSettings
{
    public CartesianOrientation Orientation { get; set; }
}