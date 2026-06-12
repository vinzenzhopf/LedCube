using LedCube.Core.Common.Config.Entities;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Orientation;

namespace LedCube.Core.Common.Config;

public record CubeDataProjectionSettings
{
    /// <summary>Hardware chirality (handedness). Reused as the hardware mirror flag.</summary>
    public CartesianOrientation Orientation { get; set; }

    /// <summary>Which physical face holds the cube's logical front (intrinsic wiring). Set once.</summary>
    public Orientation3D HardwareFront { get; set; } = Orientation3D.Front;

    /// <summary>Which face currently points to the viewer (how the cube is installed). Changed freely.</summary>
    public Orientation3D InstallationFront { get; set; } = Orientation3D.Front;

    /// <summary>
    /// The resolved orientation config. Defaults (Front / Front / RightHanded) resolve to identity,
    /// so streaming output is unchanged until these are configured.
    /// </summary>
    public CubeInstallationConfig ToConfig() => new()
    {
        HardwareFront = HardwareFront,
        HardwareHandedness = Orientation,
        InstallationFront = InstallationFront
    };
}
