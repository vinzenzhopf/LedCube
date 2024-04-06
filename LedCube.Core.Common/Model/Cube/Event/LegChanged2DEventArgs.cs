namespace LedCube.Core.Common.Model.Cube.Event;

public sealed class LegChangedEventArgs<TPos>(TPos position, bool value) : EventArgs
{
    public TPos Position { get; } = position;
    public bool Value { get; } = value;
}