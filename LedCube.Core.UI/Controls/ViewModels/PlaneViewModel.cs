using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.UI.Controls.ViewModels;

public partial class PlaneViewModel : ObservableObject, IPlaneViewModel
{
    public event IPlaneViewModel.LedChangedArgs? LedChanged;
    public event IPlaneViewModel.PlaneChangedArgs? PlaneChanged;

    [ObservableProperty]
    private IPlaneData _planeData;
    
    public void SetLed(int index, bool? value)
    {
        
    }
    
    protected virtual void OnLedChanged(int index, bool value)
    {
        LedChanged?.Invoke(index, value);
    }

    protected virtual void OnPlaneChanged(IPlaneData data)
    {
        PlaneChanged?.Invoke(data);
    }

    public bool? GetLed(int index)
    {
        if (PlaneData.GetLed(0).GetType() != typeof(bool))
        {
            return false;
        }
        return ((IPlaneData<bool>) PlaneData).GetLed(index);
    }
}