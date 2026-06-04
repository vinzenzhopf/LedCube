using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace LedCube.Core.UI.Controls.AnimationList;

public partial class AnimationList : UserControl
{
    private AnimationListEntryViewModel? _pressedEntry;
    private PointerPressedEventArgs? _pressArgs;
    private Point _pressOrigin;

    public AnimationList()
    {
        InitializeComponent();
    }

    private void OnItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is Control { DataContext: AnimationListEntryViewModel entry } &&
            DataContext is AnimationListViewModel vm)
        {
            vm.AddToPlaylistCommand.Execute(entry);
        }
    }

    private void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        _pressedEntry = (e.Source as Control)?.DataContext as AnimationListEntryViewModel;
        _pressArgs = e;
        _pressOrigin = e.GetPosition(this);
    }

    private async void OnItemPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pressedEntry is null || _pressArgs is null)
            return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _pressedEntry = null;
            _pressArgs = null;
            return;
        }

        // Only start a drag once the pointer has moved past a small threshold, so a normal
        // click still selects the item.
        var pos = e.GetPosition(this);
        if (Math.Abs(pos.X - _pressOrigin.X) < 4 && Math.Abs(pos.Y - _pressOrigin.Y) < 4)
            return;

        var entry = _pressedEntry;
        var args = _pressArgs;
        _pressedEntry = null;
        _pressArgs = null;

        var data = new DataTransfer();
        data.Add(DataTransferItem.Create(AnimationListEntryViewModel.DragFormat, entry));
        try
        {
            await DragDrop.DoDragDropAsync(args, data, DragDropEffects.Copy);
        }
        catch (Exception)
        {
            // Drag operations can throw if interrupted; nothing actionable here.
        }
    }
}
