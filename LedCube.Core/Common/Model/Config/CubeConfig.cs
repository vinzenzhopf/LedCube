using System;
using System.Drawing;

namespace LedCube.Core.Common.Model.Config
{
    public class CubeDimensions
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class LedType
    {
        public Type SystemType { get; set; }
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
    
    public class CubeConfig
    {
        public string Name { get; set; }
        public CubeDimensions Dimensions { get; set; }
        public Type LedType { get; set; }
        public bool DrawWireframe { get; set; }
    }
}