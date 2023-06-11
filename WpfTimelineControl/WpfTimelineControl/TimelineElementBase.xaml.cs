using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfTimelineControl
{
    /// <summary>
    /// Interaction logic for TimelineElement.xaml
    /// </summary>
    public partial class TimelineElementBase : UserControl
    {
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            nameof(Position), typeof(int), typeof(TimelineElementBase),
            new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty IsMouseoverProperty = DependencyProperty.Register(
            nameof(IsMouseover), typeof(bool), typeof(TimelineElementBase),
            new FrameworkPropertyMetadata(false));
        
        public static readonly DependencyProperty IsDraggingActiveProperty = DependencyProperty.Register(
            nameof(IsDraggingActive), typeof(bool), typeof(TimelineElementBase),
            new FrameworkPropertyMetadata(false));
        
        public int Position
        {
            get => (int) GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        
        public bool IsMouseover
        {
            get => (bool) GetValue(IsMouseoverProperty);
            protected set => SetValue(IsMouseoverProperty, value);
        }
        
        public bool IsDraggingActive
        {
            get => (bool) GetValue(IsDraggingActiveProperty);
            protected set => SetValue(IsDraggingActiveProperty, value);
        }

        internal void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseover) return;
            // Respond visually
            // RectOuter.Opacity = 0.6;
            IsDraggingActive = true;
            Trace.WriteLine($"DraggingActive = {IsDraggingActive}");
        }

        internal void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseover) return;
            
            // Respond visually
            // RectOuter.Opacity = 0.3;
            IsDraggingActive = false;
            Trace.WriteLine($"DraggingActive = {IsDraggingActive}");
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (sizeInfo.HeightChanged)
            {
                RectOuter.Height = sizeInfo.NewSize.Height;
                // RectInner.Height = sizeInfo.NewSize.Height;
                Trace.WriteLine("Render Size Changed");
            }
        }
        
        public TimelineElementBase()
        {
            InitializeComponent();
            
            // Setup for mouseover highlight
            MouseEnter += TimelineElement_MouseEnter;
            MouseLeave += TimelineElement_MouseLeave;
        }
        
        // Listeners
        private void TimelineElement_MouseEnter(object sender, MouseEventArgs e)
        {
            // Respond visually
            // RectOuter.Opacity = 0.5;

            // Prime
            IsMouseover = true;
            Trace.WriteLine($"Mouseover = {IsMouseover}");
        }
        private void TimelineElement_MouseLeave(object sender, MouseEventArgs e)
        {
            // Respond visually
            // RectOuter.Opacity = 0.3;

            // Deprime
            IsMouseover = false;
            Trace.WriteLine($"Mouseover = {IsMouseover}");
        }
    }
}
