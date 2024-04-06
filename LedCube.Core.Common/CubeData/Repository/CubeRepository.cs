using LedCube.Core.Common.CubeData.Projections;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;

namespace LedCube.Core.Common.CubeData.Repository;

public class CubeRepository : ICubeRepository
{
    private ICubeData _data;
    
    public CubeRepository()
    {
        _data = new Model.Cube.CubeData<CubeDataBuffer16>();
        _data.SetLed(new Point3D(0, 0, 0), true);
        _data.SetLed(new Point3D(1, 1, 1), true);
        _data.SetLed(new Point3D(2, 2, 2), true);
        _data.SetLed(new Point3D(3, 3, 3), true);
        _data.SetLed(new Point3D(4, 4, 4), true);
    }

    public ICubeData GetCubeData()
    {
        return _data;
    }

    public void SetCubeData(ICubeData cubeData)
    {
        _data = cubeData;
    }
}