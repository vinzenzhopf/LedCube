using System;
using LedCube.Core.Common.Model;
using LedCube.Plugins.Animation.AudioSpectrum.Audio;
using LedCube.Plugins.Animation.AudioSpectrum.Dsp;
using LedCube.PluginBase;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.AudioSpectrum;

/// <summary>
/// A 3D audio spectrum visualizer. Captures audio (system loopback or microphone), runs an FFT,
/// groups it into one band per X column, and shows the bar heights along Y. Each frame the spectrum
/// is pushed back along Z, so the cube becomes a scrolling waterfall of the recent spectrum.
/// </summary>
public sealed class AudioSpectrumAnimation(ILogger<AudioSpectrumAnimation> logger)
    : IFrameGenerator, IDisposable
{
    public static FrameGeneratorInfo Info => new(
        "Audio Spectrum",
        "3D audio spectrum visualizer (FFT waterfall).",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("Source", "Audio source", AnimationConfigType.Enum,
                DefaultValue: "Loopback", EnumValues: ["Loopback", "Microphone"],
                Description: "Loopback = what's playing on the PC; Microphone = a capture device."),
            new AnimationConfigDescriptor("InputVolume", "Input volume", AnimationConfigType.Float,
                DefaultValue: 2.5f, MinValue: 0.1f, MaxValue: 24.0f,
                Description: "Linear pre-amp applied to the raw samples before the FFT."),
            new AnimationConfigDescriptor("Gain", "Gain", AnimationConfigType.Float,
                DefaultValue: 2.5f, MinValue: 0.1f, MaxValue: 12.0f,
                Description: "Scales the normalized level after the dB mapping."),
            new AnimationConfigDescriptor("Decay", "Decay", AnimationConfigType.Float,
                DefaultValue: 0.85f, MinValue: 0.5f, MaxValue: 0.99f),
            new AnimationConfigDescriptor("FloorDb", "Noise floor (dB)", AnimationConfigType.Float,
                DefaultValue: -60.0f, MinValue: -100.0f, MaxValue: -10.0f,
                Description: "Level mapped to an empty bar. Lower = more sensitive to quiet sounds."),
            new AnimationConfigDescriptor("Tilt", "Tilt (dB/oct)", AnimationConfigType.Float,
                DefaultValue: 4.0f, MinValue: 0.0f, MaxValue: 9.0f,
                Description: "Boosts higher bands to offset music's natural low-frequency emphasis."),
            new AnimationConfigDescriptor("PivotHz", "Tilt pivot (Hz)", AnimationConfigType.Float,
                DefaultValue: 1000.0f, MinValue: 0.0f, MaxValue: 12000.0f,
                Description: "Frequency the tilt rotates around (0 = auto / geometric centre of the range)."),
            new AnimationConfigDescriptor("MinFreq", "Min frequency (Hz)", AnimationConfigType.Float,
                DefaultValue: 40.0f, MinValue: 20.0f, MaxValue: 2000.0f),
            new AnimationConfigDescriptor("MaxFreq", "Max frequency (Hz)", AnimationConfigType.Float,
                DefaultValue: 12000.0f, MinValue: 1000.0f, MaxValue: 24000.0f),
            DurationConfig.Descriptor(0.0f),
        ]);

    private const int FftSize = 2048;

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(25);

    private string _source = "Loopback";
    private float _inputVolume = 1.0f;
    private float _gain = 2.0f;
    private float _decay = 0.85f;
    private float _floorDb = -60.0f;
    private float _tilt = 3.0f;
    private float _pivotHz = 1000.0f;
    private float _minFreq = 40.0f;
    private float _maxFreq = 16000.0f;
    private float _durationSeconds;

    private readonly float[] _samples = new float[FftSize];
    private IAudioSource? _audio;
    private SpectrumAnalyzer? _analyzer;
    private float[] _levels = [];
    private int[,] _history = new int[0, 0]; // [z, band]
    private int _bands;

    public void Configure(AnimationConfig config)
    {
        if (config.GetString("Source") is { } source)
            _source = source;
        if (config.Get<float>("InputVolume") is { } inputVolume)
            _inputVolume = inputVolume;
        if (config.Get<float>("Gain") is { } gain)
            _gain = gain;
        if (config.Get<float>("Decay") is { } decay)
            _decay = decay;
        if (config.Get<float>("FloorDb") is { } floorDb)
            _floorDb = floorDb;
        if (config.Get<float>("Tilt") is { } tilt)
            _tilt = tilt;
        if (config.Get<float>("PivotHz") is { } pivotHz)
            _pivotHz = pivotHz;
        if (config.Get<float>("MinFreq") is { } minFreq)
            _minFreq = minFreq;
        if (config.Get<float>("MaxFreq") is { } maxFreq)
            _maxFreq = maxFreq;
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext)
    {
        var cube = animationContext.CubeData;
        cube.Clear();

        var size = cube.Size;
        _bands = size.X;
        _levels = new float[_bands];
        _history = new int[size.Y, _bands]; // [time slice along Y, band]

        StopAudio();
        try
        {
            if (OperatingSystem.IsWindows())
            {
                _audio = new NAudioCaptureSource(loopback: _source != "Microphone", ringCapacity: FftSize * 4);
                _analyzer = new SpectrumAnalyzer(
                    _audio.SampleRate, FftSize, _bands, _minFreq, _maxFreq, _floorDb, _tilt, _pivotHz, _decay);
                _audio.Start();
            }
            else
            {
                logger.LogWarning("Audio capture is only supported on Windows; spectrum will stay flat.");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to start audio capture (source: {Source}).", _source);
            StopAudio();
        }
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var cube = frameContext.Buffer;
        var size = cube.Size;

        if (_audio is not null && _analyzer is not null)
        {
            _audio.ReadLatest(_samples);
            _analyzer.Analyze(_samples, _levels, _gain, _inputVolume);
        }
        else
        {
            Array.Clear(_levels);
        }

        // Time runs along Y: scroll older spectra toward the back (lower Y); the newest enters at
        // the front (highest Y). Spectrum height is along Z (up), one band per X column.
        for (var y = 0; y < size.Y - 1; y++)
        for (var b = 0; b < _bands; b++)
        {
            _history[y, b] = _history[y + 1, b];
        }

        for (var b = 0; b < _bands; b++)
        {
            _history[size.Y - 1, b] = (int)MathF.Round(_levels[b] * (size.Z - 1));
        }

        cube.Clear();
        for (var y = 0; y < size.Y; y++)
        for (var b = 0; b < _bands && b < size.X; b++)
        {
            var height = _history[y, b];
            for (var z = 0; z <= height && z < size.Z; z++)
            {
                cube.SetLed(new Point3D(b, y, z), true);
            }
        }

        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }

    public void End(AnimationContext animationContext) => StopAudio();

    public void Dispose() => StopAudio();

    private void StopAudio()
    {
        _audio?.Dispose();
        _audio = null;
        _analyzer = null;
    }
}
