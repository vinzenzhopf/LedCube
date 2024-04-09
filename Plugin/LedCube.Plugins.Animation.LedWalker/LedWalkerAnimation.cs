using LedCube.Core.Common.CubeData.Generator;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.LedWalker;

public class LedWalkerAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("Led Walker Animation", "Walks one led through the cube.");
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<LedWalkerAnimation> _logger;

    public TimeSpan? FrameTime { get; } = null;
    
    private IEnumerator<Point3D>? _activeLedPos;
    private double _lastMove;
    private double _walkingSpeedMs;
    private GeneratorCubeConfiguration? _config = null;

    public LedWalkerAnimation(IConfiguration configuration, ILogger<LedWalkerAnimation> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _walkingSpeedMs = 1000.0 / 256; // 1 sec per Plane
    }

    public void Initialize(GeneratorCubeConfiguration config)
    {
        _config = config;
    }

    public void Start(AnimationContext animationContext)
    {
        _lastMove = animationContext.ElapsedTimeUs / 1000;
        animationContext.CubeData.Clear();
        _activeLedPos?.Dispose();
        _activeLedPos = new PositionGenerator3D(animationContext.CubeData.Size, true).GetEnumerator();
    }

    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (double) frameContext.ElapsedTimeUs / 1000;  
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