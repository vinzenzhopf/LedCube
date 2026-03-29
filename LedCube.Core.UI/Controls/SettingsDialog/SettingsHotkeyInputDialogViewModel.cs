using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using HotkeyInputBinding = LedCube.Core.UI.Services.Hotkey.InputBinding;
using HotkeyInputType = LedCube.Core.UI.Services.Hotkey.InputType;

namespace LedCube.Core.UI.Controls.SettingsDialog;

public partial class SettingsHotkeyInputDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _function = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HotkeyTokens))]
    private HotkeyInputBinding _capturedBinding;

    [ObservableProperty]
    private bool? _dialogResult;

    public IReadOnlyList<string> HotkeyTokens => BuildTokens(_capturedBinding);

    public void CaptureBinding(HotkeyInputBinding binding)
    {
        CapturedBinding = binding;
        DialogResult = true;
    }

    public void Reset()
    {
        CapturedBinding = HotkeyInputBinding.Empty;
        DialogResult = null;
    }

    private static IReadOnlyList<string> BuildTokens(HotkeyInputBinding binding)
    {
        var tokens = new List<string>();
        if (binding.Modifiers.HasFlag(ModifierKeys.Control)) tokens.Add("Ctrl");
        if (binding.Modifiers.HasFlag(ModifierKeys.Shift)) tokens.Add("Shift");
        if (binding.Modifiers.HasFlag(ModifierKeys.Alt)) tokens.Add("Alt");
        if (binding.Modifiers.HasFlag(ModifierKeys.Windows)) tokens.Add("Win");
        switch (binding.Type)
        {
            case HotkeyInputType.Keyboard when binding.Key != Key.None:
                tokens.Add(binding.Key.ToString());
                break;
            case HotkeyInputType.MouseButton:
                tokens.Add($"{binding.MouseButton} Click");
                break;
            case HotkeyInputType.MouseScrollUp:
                tokens.Add("Scroll Up");
                break;
            case HotkeyInputType.MouseScrollDown:
                tokens.Add("Scroll Down");
                break;
        }
        return tokens;
    }
}
