using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ObjParser;

namespace Viewport3dDemo.CubeViewport
{
    public static class PrimitiveGeometry3D
    {
        // public static MeshGeometry3D CreatePlane(Point3D p1, Point3D p2)
        // {
        //     
        // }

        // public static MeshGeometry3D CreateFromObj(Obj obj, Mtl mtl)
        // {
        //     var points = new Point3DCollection();
        //     var triangleIndices = new Int32Collection();
        //     var normals = new Vector3DCollection();
        //     var uvMap = new PointCollection();
        //
        //     foreach (var v in obj.VertexList)
        //     {
        //         points.Add(new Point3D(v.X, v.Y, v.Z));
        //     }
        //     foreach (var f in obj.FaceList)
        //     {
        //         f.
        //         triangleIndices.Add(v.);
        //     }
        //     
        // }

        public static MeshGeometry3D CreatePlane(double width, double length)
        {
            var points = new Point3DCollection();
            var triangleIndices = new Int32Collection();
            var normals = new Vector3DCollection();
            var uvMap = new PointCollection();

            var z = width / 2;
            var y = length / 2;
            
            points.Add(new Point3D(z, 0, y));
            points.Add(new Point3D(z, 0, -y));
            points.Add(new Point3D(-z, 0, -y));
            points.Add(new Point3D(-z, 0, y));
            
            triangleIndices.Add(2);
            triangleIndices.Add(1);
            triangleIndices.Add(0);
            
            triangleIndices.Add(0);
            triangleIndices.Add(3);
            triangleIndices.Add(2);
            
            normals.Add(new Vector3D(0,1,0));
            normals.Add(new Vector3D(0,1,0));
            normals.Add(new Vector3D(0,1,0));
            normals.Add(new Vector3D(0,1,0));
            
            return new MeshGeometry3D
            {
                Positions = points,
                TriangleIndices = triangleIndices,
                Normals = normals,
                TextureCoordinates = uvMap
            };
        }
        
        /*
         * 3D Viewport:
         *   Z front+ back-
         *   X right+ left-
         *   Y top+ bottom-
         */
        
        public static MeshGeometry3D CreateCube(double width, double length, double height)
        {
            var w = width / 2;
            var l = length / 2;
            var h = height / 2;
            var points = new Point3DCollection();
            var triangleIndices = new Int32Collection();

            points.Add(new Point3D(-w, h, l));
            points.Add(new Point3D(w, h, l));
            points.Add(new Point3D(w, h, -l));
            points.Add(new Point3D(-w, h, -l));
            points.Add(new Point3D(-w, -h, l));
            points.Add(new Point3D(w, -h, l));
            points.Add(new Point3D(w, -h, -l));
            points.Add(new Point3D(-w, -h, -l));
            
            //TOP
            triangleIndices.Add(0);
            triangleIndices.Add(1);
            triangleIndices.Add(3);
            triangleIndices.Add(1);
            triangleIndices.Add(2);
            triangleIndices.Add(3);
            //FRONT
            triangleIndices.Add(4);
            triangleIndices.Add(5);
            triangleIndices.Add(0);
            triangleIndices.Add(5);
            triangleIndices.Add(1);
            triangleIndices.Add(0);
            //RIGHT
            triangleIndices.Add(5);
            triangleIndices.Add(6);
            triangleIndices.Add(1);
            triangleIndices.Add(6);
            triangleIndices.Add(2);
            triangleIndices.Add(1);
            //BACK
            triangleIndices.Add(6);
            triangleIndices.Add(7);
            triangleIndices.Add(2);
            triangleIndices.Add(7);
            triangleIndices.Add(3);
            triangleIndices.Add(2);
            //LEFT
            triangleIndices.Add(7);
            triangleIndices.Add(4);
            triangleIndices.Add(0);
            triangleIndices.Add(3);
            triangleIndices.Add(7);
            triangleIndices.Add(0);
            //BOTTOM
            triangleIndices.Add(5);
            triangleIndices.Add(4);
            triangleIndices.Add(7);
            triangleIndices.Add(5);
            triangleIndices.Add(7);
            triangleIndices.Add(6);

            var normals = new Vector3DCollection();
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));

