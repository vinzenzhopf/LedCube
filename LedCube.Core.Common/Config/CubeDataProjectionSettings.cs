using LedCube.Core.Common.Config.Entities;

namespace LedCube.Core.Common.Config;

public record CubeDataProjectionSettings
{
    public CartesianOrientation Orientation { get; set; }
}