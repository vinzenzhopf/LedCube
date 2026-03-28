using System;
using System.Windows.Input;

namespace LedCube.Core.UI.Services.Hotkey;

/// <summary>
/// Attribute to define hotkey metadata on enum values.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class HotkeyAttribute : Attribute
{
    public string Id { get; }
    public string Description { get; }
    public InputType InputType { get; }
    public Key DefaultKey { get; }
    public ExtendedMouseButton DefaultMouseButton { get; }
    public ModifierKeys DefaultModifiers { get; }

    // Keyboard binding
    public HotkeyAttribute(string id, string description, Key defaultKey, ModifierKeys defaultModifiers = ModifierKeys.None)
    {
        Id = id;
        Description = description;
        InputType = InputType.Keyboard;
        DefaultKey = defaultKey;
        DefaultModifiers = defaultModifiers;
    }

    // Mouse button binding
    public HotkeyAttribute(string id, string description, ExtendedMouseButton mouseButton, ModifierKeys defaultModifiers = ModifierKeys.None)
    {
        Id = id;
        Description = description;
        InputType = InputType.MouseButton;
        DefaultMouseButton = mouseButton;
        DefaultModifiers = defaultModifiers;
    }

    // Scroll wheel binding
    public HotkeyAttribute(string id, string description, InputType scrollType, ModifierKeys defaultModifiers = ModifierKeys.None)
    {
        if (scrollType is not (InputType.MouseScrollUp or InputType.MouseScrollDown))
            throw new ArgumentException("Use this constructor only for scroll types", nameof(scrollType));

        Id = id;
        Description = description;
        InputType = scrollType;
        DefaultModifiers = defaultModifiers;
    }

    public InputBinding ToInputBinding() => InputType switch
    {
        InputType.Keyboard => InputBinding.FromKey(DefaultKey, DefaultModifiers),
        InputType.MouseButton => InputBinding.FromMouseButton(DefaultMouseButton, DefaultModifiers),
        InputType.MouseScrollUp => InputBinding.FromScrollUp(DefaultModifiers),
        InputType.MouseScrollDown => InputBinding.FromScrollDown(DefaultModifiers),
        _ => InputBinding.Empty
    };
}