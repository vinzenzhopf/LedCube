using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Common.CubeData.Converter;

public static class CubeDataConverter
{
    public static void ConvertCubeData(ICubeData cubeData, ref byte[] data)
    {
        for (var i = 0; i < cubeData.Size.ElementProduct; i++)
        {
            var index = i / 8;
            var bit = i % 8;
            if (bit == 0) data[index] = 0;
            var led = (byte)(cubeData.GetLed(Point3D.CoordinateFromIndex(cubeData.Size, i))?1:0);
            data[index] |= (byte)(led << bit);
        }
    }
}