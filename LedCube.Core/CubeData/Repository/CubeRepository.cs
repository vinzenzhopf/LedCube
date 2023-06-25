using LedCube.Core.Common.Model;

namespace LedCube.Core.CubeData.Repository;

public class CubeRepository : ICubeRepository
{
    private CubeData _data;
    
    public CubeRepository()
    {
        _data = new CubeData(new Point3D(16,16,16));
    }

    public ICubeData GetCubeData()
    {
        return _data;
    }
}