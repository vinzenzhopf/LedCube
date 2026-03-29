using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Messages;
using LedCube.Core.UI.Services.Hotkey;
using HotkeyInputBinding = LedCube.Core.UI.Services.Hotkey.InputBinding;
using HotkeyInputType = LedCube.Core.UI.Services.Hotkey.InputType;

namespace LedCube.Core.UI.Controls.SettingsDialog;

public partial class KeyboardControlEntryViewModel : ObservableObject
{
    private readonly IHotkeyService? _hotkeyService;
    private readonly HotkeyFunction _function;

    public KeyboardControlEntryViewModel() { }

    public KeyboardControlEntryViewModel(HotkeyEntry entry, IHotkeyService hotkeyService)
    {
        _function = entry.Function;
        _hotkeyService = hotkeyService;
        _functionId = entry.Id;
        _description = entry.Description;
        _binding = entry.DefaultBinding;
    }

    [ObservableProperty]
    private string _icon = "";

    [ObservableProperty]
    private string _functionId = "";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HotkeyTokens))]
    private HotkeyInputBinding _binding;

    public IReadOnlyList<string> HotkeyTokens => BuildTokens();

    private IReadOnlyList<string> BuildTokens()
    {
        var tokens = new List<string>();
        if (Binding.Modifiers.HasFlag(ModifierKeys.Control)) tokens.Add("Ctrl");
        if (Binding.Modifiers.HasFlag(ModifierKeys.Shift)) tokens.Add("Shift");
        if (Binding.Modifiers.HasFlag(ModifierKeys.Alt)) tokens.Add("Alt");
        if (Binding.Modifiers.HasFlag(ModifierKeys.Windows)) tokens.Add("Win");
        switch (Binding.Type)
        {
            case HotkeyInputType.Keyboard when Binding.Key != Key.None:
                tokens.Add(Binding.Key.ToString());
                break;
            case HotkeyInputType.MouseButton:
                tokens.Add($"{Binding.MouseButton} Click");
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

    [RelayCommand]
    private void InputHotkeyClicked()
    {
        var msg = new OpenSettingsHotkeyInputDialogMessage(_functionId, _description);
        WeakReferenceMessenger.Default.Send(msg);
        if (!msg.DialogResult.HasValue || !msg.DialogResult.Value) return;
        Binding = msg.ResultBinding;
        _hotkeyService?.SetCustomBinding(_function, msg.ResultBinding);
    }
}
