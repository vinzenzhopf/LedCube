using System;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using Quaternion = System.Numerics.Quaternion;

namespace Viewport3dDemo.CubeViewport
{
    
    [ObservableObject]
    public partial class CameraManager
    {
        public Viewport3D Viewport { get; }
        public PerspectiveCamera Camera { get; } = new PerspectiveCamera();
        
        [ObservableProperty]
        private Point3D? _target;

        [ObservableProperty]
        private float _nick;
        [ObservableProperty]
        private float _distance;
        [ObservableProperty]
        private float _elevation;
        [ObservableProperty]
        private float _orbit;

        [ObservableProperty] 
        private Vector2? _startDragMousePos;

        partial void OnNickChanged(float value) => UpdateCamera();
        partial void OnDistanceChanged(float value) => UpdateCamera();
        partial void OnElevationChanged(float value) => UpdateCamera();
        partial void OnOrbitChanged(float value) => UpdateCamera();
        partial void OnTargetChanged(Point3D? value) => UpdateCamera();

        /*
         * 3D Viewport:
         *   Z front+ back-
         *   X right+ left-
         *   Y top+ bottom-
         */
        
        public void UpdateCamera()
        {
            var matNick = Matrix4x4.CreateRotationX(_nick);
            var matBack = Matrix4x4.CreateTranslation(new Vector3(0, 0, _distance));
            var matElevate = Matrix4x4.CreateRotationX(_elevation);
            var matOrbit = Matrix4x4.CreateRotationZ(_orbit);
            var mat = Matrix4x4.Identity;
            mat = Matrix4x4.Multiply(mat, matNick);
            mat = Matrix4x4.Multiply(mat, matBack);
            mat = Matrix4x4.Multiply(mat, matElevate);
            mat = Matrix4x4.Multiply(mat, matOrbit);

            var pos = Vector4.Transform(Vector4.UnitW, mat);
            var viewDirection = Vector4.Negate(Vector4.Transform(Vector4.UnitZ, mat));
            var cameraUp = Vector4.Transform(Vector4.UnitY, mat);

            if (_target != null)
            {
                viewDirection = Vector4.Normalize(Vector4.Subtract(VectorExtensions.ToVector4(_target.Value), pos));
                var direction = new Vector3(viewDirection.X, viewDirection.Y, viewDirection.Z);
                direction = Vector3.Normalize(Vector3.Cross(Vector3.Cross(direction, Vector3.UnitY), direction));
                cameraUp = new Vector4(direction, 0);
            }

            Camera.Position = VectorExtensions.ToPoint3D(pos);
            Camera.LookDirection = VectorExtensions.ToVector3D(viewDirection);
            Camera.UpDirection = VectorExtensions.ToVector3D(cameraUp);
        }

        public CameraManager(Viewport3D viewport, double fieldOfView)
        {
            Viewport = viewport;
            Viewport.Camera = Camera;
            Camera.FieldOfView = fieldOfView;
            UpdateCamera();
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            Viewport.MouseWheel += OnMouseWheelEvent;
            Viewport.MouseMove += OnMouseMoveEvent;
            Viewport.MouseLeftButtonDown += OnMouseLeftDown;
            Viewport.MouseLeftButtonUp += OnMouseLeftUp;
            Viewport.KeyDown += OnKeyDownEvent;
            Viewport.PreviewKeyDown += OnPrevKeyDown;
        }
        
        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            _startDragMousePos = null;
        }

        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            _startDragMousePos = VectorExtensions.ToVector2(e.GetPosition(Viewport));
        }
        
        private void OnMouseMoveEvent(object sender, MouseEventArgs e)
        {
            if (_startDragMousePos is null)
                return;
            
            var currentMousePos = VectorExtensions.ToVector2(e.GetPosition(Viewport));
            
            var va = GetArcBallVector(_startDragMousePos.Value);
            var vb = GetArcBallVector(currentMousePos);
            
            // // Calculate the rotation matrix that represents the rotation from va to vb
            // var rotationMatrix = Matrix4x4.CreateFromQuaternion(FromToRotation(va, vb));
            //
            // // Extract the rotation axis from the rotation matrix
            // Vector3 rotationAxis = VectorExtensions.EulerAngles(rotationMatrix);

            
            float angle = MathF.Acos(MathF.Min(1.0f, Vector3.Dot(va, vb)));
            Vector3 rotationAxis = Vector3.Cross(va, vb);
            // Matrix3x3 camera2object = Vector3.Inverse(Vector3::mat3(transforms[MODE_CAMERA]) * glm::mat3(mesh.object2world));
            // Vector3 axis_in_object_coord = camera2object * axis_in_camera_coord;
            // mesh.object2world = glm::rotate(mesh.object2world, glm::degrees(angle), axis_in_object_coord);
            // last_mx = cur_mx;
            // last_my = cur_my;
            
        }
        
        private static Quaternion FromToRotation(Vector3 va, Vector3 vb)
        {
            // Calculate the cross product of the two vectors
            var cross = Vector3.Cross(va, vb);

            // Calculate the dot product of the two vectors
            var dot = Vector3.Dot(va, vb);

            // Calculate the w component of the quaternion
            var w = (float)Math.Sqrt(va.LengthSquared() * vb.LengthSquared()) + dot;

            // Create the quaternion from the calculated values
            var q = new Quaternion(cross.X, cross.Y, cross.Z, w);

            // Normalize the quaternion
            return Quaternion.Normalize(q);
        }

        /**
         * Get a normalized vector from the center of the virtual ball O to a
         * point P on the virtual ball surface, such that P is aligned on
         * screen's (X,Y) coordinates.  If (X,Y) is too far away from the
         * sphere, return the nearest point on the virtual ball surface.
         */
        private Vector3 GetArcBallVector(Vector2 pos)
        {
            var p = new Vector3(
                1 * pos.X / (float) Viewport.Width * 2 - 1,
                -(1 * pos.Y / (float) Viewport.Height * 2 - 1),
                0);
            var opSquared = Vector3.Dot(p, p);
            if (opSquared <= 1)
                p.Z = MathF.Sqrt(1 - opSquared); // Pythagoras
            else
                p = Vector3.Normalize(p); // nearest point
            return p;
        }

        private void OnPrevKeyDown(object sender, KeyEventArgs e)
        {
            // switch (e.Key)
            // {
            //     case Key.Left:
            //         Orbit -= 15.0f.ConvertToRadians();
            //         break;
            //     case Key.Right:
            //         Orbit += 15.0f.ConvertToRadians();
            //         break;
            //     case Key.Up:
            //         Elevation += 5.0f.ConvertToRadians();
            //         break;
            //     case Key.Down:
            //         Elevation -= 5.0f.ConvertToRadians();
            //         break;
            // }
        }

        private void OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    Orbit -= 15.0f.ConvertToRadians();
                    break;
                case Key.Right:
                    Orbit += 15.0f.ConvertToRadians();
                    break;
                case Key.Up:
                    Elevation += 5.0f.ConvertToRadians();
                    break;
                case Key.Down:
                    Elevation -= 5.0f.ConvertToRadians();
                    break;
            }
        }
        
        private void OnMouseWheelEvent(object sender, MouseWheelEventArgs e)
        {
            Distance -= (float)(e.Delta / 120.0);
        }
    }
}