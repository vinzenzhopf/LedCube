using System;
using LedCube.Core.Common.Config;

namespace LedCube.Core.UI.Settings;

public interface ISettingsFacade
{
    CubeSettings CubeSettings { get; }
    CubeStreamerSettings? ConnectionSettings { get; }
    Cube3DDrawingConfig? DisplaySettings { get; }

    event EventHandler<CubeSettings>? CubeChanged;
    event EventHandler<CubeStreamerSettings>? ConnectionChanged;
    event EventHandler<Cube3DDrawingConfig>? DisplayChanged;

    void SaveCubeSettings(CubeSettings settings);
    void SaveConnectionSettings(CubeStreamerSettings settings);
    void SaveDisplaySettings(Cube3DDrawingConfig settings);
}
