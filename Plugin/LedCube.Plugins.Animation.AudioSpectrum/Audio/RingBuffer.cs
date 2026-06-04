using System;

namespace LedCube.Plugins.Animation.AudioSpectrum.Audio;

/// <summary>
/// A fixed-size circular buffer of mono samples. The capture thread writes; the render thread reads
/// the most recent window. Guarded by a lock — writes are small and infrequent (~10 ms chunks).
/// </summary>
internal sealed class RingBuffer
{
    private readonly float[] _buffer;
    private readonly object _lock = new();
    private long _written;

    public RingBuffer(int capacity) => _buffer = new float[capacity];

    public void Write(ReadOnlySpan<float> samples)
    {
        lock (_lock)
        {
            var capacity = _buffer.Length;
            foreach (var sample in samples)
            {
                _buffer[(int)(_written % capacity)] = sample;
                _written++;
            }
        }
    }

    public void ReadLatest(float[] dest)
    {
        lock (_lock)
        {
            var capacity = _buffer.Length;
            var start = _written - dest.Length;
            for (var i = 0; i < dest.Length; i++)
            {
                var index = start + i;
                dest[i] = index < 0 ? 0f : _buffer[(int)(index % capacity)];
            }
        }
    }
}
