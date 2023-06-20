using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.UI.Controls.CubeView2D;

[ObservableObject]
public partial class CubeView2DViewModel
{
    public CubeConfig Config { get; }

    [ObservableProperty]
    private int _x;
    
    [ObservableProperty]
    private int _y;
    
    [ObservableProperty]
    private int _selectedPlane;
    
    [ObservableProperty]
    private Plane<BiLed> _planeData;
    
    [ObservableProperty]
    private Orientation3D _viewDirection;
    
    public CubeView2DViewModel(CubeConfig config)
    {
        Config = config;
    }
}