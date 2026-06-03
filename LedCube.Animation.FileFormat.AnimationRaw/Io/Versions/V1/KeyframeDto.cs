namespace LedCube.Animation.FileFormat.AnimationRaw.Io.Versions.V1;

/// <summary>Frozen JSON shape of a single <c>keyframes</c> entry. Original v1.</summary>
internal sealed record KeyframeDto
{
    public int At { get; init; }
    public int Id { get; init; }
}
