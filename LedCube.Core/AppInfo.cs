using System;

namespace LedCube.Core;

public record AppInfo(string Version, DateTime? BuildDate, bool DebugBuild);