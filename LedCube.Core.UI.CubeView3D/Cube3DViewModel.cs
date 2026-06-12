using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.CubeData.Repository;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Orientation;
using LedCube.Core.Common.Settings;

namespace LedCube.Core.UI.CubeView3D;

/// <summary>
/// Adapter for the 3D preview. Exposes the live repository (the control reads its current cube each
/// frame) and the orientation to render with. By default the preview shows canonical animation space
/// (identity); when <see cref="ShowAsInstalled"/> is on it applies the configured installation
/// orientation so the preview mirrors the physical cube.
/// </summary>
public partial class Cube3DViewModel : ObservableObject
{
    private readonly ICubeRepository _cubeRepository;
    private readonly ISettingsProvider<CubeStreamerSettings>? _settings;

    public Cube3DViewModel(ICubeRepository cubeRepository, ISettingsProvider<CubeStreamerSettings>? settings = null)
    {
        _cubeRepository = cubeRepository;
        _settings = settings;
        if (_settings is not null)
            _settings.SettingsChanged += (_, _) => { if (ShowAsInstalled) OnPropertyChanged(nameof(Orientation)); };
    }

    /// <summary>The live repository — the control reads its current cube each frame.</summary>
    public ICubeRepository Repository => _cubeRepository;

    public ICubeData CubeData => _cubeRepository.GetCubeData();

    /// <summary>When true, the preview applies the configured orientation (mirrors the real cube).</summary>
    [ObservableProperty]
    private bool _showAsInstalled;

    /// <summary>Identity in canonical mode; the resolved installation orientation when showing as installed.</summary>
    public CubeOrientation Orientation =>
        ShowAsInstalled && _settings is not null
            ? _settings.Settings.Projection.ToConfig().Resolve()
            : CubeOrientation.Default;

    partial void OnShowAsInstalledChanged(bool value) => OnPropertyChanged(nameof(Orientation));

    // ---- Display settings (bound to the Cube3DView control and the preview's settings band) ----

    [ObservableProperty] private double _ledScale = 0.85;
    [ObservableProperty] private double _transparency = 0.75;
    [ObservableProperty] private Color _ledColor = Color.FromRgb(40, 100, 235);
    [ObservableProperty] private Color _backgroundColor = Color.FromRgb(18, 18, 24);
    [ObservableProperty] private bool _showPodest = true;
    [ObservableProperty] private bool _showAnnotations = true;
    [ObservableProperty] private bool _showAxisGizmo = true;
    [ObservableProperty] private bool _showLed0Arrow = true;

    [RelayCommand] private void SetLedColor(string hex) => LedColor = Color.Parse(hex);
    [RelayCommand] private void SetBackgroundColor(string hex) => BackgroundColor = Color.Parse(hex);
}
