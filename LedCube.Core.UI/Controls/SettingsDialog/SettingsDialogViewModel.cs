using System;
using System.Collections.ObjectModel;
using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Config.Entities;
using LedCube.Core.UI.Services.Hotkey;
using LedCube.Core.UI.Settings;

namespace LedCube.Core.UI.Controls.SettingsDialog;

public partial class SettingsDialogViewModel : ObservableObject
{
    private readonly ISettingsFacade _facade;
    private readonly GeneralCubeSettingsViewModel _generalCubeSettingsViewModel;
    private readonly CubeStreamerSettingsViewModel? _streamerSettings;
    private readonly CubeDisplaySettingsViewModel? _cubeDisplaySettingsViewModel;
    private readonly KeyboardControlSettingViewModel _keyboardControlSettingViewModel;

    public Action? CloseAction { get; set; }

    public SettingsDialogViewModel(ISettingsFacade facade, IHotkeyService hotkeyService)
    {
        _facade = facade;

        var cube = facade.CubeSettings;
        _generalCubeSettingsViewModel = new GeneralCubeSettingsViewModel
        {
            Dimensions = new CubeDimensionsViewModel(cube.Dimensions)
        };
        Nodes.Add(new SettingsNodeViewModel("Cube", _generalCubeSettingsViewModel));

        if (facade.ConnectionSettings is { } conn)
        {
            _streamerSettings = new CubeStreamerSettingsViewModel
            {
                Port = conn.Port,
                Hostname = conn.Hostname,
                SearchPerBroadcast = conn.SearchPerBroadcast,
                Projection = new CubeDataProjectionSettingsViewModel { Orientation = conn.Projection.Orientation }
            };
            var streaming = new SettingsNodeViewModel("Streaming");
            streaming.Children.Add(new SettingsNodeViewModel("Connection", _streamerSettings));
            Nodes.Add(streaming);
        }

        if (facade.DisplaySettings is { } display)
        {
            _cubeDisplaySettingsViewModel = new CubeDisplaySettingsViewModel
            {
                Cube3DDrawingConfig = new Cube3DDrawingConfigViewModel
                {
                    DrawWireframe = display.DrawWireframe,
                    LedType = new LedTypeViewModel
                    {
                        LedDimensions = display.LedType.LedDimensions,
                        Shape = display.LedType.Shape,
                        Tint = display.LedType.Tint
                    }
                }
            };
            var displayNode = new SettingsNodeViewModel("Display");
            displayNode.Children.Add(new SettingsNodeViewModel("Rendering", _cubeDisplaySettingsViewModel));
            Nodes.Add(displayNode);
        }

        _keyboardControlSettingViewModel = new KeyboardControlSettingViewModel(hotkeyService);
        var keyboard = new SettingsNodeViewModel("Keyboard");
        keyboard.Children.Add(new SettingsNodeViewModel("Controls", _keyboardControlSettingViewModel));
        Nodes.Add(keyboard);

        SelectedNode = Nodes[0];
    }

    public ObservableCollection<SettingsNodeViewModel> Nodes { get; } = new();

    [ObservableProperty]
    private SettingsNodeViewModel _selectedNode = null!;

    [RelayCommand]
    private void Accept()
    {
        _facade.SaveCubeSettings(_facade.CubeSettings with
        {
            Dimensions = new CubeDimensions(
                _generalCubeSettingsViewModel.Dimensions.X,
                _generalCubeSettingsViewModel.Dimensions.Y,
                _generalCubeSettingsViewModel.Dimensions.Z)
        });

        if (_streamerSettings is not null && _facade.ConnectionSettings is not null)
        {
            _facade.SaveConnectionSettings(new CubeStreamerSettings
            {
                Port = _streamerSettings.Port,
                Hostname = _streamerSettings.Hostname,
                SearchPerBroadcast = _streamerSettings.SearchPerBroadcast,
                Projection = new CubeDataProjectionSettings
                {
                    Orientation = _streamerSettings.Projection.Orientation
                }
            });
        }

        if (_cubeDisplaySettingsViewModel is not null && _facade.DisplaySettings is not null)
        {
            _facade.SaveDisplaySettings(new Cube3DDrawingConfig
            {
                DrawWireframe = _cubeDisplaySettingsViewModel.Cube3DDrawingConfig.DrawWireframe,
                LedType = new LedType
                {
                    LedDimensions = _cubeDisplaySettingsViewModel.Cube3DDrawingConfig.LedType.LedDimensions,
                    Shape = _cubeDisplaySettingsViewModel.Cube3DDrawingConfig.LedType.Shape,
                    Tint = _cubeDisplaySettingsViewModel.Cube3DDrawingConfig.LedType.Tint
                }
            });
        }

        CloseAction?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseAction?.Invoke();
    }
}

public partial class SettingsNodeViewModel : ObservableObject
{
    public SettingsNodeViewModel(string title, object? editor = null)
    {
        Title = title;
        Editor = editor;
    }

    public string Title { get; }
    public object? Editor { get; }

    public ObservableCollection<SettingsNodeViewModel> Children { get; } = new();
}

public partial class GeneralCubeSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private CubeDimensionsViewModel _dimensions = new();
}

public partial class CubeDimensionsViewModel : ObservableObject
{
    public CubeDimensionsViewModel()
    {
        X = 1;
        Y = 1;
        Z = 1;
    }

    public CubeDimensionsViewModel(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public CubeDimensionsViewModel(CubeDimensions cubeDimensions)
    {
        X = cubeDimensions.X;
        Y = cubeDimensions.Y;
        Z = cubeDimensions.Z;
    }

    [ObservableProperty]
    public partial int X { get; set; }

    [ObservableProperty]
    public partial int Y { get; set; }

    [ObservableProperty]
    public partial int Z { get; set; }
}


public partial class CubeDisplaySettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private Cube3DDrawingConfigViewModel _cube3DDrawingConfig = new();
}

public partial class CubeStreamerSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private int _port = 4242;

    [ObservableProperty]
    private string _hostname = string.Empty;

    [ObservableProperty]
    private bool _searchPerBroadcast = true;

    [ObservableProperty]
    private CubeDataProjectionSettingsViewModel _projection = new();
}

public partial class CubeDataProjectionSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private CartesianOrientation _orientation;
}

public partial class Cube3DDrawingConfigViewModel : ObservableObject
{
    [ObservableProperty]
    private LedTypeViewModel _ledType = new();

    [ObservableProperty]
    private bool _drawWireframe;
}

public partial class LedTypeViewModel : ObservableObject
{
    [ObservableProperty]
    private int _ledDimensions;

    [ObservableProperty]
    private LedShape _shape;

    [ObservableProperty]
    private Color _tint;
}

public partial class KeyboardControlSettingViewModel : ObservableObject
{
    public KeyboardControlSettingViewModel() { }

    public KeyboardControlSettingViewModel(IHotkeyService hotkeyService)
    {
        foreach (var entry in hotkeyService.GetAllHotkeys())
            Entries.Add(new KeyboardControlEntryViewModel(hotkeyService.GetHotkey(entry.Function), hotkeyService));
    }

    public ObservableCollection<KeyboardControlEntryViewModel> Entries { get; set; } = [];
}
