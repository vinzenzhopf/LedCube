using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.PlaneWalker;

public class PlaneWalkerPlugin : IPlugin
{
    string IPlugin.Name => "Plane Walker Animation";
    string IPlugin.Description => "Sweeps a lit plane along each axis, back and forth.";
}
