using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.CubeData;

public delegate void LedChangedArgs(Point3D p, bool value);

public delegate void CubeChangedArgs(ICubeData cubeData);

public interface ICubeData
{
    public Point3D Size { get; }
    
    public event CubeChangedArgs? CubeChanged;
    public event LedChangedArgs? LedChanged;

    public bool GetLed(Point3D p);

    public void SetLed(Point3D p, bool value);
    
    protected static Point3D IndexToCoordinates(Point3D size, int index) => new(
        index % size.X,
        (index / size.X) % size.Y,
        (index / (size.X * size.Y)) % size.Z
    );
}