using System;

namespace LedCube.Plugins.Animation.GameOfLife;

public class GameOfLifeConfiguration
{

    public const string SectionName = "GameOfLife";
    
    public TimeSpan FrameTime { get; init; } = TimeSpan.FromSeconds(2);
    
    public int Seed { get; init; }

    public double InitialFill { get; init; } = 0.35;

    public EdgeBehaviour EdgeBehaviour { get; init; } = EdgeBehaviour.TorusRollOver;

}