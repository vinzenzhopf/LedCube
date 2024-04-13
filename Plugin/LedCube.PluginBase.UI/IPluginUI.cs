using System;
using System.Windows;

namespace LedCube.PluginBase.UI;

public interface IPluginUI
{
    public Type GetContent();
}