using System;
using System.Threading.Tasks;

namespace LedCube.Core.Common.Settings;

public interface ISettingsProvider<T> : ISettings<T> where T : class, new()
{
    /// <summary>Absolute path of the backing settings file.</summary>
    string FilePath { get; }
    event EventHandler<T>? SettingsChanged;
    void Load(T? defaultSettings = null);
    Task LoadSettingsAsync(T? defaultSettings = null);
    void SaveAndUpdate(T settings);
    Task SaveAndUpdateAsync(T settings);
}

public interface ISettings<T> where T : class, new()
{
    T Settings { get; }
}
