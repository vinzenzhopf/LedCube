using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LedCube.Core.UI.Controls.AnimationList;

namespace LedCube.Core.UI.Controls.PlaylistControl;

public partial class PlaylistControl : UserControl
{
    public PlaylistControl()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnItemDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Control { DataContext: PlaylistEntryControlViewModel vm } &&
            DataContext is PlaylistControlViewModel parent)
        {
            parent.PlayEntryCommand.Execute(vm);
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(AnimationListEntryViewModel.DragFormat)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.TryGetValue(AnimationListEntryViewModel.DragFormat) is { } entry &&
            DataContext is PlaylistControlViewModel parent)
        {
            parent.AddFromAnimationEntry(entry);
            e.Handled = true;
        }
    }
}
