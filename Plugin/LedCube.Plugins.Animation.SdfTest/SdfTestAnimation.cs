using System.Numerics;
using System.Timers;
using LedCube.Core.Common.CubeData.Generator;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.SdfTest;

public class LedWalkerAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("SDF Test Animation", "SDF Test.");
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<LedWalkerAnimation> _logger;

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(20);
    
    private GeneratorCubeConfiguration? _config = null;
    private Sdf3D? _sdf;

    public LedWalkerAnimation(IConfiguration configuration, ILogger<LedWalkerAnimation> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Initialize(GeneratorCubeConfiguration config)
    {
        _config = config;
    }

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
        SetupSdf();
    }

    private void SetupSdf()
    {
        var box = Sdf.Core.Sdf.BoxFrame(new Vector3(8, 8, 8), 1);
        var sphere = Sdf.Core.Sdf.Sphere(4);

        _sdf = Sdf.Core.Sdf.Union(sphere, box);//RotateTime(box));
        return;

        Sdf3D RotateTime(Sdf3D sdf)
        {
            return (position, time) => sdf(
                Vector3.Transform(position, 
                    Quaternion.Inverse(new Quaternion(Vector3.Zero, (time / 1000)%1.0f))), time);
        }
    }
    
    public static Sdf3D Rotate(Sdf3D sdf, Quaternion rotation)
    {
        var inverseRotation = Quaternion.Inverse(rotation);
        return (position, time) => sdf(Vector3.Transform(position, inverseRotation), time);
    }

    public void DrawFrame(FrameContext frameContext)
    {
        if (_sdf is null) return;
        var elapsedTimeS = (float) frameContext.ElapsedTimeUs / 1000000;  
        frameContext.Buffer.Render(_sdf, elapsedTimeS, new SdfRenderOptions(){Centered = true, Margin = 0.49f});
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