using System;
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
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace LedCube.Core.UI.Controls.CubeView2D;

[ObservableObject]
public partial class CubeView2DGrid : UserControl
{
    internal enum SelectionType
    {
        Enable,
        Disable,
        Toggle
    }
    
    [ObservableProperty]
    private int _x = 8;

    [ObservableProperty]
    private int _y = 8;
    
    [ObservableProperty]
    private IPlane<BiLed>? _planeData;

    [ObservableProperty]
    private Brush _ledBrush = Brushes.Blue;
    
    [ObservableProperty]
    private Brush _enableSelectionBrush = Brushes.Green;
    
    [ObservableProperty]
    private Brush _disableSelectionBrush = Brushes.Red;
    
    [ObservableProperty]
    private Brush _toggleSelectionBrush = Brushes.Yellow;
    
    private readonly List<CubeView2DLed> _leds = new();
    private readonly Grid _grid;
    private readonly UniformGrid _ledGrid;
    private readonly Canvas _selectionCanvas;
    private Point? _startPoint;
    private SelectionType _selectionType = SelectionType.Enable;
    private Rectangle _selectionRectangle;
    
    public CubeView2DGrid()
    {
        _grid = new Grid();
        _ledGrid = new UniformGrid()
        {
            Rows = Y,
            Columns = X,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
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
            Fill = _enableSelectionBrush,
            Opacity = 0.3,
            Stroke = _enableSelectionBrush,
            StrokeThickness = 3
        };
        _grid.Children.Add(_ledGrid);
        _selectionCanvas.Children.Add(_selectionRectangle);
        _grid.Children.Add(_selectionCanvas);
        CreateGrid();
        Content = _grid;
        SizeChanged += OnWindowSizeChanged;
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
            var vector = VisualTreeHelper.GetOffset(led) + VisualTreeHelper.GetOffset(_ledGrid);
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
            var vector = VisualTreeHelper.GetOffset(led) + VisualTreeHelper.GetOffset(_ledGrid);
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
    
    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var innerWidth = ActualWidth - Margin.Left - Margin.Right - Padding.Left - Padding.Right;
        var innerHeight = ActualHeight - Margin.Top - Margin.Bottom - Padding.Top - Padding.Bottom;
        var innerSize = Math.Min(innerWidth, innerHeight);
        _ledGrid.Width = innerSize;
        _ledGrid.Height = innerSize;
    }

    private void CreateGrid()
    {
        _leds.Clear();
        _ledGrid.Children.Clear();
        _ledGrid.Rows = Y;
        _ledGrid.Columns = X;
        
        var sizeX = Width / X;
        var sizeY = Height / Y;
        var size = Math.Min(sizeX, sizeY);
        if (size is Double.NaN)
            size = 10;
        for (var i = 0; i < _ledGrid.Rows; i++)
        {
            for (var j = 0; j < _ledGrid.Columns; j++)
            {
                var rect = new CubeView2DLed
                {
                    Size = size,
                    Index = i + j*_ledGrid.Columns,
                    Foreground = LedBrush
                };
                Grid.SetRow(rect, i);
                Grid.SetColumn(rect, j);
                _ledGrid.Children.Add(rect);
                _leds.Add(rect);
            }
        }

    }
    
    
}