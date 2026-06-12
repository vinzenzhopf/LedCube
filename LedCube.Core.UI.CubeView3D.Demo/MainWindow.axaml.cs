using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using LedCube.Core.Common.Config.Entities;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Core.Common.Model.Orientation;
using LedCube.Core.UI.CubeView3D.Controls;

namespace LedCube.Core.UI.CubeView3D.Demo;

public partial class MainWindow : Window
{
    private readonly ICubeData _cube = new CubeData<CubeDataBuffer16>();
    private readonly DispatcherTimer _timer;
    private Cube3DView _cube3D = null!;
    private ComboBox _hwFront = null!, _installFront = null!;
    private CheckBox _handedness = null!, _showInstalled = null!;
    private bool _led0Test;
    private double _time;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        _cube3D = this.FindControl<Cube3DView>("Cube")!;
        _cube3D.CubeData = _cube;
        InitOrientationCombos();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += (_, _) => UpdatePattern();
        _timer.Start();
        UpdatePattern();
    }

    private void UpdatePattern()
    {
        if (_led0Test)
        {
            // Light only LED 0 to verify it sits at the front-left-bottom corner in the preview.
            _cube.Clear();
            _cube.SetLed(new Point3D(0, 0, 0), true);
            return;
        }

        _time += 0.05;
        var size = _cube.Size;
        var cx = (size.X - 1) / 2f;
        var cy = (size.Y - 1) / 2f;
        var cz = (size.Z - 1) / 2f;

        // Radius of the shell oscillates; a thin shell reads clearly as a 3D surface.
        var radius = (float)(1.5 + 5.0 * (0.5 + 0.5 * Math.Sin(_time)));
        const float thickness = 0.8f;

        for (var z = 0; z < size.Z; z++)
        for (var y = 0; y < size.Y; y++)
        for (var x = 0; x < size.X; x++)
        {
            var dx = x - cx;
            var dy = y - cy;
            var dz = z - cz;
            var dist = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            _cube.SetLed(new Point3D(x, y, z), Math.Abs(dist - radius) < thickness);
        }
    }

    private bool _orientationReady;

    private void InitOrientationCombos()
    {
        _hwFront = this.FindControl<ComboBox>("HwFrontCombo")!;
        _installFront = this.FindControl<ComboBox>("InstallFrontCombo")!;
        _handedness = this.FindControl<CheckBox>("HandednessCheck")!;
        _showInstalled = this.FindControl<CheckBox>("ShowInstalledCheck")!;
        var faces = Enum.GetValues<Orientation3D>().Cast<object>().ToArray();
        _hwFront.ItemsSource = faces;
        _installFront.ItemsSource = faces;
        _hwFront.SelectedItem = Orientation3D.Front;
        _installFront.SelectedItem = Orientation3D.Front;
        _orientationReady = true;
    }

    private void Orientation_Changed(object? sender, SelectionChangedEventArgs e) => RebuildOrientation();
    private void Handedness_Changed(object? sender, RoutedEventArgs e) => RebuildOrientation();
    private void ShowInstalled_Changed(object? sender, RoutedEventArgs e) => RebuildOrientation();

    private void Led0Test_Changed(object? sender, RoutedEventArgs e)
    {
        _led0Test = (sender as CheckBox)?.IsChecked == true;
        UpdatePattern();
    }

    private void RebuildOrientation()
    {
        if (!_orientationReady) return;
        var config = new CubeInstallationConfig
        {
            HardwareFront = (Orientation3D)_hwFront.SelectedItem!,
            HardwareHandedness = _handedness.IsChecked == true
                ? CartesianOrientation.LeftHanded
                : CartesianOrientation.RightHanded,
            InstallationFront = (Orientation3D)_installFront.SelectedItem!,
        };
        if (!config.Resolve().IsValid) return;
        // "Show as installed" off => canonical/raw (identity); on => the configured installation.
        _cube3D.Orientation = _showInstalled.IsChecked == true ? config.Resolve() : CubeOrientation.Default;
    }

    private void OrientationReset_Click(object? sender, RoutedEventArgs e)
    {
        _hwFront.SelectedItem = Orientation3D.Front;
        _installFront.SelectedItem = Orientation3D.Front;
        _handedness.IsChecked = false;
    }

    private void LedColor_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string hex })
            _cube3D.LedColor = Color.Parse(hex);
    }

    private void BgColor_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string hex })
            _cube3D.BackgroundColor = Color.Parse(hex);
    }
}
