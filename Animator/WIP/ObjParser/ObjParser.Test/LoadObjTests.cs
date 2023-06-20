namespace ObjParser.Test
{
    public class LoadObjTests
    {
        private Obj obj;
        private Mtl mtl;
        
        public LoadObjTests()
        {
            obj = new Obj();
            mtl = new Mtl();
        }

        #region Vertex
        [Fact]
        public void LoadObj_OneVert_OneVertCount()
        {
            // Arrange
            var objFile = new[]
            {
                "v 0.0 0.0 0.0"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.VertexList.Count == 1);
        }

        [Fact]
        public void LoadOBj_TwoVerts_TwoVertCount()
        {
            // Arrange
            var objFile = new[]
            {
                "v 0.0 0.0 0.0",
                "v 1.0 1.0 1.0"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.VertexList.Count == 2);
        }

        [Fact]
        public void LoadObj_EmptyObj_EmptyObjNoVertsNoFaces()
        {
            // Arrange
            var objFile = new string[] {};

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.VertexList.Count == 0);
            Assert.True(obj.FaceList.Count == 0);
        }

        [Fact]
        public void LoadObj_NoVertPositions_ThrowsArgumentException()
        {
            // Arrange
            var objFile = new[]
            {
                "v 0.0 0.0 0.0",
                "v"
            };

            // Act

            // Assert
            Assert.Throws<ArgumentException>(() => obj.LoadObj(objFile));
        }

        [Fact]
        public void LoadObj_CommaSeperatedVertPositions_ThrowsArgumentException()
        {
            // Arrange
            var objFile = new[]
            {
                // Valid
                "v 0, 0, 0,",

                // Invalid
                "v 0.1, 0.1, 0.2,",
                "v 0.1, 0.1, 0.3,",
                "v 0.1, 0.1, 0.4,"
            };

            // Act

            // Assert
            Assert.Throws<ArgumentException>(() => obj.LoadObj(objFile));
        }

        [Fact]
        public void LoadObj_LettersInsteadOfPositions_ThrowsArgumentException()
        {
            // Arrange
            var objFile = new[]
            {
                "v a b c"
            };

            // Act

            // Assert
            Assert.Throws<ArgumentException>(() => obj.LoadObj(objFile));
        }
        #endregion

        #region TextureVertex
        [Fact]
        public void LoadObj_OneTextureVert_OneTextureVertCount() {
            // Arrange
            var objFile = new[]
            {
                "vt 0.0 0.0"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.TextureList.Count == 1);
        }

        [Fact]
        public void LoadOBj_TwoTextureVerts_TwoTextureVertCount() {
            // Arrange
            var objFile = new[]
            {
                "vt 0.0 0.0",
                "vt 1.0 1.0"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.TextureList.Count == 2);
        }

        [Fact]
        public void LoadOBj_TwoTextureVerts_TwoTextureVertValues() {
            // Arrange
            var objFile = new[]
            {
                "vt 5.0711 0.0003",
                "vt 5.4612 1.0000"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.TextureList.Count == 2);
            Assert.Equal(5.0711d, obj.TextureList[0].X);
            Assert.Equal(0.0003d, obj.TextureList[0].Y);
            Assert.Equal(5.4612d, obj.TextureList[1].X);
            Assert.Equal(1.0000d, obj.TextureList[1].Y);
        }
        #endregion

        #region Mtl
        [Fact]
        public void Mtl_LoadMtl_TwoMaterials() {
            // Arrange
            var mtlFile = new[]
            {
                "newmtl Material",
                "Ns 96.07843",
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
                "Ns 96.07843",
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

        #region Face
        [Fact]
        public void LoadObj_FourVertsSingleFace_FourVertsOneFaceCount()
        {
            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "f 1/1/1 2/2/1 3/3/1"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.VertexList.Count == 4);
            Assert.True(obj.FaceList.Count == 1);
            Assert.Null(obj.FaceList[0].UseMtl);
        }

        [Fact]
        public void LoadObj_FourVertsThreeFace_TwoMaterialsCount() {
            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "usemtl Material",
                "f 1/1/1 2/2/1 3/3/1",
                "usemtl Material.001",
                "f 1/1/1 2/2/1 3/3/1",
                "f 1/1/1 2/2/1 3/3/1"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.VertexList.Count == 4);
            Assert.True(obj.FaceList.Count == 3);
            Assert.Equal(obj.FaceList[0].UseMtl, "Material");
            Assert.Equal(obj.FaceList[1].UseMtl, "Material.001");
            Assert.Equal(obj.FaceList[2].UseMtl, "Material.001");
        }

        [Fact]
        public void LoadObj_FourVertsTwoFace_OneMaterialCount() {
            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "f 1/1/1 2/2/1 3/3/1",
                "usemtl Material",
                "f 1/1/1 2/2/1 3/3/1"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.VertexList.Count == 4);
            Assert.True(obj.FaceList.Count == 2);
            Assert.Null(obj.FaceList[0].UseMtl);
            Assert.Equal(obj.FaceList[1].UseMtl, "Material");
        }

        [Fact]
        public void LoadObj_FourVertsSingleFaceNoTextureVerts_FourVertsOneFaceCount() {
            // Arrange
            var objFile = new[]
            {
                "v -0.500000 -0.500000 0.500000",
                "v 0.500000 -0.500000 0.500000",
                "v -0.500000 0.500000 0.500000",
                "v 0.500000 0.500000 0.500000",
                "f 1//1 2//1 3//1"
            };

            // Act
            obj.LoadObj(objFile);

            // Assert
            Assert.True(obj.VertexList.Count == 4);
            Assert.True(obj.FaceList.Count == 1);
        }
        #endregion
    }
}
