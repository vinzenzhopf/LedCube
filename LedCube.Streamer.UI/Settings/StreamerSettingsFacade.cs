using System;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Settings;
using LedCube.Core.UI.Settings;

namespace LedCube.Streamer.UI.Settings;

public class StreamerSettingsFacade : ISettingsFacade
{
    private readonly ISettingsProvider<LedCubeStreamerSettings> _provider;

    public StreamerSettingsFacade(ISettingsProvider<LedCubeStreamerSettings> provider)
    {
        _provider = provider;
    }

    public CubeSettings CubeSettings => _provider.Settings.Cube;
    public CubeStreamerSettings? ConnectionSettings => _provider.Settings.Connection;
    public Cube3DDrawingConfig? DisplaySettings => null;

    public event EventHandler<CubeSettings>? CubeChanged;
    public event EventHandler<CubeStreamerSettings>? ConnectionChanged;
    public event EventHandler<Cube3DDrawingConfig>? DisplayChanged { add { } remove { } }

    public void SaveCubeSettings(CubeSettings settings)
    {
        _provider.SaveAndUpdate(_provider.Settings with { Cube = settings });
        CubeChanged?.Invoke(this, settings);
    }

    public void SaveConnectionSettings(CubeStreamerSettings settings)
    {
        _provider.SaveAndUpdate(_provider.Settings with { Connection = settings });
        ConnectionChanged?.Invoke(this, settings);
    }

    public void SaveDisplaySettings(Cube3DDrawingConfig settings) { }
}
