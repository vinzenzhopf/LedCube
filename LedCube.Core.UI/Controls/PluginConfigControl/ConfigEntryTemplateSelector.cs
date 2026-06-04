using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
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
            AnimationConfigType.Int => vm.HasRange ? BuildSlider(vm) : BuildText(80),
            AnimationConfigType.Float => vm.HasRange ? BuildSlider(vm) : BuildText(80),
            AnimationConfigType.String => BuildText(120),
            AnimationConfigType.Enum => BuildEnum(),
            AnimationConfigType.FilePath => BuildFilePath(vm),
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

    private static Control BuildSlider(ConfigEntryViewModel vm)
    {
        // Single row: label | slider | value box.
        var grid = new Grid
        {
            Margin = new Thickness(0, 3),
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto")
        };
        ToolTip.SetTip(grid, new Binding("Descriptor.Description"));

        var label = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        label.Bind(TextBlock.TextProperty, new Binding("Descriptor.DisplayName"));
        Grid.SetColumn(label, 0);

        var slider = new Slider { VerticalAlignment = VerticalAlignment.Center, MinWidth = 60 };
        slider.Bind(RangeBase.MinimumProperty, new Binding("Minimum"));
        slider.Bind(RangeBase.MaximumProperty, new Binding("Maximum"));
        slider.Bind(RangeBase.ValueProperty, new Binding("DoubleValue") { Mode = BindingMode.TwoWay });
        if (vm.IsInteger)
        {
            slider.IsSnapToTickEnabled = true;
            slider.TickFrequency = 1;
        }

        Grid.SetColumn(slider, 1);

        var valueBox = new TextBox
        {
            Width = 64,
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Right,
            Padding = new Thickness(4, 2)
        };
        valueBox.Bind(TextBox.TextProperty, new Binding("StringValue"));
        Grid.SetColumn(valueBox, 2);

        grid.Children.Add(label);
        grid.Children.Add(slider);
        grid.Children.Add(valueBox);
        return grid;
    }

    private static Control BuildFilePath(ConfigEntryViewModel vm)
    {
        var grid = new Grid
        {
            Margin = new Thickness(0, 3),
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto")
        };
        ToolTip.SetTip(grid, new Binding("Descriptor.Description"));

        var label = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        label.Bind(TextBlock.TextProperty, new Binding("Descriptor.DisplayName"));
        Grid.SetColumn(label, 0);

        var textBox = new TextBox { VerticalAlignment = VerticalAlignment.Center };
        textBox.Bind(TextBox.TextProperty, new Binding("StringValue"));
        Grid.SetColumn(textBox, 1);

        var browse = new Button
        {
            Content = "…",
            Margin = new Thickness(4, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(browse, 2);
        browse.Click += async (_, _) => await BrowseAsync(browse, vm);

        grid.Children.Add(label);
        grid.Children.Add(textBox);
        grid.Children.Add(browse);
        return grid;
    }

    private static async Task BrowseAsync(Visual anchor, ConfigEntryViewModel vm)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(anchor);
            if (topLevel is null)
                return;

            var startLocation = await GetStartLocationAsync(topLevel, vm.StringValue);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Select {vm.Descriptor.DisplayName}",
                AllowMultiple = false,
                FileTypeFilter = BuildFilters(vm.Descriptor.FileExtensions),
                SuggestedStartLocation = startLocation
            });

            var path = files.Count > 0 ? files[0].TryGetLocalPath() : null;
            if (!string.IsNullOrEmpty(path))
                vm.StringValue = path;
        }
        catch
        {
            // A failed/cancelled picker must never bring down the UI.
        }
    }

    private static async Task<IStorageFolder?> GetStartLocationAsync(TopLevel topLevel, string? currentPath)
    {
        var dir = string.IsNullOrWhiteSpace(currentPath) ? null : System.IO.Path.GetDirectoryName(currentPath);
        if (string.IsNullOrEmpty(dir))
            return null;

        try
        {
            return await topLevel.StorageProvider.TryGetFolderFromPathAsync(dir);
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyList<FilePickerFileType>? BuildFilters(string[]? extensions)
    {
        if (extensions is null || extensions.Length == 0)
            return null;

        var patterns = extensions
            .Select(e => e.StartsWith('.') ? $"*{e}" : $"*.{e}")
            .ToArray();

        return
        [
            new FilePickerFileType("Animation files") { Patterns = patterns },
            new FilePickerFileType("All files") { Patterns = ["*"] }
        ];
    }
}
