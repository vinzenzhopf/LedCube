using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.AudioSpectrum;

public class AudioSpectrumAnimationPlugin : IPlugin
{
    string IPlugin.Name => "Audio Spectrum Animation";
    string IPlugin.Description => "3D Audio Spectrum Visualizer Animation";
}