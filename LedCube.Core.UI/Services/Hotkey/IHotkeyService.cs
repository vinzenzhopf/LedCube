using System.Collections.Generic;

namespace LedCube.Core.UI.Services.Hotkey;

public interface IHotkeyService
{
    HotkeyEntry GetHotkey(HotkeyFunction function);
    void SetCustomBinding(HotkeyFunction function, InputBinding binding);
    void ResetToDefault(HotkeyFunction function);
    IEnumerable<HotkeyEntry> GetAllHotkeys();
    bool TryMatch(InputBinding binding, out HotkeyFunction function);
}
