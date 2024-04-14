using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.AudioSpectrum;

public class AudioSpectrumAnimationPlugin : IPlugin
{
    public string Name => "Audio Spectrum Animation";
    public string Description => "3D Audio Spectrum Visualizer Animation";
    
    public void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
    }
}