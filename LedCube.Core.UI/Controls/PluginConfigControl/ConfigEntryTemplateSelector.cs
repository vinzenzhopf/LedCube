using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using LedCube.PluginBase;

namespace LedCube.Core.UI.Controls.PluginConfigControl;

/// <summary>
/// Avalonia IDataTemplate that builds the appropriate editor control for each ConfigEntryViewModel type.
/// </summary>
public class ConfigEntryDataTemplate : IDataTemplate
{
    public bool Match(object? data) => data is ConfigEntryViewModel;

    public Control? Build(object? param)
    {
        if (param is not ConfigEntryViewModel vm) return null;

        return vm.Descriptor.Type switch
        {
            AnimationConfigType.Bool => BuildBool(),
            AnimationConfigType.Int => BuildText(80),
            AnimationConfigType.Float => BuildText(80),
            AnimationConfigType.String => BuildText(120),
            AnimationConfigType.Enum => BuildEnum(),
            _ => new TextBlock { Text = $"[{vm.Descriptor.Type}]" }
        };
    }

    private static Grid MakeRow(double valueWidth, out TextBlock label)
    {
        var grid = new Grid
        {
            Margin = new Avalonia.Thickness(0, 3),
            ColumnDefinitions = new ColumnDefinitions($"*,{valueWidth}")
        };
        label = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 8, 0)
        };
        label.Bind(TextBlock.TextProperty, new Binding("Descriptor.DisplayName"));
        ToolTip.SetTip(grid, new Binding("Descriptor.Description"));
        Grid.SetColumn(label, 0);
        grid.Children.Add(label);
        return grid;
    }

    private static Control BuildBool()
    {
        var grid = MakeRow(double.NaN, out _);
        var cb = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
        cb.Bind(CheckBox.IsCheckedProperty, new Binding("BoolValue"));
        Grid.SetColumn(cb, 1);
        grid.Children.Add(cb);
        return grid;
    }

    private static Control BuildText(double width)
    {
        var grid = MakeRow(width, out _);
        var tb = new TextBox();
        tb.Bind(TextBox.TextProperty, new Binding("StringValue"));
        Grid.SetColumn(tb, 1);
        grid.Children.Add(tb);
        return grid;
    }

    private static Control BuildEnum()
    {
        var grid = MakeRow(120, out _);
        var cb = new ComboBox();
        cb.Bind(ComboBox.ItemsSourceProperty, new Binding("Descriptor.EnumValues"));
        cb.Bind(ComboBox.SelectedItemProperty, new Binding("EnumValue"));
        Grid.SetColumn(cb, 1);
        grid.Children.Add(cb);
        return grid;
    }
}
