using System.Threading.Tasks;

namespace LedCube.Animator.Settings;

public interface ISettingsProvider<T> : ISettings<T> where T : class, new()
{
    void Load();
    Task LoadSettingsAsync();
    void SaveAndUpdate(T settings);
    Task SaveAndUpdateAsync(T settings);
}

public interface ISettings<T> where T : class, new()
{
    T Settings { get; }
}