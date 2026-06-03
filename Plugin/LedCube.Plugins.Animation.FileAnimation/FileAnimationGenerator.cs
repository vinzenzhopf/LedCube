using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LedCube.Animation.FileFormat.AnimationRaw.Io;
using LedCube.Core.Animation;
using LedCube.PluginBase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RawAnimation = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;

namespace LedCube.Plugins.Animation.FileAnimation;

/// <summary>
/// A frame generator that plays a baked <c>.lcanimraw</c> file referenced by path. The file is
/// referenced, never embedded, so playlists only store the path.
/// </summary>
public sealed class FileAnimationGenerator(
    IOptions<FileAnimationConfiguration> options,
    ILogger<FileAnimationGenerator> logger)
    : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "File Animation",
        "Plays a baked .lcanimraw animation file",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("FilePath", "File Path", AnimationConfigType.FilePath,
                Description: "Path to a .lcanimraw file",
                FileExtensions: ["lcanimraw"]),
        ]);

    private readonly FileAnimationConfiguration _configuration = options.Value;

    private string? _path;
    private TimeSpan? _frameTime;
    private RawAnimation? _animation;
    private RawAnimationPlayer? _player;
    private int? _frameCount;

    public TimeSpan? FrameTime => _frameTime;

    public int? FrameCount => _frameCount;

    public void Configure(AnimationConfig config)
    {
        _path = config.GetString("FilePath") ?? _configuration.DefaultPath;
        _frameTime = null;
        _frameCount = null;

        if (string.IsNullOrWhiteSpace(_path))
        {
            logger.LogWarning("FileAnimation configured without a FilePath.");
            return;
        }

        // Peek the manifest so FrameTime/FrameCount are known before InitializeAsync
        // (the playback loop reads them to drive the timeline / stats display).
        try
        {
            using var stream = File.OpenRead(_path);
            var manifest = LcAnimRawReader.ReadManifest(stream);
            _frameTime = TimeSpan.FromMicroseconds(manifest.FrameTimeUs);
            _frameCount = manifest.FrameCount;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to read manifest of '{Path}'.", _path);
        }
    }

    public async Task InitializeAsync(CancellationToken token)
    {
        _animation = null;

        var path = _path;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            _animation = await Task.Run(() =>
            {
                using var stream = File.OpenRead(path);
                return LcAnimRawReader.Read(stream);
            }, token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to load animation '{Path}'.", path);
        }
    }

    public void Start(AnimationContext animationContext)
    {
        _player = null;
        animationContext.CubeData.Clear();

        if (_animation is null)
        {
            return;
        }

        try
        {
            // loopOverride: false — ignore the file's authored loop flag during playlist playback so
            // the animation reports finished after one pass. Repetition is then governed by the
            // playlist's per-entry RepeatCount and RepeatMode instead of looping internally forever.
            _player = new RawAnimationPlayer(
                _animation, new CubeRenderOptions(animationContext.CubeData.Size), loopOverride: false);
            _player.Reset();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Cannot play animation '{Path}' on cube {Size}.",
                _path, animationContext.CubeData.Size);
            _player = null;
        }
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        if (_player is null)
        {
            return DrawingResult.Finished;
        }

        var step = _player.Advance(frameContext.ElapsedTimeUs, frameContext.Buffer);
        return step.Finished ? DrawingResult.Finished : DrawingResult.Continue;
    }

    public void ChangeTime(AnimationContext animationContext)
    {
        // Force a re-render at the scrubbed position, even while paused.
        _player?.Reset();
        _player?.Advance(animationContext.ElapsedTimeUs, animationContext.CubeData);
    }
}
