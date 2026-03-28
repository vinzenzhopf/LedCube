using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LedCube.Core.Common.Config.Config;
using LedCube.Core.Common.Settings;

namespace LedCube.Core.UI.Services.Hotkey;

public class HotkeyService : IHotkeyService
{
    private static readonly FrozenDictionary<HotkeyFunction, HotkeyEntry> DefaultEntries;
    private static readonly FrozenDictionary<string, HotkeyFunction> EntriesById;

    static HotkeyService()
    {
        var entries = typeof(HotkeyFunction)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f =>
            {
                var value = (HotkeyFunction)f.GetValue(null)!;
                var attr = f.GetCustomAttribute<HotkeyAttribute>();
                return new HotkeyEntry(
                    value,
                    attr?.Id ?? f.Name,
                    attr?.Description ?? f.Name,
                    attr?.ToInputBinding() ?? InputBinding.Empty
                );
            })
            .ToArray();

        DefaultEntries = entries.ToFrozenDictionary(e => e.Function);
        EntriesById = entries.ToFrozenDictionary(e => e.Id, e => e.Function);
    }

    private readonly Dictionary<HotkeyFunction, InputBinding> _customBindings = new();
    private readonly ISettingsProvider<KeyboardControlConfig>? _settingsProvider;

    public HotkeyService(ISettingsProvider<KeyboardControlConfig>? settingsProvider = null)
    {
        _settingsProvider = settingsProvider;
        LoadFromSettings();
    }

    public HotkeyEntry GetHotkey(HotkeyFunction function)
    {
        var entry = DefaultEntries[function];
        return _customBindings.TryGetValue(function, out var custom) ? entry.WithBinding(custom) : entry;
    }

    public void SetCustomBinding(HotkeyFunction function, InputBinding binding)
    {
        _customBindings[function] = binding;
        SaveToSettings();
    }

    public void ResetToDefault(HotkeyFunction function)
    {
        _customBindings.Remove(function);
        SaveToSettings();
    }

    public IEnumerable<HotkeyEntry> GetAllHotkeys() => DefaultEntries.Values;

    public bool TryMatch(InputBinding binding, out HotkeyFunction function)
    {
        foreach (var entry in DefaultEntries.Values)
        {
            var current = _customBindings.TryGetValue(entry.Function, out var custom) ? custom : entry.DefaultBinding;
            if (current == binding)
            {
                function = entry.Function;
                return true;
            }
        }
        function = default;
        return false;
    }

    private void LoadFromSettings()
    {
        if (_settingsProvider is null) return;
        foreach (var entry in _settingsProvider.Settings.KeyMap)
        {
            if (EntriesById.TryGetValue(entry.Function, out var fn) &&
                InputBinding.TryParseConfigString(entry.KeyCombination, out var b))
            {
                _customBindings[fn] = b;
            }
        }
    }

    private void SaveToSettings()
    {
        if (_settingsProvider is null) return;
        var config = new KeyboardControlConfig
        {
            KeyMap = _customBindings
                .Select(kv => new KeyboardControlConfigEntry
                {
                    Function = DefaultEntries[kv.Key].Id,
                    KeyCombination = kv.Value.ToConfigString()
                })
                .ToList()
        };
        _settingsProvider.SaveAndUpdate(config);
    }
}
