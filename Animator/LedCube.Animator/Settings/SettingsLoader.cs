using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace LedCube.Animator.Settings;

public class SettingsLoader<T> where T : class, new()
{
    public string FilePath { get; }

    public SettingsLoader(string fileName)
    {
        FilePath = GetLocalFilePath(fileName);
    }

    private static string GetLocalFilePath(string fileName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, fileName);
    }

    public T? LoadSettings() =>
        File.Exists(FilePath) ? 
            JsonSerializer.Deserialize<T>(File.ReadAllText(FilePath)) : null;
    
    public async Task<T?> LoadSettingsAsync(){
        
        if(!File.Exists(FilePath))
            return null;
        await using var fileStream = new FileStream(
            FilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);
        return await JsonSerializer.DeserializeAsync<T>(fileStream).ConfigureAwait(false);
    } 

    public void SaveSettings(T settings) => 
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings));

    public async Task SaveSettingsAsync(T settings)
    {
        await using var fileStream = new FileStream(
            FilePath, FileMode.Create, FileAccess.Write, FileShare.Write,
            bufferSize: 4096, useAsync: true);
        await JsonSerializer.SerializeAsync(fileStream, settings).ConfigureAwait(false);
    }
}