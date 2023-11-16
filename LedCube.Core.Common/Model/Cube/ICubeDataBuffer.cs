namespace LedCube.Core.Common.Model.Cube;

public interface ICubeDataBuffer : ICubeData
{
    public bool[] Buffer { get; }
}