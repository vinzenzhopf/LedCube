using System.Windows;
using System.Windows.Controls;

namespace LedCube.Core.UI.Controls.SettingsDialog;

public static class TreeViewSelectedItem
{
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewSelectedItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnChanged));

    public static object GetSelectedItem(DependencyObject obj) => obj.GetValue(SelectedItemProperty);
    public static void SetSelectedItem(DependencyObject obj, object value) => obj.SetValue(SelectedItemProperty, value);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TreeView tv) return;
        tv.SelectedItemChanged -= Tv_SelectedItemChanged;
        tv.SelectedItemChanged += Tv_SelectedItemChanged;
    }

    private static void Tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        => ((TreeView)sender).SetValue(SelectedItemProperty, e.NewValue);
}
