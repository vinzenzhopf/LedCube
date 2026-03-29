namespace LedCube.Animator.Settings;

public record AnimatorUIState
{
    public double? WindowLeft { get; init; }
    public double? WindowTop { get; init; }
    public double? WindowWidth { get; init; }
    public double? WindowHeight { get; init; }
    public string? LastOpenedFile { get; init; }
}