            var textureCoordinates = new PointCollection();
            textureCoordinates.Add(new Point(1, 0));
            textureCoordinates.Add(new Point(1, 1));
            textureCoordinates.Add(new Point(0, 1));
            textureCoordinates.Add(new Point(0, 1));
            textureCoordinates.Add(new Point(0, 0));
            textureCoordinates.Add(new Point(1, 0));
            
            return new MeshGeometry3D
            {
                Positions = points,
                TriangleIndices = triangleIndices,
                Normals = normals,
                TextureCoordinates = textureCoordinates
            };
        }
        
        /// <summary>
        /// A standard UV sphere is made out of quad faces and a triangle fan at the top and bottom. It can be used for texturing.
        /// </summary>
        /// <param name="radius">Radius of the sphere.</param>
        /// <param name="segments">Number of vertical segments. Like the Earth’s meridians, going pole to pole.</param>
        /// <param name="rings">Number of horizontal segments. These are like the Earth’s parallels.</param>
        /// <returns>A standard UV sphere as MeshGeometry3D <see cref="MeshGeometry3D"/></returns>
        public static MeshGeometry3D CreateUVSphere(double radius, int segments, int rings)
        {
            var segmentRad = Math.PI / 2 / (segments + 1);
            var ringRad = Math.PI / 2 / (rings + 1);
            
            var points = new Point3DCollection();
            var triangleIndices = new Int32Collection();
            
            //Create Vertices
            points.Add(new Point3D(0, radius, 0)); //Top Vertex
            for (var ring = 0; ring < rings; ring++)
            {
                var ringZ = Math.Cos(ringRad * ring) * radius; //Z-height of the ring
                var ringR = Math.Sin(ringRad * ring) * radius; //radius of the ring
                for (var seg = 0; seg < segments; seg++)
                {
                    var x = Math.Sin(segmentRad * seg) * ringR;
                    var y = Math.Cos(segmentRad * seg) * ringR;
                    points.Add(new Point3D(x, y, ringZ));
                }
            }
            points.Add(new Point3D(0, -1 * radius, 0)); //Bottom Vertex

            //Create Indexes
            //Top Triangle Fan
            var indexOffset = 1;
            for (var i = 0; i < segments; i++)
            {
                triangleIndices.Add(i + indexOffset);
                triangleIndices.Add(0);
                triangleIndices.Add(i + indexOffset + 1);
            }
            //Middle Triangles
            for (var ring = 0; ring < rings-1; ring++)
            {
                var ringOffset = 1 + segments + ring * segments;
                var nextRingOffset = 1 + segments + (ring+1) * segments;
                for (int seg = 0; seg < segments; seg++)
                {
                    triangleIndices.Add(ringOffset + seg);
                    triangleIndices.Add(ringOffset + seg + 1);
                    triangleIndices.Add(nextRingOffset + seg);
                    
                    triangleIndices.Add(ringOffset + seg + 1);
                    triangleIndices.Add(nextRingOffset + seg);
                    triangleIndices.Add(nextRingOffset + seg + 1);
                }
            }
            //Bottom Triangle Fan
            var bottomOffset = 1 + segments * (rings - 1);
            for (var i = 0; i < segments; i++)
            {
                triangleIndices.Add(bottomOffset +  segments); //Should be the last triangle
                triangleIndices.Add(bottomOffset);
                triangleIndices.Add(bottomOffset + 1);
            }

            return new MeshGeometry3D
            {
                Positions = points,
                TriangleIndices = triangleIndices
            };
        }

        /// <summary>
        /// An icosphere is a polyhedral sphere made up of triangles. Icospheres are normally used to achieve a more isotropical layout of vertices than a UV sphere, in other words, they are uniform in every direction.
        /// </summary>
        /// <param name="subdivisions">How many recursions are used to define the sphere. At level 1 the icosphere is an icosahedron, a solid with 20 equilateral triangular faces. Each increase in the number of subdivisions splits each triangular face into four triangles.</param>
        /// <returns>A standard UV sphere as MeshGeometry3D <see cref="MeshGeometry3D"/></returns>
        public static MeshGeometry3D CreateIcoSphere(int subdivisions)
        {
            return null;
        }
    }
}