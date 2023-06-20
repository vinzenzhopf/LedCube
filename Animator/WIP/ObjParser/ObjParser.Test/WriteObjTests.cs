namespace ObjParser.Test
{
    public class WriteObjTests
    {
        private Obj obj;
        private Mtl mtl;
        
        public WriteObjTests()
        {
            obj = new Obj();
            mtl = new Mtl();
        }

        #region Obj
        [Fact]
        public void Obj_WriteObj_TwoMaterials() {
            string tempfilepath = Path.GetTempFileName();
            string[] headers = new string[] { "ObjParser" };

            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "vt 5.0711 0.0003",
                "vt 5.4612 1.0000",
                "usemtl Material",
                "f 1/1/1 2/2/1 3/3/1",
                "usemtl Material.001",
                "f 1/1/1 2/2/1 3/3/1",
                "f 1/1/1 2/2/1 3/3/1"
            };

            // Act
            obj.LoadObj(objFile);
            obj.WriteObjFile(tempfilepath, headers);
            obj = new Obj();
            obj.LoadObj(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.True(obj.VertexList.Count == 4);
            Assert.True(obj.FaceList.Count == 3);
            Assert.Equal("Material", obj.FaceList[0].UseMtl);
            Assert.Equal("Material.001", obj.FaceList[1].UseMtl);
            Assert.Equal("Material.001", obj.FaceList[2].UseMtl);
            Assert.Equal(5.0711d, obj.TextureList[0].X);
            Assert.Equal(0.0003d, obj.TextureList[0].Y);
            Assert.Equal(5.4612d, obj.TextureList[1].X);
            Assert.Equal(1.0000d, obj.TextureList[1].Y);
        }

        [Fact]
        public void Obj_WriteObj_NoMaterials() {
            string tempfilepath = Path.GetTempFileName();
            string[] headers = new string[] { "ObjParser" };

            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "f 1/1/1 2/2/1 3/3/1",
                "f 1/1/1 2/2/1 3/3/1",
                "f 1/1/1 2/2/1 3/3/1"
            };

            // Act
            obj.LoadObj(objFile);
            obj.WriteObjFile(tempfilepath, headers);
            obj = new Obj();
            obj.LoadObj(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.True(obj.VertexList.Count == 4);
            Assert.True(obj.FaceList.Count == 3);
            Assert.Null(obj.FaceList[0].UseMtl);
            Assert.Null(obj.FaceList[1].UseMtl);
            Assert.Null(obj.FaceList[2].UseMtl);
        }
        #endregion

        #region Mtl
        [Fact]
        public void Mtl_WriteMtl_TwoMaterials() {
            string tempfilepath = Path.GetTempFileName();
            string[] headers = new string[] { "ObjParser" };

            // Arrange
            var mtlFile = new[]
            {
                "newmtl Material",
                "Ns 96.078431",
                "Ka 1.000000 1.000000 1.000000",
                "Kd 0.630388 0.620861 0.640000",
                "Ks 0.500000 0.500000 0.500000",
                "Ke 0.000000 0.000000 0.000000",
                "Tf 0.000000 0.000000 0.000000",
                "Ni 1.000000",
                "d 1.000000",
                "illum 2",
                "",
                "newmtl Material.001",
                "Ns 96.078431",
                "Ka 1.000000 1.000000 1.000000",
                "Kd 0.640000 0.026578 0.014364",
                "Ks 0.500000 0.500000 0.500000",
                "Ke 0.000000 0.000000 0.000000",
                "Ni 1.000000",
                "d 1.000000",
                "illum 2"
            };

            // Act
            mtl.LoadMtl(mtlFile);
            mtl.WriteMtlFile(tempfilepath, headers);
            mtl = new Mtl();
            mtl.LoadMtl(tempfilepath);
            File.Delete(tempfilepath);

            // Assert
            Assert.Equal(2, mtl.MaterialList.Count);
            ObjParser.Types.Material first = mtl.MaterialList[0];
            Assert.Equal("Material", first.Name);
            Assert.Equal(96.078431f, first.SpecularExponent);
            Assert.Equal(1.0f, first.AmbientReflectivity.r);
            Assert.Equal(1.0f, first.AmbientReflectivity.g);
            Assert.Equal(1.0f, first.AmbientReflectivity.b);
            Assert.Equal(0.630388f, first.DiffuseReflectivity.r);
            Assert.Equal(0.620861f, first.DiffuseReflectivity.g);
            Assert.Equal(0.640000f, first.DiffuseReflectivity.b);
            Assert.Equal(0.5f, first.SpecularReflectivity.r);
            Assert.Equal(0.5f, first.SpecularReflectivity.g);
            Assert.Equal(0.5f, first.SpecularReflectivity.b);
            Assert.Equal(0.0f, first.EmissiveCoefficient.r);
            Assert.Equal(0.0f, first.EmissiveCoefficient.g);
            Assert.Equal(0.0f, first.EmissiveCoefficient.b);
            Assert.Equal(0.0f, first.TransmissionFilter.r);
            Assert.Equal(0.0f, first.TransmissionFilter.g);
            Assert.Equal(0.0f, first.TransmissionFilter.b);
            Assert.Equal(1.0f, first.OpticalDensity);
            Assert.Equal(1.0f, first.Dissolve);
            Assert.Equal(2, first.IlluminationModel);

            ObjParser.Types.Material second = mtl.MaterialList[1];
            Assert.Equal("Material.001", second.Name);
            Assert.Equal(96.078431f, second.SpecularExponent);
        }
        #endregion
    }
}
