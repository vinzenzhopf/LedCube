using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.UI.Controls.CubeView2D;

[ObservableObject]
public partial class CubeView2D : UserControl
{
    public static readonly DependencyProperty GridWidthProperty = DependencyProperty.Register(
        nameof(GridWidth), typeof(int), typeof(CubeView2D),
        new FrameworkPropertyMetadata(8, OnGridDimensionsChanged));

    public static readonly DependencyProperty GridHeightProperty = DependencyProperty.Register(
        nameof(GridHeight), typeof(int), typeof(CubeView2D),
        new FrameworkPropertyMetadata(8, OnGridDimensionsChanged));
    
    public static readonly DependencyProperty SelectedPlaneProperty = DependencyProperty.Register(
        nameof(SelectedPlane), typeof(int), typeof(CubeView2D),
        new FrameworkPropertyMetadata(0, OnGridDimensionsChanged));

    private static void OnGridDimensionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        (d as CubeView2D)?.OnGridDimensionsChanged(e);

    public static readonly DependencyProperty LedBrushProperty = DependencyProperty.Register(
        nameof(LedBrush), typeof(Brush), typeof(CubeView2D),
        new FrameworkPropertyMetadata(Brushes.Blue));
    private static void OnLedBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        (d as CubeView2D)?.OnLedBrushChanged(e);

    public static readonly DependencyProperty ShowNumbersProperty = DependencyProperty.Register(
        nameof(ShowNumbers), typeof(bool), typeof(CubeView2D),
        new FrameworkPropertyMetadata(true));

    private static void OnShowNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        (d as CubeView2D)?.OnShowNumbersChanged(e);

    public int GridWidth
    {
        get => (int)GetValue(GridWidthProperty);
        set => SetValue(GridWidthProperty, value);
    }
    
    public int GridHeight
    {
        get => (int)GetValue(GridHeightProperty);
        set => SetValue(GridHeightProperty, value);
    }
    
    public int SelectedPlane
    {
        get => (int)GetValue(SelectedPlaneProperty);
        set => SetValue(SelectedPlaneProperty, value);
    }
    
    public Brush LedBrush{
        get => (Brush)GetValue(LedBrushProperty);
        set => SetValue(LedBrushProperty, value);
    }

    public bool ShowNumbers{
        get => (bool)GetValue(ShowNumbersProperty);
        set => SetValue(ShowNumbersProperty, value);
    }

    [ObservableProperty]
    private CubeConfig _cubeConfig;

    [ObservableProperty]
    private Plane<BiLed> _planeData;
    
    [ObservableProperty]
    private Orientation3D _viewDirection = Orientation3D.Front;

    public ObservableCollection<int> AllPlanes { get; } = new();
    
    public ObservableCollection<int> SelectedPlanes { get; } = new();

    public CubeView2D()
    {
        InitializeComponent();
        for (var i = 0; i < 8; i++)
        {
            AllPlanes.Add(i);
        }
    }
    
    [RelayCommand]
    private void SelectedItemsChanged(SelectionChangedEventArgs a)
    {
        
    }
    
    private void OnGridDimensionsChanged(DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        // throw new System.NotImplementedException();
    }
    private void OnLedBrushChanged(DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        // throw new System.NotImplementedException();
    }
    
    private void OnShowNumbersChanged(DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        // throw new System.NotImplementedException();
    }
}