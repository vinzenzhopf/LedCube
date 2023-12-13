using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.CubeData.Repository;

public class CubeRepository : ICubeRepository
{
    private Common.Model.Cube.CubeData _data;
    
    public CubeRepository()
    {
        _data = new Common.Model.Cube.CubeData(new Point3D(16,16,16));
    }

    public ICubeData GetCubeData()
    {
        return _data;
    }
}