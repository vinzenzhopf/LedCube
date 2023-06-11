using System.Windows;
using System.Windows.Controls;

namespace WpfTimelineControl
{
    /// <summary>
    /// Interaction logic for TimelineMark.xaml
    /// </summary>
    public partial class TimelineMark : UserControl
    {
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            nameof (Position), typeof (int), typeof (TimelineMark),
            new FrameworkPropertyMetadata(0));
        
        public static readonly DependencyProperty IsMajorTickProperty = DependencyProperty.Register(
            nameof (IsMajorTick), typeof (bool), typeof (TimelineMark),
            new FrameworkPropertyMetadata(false));

        public int Position
        {
            get => (int) GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        public bool IsMajorTick
        {
            get => (bool) GetValue(IsMajorTickProperty);
            set => SetValue(IsMajorTickProperty, value);
        }

        public TimelineMark()
        {
            InitializeComponent();
        }
    }
}
