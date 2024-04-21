using System;
using System.Numerics;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.SdfTest;

public class SdfTestAnimation(IConfiguration configuration, ILogger<SdfTestAnimation> logger)
    : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("SDF Test Animation", "SDF Test.");
    
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<SdfTestAnimation> _logger = logger;

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(20);
    
    private Sdf3D _sdf = Sdf.Core.Sdf.Void();

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
        _sdf = SetupSdf();
    }

    private Sdf3D SetupSdf()
    {
        var box = Sdf.Core.Sdf.BoxFrame(new Vector3(5, 5, 5), 0.25f);
        var sphere = Sdf.Core.Sdf.Sphere(4);
        var boxDriver = Driver.ConstantAngularVelotcity(box, Vector3.UnitZ,MathF.Tau / 4);
        
        return Sdf.Core.Sdf.Union(sphere, boxDriver);
    }
    

    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeS = (float) frameContext.ElapsedTimeUs / 1_000_000;  
        frameContext.Buffer.Render(_sdf, elapsedTimeS, new SdfRenderOptions{Centered = true, Margin = 0.49f});
    }
}