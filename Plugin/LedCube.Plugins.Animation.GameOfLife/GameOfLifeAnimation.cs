using System;
using LedCube.Core.Common.Model.Cube;
using LedCube.PluginBase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LedCube.Plugins.Animation.GameOfLife;

public class GameOfLifeAnimation(IOptions<GameOfLifeConfiguration> options, ILogger<GameOfLifeAnimation> logger) : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("CellularAutomata Animation", "Game of Life.");

    private readonly GameOfLifeConfiguration _configuration = options.Value;
    private readonly ILogger<GameOfLifeAnimation> _logger = logger;

    public TimeSpan? FrameTime { get; } = null;
    private double _lastMove;
    private GeneratorCubeConfiguration? _config = null;
    private Random _random = Random.Shared;

    public void Initialize(GeneratorCubeConfiguration config)
    {
        _config = config;
    }

    public void Start(AnimationContext animationContext)
    {
        _random = new Random(_configuration.Seed);
        _lastMove = animationContext.ElapsedTimeUs / 1000;
        animationContext.CubeData.ForEach((_, _) => _random.NextDouble() < _configuration.InitialFill);
    }

    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (double)frameContext.ElapsedTimeUs / 1000;
        var lastMoveDiff = elapsedTimeMs - _lastMove;
        if (!(lastMoveDiff > _configuration.FrameTime.TotalMilliseconds))
        {
            return;
        }

        RunCellularAutomata(frameContext.Buffer);
        _lastMove = elapsedTimeMs;
        _logger.LogInformation("New Frame at: {0}ms", elapsedTimeMs);
    }

    private void RunCellularAutomata(ICubeData frameContextBuffer)
    {
        var currentFrame = new bool[frameContextBuffer.Size.Z, frameContextBuffer.Size.Y, frameContextBuffer.Size.X];
        var nextFrame = new bool[frameContextBuffer.Size.Z, frameContextBuffer.Size.Y, frameContextBuffer.Size.X];
        frameContextBuffer.ForEach((pos, value) => currentFrame[pos.Z, pos.Y, pos.X] = value);
        RunCellularAutomata(currentFrame, nextFrame);
        frameContextBuffer.ForEach((pos, _) => nextFrame[pos.Z, pos.Y, pos.X]);
    }

    private void RunCellularAutomata(bool[,,] currentFrameBuffer, bool[,,] nextFrameBuffer)
    {
        var lZ = currentFrameBuffer.GetLength(0);
        var lY = currentFrameBuffer.GetLength(1);
        var lX = currentFrameBuffer.GetLength(2);
        for (var z = 0; z < lZ; z++)
        {
            for (var y = 0; y < lY; y++)
            {
                for (var x = 0; x < lX; x++)
                {
                    var neighbors = 0;
                    for (var dZ = -1; dZ <= 1; dZ++)
                    {
                        var cZ = z + dZ;
                        if (cZ < 0 || cZ >= lZ)
                        {
                            if (_configuration.EdgeBehaviour is EdgeBehaviour.AssumeEmpty)
                            {
                                continue;
                            }

                            cZ %= lZ;
                            if (cZ < 0)
                            {
                                cZ += lZ;
                            }
                        }
                        for (var dY = -1; dY <= 1; dY++)
                        {
                            var cY = y + dY;
                            if (cY < 0 || cY >= lY)
                            {
                                if (_configuration.EdgeBehaviour is EdgeBehaviour.AssumeEmpty)
                                {
                                    continue;
                                }

                                cY %= lY;
                                if (cY < 0)
                                {
                                    cY += lY;
                                }
                            }
                            for (var dX = -1; dX <= 1; dX++)
                            {

                                if (dZ is 0 && dY is 0 && dX is 0)
                                {
                                    continue;
                                }
                                
                                var cX = x + dX;
                                if (cX < 0 || cX >= lX)
                                {
                                    if (_configuration.EdgeBehaviour is EdgeBehaviour.AssumeEmpty)
                                    {
                                        continue;
                                    }

                                    cX %= lX;
                                    if (cX < 0)
                                    {
                                        cX += lX;
                                    }
                                }

                                if (currentFrameBuffer[cZ, cY, cX])
                                {
                                    neighbors++;
                                }
                            }
                        }
                    }

                    if (!currentFrameBuffer[z, y, x] && neighbors is 4)
                    {
                        nextFrameBuffer[z, y, x] = true;
                    }
                    else if (currentFrameBuffer[z,y,x] && neighbors is 5 or 6)
                    {
                        nextFrameBuffer[z, y, x] = true;
                    }
                    else
                    {
                        nextFrameBuffer[z, y, x] = false;
                    }
                }
            }
        }
    }

    public void End(AnimationContext animationContext)
    {
    }

    public void Pause(AnimationContext animationContext)
    {
        // throw new NotImplementedException();
    }

    public void Continue(AnimationContext animationContext)
    {
        // throw new NotImplementedException();
    }

    public void ChangeTime(AnimationContext animationContext)
    {
        // throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}