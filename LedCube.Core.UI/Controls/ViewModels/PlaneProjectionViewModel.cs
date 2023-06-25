using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.UI.Controls.ViewModels;


[ObservableObject]
public partial class PlaneProjectionViewModel : IPlaneViewModel
{
    public event ILedViewModel.LedChangedArgs? LedChanged;
    public event IPlaneViewModel.PlaneChangedArgs? PlaneChanged;

    [ObservableProperty]
    private ICubeData _cubeData;

    [ObservableProperty]
    private Orientation3D _orientation;

    [ObservableProperty]
    private IReadOnlyList<int> _selectedPlanes;

    [ObservableProperty]
    private IPlaneData _planeData;

    partial void OnCubeDataChanged(ICubeData? oldValue, ICubeData newValue)
    {
        throw new System.NotImplementedException();
    }

    partial void OnOrientationChanged(Orientation3D oldValue, Orientation3D newValue)
    {
        throw new System.NotImplementedException();
    }

    partial void OnSelectedPlanesChanged(IReadOnlyList<int>? oldValue, IReadOnlyList<int> newValue)
    {
        throw new System.NotImplementedException();
    }
}