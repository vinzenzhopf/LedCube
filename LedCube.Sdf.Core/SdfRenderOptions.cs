namespace LedCube.Sdf.Core;

public readonly record struct SdfRenderOptions
{
    public bool Centered { get; init; } 

    public float Margin { get; init; } 
}