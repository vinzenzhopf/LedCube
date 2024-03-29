﻿namespace LedCube.Core.Common.Config.Config;

public record CubeConfig()
{
    public string Name { get; set; } = string.Empty;
    public CubeDimensions Dimensions { get; set; } = new();

    public CubeStreamerSettings StreamerSettings { get; set; } = new();

    public Cube3DDrawingConfig Cube3DDrawingConfig { get; set; } = new();
}