using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Event;
using Serilog;

namespace LedCube.Core.UI.Controls.CubeView2D;

public partial class CubeView2DGrid : UserControl
{
    internal enum SelectionType { Enable, Disable, Toggle }

    public static readonly StyledProperty<Point2D> GridSizeProperty =
        AvaloniaProperty.Register<CubeView2DGrid, Point2D>(nameof(GridSize), new Point2D(8, 8));

    public static readonly StyledProperty<IBrush> LedBrushProperty =
        AvaloniaProperty.Register<CubeView2DGrid, IBrush>(nameof(LedBrush), Brushes.Blue);

    public static readonly StyledProperty<IBrush> EnableSelectionBrushProperty =
        AvaloniaProperty.Register<CubeView2DGrid, IBrush>(nameof(EnableSelectionBrush), Brushes.Green);

    public static readonly StyledProperty<IBrush> DisableSelectionBrushProperty =
        AvaloniaProperty.Register<CubeView2DGrid, IBrush>(nameof(DisableSelectionBrush), Brushes.Red);

    public static readonly StyledProperty<IBrush> ToggleSelectionBrushProperty =
        AvaloniaProperty.Register<CubeView2DGrid, IBrush>(nameof(ToggleSelectionBrush), Brushes.Yellow);

    public static readonly StyledProperty<bool> ShowNumbersProperty =
        AvaloniaProperty.Register<CubeView2DGrid, bool>(nameof(ShowNumbers), true);

    public static readonly StyledProperty<IPlaneData?> PlaneDataProperty =
        AvaloniaProperty.Register<CubeView2DGrid, IPlaneData?>(nameof(PlaneData));

    public Point2D GridSize
    {
        get => GetValue(GridSizeProperty);
        set => SetValue(GridSizeProperty, value);
    }

    public IBrush LedBrush
    {
        get => GetValue(LedBrushProperty);
        set => SetValue(LedBrushProperty, value);
    }

    public IBrush EnableSelectionBrush
    {
        get => GetValue(EnableSelectionBrushProperty);
        set => SetValue(EnableSelectionBrushProperty, value);
    }

    public IBrush DisableSelectionBrush
    {
        get => GetValue(DisableSelectionBrushProperty);
        set => SetValue(DisableSelectionBrushProperty, value);
    }

    public IBrush ToggleSelectionBrush
    {
        get => GetValue(ToggleSelectionBrushProperty);
        set => SetValue(ToggleSelectionBrushProperty, value);
    }

    public bool ShowNumbers
    {
        get => GetValue(ShowNumbersProperty);
        set => SetValue(ShowNumbersProperty, value);
    }

    public IPlaneData? PlaneData
    {
        get => GetValue(PlaneDataProperty);
        set => SetValue(PlaneDataProperty, value);
    }

    private readonly List<CubeView2DLed> _leds = new();
    private readonly Grid _grid;
    private readonly Grid _innerGrid;
    private readonly UniformGrid _ledGrid;
    private readonly Canvas _selectionCanvas;
    private Point? _startPoint;
    private SelectionType _selectionType = SelectionType.Enable;
    private readonly Rectangle _selectionRectangle;
    private readonly UniformGrid[] _numbersGrid;
    private bool _disableLedChangedEventPropagation;

