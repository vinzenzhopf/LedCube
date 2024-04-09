using System;
using System.Numerics;

namespace LedCube.Sdf.Core;

public static class Driver
{

    public static Sdf3D ConstantAngularVelotcity(Sdf3D sdf, Vector3 rotationAxis, float angularVelocity)
    {
        rotationAxis = Vector3.Normalize(rotationAxis);
        return (position, time) =>
        {
            var angle = angularVelocity * time;
            angle %= MathF.Tau;
            var rotation = Quaternion.CreateFromAxisAngle(rotationAxis, angle);
            var inverseRotation = Quaternion.Inverse(rotation);
            return sdf(Vector3.Transform(position, inverseRotation), time);
        };
    }
    
}