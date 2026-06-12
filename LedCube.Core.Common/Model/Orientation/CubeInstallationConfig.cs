using LedCube.Core.Common.Config.Entities;

namespace LedCube.Core.Common.Model.Orientation;

/// <summary>
/// The two-part cube orientation config, resolved into a single <see cref="CubeOrientation"/>.
///
/// All animations are authored in the <b>canonical</b> frame: right-handed, LED 0 at front-bottom-left
/// (+X right, +Y up, +Z back). The default config (Front / RightHanded / Front) is the identity, i.e.
/// the canonical frame itself. The two parts describe how a physical cube deviates from canonical:
/// <list type="bullet">
/// <item><b>Hardware</b> — intrinsic wiring: which face holds the logical front, plus chirality
/// (left-handed = a mirror of the canonical right-handed frame). Set once.</item>
/// <item><b>Installation</b> — how the cube currently sits (which face you turn toward the front).
/// Changed freely; it just rotates the whole presentation.</item>
/// </list>
/// The resolved transform is <c>Installation ∘ Hardware</c>, applied to the streamed output (and the
/// preview's "show as installed" mode) — never to the canonical authoring space.
/// </summary>
public sealed record CubeInstallationConfig
{
    public Orientation3D HardwareFront { get; init; } = Orientation3D.Front;
    public CartesianOrientation HardwareHandedness { get; init; } = CartesianOrientation.RightHanded;
    public Orientation3D InstallationFront { get; init; } = Orientation3D.Front;

    public static CubeInstallationConfig Default => new();

    public CubeOrientation Resolve()
    {
        var hardware = OrientationPresets.ApplyHandedness(
            OrientationPresets.BringFaceToFront(HardwareFront), HardwareHandedness);
        var installation = OrientationPresets.BringFaceToFront(InstallationFront);
        return CubeOrientation.Compose(hardware, installation);
    }
}
