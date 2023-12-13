namespace LedCube.Core.Common.Model.Cube;

public interface ICubeDataBuffer : ICubeData
{
    public bool[] Buffer { get; }
    public int Length { get; }

    bool GetLed(int index);
    void SetLed(int index, bool value);

    int CoordinatesToIndex(Point3D p);
    Point3D IndexToCoordinates(int index);
}