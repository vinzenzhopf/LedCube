using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Viewport3dDemo.CubeViewport
{
    public static class VectorExtensions
    {
        public static Vector3D ToVector3D(in Vector4 source)
        {
            return new Vector3D(source.X, source.Y, source.Z);
        }
        
        public static Point3D ToPoint3D(in Vector4 source)
        {
            return new Point3D(source.X, source.Y, source.Z);
        }
        
        public static Vector4 ToVector4(in Point3D source)
        {
            return new Vector4((float)source.X, (float)source.Y, (float)source.Z, 1);
        }

        public static Vector2 ToVector2(in Point source)
        {
            return new Vector2((float) source.X, (float) source.Y);
        }
        
        public static float ConvertToRadians(this float angle)
        {
            return (MathF.PI / 180.0f) * angle;
        }

        public static float ConvertToDegree(this float rad)
        {
            return (180.0f / MathF.PI) * rad;
        }
        
        public static Vector3 EulerAngles(in Matrix4x4 m)
        {
            // Calculate the Euler angles from the rotation matrix
            var x = (float)Math.Atan2(m.M23, m.M33);
            var y = (float)Math.Atan2(-m.M13, Math.Sqrt(m.M11 * m.M11 + m.M12 * m.M12));
            var z = (float)Math.Atan2(m.M12, m.M11);

            // Return the Euler angles as a Vector3 object
            return new Vector3(x, y, z);
        }
        
        public static Vector3 EulerAngles(in Matrix4x4 m, in EulerOrder order)
        {
            // Calculate the Euler angles based on the specified order
            var x = 0f;
            var y = 0f;
            var z = 0f;
            switch (order)
            {
                case EulerOrder.XYZ:
                    x = (float)Math.Atan2(m.M23, m.M33);
                    y = (float)Math.Atan2(-m.M13, Math.Sqrt(m.M11 * m.M11 + m.M12 * m.M12));
                    z = (float)Math.Atan2(m.M12, m.M11);
                    break;
                case EulerOrder.XZY:
                    x = (float)Math.Atan2(-m.M32, m.M22);
                    y = (float)Math.Atan2(m.M31, Math.Sqrt(m.M33 * m.M33 + m.M22 * m.M22));
                    z = (float)Math.Atan2(-m.M21, m.M11);
                    break;
            }

            // Check for singularities and adjust the angles accordingly
            if (Math.Abs(x) + Math.Abs(y) + Math.Abs(z) > 1e-6)
            {
                if (Math.Abs(x) < 1e-6)
                {
                    x = 0f;
                    y = (float)Math.Sign(m.M32) * (float)Math.PI / 2f;
                    z = (float)Math.Atan2(m.M21, m.M22);
                }
                else if (Math.Abs(y) < 1e-6)
                {
                    x = (float)Math.Atan2(m.M23, m.M33);
                    y = 0f;
                    z = (float)Math.Sign(m.M13) * (float)Math.PI / 2f;
                }
                else if (Math.Abs(z) < 1e-6)
                {
                    x = (float)Math.Atan2(m.M12, m.M11);
                    y = (float)Math.Sign(m.M31) * (float)Math.PI / 2f;
                    z = 0f;
                }
            }

            // Return the Euler angles as a Vector3 object
            return new Vector3(x, y, z);
        }
    }

    public enum EulerOrder
    {
        XYZ,
        XZY
    }
}