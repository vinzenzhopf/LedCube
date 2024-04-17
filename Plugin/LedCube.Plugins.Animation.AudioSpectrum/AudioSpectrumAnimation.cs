using System;
using System.Numerics;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.AudioSpectrum;

public class AudioSpectrumAnimation(IConfiguration configuration, ILogger<AudioSpectrumAnimation> logger)
    : FrameGeneratorBase
{
    public new static FrameGeneratorInfo Info => new("Audio Spectrum", "Audio Spectrum Visualizer");

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