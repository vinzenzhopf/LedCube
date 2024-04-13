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
     
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<TextWriterAnimation> _logger = logger;

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(10);
    
    private GeneratorCubeConfiguration? _config = null;

    public void Initialize(GeneratorCubeConfiguration config)
    {
        _config = config;
    }

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
    }

    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (float) frameContext.ElapsedTimeUs / 1_000;  
    }

    public void End(AnimationContext animationContext)
    {
    }

    public void Pause(AnimationContext animationContext)
    {
    }

    public void Continue(AnimationContext animationContext)
    {
    }

    public void ChangeTime(AnimationContext animationContext)
    {
    }

    public void Dispose()
    {
    }
}