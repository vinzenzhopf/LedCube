using System.Windows.Input;
using MahApps.Metro.Controls;
using LedCube.Core.UI.Services.Hotkey;
using HotkeyInputBinding = LedCube.Core.UI.Services.Hotkey.InputBinding;

namespace LedCube.Core.UI.Controls.SettingsDialog;

public partial class SettingsHotkeyInputDialog : MetroWindow
{
    public SettingsHotkeyInputDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => Focus();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (DataContext is not SettingsHotkeyInputDialogViewModel vm) return;
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
            return;
        }
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl
                or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin)
            return;
        Capture(vm, HotkeyInputBinding.FromKey(key, Keyboard.Modifiers));
        e.Handled = true;
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        if (DataContext is not SettingsHotkeyInputDialogViewModel vm) return;
        var binding = e.Delta > 0
            ? HotkeyInputBinding.FromScrollUp(Keyboard.Modifiers)
            : HotkeyInputBinding.FromScrollDown(Keyboard.Modifiers);
        Capture(vm, binding);
        e.Handled = true;
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        if (DataContext is not SettingsHotkeyInputDialogViewModel vm) return;
        var btn = e.ChangedButton switch
        {
            MouseButton.Left   => ExtendedMouseButton.Left,
            MouseButton.Right  => ExtendedMouseButton.Right,
            MouseButton.Middle => ExtendedMouseButton.Middle,
            MouseButton.XButton1 => ExtendedMouseButton.XButton1,
            MouseButton.XButton2 => ExtendedMouseButton.XButton2,
            _ => ExtendedMouseButton.None
        };
        if (btn == ExtendedMouseButton.None) return;
        Capture(vm, HotkeyInputBinding.FromMouseButton(btn, Keyboard.Modifiers));
        e.Handled = true;
    }

    private void Capture(SettingsHotkeyInputDialogViewModel vm, HotkeyInputBinding binding)
    {
        vm.CaptureBinding(binding);
        DialogResult = true;
    }
}
