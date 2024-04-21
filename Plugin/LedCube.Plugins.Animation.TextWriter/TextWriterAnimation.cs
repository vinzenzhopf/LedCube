using System;
using System.Numerics;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.TextWriter;

public class TextWriterAnimation(IConfiguration configuration, ILogger<TextWriterAnimation> logger)
    : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("Text Writer", "Animation to Write Text to the Cube.");
    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(10);
    
    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
    }

    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (float) frameContext.ElapsedTimeUs / 1_000;  
    }
    
    public void Dispose(){}
}