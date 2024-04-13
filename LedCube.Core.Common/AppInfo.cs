using System;

namespace LedCube.Core.Common;

public record AppInfo(string Version, DateTime? BuildDate, bool DebugBuild);