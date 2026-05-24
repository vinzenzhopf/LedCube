using System;
using LedCube.Core.Common.Config.Entities;
using LedCube.Core.Common.Model;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Helpers;

public static class EnumValues
{
    public static CartesianOrientation[] CartesianOrientations { get; } = Enum.GetValues<CartesianOrientation>();
    public static LedShape[] LedShapes { get; } = Enum.GetValues<LedShape>();
    public static Orientation3D[] Orientation3Ds { get; } = Enum.GetValues<Orientation3D>();
    public static LogLevel[] LogLevels { get; } = Enum.GetValues<LogLevel>();
}
