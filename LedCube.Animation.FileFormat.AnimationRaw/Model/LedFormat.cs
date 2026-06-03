namespace LedCube.Animation.FileFormat.AnimationRaw.Model;

/// <summary>
/// Per-LED encoding of a baked frame's payload bytes.
/// </summary>
public enum LedFormat
{
    /// <summary>1 bit per LED, LSB-first bit-packed (matches <c>CubeData.Serialize</c>).</summary>
    Binary,

    /// <summary>1 byte per LED, 0 = off .. 255 = full, linear light intensity.</summary>
    Grayscale,

    /// <summary>3 bytes per LED in R, G, B order, linear light per channel.</summary>
    Rgb,
}
