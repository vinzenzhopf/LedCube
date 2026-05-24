using System;
using Avalonia.Input;

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
    public KeyModifiers DefaultModifiers { get; }

    // Keyboard binding
    public HotkeyAttribute(string id, string description, Key defaultKey, KeyModifiers defaultModifiers = KeyModifiers.None)
    {
        Id = id;
        Description = description;
        InputType = InputType.Keyboard;
        DefaultKey = defaultKey;
        DefaultModifiers = defaultModifiers;
    }

    // Mouse button binding
    public HotkeyAttribute(string id, string description, ExtendedMouseButton mouseButton, KeyModifiers defaultModifiers = KeyModifiers.None)
    {
        Id = id;
        Description = description;
        InputType = InputType.MouseButton;
        DefaultMouseButton = mouseButton;
        DefaultModifiers = defaultModifiers;
    }

    // Scroll wheel binding
    public HotkeyAttribute(string id, string description, InputType scrollType, KeyModifiers defaultModifiers = KeyModifiers.None)
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
