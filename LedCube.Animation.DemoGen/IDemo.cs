using RawAnimation = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;

namespace LedCube.Animation.DemoGen;

/// <summary>A named, self-contained animation generator. Add one class per demo and it is picked up by <c>Program</c>.</summary>
public interface IDemo
{
    /// <summary>Output file stem (no extension), e.g. <c>"rising-plane"</c>.</summary>
    string Name { get; }

    /// <summary>Builds the baked animation to write out.</summary>
    RawAnimation Build();
}
