using System;
using System.IO;
using System.Reflection;

namespace LedCube.Core.UI.CubeView3D.Mesh;

/// <summary>
/// Loads the embedded LED model used for cube instancing. The raw Led5mmRGB model is a
/// real-world through-hole LED whose legs run to local-z -24.5 (vs. an xy body radius of ~3);
/// we clip the legs and normalize the remaining body/dome to a unit size so it instances cleanly.
/// Tweak <see cref="LedOptions"/> if you swap the mesh.
/// </summary>
public static class LedMeshProvider
{
    public const string ResourceName = "LedCube.Core.UI.CubeView3D.Assets.Led5mmRGB.obj";

    /// <summary>Clip legs (anything below local-z -4) and normalize the body/dome to a unit cube.</summary>
    public static ObjLoadOptions LedOptions { get; } = new(ClipMinLocalZ: -4f, Recenter: true, NormalizeToUnit: true);

    public static ObjMesh LoadLedMesh()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded LED mesh '{ResourceName}' not found.");
        return ObjMeshLoader.Load(stream, LedOptions);
    }
}
