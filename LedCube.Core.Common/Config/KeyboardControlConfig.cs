using System.Collections.Generic;

namespace LedCube.Core.Common.Config;

public class KeyboardControlConfig
{
    public List<KeyboardControlConfigEntry> KeyMap { get; set; } = [];
}

public class KeyboardControlConfigEntry
{
    public string Function { get; set; } = "";
    public string KeyCombination { get; set; } = "";
}
