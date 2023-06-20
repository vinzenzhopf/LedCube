using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LedCube.Core.Json;

namespace LedCube.Core.Settings;

public class SettingsLoader<T> where T : class, new()
{
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        Converters = { new HtmlColorJsonConverter() }
    };
    
    public string FilePath { get; }

    public SettingsLoader(string applicationName, string fileName)
    {
        FilePath = GetLocalFilePath(applicationName, fileName);
    }

    private static string GetLocalFilePath(string applicationName, string fileName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, applicationName, fileName);
    }

    public bool Exists() => File.Exists(FilePath); 
    
    public T? LoadSettings() =>
        File.Exists(FilePath) ? 
            JsonSerializer.Deserialize<T>(File.ReadAllText(FilePath), _serializerOptions) : null;
    
    public async Task<T?> LoadSettingsAsync(){
        
        if(!File.Exists(FilePath))
            return null;
        await using var fileStream = new FileStream(
            FilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);
        return await JsonSerializer.DeserializeAsync<T>(fileStream, _serializerOptions).ConfigureAwait(false);
    } 

    public void SaveSettings(T settings)
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir!);
        }
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, _serializerOptions));
    }

    public async Task SaveSettingsAsync(T settings)
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir!);
        }
        await using var fileStream = new FileStream(
            FilePath, FileMode.Create, FileAccess.Write, FileShare.Write,
            bufferSize: 4096, useAsync: true);
        await JsonSerializer.SerializeAsync(fileStream, settings, _serializerOptions).ConfigureAwait(false);
    }
}