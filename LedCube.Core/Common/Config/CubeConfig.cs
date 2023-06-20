using System;
using System.Drawing;

namespace LedCube.Core.Common.Config
{
    public record CubeDimensions
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public EuclideanOrientation Orientation { get; set; }
    }

    public enum EuclideanOrientation
    {
        RightHanded,
        LeftHanded
    }

    public record LedType
    {
        public int LedDimensions { get; set; }
        public LedShape Shape { get; set; }
        public Color Tint { get; set; }
    }

    public enum LedShape
    {
        Led,
        Sphere,
        Zylinder
    }
    
    public record CubeConfig()
    {
        public string Name { get; set; } = string.Empty;
        public CubeDimensions Dimensions { get; set; } = new();
        public LedType LedType { get; set; } = new();
        public bool DrawWireframe { get; set; }
    }
}