using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WpfTimelineControl.Element;

/// <summary>
/// Base Class for defining an Element to add to the <see cref="Timeline"/> Control.
/// </summary>
public class TimelineElement : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public event MouseClickedEventHandler? MouseClicked;
    public event MouseDraggingStartedEventHandler? MouseDraggingStarted;
    public event MouseDraggingEndedEventHandler? MouseDraggingEnded;
    
    private int _position = 0;
    private bool _selected = false;
    private DataTemplate? _customDataTemplate = null;
    
    public int Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }

    public bool IsSelected
    {
        get => _selected;
        set => SetField(ref _selected, value);
    }

    public DataTemplate? CustomDataTemplate
    {
        get => _customDataTemplate;
        set
        {
            SetField(ref _customDataTemplate, value);
            OnPropertyChanged(nameof(HasCustomDataTemplate));            
        }
    }

    public bool HasCustomDataTemplate => _customDataTemplate != null;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual void OnMouseClicked(object sender)
    {
        MouseClicked?.Invoke(this, new MouseClickedEventArgs());
    }
    
    protected virtual void OnMouseDraggingStarted(object sender, int start)
    {
        MouseDraggingStarted?.Invoke(this, new MouseDragStartEventArgs(start));
    }
    
    protected virtual void OnMouseDraggingEnded(object sender, int start, int end)
    {
        MouseDraggingEnded?.Invoke(this, new MouseDragEndEventArgs(start, end));
    }
    
    public delegate void MouseClickedEventHandler(object? sender, MouseClickedEventArgs e);
    public delegate void MouseDraggingStartedEventHandler(object? sender, MouseDragStartEventArgs e);
    public delegate void MouseDraggingEndedEventHandler(object? sender, MouseDragEndEventArgs e);
}