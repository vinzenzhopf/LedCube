using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ObjParser;
using Viewport3dDemo.CubeViewport;

namespace Viewport3dDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ObservableObject]
    public partial class MainWindow : Window
    {
        /*
         * 3D Viewport:
         *   Z front+ back-
         *   X right+ left-
         *   Y top+ bottom-
         */

        public GeometryModel3D TestModelTeapod { get; }
        
        private CancellationTokenSource cts = new CancellationTokenSource();

        [ObservableProperty]
        private string _debugInfo;
        
        private Task _infoUpdateTask;

        private Model3DGroup _modelGroup;
        private ModelVisual3D _myModelVisual3D;

        public CameraManager Manager { get; }

        [ObservableProperty]
        private float _nick;
        [ObservableProperty]
        private float _distance;
        [ObservableProperty]
        private float _elevation;
        [ObservableProperty]
        private float _orbit;

        partial void OnNickChanged(float value) => Manager.Nick = value;
        partial void OnDistanceChanged(float value) => Manager.Distance = value;
        partial void OnElevationChanged(float value) => Manager.Elevation = value;
        partial void OnOrbitChanged(float value) => Manager.Orbit = value;

        private const string MODEL_PATH = "Model.stl";
        
        public MainWindow()
        {
            // TestModelTeapod = new GeometryModel3D()
            //     _contentLoaded
            //
            //     using (var file = File.OpenRead("Blender/teacup.obj"))
            //     {
            //         var obj = new Obj();
            //         obj.LoadObj(file);
            //         var mtl = new Mtl();
            //         mtl.LoadMtl(file);
            //         
            //         obj.
            //     }


            InitializeComponent();
            _infoUpdateTask = Task.Run(() => RunInfoUpdateTask(cts.Token));
            
            _modelGroup = new Model3DGroup();
            _myModelVisual3D = new ModelVisual3D();
            
            this.Background = Brushes.CornflowerBlue;
            Manager = new CameraManager(Viewport, 80)
            {
                Distance = 10,
                Elevation = 30.0f.ConvertToRadians()
            };
            
            DirectionalLight light = new DirectionalLight();
            light.Color = Colors.White;
            light.Direction = new Vector3D(10, 10, 10);
            _modelGroup.Children.Add(light);

            _modelGroup.Children.Add(CreatePlane());
            _modelGroup.Children.Add(CreateModel());


            _myModelVisual3D.Content = _modelGroup;
            
            
            // // Create a new 2D text string
            // var text = new FormattedText("Hello, world!",
            //     CultureInfo.InvariantCulture,
            //     FlowDirection.LeftToRight,
            //     new Typeface("Arial"),
            //     24,
            //     Brushes.Black);
            //
            // // Create a 2D geometry that represents the text
            // Geometry textGeometry = text.BuildGeometry(new Point(0, 0));
            //
            // // Create a 3D mesh that represents the text in 3D space
            // MeshGeometry3D textMesh = new MeshGeometry3D(textGeometry);
            //
            // // Create a 3D geometry model that uses the mesh
            // GeometryModel3D textGeometryModel = new GeometryModel3D();
            //
            // // Add the 2D text geometry to the 3D mesh
            // textGeometryModel.Geometry = textMesh;
            //
            // _modelGroup.Children.Add(textGeometryModel);
            
            // Viewport.Children.Add(_myModelVisual3D);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            cts.Cancel();
            base.OnClosing(e);
        }

        private void RunInfoUpdateTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var info = new StringBuilder()
                        .Append($"Nick: {Manager?.Nick}\n")
                        .Append($"Distance: {Manager?.Distance}\n")
                        .Append($"Elevation: {Manager?.Elevation}\n")
                        .Append($"Orbit: {Manager?.Orbit}\n")
                        .Append($"Position: {Manager?.Camera.Position}\n")
                        .Append($"LookDirection: {Manager?.Camera.LookDirection}\n")
                        .Append($"UpDirection: {Manager?.Camera.UpDirection}\n")
                        .Append($"FieldOfView: {Manager?.Camera.FieldOfView}\n")
                        .Append($"StartDragMousePos: {Manager?.StartDragMousePos}\n")
                        .Append($"Nick: {Nick}\n")
                        .Append($"Distance: {Distance}\n")
                        .Append($"Elevation: {Elevation}\n")
                        .Append($"Orbit: {Orbit}\n")
                        .ToString();
                    DebugInfo = info;
                });
                Task.Delay(100, cancellationToken);
            }
        }

        private Model3D CreateLine(Point3D startPoint, Point3D endPoint)
        {
            SolidColorBrush brush = new SolidColorBrush(Colors.Red);
            var material = new DiffuseMaterial(brush);
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(startPoint);
            mesh.Positions.Add(endPoint);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            return new GeometryModel3D(mesh, material);
        }
        
        private Model3D CreatePlane()
        {
            GeometryModel3D model = new GeometryModel3D();
            
            model.Geometry = PrimitiveGeometry3D.CreatePlane(3, 3);
            
            var brush = new SolidColorBrush(Colors.YellowGreen);
            model.Material = new DiffuseMaterial(brush);
            
            return model;
        }

        public GeometryModel3D CreateModel()
        {
            GeometryModel3D model = new GeometryModel3D();
            
            model.Geometry = PrimitiveGeometry3D.CreateCube(1, 1, 1);
            
            // The material specifies the material applied to the 3D object. In this sample a
            // linear gradient covers the surface of the 3D object.
            
            // Create a horizontal linear gradient with four stops.
            LinearGradientBrush gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0, 0.5);
            gradient.EndPoint = new Point(1, 0.5);
            gradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            gradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            gradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            gradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));
            
            // Define material and apply to the mesh geometries.
            model.Material = new DiffuseMaterial(gradient);

            // return new GeometryModel3D(
            //     new MeshGeometry3D(){
            //         Positions = new Point3DCollection(new Point3D[] {
            //             new Point3D(-1, -1, -1),
            //             new Point3D(1, -1, -1),
            //             new Point3D(1, 1, -1),
            //             new Point3D(-1, 1, -1),
            //             new Point3D(-1, -1, 1),
            //             new Point3D(1, -1, 1),
            //             new Point3D(1, 1, 1),
            //             new Point3D(-1, 1, 1)
            //         }),
            //         TriangleIndices = new Int32Collection(new int[] {
            //             0, 1, 2, 3,
            //             1, 5, 6, 2,
            //             5, 4, 7, 6,
            //             4, 0, 3, 7,
            //             3, 2, 6, 7,
            //             4, 5, 1, 0
            //         }),
            //         TextureCoordinates = new Vector3DCollection(new Vector3D[] {
            //             new Vector3D(0, 0, -1),
            //             new Vector3D(0, 0, -1),
            //             new Vector3D(0, 0, -1),
            //             new Vector3D(0, 0, -1),
            //             new Vector3D(0, 0, 1),
            //             new Vector3D(0, 0, 1),
            //             new Vector3D(0, 0, 1),
            //             new Vector3D(0, 0, 1)
            //         })
            //     ),
            //     new DiffuseMaterial(Brushes.LightBlue)
            // );
            
            return model;
        }

        private MeshGeometry3D CreateGeometry()
        {
            // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet
            // is created.
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            // // Create a collection of normal vectors for the MeshGeometry3D.
            // Vector3DCollection myNormalCollection = new Vector3DCollection();
            // myNormalCollection.Add(new Vector3D(0,0,1));
            // myNormalCollection.Add(new Vector3D(0,0,1));
            // myNormalCollection.Add(new Vector3D(0,0,1));
            // myNormalCollection.Add(new Vector3D(0,0,1));
            // myNormalCollection.Add(new Vector3D(0,0,1));
            // myNormalCollection.Add(new Vector3D(0,0,1));
            // myMeshGeometry3D.Normals = myNormalCollection;

            // Create a collection of vertex positions for the MeshGeometry3D.
            Point3DCollection myPositionCollection = new Point3DCollection();
            myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
            myPositionCollection.Add(new Point3D(0.5, -0.5, 0.5));
            myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
            myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
            myPositionCollection.Add(new Point3D(-0.5, 0.5, 0.5));
            myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
            myMeshGeometry3D.Positions = myPositionCollection;

            // // Create a collection of texture coordinates for the MeshGeometry3D.
            // PointCollection myTextureCoordinatesCollection = new PointCollection();
            // myTextureCoordinatesCollection.Add(new Point(0, 0));
            // myTextureCoordinatesCollection.Add(new Point(1, 0));
            // myTextureCoordinatesCollection.Add(new Point(1, 1));
            // myTextureCoordinatesCollection.Add(new Point(1, 1));
            // myTextureCoordinatesCollection.Add(new Point(0, 1));
            // myTextureCoordinatesCollection.Add(new Point(0, 0));
            // myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            myTriangleIndicesCollection.Add(0);
            myTriangleIndicesCollection.Add(1);
            myTriangleIndicesCollection.Add(2);
            myTriangleIndicesCollection.Add(3);
            myTriangleIndicesCollection.Add(4);
            myTriangleIndicesCollection.Add(5);
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            return myMeshGeometry3D;
        }
    }
}