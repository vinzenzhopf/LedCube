using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.CubeData;
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
    
    public static readonly DependencyProperty GridSizeProperty = DependencyProperty.Register(
        nameof(GridSize), typeof(Point2D), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(new Point2D(8,8), OnGridDimensionsChanged));
    
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
        nameof(PlaneData), typeof(IPlaneData), typeof(CubeView2DGrid),
        new FrameworkPropertyMetadata(null, OnPlaneDataChanged));
    
    private static void OnPlaneDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => 
        (d as CubeView2DGrid)?.OnPlaneDataChanged(e);
    
    public Point2D GridSize
    {
        get => (Point2D)GetValue(GridSizeProperty);
        set => SetValue(GridSizeProperty, value);
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
    
    public IPlaneData? PlaneData{
        get => (IPlaneData?)GetValue(PlaneDataProperty);
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
    
    private bool _disableLedChangedEventPropagation;

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
            Rows = GridSize.Y,
            Columns = GridSize.X,
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
        if (PlaneData is not IPlaneData vm)
        {
            return;
        }

        try
        {
            _disableLedChangedEventPropagation = true;
            for (var index = 0; index < _leds.Count; index++)
            {

                _leds[index].IsChecked = vm.GetLed(IndexToCoordinates(vm.Size, index));
            }
        }
        finally
        {
            _disableLedChangedEventPropagation = false;
        }
    }
    
    public static Point2D IndexToCoordinates(Point2D size, int index) => new(
        index % size.X,
        (index / size.X) % size.Y
    );

    public static int CoordinatesToIndex(Point2D size, Point2D p) =>
        p.X + p.Y * size.Y;

    private void OnLedBrushChanged(DependencyPropertyChangedEventArgs e)
    {
        foreach (var led in _leds)
        {
            led.Foreground = LedBrush;
        }
    }
    
    private void OnPlaneDataChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is IPlaneData oldVm)
        {
            oldVm.LedChanged -= OnDataLedChanged;
            oldVm.PlaneChanged -= OnDataPlaneChanged;
        }
        if (e.NewValue is IPlaneData newVm)
        {
            newVm.LedChanged += OnDataLedChanged;
            newVm.PlaneChanged += OnDataPlaneChanged;
            GridSize = newVm.Size;
        }
    }

    private void OnDataLedChanged(Point2D led, bool value)
    {
        var index = led.Y * GridSize.X + led.X;
        Application.Current.Dispatcher.Invoke(() =>
        {
            _leds[index].IsChecked = value;
        });
    }

    private void OnDataPlaneChanged(IPlaneData data)
    {
        GridSize = data.Size;
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
        
        for (var x = 0; x < GridSize.X; x++)
        {
            _numbersGrid[0].Children.Add(new TextBlock()
            {
                Text = $"{x}",
                Style = style
            });
            _numbersGrid[1].Children.Add(new TextBlock()
            {
                Text = $"{x}",
                Style = style
            });
        }
        for (var y = 0; y < GridSize.Y; y++)
        {
            _numbersGrid[2].Children.Add(new TextBlock()
            {
                Text = $"{GridSize.Y-1-y}",
                Style = style
            });
            _numbersGrid[3].Children.Add(new TextBlock()
            {
                Text = $"{GridSize.Y-1-y}",
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
        _ledGrid.Rows = GridSize.Y;
        _ledGrid.Columns = GridSize.X;
        
        var sizeX = Width / GridSize.X;
        var sizeY = Height / GridSize.Y;
        var size = Math.Min(sizeX, sizeY);
        if (size is Double.NaN)
            size = 10;
        var max = _ledGrid.Rows * _ledGrid.Columns;
        for (var i = 0; i < max; i++)
        {
            var led = new CubeView2DLed
            {
                Size = size,
                Index = i,
                Foreground = LedBrush,
            };
            led.Checked += OnLedChecked;
            led.Unchecked += OnLedUnchecked;
            _leds.Add(led);
        }
        for (var i = 0; i < max; i++)
        {
            var x = i % _ledGrid.Columns;
            var y = _ledGrid.Rows - 1 - (i / _ledGrid.Columns);
            
            var led = _leds[x+y*_ledGrid.Columns];
            _ledGrid.Children.Add(led);
            Grid.SetRow(led, y);
            Grid.SetColumn(led, x);
        }
    }

    private void OnLedChecked(object sender, RoutedEventArgs e)
    {
        if (_disableLedChangedEventPropagation)
            return;
        if (sender is not CubeView2DLed led)
            return;
        HandleLedChanged(led.Index, true);
    }

    private void OnLedUnchecked(object sender, RoutedEventArgs e)
    {
        if (_disableLedChangedEventPropagation)
            return;
        if (sender is not CubeView2DLed led)
            return;
        HandleLedChanged(led.Index, false);
    }

    private void HandleLedChanged(int index, bool value)
    {
        Log.Information("LED with index {0} changed to {1}", index, value);
        if (PlaneData is not IPlaneData vm)
        {
            return;
        }
        vm.SetLed(IndexToCoordinates(vm.Size, index), value);
    }
    
    private void RecalculateSize()
    {
        var innerWidth = ActualWidth - Margin.Left - Margin.Right - Padding.Left - Padding.Right;
        var innerHeight = ActualHeight - Margin.Top - Margin.Bottom - Padding.Top - Padding.Bottom;
        var ledWidth = innerWidth / GridSize.X;
        var ledHeight = innerHeight / GridSize.Y;
        var ledSize = Math.Min(ledWidth, ledHeight);
        _innerGrid.Width = ledSize * GridSize.X;
        _innerGrid.Height = ledSize * GridSize.Y;
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