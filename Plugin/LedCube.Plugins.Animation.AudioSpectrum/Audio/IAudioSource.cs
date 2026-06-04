using System;

namespace LedCube.Plugins.Animation.AudioSpectrum.Audio;

/// <summary>
/// A source of mono audio samples. Implementations capture from a device on their own thread and
/// hand the latest samples to the render loop on demand. The seam keeps the NAudio (Windows-only)
/// backend swappable for a cross-platform one later.
/// </summary>
public interface IAudioSource : IDisposable
{
    int SampleRate { get; }

    void Start();

    void Stop();

    /// <summary>Copies the most recent <c>dest.Length</c> mono samples into <paramref name="dest"/>
    /// (zero-padded at the front when not enough has been captured yet).</summary>
    void ReadLatest(float[] dest);
}
