namespace LedCube.Core.Common.Model.Cube.Event;

public delegate void LedChangedEventHandler<TPos>(object? sender, LegChangedEventArgs<TPos> value);