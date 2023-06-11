using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfTimelineControl.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ObservableObject]
    public partial class MainWindow : Window
    {
        [ObservableProperty]
        private double _scaleValue = 3.0;
        [ObservableProperty]
        private int _startValue = 0;
        [ObservableProperty]
        private int _endValue = 100;
        [ObservableProperty]
        private int _majorTicks = 10;
        [ObservableProperty]
        private int _minorTicks = 1;
        [ObservableProperty]
        private bool _readOnlyValue = false;
        [ObservableProperty]
        private int _cursorPosition = 42;

        public MainWindow()
        {
            InitializeComponent();
            
            Timeline.AddElement(12);
            Timeline.AddElement(18);
        }
        
        
    }
}