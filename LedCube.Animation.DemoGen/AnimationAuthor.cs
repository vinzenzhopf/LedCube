using System;
using System.Collections.Generic;
using System.IO;
using LedCube.Animation.FileFormat.AnimationRaw.Io;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Animation;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using RawAnimation = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;

namespace LedCube.Animation.DemoGen;

/// <summary>
/// Fluent helper for authoring a baked animation in code: append one frame per timeline step by
/// drawing into an <see cref="ICubeData"/>, then <see cref="Save"/> it to a <c>.lcanimraw</c> file.
/// Identical frames are deduplicated by the writer, so holding a frame is cheap — just append it again.
/// </summary>
public sealed class AnimationAuthor
{
    private readonly Point3D _size;
    private readonly LedFormat _format;
    private readonly uint _frameTimeUs;
    private readonly bool _seamlessLoop;
    private readonly string _name;
    private readonly string? _author;
    private readonly string? _description;

    private readonly IFrameEncoder _encoder;
    private readonly List<Frame> _frames = new();

    public AnimationAuthor(
        Point3D size,
        uint frameTimeUs,
        bool seamlessLoop = false,
        LedFormat format = LedFormat.Binary,
        string name = "Demo Animation",
        string? author = null,
        string? description = null)
    {
        _size = size;
        _format = format;
        _frameTimeUs = frameTimeUs;
        _seamlessLoop = seamlessLoop;
        _name = name;
        _author = author;
        _description = description;
        _encoder = SelectEncoder(format);
    }

    /// <summary>Number of timeline frames appended so far.</summary>
    public int FrameCount => _frames.Count;

    /// <summary>Encodes the cube's current state and appends it as the next timeline frame.</summary>
    public AnimationAuthor AppendFrame(ICubeData cube)
    {
        ArgumentNullException.ThrowIfNull(cube);
        if (cube.Size != _size)
        {
            throw new ArgumentException(
                $"Cube size {cube.Size} does not match the authored size {_size}.", nameof(cube));
        }

        _frames.Add(_encoder.Encode(cube));
        return this;
    }

    /// <summary>Clears the cube, runs <paramref name="draw"/>, then appends the result.</summary>
    public AnimationAuthor AppendFrame(ICubeData cube, Action<ICubeData> draw)
    {
        ArgumentNullException.ThrowIfNull(draw);
        cube.Clear();
        draw(cube);
        return AppendFrame(cube);
    }

    /// <summary>Builds the in-memory animation model. One keyframe per appended frame; the writer dedups.</summary>
    public RawAnimation Build()
    {
        if (_frames.Count == 0)
        {
            throw new InvalidOperationException("Append at least one frame before building.");
        }

        var keyframes = new List<Keyframe>(_frames.Count);
        for (var i = 0; i < _frames.Count; i++)
        {
            keyframes.Add(new Keyframe(i, i));
        }

        var manifest = new AnimationManifest
        {
            Name = _name,
            Author = _author,
            Description = _description,
            CreatedUtc = DateTimeOffset.UtcNow,
            Size = _size,
            LedFormat = _format,
            FrameCount = _frames.Count,
            FrameTimeUs = _frameTimeUs,
            SeamlessLoop = _seamlessLoop,
        };

        return new RawAnimation(manifest, _frames, keyframes);
    }

    /// <summary>Builds and writes the animation to <paramref name="path"/>.</summary>
    public void Save(string path)
    {
        var animation = Build();
        using var stream = File.Create(path);
        LcAnimRawWriter.Write(stream, animation);
    }

    private static IFrameEncoder SelectEncoder(LedFormat format) => format switch
    {
        LedFormat.Binary => new BinaryFrameEncoder(),
        _ => throw new NotSupportedException(
            $"LED format '{format}' has no encoder yet; v1 authors Binary animations only."),
    };
}
