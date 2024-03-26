using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Common.CubeData.Repository;

public interface ICubeRepository
{
    public ICubeData GetCubeData();

    public void SetCubeData(ICubeData cubeData);
}