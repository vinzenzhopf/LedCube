using System.Threading.Tasks;

namespace LedCube.Core.Settings;

public interface ISettingsProvider<T> : ISettings<T> where T : class, new()
{
    void Load(T? defaultSettings = null);
    Task LoadSettingsAsync(T? defaultSettings = null);
    void SaveAndUpdate(T settings);
    Task SaveAndUpdateAsync(T settings);
}

public interface ISettings<T> where T : class, new()
{
    T Settings { get; }
}