namespace LedCube.Plugins.Animation.FileAnimation;

public class FileAnimationConfiguration
{
    public const string SectionName = "FileAnimation";

    /// <summary>Optional fallback file path used when an instance is configured without a FilePath.</summary>
    public string? DefaultPath { get; set; }
}
