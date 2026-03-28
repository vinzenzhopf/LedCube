using System;
using System.Windows.Input;

namespace LedCube.Core.UI.Services.Hotkey;

/// <summary>
/// Represents a unified input binding that can be a key, mouse button, or scroll action.
/// </summary>
public readonly record struct InputBinding
{
    public InputType Type { get; init; }
    public Key Key { get; init; }
    public ExtendedMouseButton MouseButton { get; init; }
    public ModifierKeys Modifiers { get; init; }

    // Factory methods for cleaner construction
    public static InputBinding FromKey(Key key, ModifierKeys modifiers = ModifierKeys.None) =>
        new() { Type = InputType.Keyboard, Key = key, Modifiers = modifiers };

    public static InputBinding FromMouseButton(ExtendedMouseButton button, ModifierKeys modifiers = ModifierKeys.None) =>
        new() { Type = InputType.MouseButton, MouseButton = button, Modifiers = modifiers };

    public static InputBinding FromScrollUp(ModifierKeys modifiers = ModifierKeys.None) =>
        new() { Type = InputType.MouseScrollUp, Modifiers = modifiers };

    public static InputBinding FromScrollDown(ModifierKeys modifiers = ModifierKeys.None) =>
        new() { Type = InputType.MouseScrollDown, Modifiers = modifiers };

    public static readonly InputBinding Empty = new() { Type = InputType.None };

    public override string ToString()
    {
        var modifierStr = Modifiers != ModifierKeys.None ? $"{Modifiers} + " : "";
        return Type switch
        {
            InputType.Keyboard => $"{modifierStr}{Key}",
            InputType.MouseButton => $"{modifierStr}{MouseButton} Click",
            InputType.MouseScrollUp => $"{modifierStr}Scroll Up",
            InputType.MouseScrollDown => $"{modifierStr}Scroll Down",
            _ => "Unassigned"
        };
    }

    public string ToConfigString() => Type switch
    {
        InputType.Keyboard => $"Keyboard:{Key}:{Modifiers}",
        InputType.MouseButton => $"MouseButton:{MouseButton}:{Modifiers}",
        InputType.MouseScrollUp => $"MouseScrollUp::{Modifiers}",
        InputType.MouseScrollDown => $"MouseScrollDown::{Modifiers}",
        _ => "None::"
    };

    public static bool TryParseConfigString(string s, out InputBinding binding)
    {
        binding = Empty;
        if (string.IsNullOrEmpty(s)) return false;
        var parts = s.Split(':');
        if (parts.Length < 3) return false;
        if (!Enum.TryParse<InputType>(parts[0], out var type)) return false;
        if (!Enum.TryParse<ModifierKeys>(parts[2], out var modifiers)) return false;
        switch (type)
        {
            case InputType.Keyboard:
                if (!Enum.TryParse<Key>(parts[1], out var key)) return false;
                binding = FromKey(key, modifiers);
                return true;
            case InputType.MouseButton:
                if (!Enum.TryParse<ExtendedMouseButton>(parts[1], out var btn)) return false;
                binding = FromMouseButton(btn, modifiers);
                return true;
            case InputType.MouseScrollUp:
                binding = FromScrollUp(modifiers);
                return true;
            case InputType.MouseScrollDown:
                binding = FromScrollDown(modifiers);
                return true;
            default:
                return false;
        }
    }
}