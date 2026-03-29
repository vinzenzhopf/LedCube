using System.Windows;
using System.Windows.Controls;
using LedCube.PluginBase;

namespace LedCube.Core.UI.Controls.PluginConfigControl;

public class ConfigEntryTemplateSelector : DataTemplateSelector
{
    public DataTemplate? BoolTemplate { get; set; }
    public DataTemplate? IntTemplate { get; set; }
    public DataTemplate? FloatTemplate { get; set; }
    public DataTemplate? StringTemplate { get; set; }
    public DataTemplate? EnumTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is ConfigEntryViewModel vm)
        {
            return vm.Descriptor.Type switch
            {
                AnimationConfigType.Bool => BoolTemplate,
                AnimationConfigType.Int => IntTemplate,
                AnimationConfigType.Float => FloatTemplate,
                AnimationConfigType.String => StringTemplate,
                AnimationConfigType.Enum => EnumTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
        return base.SelectTemplate(item, container);
    }
}
