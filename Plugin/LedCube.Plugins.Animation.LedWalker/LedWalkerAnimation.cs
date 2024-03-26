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
    private long _lastMove;
    private int _walkingSpeedMs;
    private GeneratorCubeConfiguration? _config = null;

    public LedWalkerAnimation(IConfiguration configuration, ILogger<LedWalkerAnimation> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _walkingSpeedMs = 200;
    }

    public void Initialize(GeneratorCubeConfiguration config)
    {
        _config = config;
    }

    public void AnimationStart(AnimationContext animationContext)
    {
        _activeLedPos = 0;
        _lastMove = animationContext.CurrentTicks;
    }

    public void DrawFrame(FrameContext frameContext)
    {
        var lastMoveDiff = (frameContext.ElapsedTimeUs/1000 - _lastMove);
        if (lastMoveDiff > _walkingSpeedMs)
        {
            _activeLedPos = (_activeLedPos + 1) % frameContext.Buffer.Length;
        }
        frameContext.Buffer.Clear();
        frameContext.Buffer.SetLed(_activeLedPos, true);
    }

    public void AnimationEnd(AnimationContext animationContext)
    {
    }

    public void Dispose()
    {
    }
}