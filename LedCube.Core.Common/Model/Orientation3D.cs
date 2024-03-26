namespace LedCube.Core.Common.Model;

public enum Orientation3D
{
    Front   = 0b001, //1
    Right   = 0b010, //2
    Top     = 0b011, //3
    Back    = 0b101, //5
    Left    = 0b110, //6
    Bottom  = 0b111  //7
}

public enum Plane
{
    XY,
    XZ,
    YZ
}
