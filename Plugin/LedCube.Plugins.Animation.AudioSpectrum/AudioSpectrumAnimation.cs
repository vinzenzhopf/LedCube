using System;
using System.Numerics;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.AudioSpectrum;

public class AudioSpectrumAnimation(IConfiguration configuration, ILogger<AudioSpectrumAnimation> logger)
    : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("Audio Spectrum", "Audio Spectrum Visualizer");

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(10);

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
    }

    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (float) frameContext.ElapsedTimeUs / 1_000;  
    }
}