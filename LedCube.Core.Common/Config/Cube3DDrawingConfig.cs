using LedCube.Core.Common.Config.Entities;

namespace LedCube.Core.Common.Config;

public record Cube3DDrawingConfig()
{
    public LedType LedType { get; set; } = new();
    public bool DrawWireframe { get; set; }
}