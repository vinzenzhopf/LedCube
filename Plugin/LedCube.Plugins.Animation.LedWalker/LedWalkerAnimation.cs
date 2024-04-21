using System;
using System.Collections.Generic;
using LedCube.Core.Common.CubeData.Generator;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.LedWalker;

public class LedWalkerAnimation(IConfiguration configuration, ILogger<LedWalkerAnimation> logger)
    : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("Led Walker Animation", "Walks one led through the cube.");
    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(10);
    
    private IEnumerator<Point3D>? _activeLedPos;
    private float _lastMove;
    private float _walkingSpeedMs = 1000.0f / 256; // 1 sec per Plane
    
    public void Start(AnimationContext animationContext)
    { 
        _lastMove = (float) animationContext.ElapsedTimeUs / 1000.0f;
        animationContext.CubeData.Clear();
        _activeLedPos?.Dispose();
        _activeLedPos = new PositionGenerator3D(animationContext.CubeData.Size, true).GetEnumerator();
    }

    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (float) frameContext.ElapsedTimeUs / 1_000;  
        var lastMoveDiff = elapsedTimeMs - _lastMove;
        if (!(lastMoveDiff > _walkingSpeedMs))
        {
            return;
        }

        if (_activeLedPos?.MoveNext() is true)
        {
            frameContext.Buffer.SetLed(_activeLedPos.Current, !frameContext.Buffer.GetLed(_activeLedPos.Current));
            _lastMove = elapsedTimeMs;
        }
    }

    public void End(AnimationContext animationContext)
    {
        _activeLedPos?.Dispose();
        _activeLedPos = null;
    }
}