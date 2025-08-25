using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common.Config.Config;
using LedCube.Core.Common.CubeData.Projections;
using LedCube.Core.Common.CubeData.Repository;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.UI.Messages;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Controls.CubeView2D;

public partial class CubeView2DViewModel : ObservableObject
{
    private ILogger Logger { get; }
    
    private readonly ICubeConfigRepository _cubeConfigRepository;
    private ICubeRepository _cubeRepository;

    public ObservableCollection<int> AllPlanes { get; } = new();
    public ObservableCollection<int> SelectedPlanes { get; } = new();

    [ObservableProperty]
    private int _selectedPlane = 0;

    [ObservableProperty]
    private Brush _ledBrush = Brushes.Blue;

    [ObservableProperty]
    private bool _showNumbers = true;

    [ObservableProperty]
    private IPlaneData _planeData;
    
    [ObservableProperty]
    private Orientation3D _viewDirection = Orientation3D.Front;

    private readonly SimpleRotationCubeProjection _cubeProjection;
    private readonly PlaneCubeProjection _planeCubeProjection;

    public CubeView2DViewModel(ILoggerFactory loggerFactory, ICubeConfigRepository cubeConfigRepository, ICubeRepository cubeRepository)
    {
        Logger = loggerFactory.CreateLogger(GetType());
        _cubeConfigRepository = cubeConfigRepository;
        _cubeRepository = cubeRepository;

        //Init values
        // _cubeProjection = new SimpleRotationCubeProjection(cubeRepository.GetCubeData() , _viewDirection);
        // _planeCubeProjection = new PlaneCubeProjection(_cubeProjection, _selectedPlane);
        // var dimensions = _planeCubeProjection.Size;
        // PlaneData = _planeCubeProjection;
        // UpdatePlaneElements();
        // SelectedPlanes.CollectionChanged += OnSelectedPlanesCollectionChanged;
        // _cubeProjection.CubeChanged += CubeProjection_OnCubeChanged;
        // _planeData.LedChanged += PlaneData_OnLedChanged;
        // _planeData.PlaneChanged += PlaneData_OnPlaneChanged;

        //Register Listeners
        WeakReferenceMessenger.Default.Register<CubeConfigChangedMessage>(this, HandleCubeConfigChangedMessage);
        
        this.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Logger.LogTrace("PropertyChanged on {property}, {sender}", e.PropertyName, sender);
    }

    private void CubeProjection_OnCubeChanged(ICubeData cubedata)
    {
        Logger.LogDebug("CubeProjection_OnCubeChanged");
    }

    private void PlaneData_OnLedChanged(Point2D p, bool value)
    {
        Logger.LogDebug("PlaneData_OnLedChanged");
    }
    
    private void PlaneData_OnPlaneChanged(IPlaneData plane)
    {
        Logger.LogDebug("PlaneData_OnPlaneChanged");
    }

    private void UpdatePlaneElements()
    {
        var target = _cubeProjection.Size.Z;
        for(var i = AllPlanes.Count-1; i >= target; i--)
        {
            AllPlanes.RemoveAt(i);
        }
        for(var i = AllPlanes.Count; i < target; i++)
        {
            AllPlanes.Add(i);
        }
    }

    private void OnSelectedPlanesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Logger.LogDebug("Selected Planes changed: {action}, {startIndex}, {endIndex}", e.Action, e.NewStartingIndex, e.OldStartingIndex);
    }
    
    partial void OnViewDirectionChanged(Orientation3D value)
    {
        Logger.LogDebug("View-Direction changed: {orientation}", value);
        _cubeProjection.Rotation = value;
    }

    partial void OnSelectedPlaneChanged(int value)
    {
        Logger.LogDebug("Selected Plane changed: {value}", value);
        _planeCubeProjection.Z = value;
    }

    private void HandleCubeConfigChangedMessage(object recipient, CubeConfigChangedMessage message)
    {
        Logger.LogDebug("Received Message: CubeConfigChanged {data}", message);
    }

    [RelayCommand]
    private void ClearSelectedPlanesClicked()
    {
        Logger.LogDebug("ClearSelectedPlanesClicked");
    }

    [RelayCommand]
    private void SetSelectedPlanesClicked()
    {
        Logger.LogDebug("SetSelectedPlanesClicked");
    }
    
    [RelayCommand]
    private void ToggleSelectedPlanesClicked()
    {
        Logger.LogDebug("ToggleSelectedPlanesClicked");
    }
    
    [RelayCommand]
    private void SelectAllPlanesClicked()
    {
        Logger.LogDebug("SelectAllPlanesClicked");
    }
    
    [RelayCommand]
    private void ClearPlaneSelectionClicked()
    {
        Logger.LogDebug("ClearPlaneSelectionClicked");
    }
}