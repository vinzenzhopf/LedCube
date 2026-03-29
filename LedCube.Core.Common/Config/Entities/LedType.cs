using System.Drawing;

namespace LedCube.Core.Common.Config.Entities;

public record LedType
{
    public int LedDimensions { get; set; }
    public LedShape Shape { get; set; }
    public Color Tint { get; set; }
}