    public CubeView2DGrid()
    {
        _grid = new Grid();
        _innerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto)
            },
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        _ledGrid = new UniformGrid
        {
            Rows = GridSize.Y,
            Columns = GridSize.X,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        _innerGrid.Children.Add(_ledGrid);
        Grid.SetRow(_ledGrid, 1);
        Grid.SetColumn(_ledGrid, 1);

        _numbersGrid = new[]
        {
            new UniformGrid { Rows = 1 },    // Top
            new UniformGrid { Rows = 1 },    // Bottom
            new UniformGrid { Columns = 1 }, // Left
            new UniformGrid { Columns = 1 }  // Right
        };
        _innerGrid.Children.Add(_numbersGrid[0]);
        _innerGrid.Children.Add(_numbersGrid[1]);
        _innerGrid.Children.Add(_numbersGrid[2]);
        _innerGrid.Children.Add(_numbersGrid[3]);
        Grid.SetRow(_numbersGrid[0], 0); Grid.SetColumn(_numbersGrid[0], 1);
        Grid.SetRow(_numbersGrid[1], 2); Grid.SetColumn(_numbersGrid[1], 1);
        Grid.SetRow(_numbersGrid[2], 1); Grid.SetColumn(_numbersGrid[2], 0);
        Grid.SetRow(_numbersGrid[3], 1); Grid.SetColumn(_numbersGrid[3], 2);

        _selectionCanvas = new Canvas
        {
            IsHitTestVisible = false,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        _selectionRectangle = new Rectangle
        {
            IsVisible = false,
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

        _grid.Children.Add(_innerGrid);
        _grid.Children.Add(_selectionCanvas);
        Content = _grid;
        SizeChanged += OnWindowSizeChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ShowNumbersProperty)
            CreateGridNumbers();
        else if (change.Property == GridSizeProperty)
        {
            CreateGrid();
            CreateGridNumbers();
            RecalculateSize();
            UpdateLedStatus();
        }
        else if (change.Property == LedBrushProperty)
        {
            foreach (var led in _leds)
                led.Foreground = LedBrush;
        }
        else if (change.Property == PlaneDataProperty)
        {
            if (change.OldValue is IPlaneData oldVm)
            {
                oldVm.LedChanged -= OnDataLedChanged;
                oldVm.PlaneChanged -= OnDataPlaneChanged;
            }
            if (change.NewValue is IPlaneData newVm)
            {
                newVm.LedChanged += OnDataLedChanged;
                newVm.PlaneChanged += OnDataPlaneChanged;
                GridSize = newVm.Size;
            }
        }
    }

    private void UpdateLedStatus()
    {
        if (PlaneData is not IPlaneData vm) return;
        try
        {
            _disableLedChangedEventPropagation = true;
            for (var index = 0; index < _leds.Count; index++)
                _leds[index].IsChecked = vm.GetLed(IndexToCoordinates(vm.Size, index));
        }
        finally
        {
            _disableLedChangedEventPropagation = false;
        }
    }

    public static Point2D IndexToCoordinates(Point2D size, int index) =>
        new(index % size.X, (index / size.X) % size.Y);

    public static int CoordinatesToIndex(Point2D size, Point2D p) =>
        p.X + p.Y * size.Y;

    private void OnDataLedChanged(object? sender, LegChangedEventArgs<Point2D> args)
    {
        var index = args.Position.Y * GridSize.X + args.Position.X;
        Dispatcher.UIThread.Post(() => _leds[index].IsChecked = args.Value);
    }

    private void OnDataPlaneChanged(object? sender, EventArgs args)
    {
        if (sender is IPlaneData data)
            GridSize = data.Size;
        UpdateLedStatus();
    }

    private void CreateGridNumbers()
    {
        foreach (var grid in _numbersGrid)
            grid.Children.Clear();
        if (!ShowNumbers) return;

        for (var x = 0; x < GridSize.X; x++)
        {
            _numbersGrid[0].Children.Add(new TextBlock { Text = $"{x}" });
            _numbersGrid[1].Children.Add(new TextBlock { Text = $"{x}" });
        }
        for (var y = 0; y < GridSize.Y; y++)
        {
            _numbersGrid[2].Children.Add(new TextBlock { Text = $"{GridSize.Y - 1 - y}" });
            _numbersGrid[3].Children.Add(new TextBlock { Text = $"{GridSize.Y - 1 - y}" });
        }
    }

    private void CreateGrid()
    {
        _ledGrid.Children.Clear();
        foreach (var led in _leds)
            led.IsCheckedChanged -= OnLedCheckedChanged;
        _leds.Clear();
        _ledGrid.Rows = GridSize.Y;
        _ledGrid.Columns = GridSize.X;

        var sizeX = double.IsNaN(Width) ? 10 : Width / GridSize.X;
        var sizeY = double.IsNaN(Height) ? 10 : Height / GridSize.Y;
        var size = Math.Min(sizeX, sizeY);

        var max = _ledGrid.Rows * _ledGrid.Columns;
        for (var i = 0; i < max; i++)
        {
            var led = new CubeView2DLed { Size = size, Index = i, Foreground = LedBrush };
            led.IsCheckedChanged += OnLedCheckedChanged;
            _leds.Add(led);
        }
        for (var i = 0; i < max; i++)
        {
            var x = i % _ledGrid.Columns;
            var y = _ledGrid.Rows - 1 - (i / _ledGrid.Columns);
            var led = _leds[x + y * _ledGrid.Columns];
            _ledGrid.Children.Add(led);
            Grid.SetRow(led, y);
            Grid.SetColumn(led, x);
        }
    }

    private void OnLedCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (_disableLedChangedEventPropagation) return;
        if (sender is not CubeView2DLed led) return;
        HandleLedChanged(led.Index, led.IsChecked == true);
    }

    private void HandleLedChanged(int index, bool value)
    {
        Log.Information("LED with index {0} changed to {1}", index, value);
        if (PlaneData is not IPlaneData vm) return;
        vm.SetLed(IndexToCoordinates(vm.Size, index), value);
    }

    private void RecalculateSize()
    {
        var innerWidth = Bounds.Width - Margin.Left - Margin.Right - Padding.Left - Padding.Right;
        var innerHeight = Bounds.Height - Margin.Top - Margin.Bottom - Padding.Top - Padding.Bottom;
        var ledWidth = innerWidth / GridSize.X;
        var ledHeight = innerHeight / GridSize.Y;
        var ledSize = Math.Min(ledWidth, ledHeight);
        _innerGrid.Width = ledSize * GridSize.X;
        _innerGrid.Height = ledSize * GridSize.Y;
    }

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        RecalculateSize();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        if (props.IsLeftButtonPressed)
            MouseDragButtonDown(e.GetPosition(this), SelectionType.Enable);
        else if (props.IsMiddleButtonPressed)
            MouseDragButtonDown(e.GetPosition(this), SelectionType.Toggle);
        else if (props.IsRightButtonPressed)
            MouseDragButtonDown(e.GetPosition(this), SelectionType.Disable);
        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var type = e.InitialPressMouseButton switch
        {
            MouseButton.Middle => SelectionType.Toggle,
            MouseButton.Right => SelectionType.Disable,
            _ => SelectionType.Enable
        };
        MouseDragButtonUp(e.GetPosition(this), type);
        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_startPoint is null)
        {
            _selectionRectangle.IsVisible = false;
            return;
        }
        var start = _startPoint.Value;
        var end = e.GetPosition(this);
        var rect = new Rect(
            Math.Min(start.X, end.X), Math.Min(start.Y, end.Y),
            Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
        if (rect.Width < 5 && rect.Height < 5)
        {
            _selectionRectangle.IsVisible = false;
            return;
        }
        UpdateSelectionRectangle(start, end);
        base.OnPointerMoved(e);
    }

    private void ResetSelectionRectangle()
    {
        _selectionRectangle.IsVisible = false;
    }

    private void UpdateSelectionRectangle(Point p1, Point p2)
    {
        var rect = new Rect(
            Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
            Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
        if (rect.Width < 5 && rect.Height < 5)
        {
            ResetSelectionRectangle();
            return;
        }
        _selectionRectangle.IsVisible = true;
        var brush = _selectionType switch
        {
            SelectionType.Disable => DisableSelectionBrush,
            SelectionType.Toggle => ToggleSelectionBrush,
            _ => EnableSelectionBrush
        };
        _selectionRectangle.Fill = brush;
        _selectionRectangle.Stroke = brush;
        Canvas.SetLeft(_selectionRectangle, rect.Left);
        Canvas.SetTop(_selectionRectangle, rect.Top);
        _selectionRectangle.Width = rect.Width;
        _selectionRectangle.Height = rect.Height;
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
        if (_selectionType != type) return;

        var start = _startPoint.Value;
        _startPoint = null;
        ResetSelectionRectangle();

        var selectionRect = new Rect(
            Math.Min(start.X, end.X), Math.Min(start.Y, end.Y),
            Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
        if (selectionRect.Width < 5 && selectionRect.Height < 5) return;

        foreach (var led in _leds)
        {
            var topLeft = led.TranslatePoint(new Point(0, 0), this);
            if (topLeft is null) continue;
            var ledBounds = new Rect(topLeft.Value, new Size(led.Bounds.Width, led.Bounds.Height));
            if (!ledBounds.Intersects(selectionRect)) continue;
            switch (type)
            {
                case SelectionType.Enable: led.IsChecked = true; break;
                case SelectionType.Disable: led.IsChecked = false; break;
                case SelectionType.Toggle: led.IsChecked = !led.IsChecked; break;
            }
        }
    }
}
