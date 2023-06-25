using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Security.Cryptography.Pkcs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.UI.Controls.ViewModels;
using Serilog;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace LedCube.Core.UI.Controls.CubeView2D;

public partial class CubeView2DGrid : UserControl
{
    internal enum SelectionType
    {
        Enable,
        Disable,
        Toggle
    }

    public static readonly DependencyProperty GridWidthProperty = DependencyProperty.Register(
        nameof(GridWidth), typeof(int), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(8, OnGridDimensionsChanged));

    public static readonly DependencyProperty GridHeightProperty = DependencyProperty.Register(
        nameof(GridHeight), typeof(int), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(8, OnGridDimensionsChanged));
    
    private static void OnGridDimensionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        (d as CubeView2DGrid)?.OnGridDimensionsChanged(e);

    public static readonly DependencyProperty LedBrushProperty = DependencyProperty.Register(
        nameof(LedBrush), typeof(Brush), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(Brushes.Blue));
    
    private static void OnLedBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        (d as CubeView2DGrid)?.OnLedBrushChanged(e);

    public static readonly DependencyProperty EnableSelectionBrushProperty = DependencyProperty.Register(
        nameof(EnableSelectionBrush), typeof(Brush), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(Brushes.Green));

    public static readonly DependencyProperty DisableSelectionBrushProperty = DependencyProperty.Register(
        nameof(DisableSelectionBrush), typeof(Brush), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(Brushes.Red));
    
    public static readonly DependencyProperty ToggleSelectionBrushProperty = DependencyProperty.Register(
        nameof(ToggleSelectionBrush), typeof(Brush), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(Brushes.Yellow));
    
    public static readonly DependencyProperty ShowNumbersProperty = DependencyProperty.Register(
        nameof(ShowNumbers), typeof(bool), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(true));
    
    private static void OnShowNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        (d as CubeView2DGrid)?.OnShowNumbersChanged(e);

    public static readonly DependencyProperty PlaneDataProperty = DependencyProperty.Register(
        nameof(PlaneData), typeof(PlaneViewModel), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(null));
    
    private static void OnPlaneDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        (d as CubeView2DGrid)?.OnPlaneDataChanged(e);

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
    
    public Brush LedBrush{
        get => (Brush)GetValue(LedBrushProperty);
        set => SetValue(LedBrushProperty, value);
    }
    
    public Brush EnableSelectionBrush{
        get => (Brush)GetValue(EnableSelectionBrushProperty);
        set => SetValue(EnableSelectionBrushProperty, value);
    }
    
    public Brush DisableSelectionBrush{
        get => (Brush)GetValue(DisableSelectionBrushProperty);
        set => SetValue(DisableSelectionBrushProperty, value);
    }
    
    public Brush ToggleSelectionBrush{
        get => (Brush)GetValue(ToggleSelectionBrushProperty);
        set => SetValue(ToggleSelectionBrushProperty, value);
    }
    
    public bool ShowNumbers{
        get => (bool)GetValue(ShowNumbersProperty);
        set => SetValue(ShowNumbersProperty, value);
    }
    
    public PlaneViewModel? PlaneData{
        get => (PlaneViewModel?)GetValue(PlaneDataProperty);
        set => SetValue(PlaneDataProperty, value);
    }

    private readonly List<CubeView2DLed> _leds = new();
    private readonly Grid _grid;
    private readonly Grid _innerGrid;
    private readonly UniformGrid _ledGrid;
    private readonly Canvas _selectionCanvas;
    private Point? _startPoint;
    private SelectionType _selectionType = SelectionType.Enable;
    private Rectangle _selectionRectangle;
    private readonly UniformGrid[] _numbersGrid;
    
