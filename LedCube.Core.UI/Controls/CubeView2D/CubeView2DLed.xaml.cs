using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Core.UI.Controls.CubeView2D;

[ObservableObject]
public partial class CubeView2DLed : CheckBox
{
    [ObservableProperty]
    private int _index;
    
    [ObservableProperty]
    private double _size;
    
    public CubeView2DLed()
    {
        InitializeComponent();
        this.SizeChanged += OnWindowSizeChanged;
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var innerWidth = this.ActualWidth - Margin.Left - Margin.Right - Padding.Left - Padding.Right;
        var innerHeight = this.ActualHeight - Margin.Top - Margin.Bottom - Padding.Top - Padding.Bottom;
        var minSize = Math.Min(innerWidth, innerHeight);
        Size = minSize;
    }
}