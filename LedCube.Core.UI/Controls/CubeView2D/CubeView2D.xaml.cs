using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Core.UI.Controls.CubeView2D;

[ObservableObject]
public partial class CubeView2D : UserControl
{
    private readonly CubeView2DViewModel? _viewModel = null;
    private CheckBox[]? _checkBoxes; 
    
    public CubeView2D()
    {
        InitializeComponent();
        Update();
    }

    private void Update()
    {
        UpdateContentLayout();
        RecalculateDimensions();
    }

    private void RecalculateDimensions()
    {
        // throw new System.NotImplementedException();
    }

    private void UpdateContentLayout()
    {
        if (_viewModel is null)
            return;
        _checkBoxes = new CheckBox[_viewModel.X * _viewModel.Y];
        
        LedGrid = new Grid();
        for (var x = 0; x < _viewModel.Config.Dimensions.X; x++)
        {
            LedGrid.ColumnDefinitions.Add(new ColumnDefinition());    
        }
        for (var y = 0; y < _viewModel.Config.Dimensions.Y; y++)
        {
            LedGrid.RowDefinitions.Add(new RowDefinition());    
        }
        var width = LedGrid.Width / _viewModel.Config.Dimensions.X;
        var height = LedGrid.Height / _viewModel.Config.Dimensions.Z;
        
        var ledBaseStyle = this.FindResource("LedCheckBoxStyle") as Style;
        var ledStyle = new Style(typeof(CheckBox), ledBaseStyle);
        ledStyle.Setters.Add(new Setter(CheckBox.HeightProperty, height));
        ledStyle.Setters.Add(new Setter(CheckBox.WidthProperty, width));
        
        for (var i = 0; i < _viewModel.Config.Dimensions.X * _viewModel.Config.Dimensions.Y; i++)
        {
            var cb = new CheckBox()
            {
                Style = ledStyle
            };
            LedGrid.Children.Add(cb);
            var x = i % _viewModel.Config.Dimensions.X;
            var y = i / _viewModel.Config.Dimensions.Y;
            Grid.SetColumn(LedGrid, _viewModel.Config.Dimensions.X - 1 - x);
            Grid.SetRow(LedGrid, _viewModel.Config.Dimensions.Y - 1 - y);
        }
    }
}