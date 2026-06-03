using LedCube.Animation.FileFormat.AnimationRaw.Io;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Test.Fixtures;

namespace LedCube.Animation.FileFormat.Test.AnimationRaw;

public class ValidationTests
{
    // 4x4x4 Binary => N=64, stride=ceil(64/8)=8. One valid pool frame is 8 bytes.
    private static byte[] OnePoolFrame() => new byte[8];

    private static InvalidFileFormatException ReadExpectingInvalid(string manifestJson, byte[] framesBin)
    {
        using var zip = RawZip.FromEntries(RawZip.Text("manifest.json", manifestJson), RawZip.Bytes("frames.bin", framesBin));
        return Assert.Throws<InvalidFileFormatException>(() => LcAnimRawReader.Read(zip));
    }

    [Fact]
    public void Validation_EmptyKeyframes_Throws()
    {
        var ex = ReadExpectingInvalid(ManifestJson.Build(keyframes: "[]"), OnePoolFrame());
        Assert.Contains("non-empty", ex.Message);
    }

    [Fact]
    public void Validation_FirstKeyframeNotAtZero_Throws()
    {
        var ex = ReadExpectingInvalid(
            ManifestJson.Build(frameCount: 10, keyframes: "[{\"at\":1,\"id\":0}]"), OnePoolFrame());
        Assert.Contains("at == 0", ex.Message);
    }

    [Fact]
    public void Validation_NonAscendingKeyframes_Throws()
    {
        var ex = ReadExpectingInvalid(
            ManifestJson.Build(frameCount: 10, keyframes: "[{\"at\":0,\"id\":0},{\"at\":5,\"id\":0},{\"at\":3,\"id\":0}]"),
            OnePoolFrame());
        Assert.Contains("ascending", ex.Message);
    }

    [Fact]
    public void Validation_DuplicateKeyframeAt_Throws()
    {
        var ex = ReadExpectingInvalid(
            ManifestJson.Build(frameCount: 10, keyframes: "[{\"at\":0,\"id\":0},{\"at\":2,\"id\":0},{\"at\":2,\"id\":0}]"),
            OnePoolFrame());
        Assert.Contains("ascending", ex.Message);
    }

    [Fact]
    public void Validation_KeyframeAtBeyondFrameCount_Throws()
    {
        var ex = ReadExpectingInvalid(
            ManifestJson.Build(frameCount: 3, keyframes: "[{\"at\":0,\"id\":0},{\"at\":5,\"id\":0}]"),
            OnePoolFrame());
        Assert.Contains("frameCount", ex.Message);
    }

    [Fact]
    public void Validation_KeyframeIdOutOfPoolRange_Throws()
    {
        var ex = ReadExpectingInvalid(
            ManifestJson.Build(frameCount: 10, keyframes: "[{\"at\":0,\"id\":3}]"), OnePoolFrame());
        Assert.Contains("pool", ex.Message);
    }

    [Fact]
    public void Validation_ZeroFrameCount_Throws()
    {
        var ex = ReadExpectingInvalid(ManifestJson.Build(frameCount: 0), OnePoolFrame());
        Assert.Contains("frameCount", ex.Message);
    }

    [Fact]
    public void Validation_FramesBinSizeNotMultipleOfStride_Throws()
    {
        // stride is 8; 7 bytes is not a whole number of frames.
        var ex = ReadExpectingInvalid(ManifestJson.Build(), new byte[7]);
        Assert.Contains("multiple", ex.Message);
    }

    [Fact]
    public void Validation_FutureFormatVersion_Throws()
    {
        using var zip = RawZip.FromEntries(
            RawZip.Text("manifest.json", ManifestJson.Build(formatVersion: 2)),
            RawZip.Bytes("frames.bin", OnePoolFrame()));

        var ex = Assert.Throws<UnsupportedFormatVersionException>(() => LcAnimRawReader.Read(zip));
        Assert.Equal(2, ex.FileVersion);
        Assert.Equal(LcAnimRawFormat.CurrentVersion, ex.MaxSupportedVersion);
    }

    [Fact]
    public void Validation_MissingManifestEntry_Throws()
    {
        using var zip = RawZip.FromEntries(RawZip.Bytes("frames.bin", OnePoolFrame()));
        var ex = Assert.Throws<MissingEntryException>(() => LcAnimRawReader.Read(zip));
        Assert.Equal("manifest.json", ex.EntryName);
    }

    [Fact]
    public void Validation_MissingFramesEntry_Throws()
    {
        using var zip = RawZip.FromEntries(RawZip.Text("manifest.json", ManifestJson.Build()));
        var ex = Assert.Throws<MissingEntryException>(() => LcAnimRawReader.Read(zip));
        Assert.Equal("frames.bin", ex.EntryName);
    }
}
