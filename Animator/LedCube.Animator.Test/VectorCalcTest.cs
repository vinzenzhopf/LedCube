using Xunit;
using System;
using System.Windows.Media.Media3D;

namespace LedCube.Animator.Test
{
    public class VectorCalcTest
    {
        [Fact]
        public void TestCameraVertRotation()
        {
            var cameraDir = new Vector3D(1, 3, 4);
            cameraDir.Normalize();



            var angleOnX = Math.Atan(cameraDir.Z / cameraDir.X);
            
            
                        
                        
            //current angle around the y axis
            
            // var angleFromYAxis = Vector3D.AngleBetween(Vectors3D.Up, _cameraDirection);
            // angleFromYAxis += diff.Y * 0.2;
            //
            // var angleFromXAxis = Vector3D.AngleBetween(Vectors3D.Right, _cameraDirection);
            //             
            //             
            // //Rotate by the given angle
            // var rotationVerticalZ = new AxisAngleRotation3D(Vectors3D.Front, diff.Y * 0.2);
            // var transformVerticalZ = new RotateTransform3D(rotationVerticalZ);
            // //Then Rotate by the current y angle
            // var rotationVerticalY = new AxisAngleRotation3D(Vectors3D.Up, currYAngle);
            // var transformVerticalY = new RotateTransform3D(rotationVerticalY);
            //             
            // // var rot = new QuaternionRotation3D(new Quaternion())
            //             
            // _cameraDirection *= (transformVerticalZ.Value * transformVerticalY.Value);
            // //
            // Console.WriteLine("Rotate Camera by diff:" + diff);
            // // _cameraDirection = _cameraDirection.RotateZ(diff.X.ToRadians() * 0.2);
            // // _cameraDirection = _cameraDirection.RotateY(diff.Y.ToRadians() * 0.2);
            // // _cameraDirection = _cameraDirection.RotateX(diff.Y.ToRadians() * 0.2);
            // _cameraDirection.Normalize();
            // UpdateCameraPosition();   
            
            
        }
    }
}