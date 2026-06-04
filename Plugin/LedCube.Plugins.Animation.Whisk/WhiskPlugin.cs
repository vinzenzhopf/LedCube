using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.Whisk;

public class WhiskPlugin : IPlugin
{
    string IPlugin.Name => "Whisk Animation";
    string IPlugin.Description => "Two crossed planes spinning around the vertical axis.";
}
