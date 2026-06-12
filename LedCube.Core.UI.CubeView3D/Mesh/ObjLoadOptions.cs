namespace LedCube.Core.UI.CubeView3D.Mesh;

/// <summary>
/// Post-processing options applied while loading an OBJ. The exported LED models are
/// real-world-proportioned through-hole LEDs with very long legs, so clipping and
/// normalization let us instance just the body/dome at a sensible size.
/// </summary>
public readonly record struct ObjLoadOptions(
    float? ClipMinLocalZ = null,
    bool Recenter = true,
    bool NormalizeToUnit = true)
{
    public static ObjLoadOptions Default => new();
}
