namespace LedCube.Core.Config;

public record Cube3DDrawingConfig()
{
    public LedType LedType { get; set; } = new();
    public bool DrawWireframe { get; set; }
}