    public CubeView2DGrid()
    {
        _grid = new Grid();
        _innerGrid = new Grid()
        {
            ColumnDefinitions = {
                new ColumnDefinition() {Width = GridLength.Auto},
                new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition() {Width = GridLength.Auto}
            },
            RowDefinitions = {
                new RowDefinition() {Height = GridLength.Auto},
                new RowDefinition() {Height = new GridLength(1, GridUnitType.Star)},
                new RowDefinition() {Height = GridLength.Auto}
            },
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _ledGrid = new UniformGrid()
        {
            Rows = GridHeight,
            Columns = GridWidth,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _innerGrid.Children.Add(_ledGrid);
        Grid.SetRow(_ledGrid, 1);
        Grid.SetColumn(_ledGrid, 1);
        _numbersGrid = new[]
        {
            new UniformGrid() {Rows = 1}, //Top
            new UniformGrid() {Rows = 1}, //Bottom
            new UniformGrid() {Columns = 1}, //Left
            new UniformGrid() {Columns = 1} //Right
        };
        _innerGrid.Children.Add(_numbersGrid[0]);
        _innerGrid.Children.Add(_numbersGrid[1]);
        _innerGrid.Children.Add(_numbersGrid[2]);
        _innerGrid.Children.Add(_numbersGrid[3]);
        Grid.SetRow(_numbersGrid[0], 0);
        Grid.SetColumn(_numbersGrid[0], 1);
        Grid.SetRow(_numbersGrid[1], 2);
        Grid.SetColumn(_numbersGrid[1], 1);
        Grid.SetRow(_numbersGrid[2], 1);
        Grid.SetColumn(_numbersGrid[2], 0);
        Grid.SetRow(_numbersGrid[3], 1);
        Grid.SetColumn(_numbersGrid[3], 2);
        
        _selectionCanvas = new Canvas()
        {
            IsHitTestVisible = false,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _selectionRectangle = new Rectangle()
        {
            Visibility = Visibility.Hidden,
            IsHitTestVisible = false,
            Fill = EnableSelectionBrush,
            Opacity = 0.3,
            Stroke = EnableSelectionBrush,
            StrokeThickness = 3
        };
        _selectionCanvas.Children.Add(_selectionRectangle);
        
        InitializeComponent();
        CreateGridNumbers();
        CreateGrid();
        UpdateLedStatus();
        
        //Finally add everything to the outer grid and content.
        _grid.Children.Add(_innerGrid);
        _grid.Children.Add(_selectionCanvas);
        Content = _grid;
        SizeChanged += OnWindowSizeChanged;
    }
    
    private void OnShowNumbersChanged(DependencyPropertyChangedEventArgs e)
    {
        CreateGridNumbers();
    }

    private void OnGridDimensionsChanged(DependencyPropertyChangedEventArgs e)
    {
        CreateGrid();
        CreateGridNumbers();
        RecalculateSize();
        UpdateLedStatus();
    }

    private void UpdateLedStatus()
    {
        if (PlaneData is not PlaneViewModel vm)
        {
            return;
        }
        for (var index = 0; index < _leds.Count; index++)
        {
            _leds[index].IsChecked = vm.GetLed(index);
        }
    }

    private void OnLedBrushChanged(DependencyPropertyChangedEventArgs e)
    {
        foreach (var led in _leds)
        {
            led.Foreground = LedBrush;
        }
    }
    
    private void OnPlaneDataChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is PlaneViewModel oldVm)
        {
            oldVm.LedChanged -= OnVmLedChanged;
            oldVm.PlaneChanged -= OnVmPlaneChanged;
        }
        if (e.NewValue is PlaneViewModel newVm)
        {
            newVm.LedChanged += OnVmLedChanged;
            newVm.PlaneChanged += OnVmPlaneChanged;
        }
    }

    private void OnVmLedChanged(int index, bool? value)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _leds[index].IsChecked = value;
        });
    }

    private void OnVmPlaneChanged(IPlaneData data)
    {
        UpdateLedStatus();
    }

    private void CreateGridNumbers()
    {
        _numbersGrid[0].Children.Clear();//Top
        _numbersGrid[1].Children.Clear();//Bottom
        _numbersGrid[2].Children.Clear();//Left
        _numbersGrid[3].Children.Clear();//Right
        if (!ShowNumbers)
        {
            return;
        }
        
        var style = FindResource("NumberGridTextStyle") as Style;
        
        for (var x = 0; x < GridWidth; x++)
        {
            _numbersGrid[0].Children.Add(new TextBlock()
            {
                Text = $"{GridWidth-x}",
                Style = style
            });
            _numbersGrid[1].Children.Add(new TextBlock()
            {
                Text = $"{GridWidth-x}",
                Style = style
            });
        }
        for (var y = 0; y < GridHeight; y++)
        {
            _numbersGrid[2].Children.Add(new TextBlock()
            {
                Text = $"{GridHeight-y}",
                Style = style
            });
            _numbersGrid[3].Children.Add(new TextBlock()
            {
                Text = $"{GridHeight-y}",
                Style = style
            });
        }
    }

    private void CreateGrid()
    {
        _ledGrid.Children.Clear();
        foreach (var led in _leds)
        {
            led.Checked -= OnLedChecked;
            led.Unchecked -= OnLedUnchecked;
        }        
        _leds.Clear();
        _ledGrid.Rows = GridHeight;
        _ledGrid.Columns = GridWidth;
        
        var sizeX = Width / GridWidth;
        var sizeY = Height / GridHeight;
        var size = Math.Min(sizeX, sizeY);
        if (size is Double.NaN)
            size = 10;
        var ledCount = GridWidth * GridHeight;
        for (var y = 0; y < GridWidth; y++)
        {
            for (var x = 0; x < GridHeight; x++)
            {
                var led = new CubeView2DLed
                {
                    Size = size,
                    Index = --ledCount,
                    Foreground = LedBrush,
                };
                led.Checked += OnLedChecked;
                led.Unchecked += OnLedUnchecked;
                Grid.SetRow(led, x);
                Grid.SetColumn(led, y);
                _ledGrid.Children.Add(led);
                _leds.Add(led);
            }
        }
    }

    private void OnLedChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not CubeView2DLed led)
            return;
        HandleLedChanged(led.Index, true);
    }

    private void OnLedUnchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not CubeView2DLed led)
            return;
        HandleLedChanged(led.Index, false);
    }

    private void HandleLedChanged(int index, bool value)
    {
        Log.Information("LED with index {0} changed to {1}", index, value);
        if (PlaneData is not PlaneViewModel vm)
        {
            return;
        }
        if (value) vm.SetLed(index, value);
    }
    
    private void RecalculateSize()
    {
        var innerWidth = ActualWidth - Margin.Left - Margin.Right - Padding.Left - Padding.Right;
        var innerHeight = ActualHeight - Margin.Top - Margin.Bottom - Padding.Top - Padding.Bottom;
        var ledWidth = innerWidth / GridWidth;
        var ledHeight = innerHeight / GridHeight;
        var ledSize = Math.Min(ledWidth, ledHeight);
        _innerGrid.Width = ledSize * GridWidth;
        _innerGrid.Height = ledSize * GridHeight;
    }
        
    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RecalculateSize();
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (e.ChangedButton)
        {
            case MouseButton.Left:
                MouseDragButtonDown(e.GetPosition(this), SelectionType.Enable);
                break;
            case MouseButton.Middle:
                MouseDragButtonDown(e.GetPosition(this), SelectionType.Toggle);
                break;
            case MouseButton.Right:
                MouseDragButtonDown(e.GetPosition(this), SelectionType.Disable);
                break;
        }
        base.OnPreviewMouseDown(e);
    }

    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (e.ChangedButton)
        {
            case MouseButton.Left:
                MouseDragButtonUp(e.GetPosition(this), SelectionType.Enable);
                break;
            case MouseButton.Middle:
                MouseDragButtonUp(e.GetPosition(this), SelectionType.Toggle);
                break;
            case MouseButton.Right:
                MouseDragButtonUp(e.GetPosition(this), SelectionType.Disable);
                break;
        }
        base.OnPreviewMouseUp(e);
    }

    private void ResetSelectionRectangle()
    {
        _selectionRectangle.Visibility = Visibility.Hidden;
        return;
    }
    
    private void UpdateSelectionRectangle(Point p1, Point p2)
    {
        var rect = new Rect(p1, p2);
        if (rect is {Width: < 5, Height: < 5})
        {
            ResetSelectionRectangle();
            return;
        }
        
        //  show the rectangle again
        _selectionRectangle.Visibility = Visibility.Visible;
        switch (_selectionType)
        {
            case SelectionType.Disable:
                _selectionRectangle.Fill = DisableSelectionBrush;
                _selectionRectangle.Stroke = DisableSelectionBrush;
                break;
            case SelectionType.Toggle:
                _selectionRectangle.Fill = ToggleSelectionBrush;
                _selectionRectangle.Stroke = ToggleSelectionBrush;
                break;
            default:
                _selectionRectangle.Fill = EnableSelectionBrush;
                _selectionRectangle.Stroke = EnableSelectionBrush;
                break;
        }
        
        Canvas.SetLeft(_selectionRectangle, Math.Min(p1.X, p2.X));
        Canvas.SetTop(_selectionRectangle, Math.Min(p1.Y, p2.Y));
        _selectionRectangle.Width = Math.Abs((p2.X - p1.X));
        _selectionRectangle.Height = Math.Abs((p2.Y - p1.Y));
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        if (_startPoint is null)
        {
            _selectionRectangle.Visibility = Visibility.Hidden;
            return;
        }
        var start = _startPoint.Value;
        var end = e.GetPosition(this);
        var rect = new Rect(start, end);
        if (rect is {Width: < 5, Height: < 5}) {
            _selectionRectangle.Visibility = Visibility.Hidden;
            return;
        }
        
        UpdateSelectionRectangle(start, end);
        foreach (var led in _leds) {
            var rectBounds = VisualTreeHelper.GetDescendantBounds(led);
            var vector = VisualTreeHelper.GetOffset(led) + 
                         VisualTreeHelper.GetOffset(_ledGrid) + 
                         VisualTreeHelper.GetOffset(_innerGrid);
            rectBounds.Offset(vector);
            // if (rectBounds.IntersectsWith(rect)) {
            //     led.Foreground = Brushes.LightGreen;
            // }
        }
        base.OnPreviewMouseMove(e);
    }

    private void MouseDragButtonDown(Point start, SelectionType type)
    {
        _startPoint = start;
        _selectionType = type;
    }

    private void MouseDragButtonUp(Point end, SelectionType type)
    {
        if (_startPoint is null)
        {
            ResetSelectionRectangle();
            return;
        }
        if(_selectionType != type){
            return;
        }
        var start = _startPoint!.Value;
        _startPoint = null;
        ResetSelectionRectangle();
        
        var rect = new Rect(start, end);
        if (rect is {Width: < 5, Height: < 5}) {
            return;
        }
        foreach (var led in _leds) {
            var rectBounds = VisualTreeHelper.GetDescendantBounds(led);
            var vector = VisualTreeHelper.GetOffset(led) + 
                         VisualTreeHelper.GetOffset(_ledGrid) + 
                         VisualTreeHelper.GetOffset(_innerGrid);
            rectBounds.Offset(vector);
            if (rectBounds.IntersectsWith(rect)) {
                switch (type)
                {
                    case SelectionType.Enable:
                        led.IsChecked = true;
                        break;
                    case SelectionType.Disable:
                        led.IsChecked = false;
                        break;
                    case SelectionType.Toggle:
                        led.IsChecked = !led.IsChecked;
                        break;
                }
            }
        }
    }
}