using System;
using System.Drawing;

namespace LedCube.Core.Common.Config
{
    public record CubeDimensions
    {
        public int X { get; set; } = 8;
        public int Y { get; set; } = 8;
        public int Z { get; set; } = 8;
    }

    public enum CartesianOrientation
    {
        RightHanded,
        LeftHanded
    }

    public enum LedShape
    {
        Led,
        Sphere,
        Zylinder
    }

    public record LedType
    {
        public int LedDimensions { get; set; }
        public LedShape Shape { get; set; }
        public Color Tint { get; set; }
    }

    public record Cube3DDrawingConfig()
    {
        public LedType LedType { get; set; } = new();
        public bool DrawWireframe { get; set; }
    }

    public record CubeStreamerSettings()
    {
        public int Port { get; set; } = 4242;
        public string Hostname { get; set; } = string.Empty;
        public bool SearchPerBroadcast { get; set; } = true;
        public CubeDataProjectionSettings Projection = new();
    }

    public record CubeDataProjectionSettings
    {
        public CartesianOrientation Orientation { get; set; }
    }
        
    public record CubeConfig()
    {
        public string Name { get; set; } = string.Empty;
        public CubeDimensions Dimensions { get; set; } = new();

        public CubeStreamerSettings StreamerSettings { get; set; } = new();

        public Cube3DDrawingConfig Cube3DDrawingConfig { get; set; } = new();
    }
}