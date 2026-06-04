using System;

namespace LedCube.Plugins.Animation.AudioSpectrum.Dsp;

/// <summary>
/// Turns a window of mono samples into a set of normalized band levels: Hanning window → FFT
/// (FftSharp) → magnitudes → grouped into log-spaced bands → dB-scaled, gained, and smoothed with a
/// fast-attack / slow-decay envelope. Pure DSP with no audio or rendering dependencies.
/// </summary>
public sealed class SpectrumAnalyzer
{
    private readonly int _fftSize;
    private readonly int _bands;
    private readonly int[] _bandEdges; // bin index at each band edge, length _bands + 1
    private readonly double[] _windowed;
    private readonly FftSharp.Windows.Hanning _window = new();
    private readonly float[] _smoothed;
    private readonly double[] _tiltDb; // per-band spectral-tilt boost, in dB
    private readonly float _decay;
    private readonly double _floorDb;

    public SpectrumAnalyzer(
        int sampleRate,
        int fftSize,
        int bands,
        float minFrequency = 40f,
        float maxFrequency = 16000f,
        float floorDb = -60f,
        float tiltDbPerOctave = 3f,
        float decay = 0.85f)
    {
        _fftSize = fftSize;
        _bands = bands;
        _decay = Math.Clamp(decay, 0.5f, 0.999f);
        _floorDb = Math.Min(floorDb, -1f); // must stay below the 0 dB ceiling
        _windowed = new double[fftSize];
        _smoothed = new float[bands];

        maxFrequency = MathF.Min(maxFrequency, sampleRate / 2f);
        _bandEdges = ComputeBandEdges(sampleRate, fftSize, bands, minFrequency, maxFrequency);
        _tiltDb = ComputeTilt(bands, minFrequency, maxFrequency, tiltDbPerOctave);
    }

    /// <summary>
    /// Writes <see cref="_bands"/> levels in [0, 1] into <paramref name="levels"/>.
    /// <paramref name="inputVolume"/> amplifies the raw samples before the FFT (a linear pre-amp);
    /// <paramref name="gain"/> scales the normalized level afterwards.
    /// </summary>
    public void Analyze(ReadOnlySpan<float> samples, float[] levels, float gain, float inputVolume)
    {
        var available = Math.Min(samples.Length, _fftSize);
        for (var i = 0; i < _fftSize; i++)
        {
            _windowed[i] = i < available ? samples[i] * inputVolume : 0.0;
        }

        _window.ApplyInPlace(_windowed);
        var magnitude = FftSharp.FFT.Magnitude(FftSharp.FFT.Forward(_windowed));

        for (var b = 0; b < _bands; b++)
        {
            var lo = _bandEdges[b];
            var hi = Math.Max(lo + 1, _bandEdges[b + 1]);

            // Peak (loudest) bin in the band — averaging would dilute the spectral peaks to noise.
            double peak = 0;
            for (var k = lo; k < hi && k < magnitude.Length; k++)
            {
                if (magnitude[k] > peak)
                {
                    peak = magnitude[k];
                }
            }

            // dB level, plus the spectral-tilt boost that lifts higher bands to offset music's
            // natural low-frequency emphasis.
            var db = (20.0 * Math.Log10(peak + 1e-9)) + _tiltDb[b];
            var normalized = (float)((db - _floorDb) / -_floorDb); // map [floorDb, 0 dB] -> [0, 1]
            normalized = Math.Clamp(normalized * gain, 0f, 1f);

            // Fast attack, slow decay for a natural-looking equalizer.
            _smoothed[b] = normalized >= _smoothed[b] ? normalized : _smoothed[b] * _decay;
            levels[b] = _smoothed[b];
        }
    }

    private static int[] ComputeBandEdges(int sampleRate, int fftSize, int bands, float minFrequency, float maxFrequency)
    {
        var binCount = fftSize / 2;
        var binHz = (float)sampleRate / fftSize;
        var nyquist = sampleRate / 2f;
        maxFrequency = MathF.Min(maxFrequency, nyquist);

        var edges = new int[bands + 1];
        for (var b = 0; b <= bands; b++)
        {
            var fraction = (float)b / bands;
            var frequency = minFrequency * MathF.Pow(maxFrequency / minFrequency, fraction); // log spacing
            edges[b] = Math.Clamp((int)MathF.Round(frequency / binHz), 0, binCount);
        }

        return edges;
    }

    /// <summary>
    /// Per-band tilt in dB: 0 at the lowest band, rising by <paramref name="dbPerOctave"/> for every
    /// octave above <paramref name="minFrequency"/>, so higher bands are progressively boosted.
    /// </summary>
    private static double[] ComputeTilt(int bands, float minFrequency, float maxFrequency, float dbPerOctave)
    {
        var tilt = new double[bands];
        var log2 = Math.Log(2.0);
        for (var b = 0; b < bands; b++)
        {
            var fraction = (b + 0.5f) / bands;
            var centerFrequency = minFrequency * MathF.Pow(maxFrequency / minFrequency, fraction);
            var octaves = Math.Log(centerFrequency / minFrequency) / log2;
            tilt[b] = dbPerOctave * octaves;
        }

        return tilt;
    }
}
