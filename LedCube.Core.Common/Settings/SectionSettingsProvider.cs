using System;
using System.Threading.Tasks;

namespace LedCube.Core.Common.Settings;

public class SectionSettingsProvider<TRoot, TSection> : ISettingsProvider<TSection>
    where TRoot : class, new()
    where TSection : class, new()
{
    private readonly ISettingsProvider<TRoot> _root;
    private readonly Func<TRoot, TSection> _getter;
    private readonly Func<TRoot, TSection, TRoot> _setter;

    public TSection Settings => _getter(_root.Settings);

    public event EventHandler<TSection>? SettingsChanged;

    public SectionSettingsProvider(ISettingsProvider<TRoot> root, Func<TRoot, TSection> getter, Func<TRoot, TSection, TRoot> setter)
    {
        _root = root;
        _getter = getter;
        _setter = setter;
        _root.SettingsChanged += OnRootChanged;
    }

    private void OnRootChanged(object? sender, TRoot rootSettings)
        => SettingsChanged?.Invoke(this, _getter(rootSettings));

    public void Load(TSection? defaultSettings = null) { }

    public Task LoadSettingsAsync(TSection? defaultSettings = null) => Task.CompletedTask;

    public void SaveAndUpdate(TSection settings)
        => _root.SaveAndUpdate(_setter(_root.Settings, settings));

    public Task SaveAndUpdateAsync(TSection settings)
        => _root.SaveAndUpdateAsync(_setter(_root.Settings, settings));
}
