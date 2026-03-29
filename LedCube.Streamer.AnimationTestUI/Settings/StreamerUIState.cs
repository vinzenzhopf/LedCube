namespace LedCube.Streamer.AnimationTestUI.Settings;

public record StreamerUIState
{
    public double? WindowLeft { get; init; }
    public double? WindowTop { get; init; }
    public double? WindowWidth { get; init; }
    public double? WindowHeight { get; init; }
    public string LastHostname { get; init; } = string.Empty;
}
