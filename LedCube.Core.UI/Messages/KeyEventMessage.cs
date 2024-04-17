using System.Windows.Input;

namespace LedCube.Core.UI.Messages;

public record KeyEventMessage(int Timestamp, Key Key, KeyStates KeyStates, ModifierKeys Modifiers);