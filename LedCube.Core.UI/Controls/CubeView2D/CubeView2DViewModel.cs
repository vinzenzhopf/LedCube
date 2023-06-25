using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.CubeData;
using LedCube.Core.CubeData.Projections;
using LedCube.Core.CubeData.Repository;
using LedCube.Core.UI.Messages;
using Microsoft.Extensions.Logging;
using IPlaneData = LedCube.Core.UI.Controls.ViewModels.IPlaneData;

namespace LedCube.Core.UI.Controls.CubeView2D;

[ObservableObject]
public partial class CubeView2DViewModel
{
    private ILogger Logger { get; }
    
    private readonly ICubeConfigRepository _cubeConfigRepository;
    private ICubeRepository _cubeRepository;

    public ObservableCollection<int> AllPlanes { get; } = new();
    public ObservableCollection<int> SelectedPlanes { get; } = new();
    
    [ObservableProperty]
    private int _gridWidth;

    [ObservableProperty]
    private int _gridHeight;

    [ObservableProperty]
    private int _selectedPlane = 0;

    [ObservableProperty]
    private Brush _ledBrush = Brushes.Blue;

    [ObservableProperty]
    private bool _showNumbers = true;
    
    [ObservableProperty]
    private ICubeData _cubeConfig;

    [ObservableProperty]
    private IPlaneData _planeData;
    
    [ObservableProperty]
    private Orientation3D _viewDirection = Orientation3D.Front;

    private readonly SimpleRotationCubeProjection _cubeProjection;
    private readonly PlaneCubeProjection _planeCubeProjection;

    public CubeView2DViewModel(ICubeConfigRepository cubeConfigRepository, ICubeRepository cubeRepository, ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger(GetType());
        _cubeConfigRepository = cubeConfigRepository;
        _cubeRepository = cubeRepository;

        
        //Init values
        _cubeProjection = new SimpleRotationCubeProjection(cubeRepository.GetCubeData(), _viewDirection);
        _planeCubeProjection = new PlaneCubeProjection(_cubeProjection, _selectedPlane);
        var dimensions = _planeCubeProjection.Size;
        _gridWidth = dimensions.X;
        _gridHeight = dimensions.Y;

        //Register Listeners
        WeakReferenceMessenger.Default.Register<CubeConfigChangedMessage>(this, HandleCubeConfigChangedMessage);
    }

    private void HandleCubeConfigChangedMessage(object recipient, CubeConfigChangedMessage message)
    {
        Logger.LogDebug("Received Message: CubeConfigChanged {data}", message);
    }
}