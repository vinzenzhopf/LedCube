using Avalonia.Input;

namespace LedCube.Core.UI.Messages;

public record KeyEventMessage(int Timestamp, Key Key, bool IsDown, KeyModifiers Modifiers);