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
    
    private int _activeLedPos;
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
        _activeLedPos = 0;
        _lastMove = animationContext.ElapsedTimeUs / 1000;
        animationContext.CubeData.Clear();
    }

    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (double) frameContext.ElapsedTimeUs / 1000;  
        var lastMoveDiff = elapsedTimeMs - _lastMove;
        if (!(lastMoveDiff > _walkingSpeedMs))
        {
            return;
        }
        
        frameContext.Buffer.SetLed(_activeLedPos, !frameContext.Buffer.GetLed(_activeLedPos));
        _lastMove = elapsedTimeMs;
        _activeLedPos = (_activeLedPos + 1) % frameContext.Buffer.Length;
        // _logger.LogDebug("LedMove {0}ms", _lastMove);
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