using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace LedCube.Plugins.Animation.AudioSpectrum.Audio;

/// <summary>
/// Captures audio via WASAPI: either the system mix (loopback — "what you hear") or a capture
/// device (microphone / line-in). Downmixes to mono and feeds a <see cref="RingBuffer"/>.
/// Windows-only.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class NAudioCaptureSource : IAudioSource
{
    private readonly IWaveIn _capture;
    private readonly WaveFormat _format;
    private readonly RingBuffer _ring;

    public int SampleRate { get; }

    public NAudioCaptureSource(bool loopback, int ringCapacity)
    {
        _capture = loopback ? new WasapiLoopbackCapture() : new WasapiCapture();
        _format = _capture.WaveFormat;
        SampleRate = _format.SampleRate;
        _ring = new RingBuffer(ringCapacity);
        _capture.DataAvailable += OnDataAvailable;
    }

    public void Start() => _capture.StartRecording();

    public void Stop()
    {
        try
        {
            _capture.StopRecording();
        }
        catch
        {
            // Stopping a capture that never started / is already stopping must not throw.
        }
    }

    public void ReadLatest(float[] dest) => _ring.ReadLatest(dest);

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var channels = _format.Channels;
        if (channels <= 0 || e.BytesRecorded <= 0)
        {
            return;
        }

        float[] mono;
        if (_format.Encoding == WaveFormatEncoding.IeeeFloat && _format.BitsPerSample == 32)
        {
            var samples = MemoryMarshal.Cast<byte, float>(e.Buffer.AsSpan(0, e.BytesRecorded));
            mono = Downmix(samples, channels);
        }
        else if (_format.Encoding == WaveFormatEncoding.Pcm && _format.BitsPerSample == 16)
        {
            var samples = MemoryMarshal.Cast<byte, short>(e.Buffer.AsSpan(0, e.BytesRecorded));
            var frames = samples.Length / channels;
            mono = new float[frames];
            for (var f = 0; f < frames; f++)
            {
                var sum = 0f;
                for (var c = 0; c < channels; c++)
                {
                    sum += samples[(f * channels) + c] / 32768f;
                }

                mono[f] = sum / channels;
            }
        }
        else
        {
            return; // unsupported format; leave the ring untouched (silence)
        }

        _ring.Write(mono);
    }

    private static float[] Downmix(ReadOnlySpan<float> interleaved, int channels)
    {
        var frames = interleaved.Length / channels;
        var mono = new float[frames];
        for (var f = 0; f < frames; f++)
        {
            var sum = 0f;
            for (var c = 0; c < channels; c++)
            {
                sum += interleaved[(f * channels) + c];
            }

            mono[f] = sum / channels;
        }

        return mono;
    }

    public void Dispose()
    {
        _capture.DataAvailable -= OnDataAvailable;
        Stop();
        _capture.Dispose();
    }
}
