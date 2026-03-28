namespace LedCube.Core.UI.Services.Hotkey;

public record HotkeyEntry(
    HotkeyFunction Function,
    string Id,
    string Description,
    InputBinding DefaultBinding)
{
    /// <summary>
    /// Creates a new entry with an overridden binding.
    /// </summary>
    public HotkeyEntry WithBinding(InputBinding binding) => this with { DefaultBinding = binding };
}