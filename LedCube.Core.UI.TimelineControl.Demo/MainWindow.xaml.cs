using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Core.UI.TimelineControl.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ObservableObject]
public partial class MainWindow : Window

{
    [ObservableProperty] private double _scaleValue = 3.0;
    [ObservableProperty] private int _startValue = 0;
    [ObservableProperty] private int _endValue = 100;
    [ObservableProperty] private int _majorTicks = 10;
    [ObservableProperty] private int _minorTicks = 1;
    [ObservableProperty] private bool _readOnlyValue = false;
    [ObservableProperty] private int _cursorPosition = 42;

    public MainWindow()
    {
        InitializeComponent();

        // Timeline.AddElement(12);
        // Timeline.AddElement(18);
    }
}