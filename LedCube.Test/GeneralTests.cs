using LedCube.Core.Common.Model.Cube;
using Xunit;

namespace LedCube.Test;

public class GeneralTests
{

    [Fact]
    void TestStuff()
    {
        // var cube1 = new Cube<bool>(8, 8, 8);

        var led = new BiLed();
        var row = new Row<BiLed>(8);
        var plane = new Plane<BiLed>(8, 8);
        var cube = (ICube<BiLed>)new Cube<BiLed>(8, 8, 8);

        cube.GetPlane(1, Orientation3D.Front).GetRow(4, Orientation2D.Top).GetLed(1, Orientation1D.Left).Value.Value = true;


        // var cube2 = new Cube<RgbLed>(8, 8, 8);


    }
}