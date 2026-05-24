using Avalonia.Controls;
using Avalonia.Input;
using LedCube.Core.UI.Services.Hotkey;
using HotkeyInputBinding = LedCube.Core.UI.Services.Hotkey.InputBinding;

namespace LedCube.Core.UI.Controls.SettingsDialog;

public partial class SettingsHotkeyInputDialog : Window
{
    public SettingsHotkeyInputDialog()
    {
        InitializeComponent();
        Opened += (_, _) => Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is not SettingsHotkeyInputDialogViewModel vm) return;
        if (e.Key == Key.Escape)
        {
            Close(false);
            e.Handled = true;
            return;
        }
        var key = e.Key;
        if (key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl
                or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin)
            return;
        Capture(vm, HotkeyInputBinding.FromKey(key, e.KeyModifiers));
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (DataContext is not SettingsHotkeyInputDialogViewModel vm) return;
        var binding = e.Delta.Y > 0
            ? HotkeyInputBinding.FromScrollUp(e.KeyModifiers)
            : HotkeyInputBinding.FromScrollDown(e.KeyModifiers);
        Capture(vm, binding);
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (DataContext is not SettingsHotkeyInputDialogViewModel vm) return;
        var btn = e.GetCurrentPoint(this).Properties.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonPressed   => ExtendedMouseButton.Left,
            PointerUpdateKind.RightButtonPressed  => ExtendedMouseButton.Right,
            PointerUpdateKind.MiddleButtonPressed => ExtendedMouseButton.Middle,
            PointerUpdateKind.XButton1Pressed     => ExtendedMouseButton.XButton1,
            PointerUpdateKind.XButton2Pressed     => ExtendedMouseButton.XButton2,
            _                                     => ExtendedMouseButton.None
        };
        if (btn == ExtendedMouseButton.None) return;
        Capture(vm, HotkeyInputBinding.FromMouseButton(btn, e.KeyModifiers));
        e.Handled = true;
    }

    private void Capture(SettingsHotkeyInputDialogViewModel vm, HotkeyInputBinding binding)
    {
        vm.CaptureBinding(binding);
        Close(true);
    }
}
