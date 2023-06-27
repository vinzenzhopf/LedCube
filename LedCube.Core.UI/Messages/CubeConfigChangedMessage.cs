using System;

namespace LedCube.Core.UI.Messages;

public sealed record CubeConfigChangedMessage(string entry, Type type, object oldValue, object newValue);