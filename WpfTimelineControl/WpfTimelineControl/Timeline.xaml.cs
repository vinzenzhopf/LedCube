using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfTimelineControl;

/// <summary>
/// Interaction logic for Timeline.xaml
/// </summary>
public partial class Timeline : UserControl
{
    public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
        nameof(Scale), typeof(double), typeof(Timeline),
        new PropertyMetadata(1.0, OnScalePropertyChanged));

    public static readonly DependencyProperty StartValueProperty = DependencyProperty.Register(
        nameof(StartValue), typeof(int), typeof(Timeline),
        new PropertyMetadata(0, OnStartValuePropertyChanged));
        
    public static readonly DependencyProperty EndValueProperty = DependencyProperty.Register(
        nameof(EndValue), typeof(int), typeof(Timeline),
        new PropertyMetadata(100, OnEndValuePropertyChanged));

    public static readonly DependencyProperty CursorPositionProperty = DependencyProperty.Register(
        nameof(CursorPosition), typeof(int), typeof(Timeline),
        new PropertyMetadata(0));
        
    public static readonly DependencyProperty TickMarksMajorProperty = DependencyProperty.Register(
        nameof(TickMarksMajor), typeof(int), typeof(Timeline),
        new PropertyMetadata(10, OnTickMarksMajorPropertyChanged));
        
    public static readonly DependencyProperty TickMarksMinorProperty = DependencyProperty.Register(
        nameof(TickMarksMinor), typeof(int), typeof(Timeline),
        new PropertyMetadata(5, OnTickMarksMinorPropertyChanged));

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof (IsReadOnly), typeof (bool), typeof (Timeline),
        new FrameworkPropertyMetadata(false, IsReadOnlyChanged));

    public static readonly DependencyProperty ElementsProperty = DependencyProperty.Register(
        nameof (Elements), typeof (IEnumerable<TimelineElementBase>), typeof (Timeline), 
        new FrameworkPropertyMetadata(null, OnElementsChanged));

    private ObservableCollection<TimelineElementBase> _elements = new();

    private const double LegendHeight = 20;
    private const double MinContentHeight = 30; 
        
    public double Scale
    {
        get => (double) GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
        
    public int StartValue
    {
        get => (int) GetValue(StartValueProperty);
        set => SetValue(StartValueProperty, value);
    }
        
    public int EndValue
    {
        get => (int) GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }
        
    public int CursorPosition
    {
        get => (int) GetValue(CursorPositionProperty);
        set => SetValue(CursorPositionProperty, value);
    }
    public int TickMarksMajor
    {
        get => (int) GetValue(TickMarksMajorProperty);
        set => SetValue(TickMarksMajorProperty, value);
    }
    public int TickMarksMinor
    {
        get => (int) GetValue(TickMarksMinorProperty);
        set => SetValue(TickMarksMinorProperty, value);
    }
        
    public bool IsReadOnly
    {
        get => (bool) GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    private double UnitWidth => Scale * 10;
    private double TickSpacingMajor => UnitWidth * TickMarksMajor;
    
    private double TickSpacingMinor => UnitWidth * TickMarksMinor;

    private int Length => EndValue - StartValue;

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ObservableCollection<TimelineElementBase> Elements
    {
        get => _elements;
        set => SetValue(ElementsProperty, value);
    }

    private void OnElementItemsChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
    }

    private void OnElementsChangedInternal(ObservableCollection<TimelineElementBase> oldValue,
        ObservableCollection<TimelineElementBase> newValue)
    {
        _elements = newValue ?? new ObservableCollection<TimelineElementBase>();
        //TODO: Remove all Views for oldValues
        Clear();
        RefreshElements();

        if(oldValue is not null)
            oldValue.CollectionChanged -= OnElementItemsChanged;
        if(newValue is not null)
            newValue.CollectionChanged += OnElementItemsChanged;
        //TODO: Add elements from the new collection
        Update();
        UpdateReadOnlyState();
            
        OnElementsChanged(oldValue, newValue);   
    }
    protected virtual void OnElementsChanged(ObservableCollection<TimelineElementBase>? oldValue, 
        ObservableCollection<TimelineElementBase>? newValue)
    {
    }

    #region StaticPropertyChangeListeners
    
        private static void OnScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Timeline)?.ContentInvalidated();
        }

        private static void OnStartValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Timeline)?.ContentInvalidated();
        }
        
        private static void OnEndValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Timeline)?.ContentInvalidated();
        }
        
        private static void OnTickMarksMajorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Timeline)?.ContentInvalidated();
        }
        
        private static void OnTickMarksMinorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Timeline)?.ContentInvalidated();
        }
        
        private static void OnElementsChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not Timeline timeline)
                return;
            var oldValue = (ObservableCollection<TimelineElementBase>?) e.OldValue;
            var newValue = (ObservableCollection<TimelineElementBase>?) e.NewValue;
            timeline.OnElementsChangedInternal(oldValue, newValue);
        }
    
        private static void IsReadOnlyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as Timeline)?.UpdateReadOnlyState();
        }

    #endregion
    
    /// <summary>
    /// Called by multiple PropertyChanged listeners to invalidate the drawn content.
    /// If this method is called, all TickMarks, Elements and Cursors ans Sizes have to be recalculated.
    /// </summary>
    private void ContentInvalidated()
    {
        Update();
    }
    
    private readonly List<TimelineMark> _tickMarks = new();
    private double _innerHeight;
    private int _elementTop;

    public Timeline()
    {
        InitializeComponent();
        
        MouseMove += Timeline_MouseMove;
        MouseLeftButtonUp += Timeline_MouseLeftButtonUp;
    }
    
    private void RefreshElements()
    {
        // throw new NotImplementedException();
    }

    private void Clear()
    {
        
    }
    
    private void UpdateReadOnlyState()
    {
        // throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new TimelineElement at the desired location (in seconds)
    /// </summary>
    /// <param name="seconds">Position of the element in seconds</param>
    public void AddElement(int seconds)
    {
        var element = new TimelineElementBase()
        {
            Position = seconds,
            Height = 100
        };
        element.MouseLeftButtonDown += TimelineElement_MouseLeftButtonDown;
        
        Elements.Add(element);
        MainCanvas.Children.Add(element);
        Canvas.SetTop(element, _elementTop);
        
        var halfWidth = element.ActualWidth / 2;
        Canvas.SetLeft(element, UnitWidth * element.Position - halfWidth);
    }

    /// <summary>
    /// Called by a TimelineElement once it has finished being dragged to a new location
    /// </summary>
    /// <param name="te">The TimelineElement in question</param>
    public void RefreshElement(TimelineElementBase te)
    {
        // te.SetSeconds((int)((Canvas.GetLeft(te) + 2) * (EndValue - StartValue) / pixelDistance) + StartValue);
    }

    private void UpdateTickMarks()
    {
        var minorTicksValid = TickMarksMajor / (double)TickMarksMinor % 1.0 == 0.0;
        var majorCount = Length / TickMarksMajor;
        var minorCount = Length / TickMarksMinor;
        var totalCount = (minorTicksValid ? minorCount : majorCount) + 1; //Total TickMark count, +1 for the last Mark

        //Create missing TickMarks, delete unnecessary ones
        var missing = totalCount - _tickMarks.Count;
        switch (missing)
        {
            case > 0:
                for (var i = 0; i < missing; i++)
                {
                    var tm = new TimelineMark();
                    _tickMarks.Add(tm);
                    MainCanvas.Children.Add(tm);
                }
                break;
            case < 0:
                for (var i = 0; i < (missing * -1); i++)
                {
                    var old = _tickMarks[^1];
                    _tickMarks.RemoveAt(_tickMarks.Count-1);
                    MainCanvas.Children.Remove(old);
                }
                break;
        }
        
        var tickWidth = minorTicksValid ? TickMarksMinor : TickMarksMajor;
        var tickSpacing = minorTicksValid ? TickSpacingMinor : TickSpacingMajor;
        for (var i = 0; i < totalCount; i++)
        {
            var position = i * tickWidth + StartValue;
            _tickMarks[i].Position = position;
            _tickMarks[i].IsMajorTick = position % TickMarksMajor == 0;
            Canvas.SetLeft(_tickMarks[i], tickSpacing * i);
            Canvas.SetTop(_tickMarks[i], 1);
        }
    }

    private void UpdateContentLayout()
    {
        UpdateTickMarks();

        UpdateElements();
        //Todo: update positions of Elements and Cursors
    }

    private void UpdateElements()
    {
        foreach (var element in Elements)
        {
            var halfWidth = element.ActualWidth / 2;
            Canvas.SetLeft(element, UnitWidth * element.Position - halfWidth);    
        }
    }

    private void Update()
    {
        UpdateContentLayout();

        // // Size & place the controls
        // Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        // Arrange(new Rect(0, 0, ActualWidth, ActualHeight));
        // var test = DesiredSize;

        RecalculateWidth();
        RecalculateHeight();
    }

    private void RecalculateWidth()
    {
        var width = Length * UnitWidth + 1;
        InnerBorder.Width = width;
        //_pixelDistance = (int) InnerBorder.Width - 1; // Region of the border aka the timeline's length in pixels
        MainCanvas.Width = width + TickSpacingMajor; // Set the canvas's width so the ScrollViewer knows how big it is
    }

    private void RecalculateHeight()
    {
        Canvas.SetTop(InnerBorder, 1 + LegendHeight);
        _elementTop = 1 + (int) LegendHeight + 1; // Canvas.Top value for TimelineElements
        
        var borderHeight = double.Max(MainCanvas.ActualHeight - LegendHeight, MinContentHeight); 
        InnerBorder.Height = borderHeight; // To account for TimelineMark height & scrollbar height. This value assumes the height of the Aero-style scrollbar.
        _innerHeight = borderHeight - 2; // Height of region inside the border
        
        foreach (var t in _tickMarks)
        {
            t.Height = MainCanvas.ActualHeight;
        }

        foreach (var e in _elements)
        {
            e.Height = MainCanvas.ActualHeight;
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        if (sizeInfo.HeightChanged)
        {
            RecalculateHeight();
        }

        if (sizeInfo.WidthChanged)
        {
            RecalculateWidth();
        }
    }
    
    private TimelineElementBase? _draggedElement;
    private double _dragStartCanvasX;
    private double _dragStartMouseX;
    
    private void TimelineElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TimelineElementBase {IsMouseover: true} element)
            return;

        if (_draggedElement is not null) return;
        // Enter dragging
        _draggedElement = element;
        _dragStartCanvasX = Canvas.GetLeft(_draggedElement);
        _dragStartMouseX = Mouse.GetPosition(this).X;
        _draggedElement.OnMouseLeftButtonDown(sender, e);
        //element.ToolTip = "";
    }

    // Dragging handler
    private void Timeline_MouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedElement is null) 
            return;
        
        if (Mouse.LeftButton == MouseButtonState.Pressed)
        {   
            var diff = _dragStartMouseX - Mouse.GetPosition(this).X;
            var halfWidth = _draggedElement.ActualWidth / 2;
            var position = (int)Math.Round((_dragStartCanvasX - diff + halfWidth) / UnitWidth);
            Canvas.SetLeft(_draggedElement, position * UnitWidth - halfWidth);
            //Limit Position to min/max
            var canvasSize = Length * UnitWidth;
            if (Canvas.GetLeft(_draggedElement) > canvasSize - halfWidth)
                Canvas.SetLeft(_draggedElement, canvasSize - halfWidth);
            if (Canvas.GetLeft(_draggedElement) < -halfWidth)
                Canvas.SetLeft(_draggedElement, -halfWidth);
        }
        else
        {
            //LeftMouseUp not registered
            _draggedElement.OnMouseLeftButtonUp(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
            _draggedElement = null;
        }
    }
    
    private void Timeline_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggedElement is null) 
            return;
        _draggedElement.OnMouseLeftButtonUp(sender, e);
        _draggedElement = null;
    }
}