using LedCube.Core.UI.Services.Hotkey;

namespace LedCube.Core.UI.Messages;

public record OpenSettingsHotkeyInputDialogMessage(string Function, string Description)
{
    public bool? DialogResult { get; set; }
    public InputBinding ResultBinding { get; set; }
}
