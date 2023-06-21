using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.UI.Controls.ViewModels;


[ObservableObject]
public partial class PlaneViewModel
{
    private IPlane<BiLed>? _data;
    
    public delegate void LedChangedArgs(int index, bool value); 
    public event LedChangedArgs? LedChanged;
    
    public delegate void PlaneChangedArgs(IPlane<BiLed>? data); 
    public event PlaneChangedArgs? PlaneChanged;
    
    public void ChangeLed(int index, bool value)
    {
        
    }

    // [RelayCommand]
    // void LedChanged(int index, bool value)
    // {
    //     
    // }
    //
    // [RelayCommand]
    // void PlaneChanged(IPlane<BiLed> data)
    // {
    //     
    // }
}