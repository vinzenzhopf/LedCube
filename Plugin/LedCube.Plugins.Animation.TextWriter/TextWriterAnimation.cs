using System;
using System.Numerics;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.TextWriter;

public class TextWriterAnimation(IConfiguration configuration, ILogger<TextWriterAnimation> logger)
    : FrameGeneratorBase
{
    public new static FrameGeneratorInfo Info => new("Text Writer", "Animation to Write Text to the Cube.");
    public override TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(10);
    
    public override void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
    }

    public override void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (float) frameContext.ElapsedTimeUs / 1_000;  
    }
}