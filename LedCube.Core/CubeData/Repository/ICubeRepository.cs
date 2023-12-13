using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.CubeData.Repository;

public interface ICubeRepository
{
    public ICubeData GetCubeData();
}