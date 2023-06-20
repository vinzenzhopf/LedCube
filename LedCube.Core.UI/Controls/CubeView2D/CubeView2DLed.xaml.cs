using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Core.UI.Controls.CubeView2D;

public partial class CubeView2DLed : CheckBox
{
    public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
        nameof(Index), typeof(int), typeof(CubeView2DLed),
        new FrameworkPropertyMetadata(0));
    
    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size), typeof(double), typeof(CubeView2DLed),
        new FrameworkPropertyMetadata(Double.NaN));
    
    public int Index{
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }
    
    public double Size{
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
    
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