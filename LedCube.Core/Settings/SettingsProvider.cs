using System;
using System.Threading.Tasks;
using Serilog;

namespace LedCube.Core.Settings;

public class SettingsProvider<T> : ISettingsProvider<T> where T : class, new()
{
    public T Settings { get; private set; } = new T();

    private readonly SettingsLoader<T> _loader;

    public SettingsProvider(string applicationName, string filename)
    {
        _loader = new SettingsLoader<T>(applicationName, filename);
    }

    public void Load(T? defaultSettings = null)
    {
        try
        {
            if (defaultSettings is not null && !_loader.Exists())
            {
                _loader.SaveSettings(defaultSettings);
            }
            var settings = _loader.LoadSettings();
            if (settings is null)
            {
                throw new SettingsLoaderException("Error reading settings file.");
            }
        }
        catch (Exception e)
        {
            Log.ForContext(GetType()).Error(e, "Could not load settings file from {0}'", _loader.FilePath);
        }
    }
    
    public async Task LoadSettingsAsync(T? defaultSettings = null)
    {
        try
        {
            if (defaultSettings is not null && !_loader.Exists())
            {
                await _loader.SaveSettingsAsync(defaultSettings);   
            }
            var settings = await _loader.LoadSettingsAsync().ConfigureAwait(false);
            if (settings is null)
            {
                throw new SettingsLoaderException("Error reading settings file.");
            }
        }
        catch (Exception e)
        {
            Log.ForContext(GetType()).Error(e, "Could not load settings file from {0}'", _loader.FilePath);
        }
    }

    public void SaveAndUpdate(T settings)
    {
        try
        {
            Settings = settings;
            _loader.SaveSettings(Settings);
        }
        catch (Exception e)
        {
            Log.ForContext(GetType()).Error(e, "Could not load settings file from {0}'", _loader.FilePath);
        }
    }
    
    public async Task SaveAndUpdateAsync(T settings)
    {
        try
        {
            Settings = settings;
            await _loader.SaveSettingsAsync(Settings);
        }
        catch (Exception e)
        {
            Log.ForContext(GetType()).Error(e, "Could not load settings file from {0}'", _loader.FilePath);
        }
    }
}