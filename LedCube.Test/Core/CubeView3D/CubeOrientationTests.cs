using System.Numerics;
using LedCube.Core.Common.Config.Entities;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Core.Common.Model.Orientation;
using LedCube.Core.UI.CubeView3D.Rendering;
using Xunit;

namespace LedCube.Test.Core.CubeView3D;

public class CubeOrientationTests
{
    private static readonly Point3D Size = new(16, 16, 16);

    [Fact]
    public void Default_is_identity()
    {
        var o = CubeOrientation.Default;
        Assert.True(o.IsValid);
        Assert.Equal(new Point3D(3, 5, 7), o.ToDisplay(new Point3D(3, 5, 7), Size));
        Assert.Equal(new Point3D(0, 0, 0), o.ToDisplay(new Point3D(0, 0, 0), Size));
    }

    [Fact]
    public void Repeated_axis_is_invalid()
    {
        var o = new CubeOrientation { AxisX = CubeDirection.Right, AxisY = CubeDirection.Left };
        Assert.False(o.IsValid); // X and Y both on the physical X axis
    }

    [Fact]
    public void Negative_direction_flips_the_coordinate()
    {
        var o = new CubeOrientation { AxisX = CubeDirection.Left }; // logical +X -> display -X
        Assert.True(o.IsValid);
        Assert.Equal(15, o.ToDisplay(new Point3D(0, 0, 0), Size).X);  // LED 0 lands at the far X edge
        Assert.Equal(0, o.ToDisplay(new Point3D(15, 0, 0), Size).X);
    }

    [Fact]
    public void Axes_can_be_permuted()
    {
        // logical X -> back (Z), Y -> up (Y), Z -> right (X)
        var o = new CubeOrientation { AxisX = CubeDirection.Back, AxisY = CubeDirection.Up, AxisZ = CubeDirection.Right };
        Assert.True(o.IsValid);
        var d = o.ToDisplay(new Point3D(1, 2, 3), Size);
        Assert.Equal(new Point3D(3, 2, 1), d); // X<-logicalZ, Y<-logicalY, Z<-logicalX
    }

    [Fact]
    public void Led0_corner_moves_with_orientation()
    {
        // A front/back flip puts logical LED 0 (front) at the back edge.
        var o = new CubeOrientation { AxisZ = CubeDirection.Front };
        var led0 = o.ToDisplay(new Point3D(0, 0, 0), Size);
        Assert.Equal(15, led0.Z);
    }

    [Fact]
    public void Compose_is_apply_inner_then_outer()
    {
        var inner = new CubeOrientation { AxisZ = CubeDirection.Front }; // flip Z
        var outer = new CubeOrientation { AxisX = CubeDirection.Left };  // flip X
        var c = CubeOrientation.Compose(inner, outer);
        var p = new Point3D(1, 2, 3);
        var expected = outer.ToDisplay(inner.ToDisplay(p, Size), Size);
        Assert.Equal(expected, c.ToDisplay(p, Size));
    }

    [Fact]
    public void Default_config_resolves_to_identity()
    {
        var resolved = CubeInstallationConfig.Default.Resolve();
        Assert.Equal(new Point3D(3, 5, 7), resolved.ToDisplay(new Point3D(3, 5, 7), Size));
    }

    [Fact]
    public void Left_handed_right_install_config_is_a_valid_distinct_orientation()
    {
        // The shipped default for the real cube: left-handed wiring, installed turned to the right.
        var config = new CubeInstallationConfig
        {
            HardwareFront = Orientation3D.Front,
            HardwareHandedness = CartesianOrientation.LeftHanded,
            InstallationFront = Orientation3D.Right
        };
        var resolved = config.Resolve();
        Assert.True(resolved.IsValid);
        Assert.NotEqual(CubeOrientation.Default, resolved); // actually transforms
    }

    [Fact]
    public void All_face_presets_are_valid_rotations()
    {
        foreach (var face in System.Enum.GetValues<Orientation3D>())
            Assert.True(OrientationPresets.BringFaceToFront(face).IsValid, $"{face} preset invalid");
    }

    [Fact]
    public void Installation_back_turns_the_cube_around()
    {
        // Turning the installed front to "Back" should send logical front (z=0) to the display back.
        var config = CubeInstallationConfig.Default with { InstallationFront = Orientation3D.Back };
        var d = config.Resolve().ToDisplay(new Point3D(0, 0, 0), Size);
        Assert.Equal(15, d.Z);
    }

    [Fact]
    public void Canonical_display_basis_is_right_handed_with_led0_front_bottom_left()
    {
        // The fixed preview display basis: data X->right(+X), Y->depth away (Front=-Z), Z->up(+Y).
        var canonical = new CubeOrientation
        {
            AxisX = CubeDirection.Right,
            AxisY = CubeDirection.Front,
            AxisZ = CubeDirection.Up
        };
        var pos = CubeInstanceLayout.Build(Size, canonical);

        // LED 0 at left(-X), bottom(-Y), front(+Z toward the camera).
        Assert.Equal(new Vector3(-7.5f, -7.5f, 7.5f), pos[0]);

        var dx = pos[1] - pos[0];           // data +X
        var dy = pos[16] - pos[0];          // data +Y
        var dz = pos[16 * 16] - pos[0];     // data +Z
        Assert.Equal(new Vector3(1, 0, 0), dx);   // right
        Assert.Equal(new Vector3(0, 0, -1), dy);  // depth, away from the camera
        Assert.Equal(new Vector3(0, 1, 0), dz);   // up

        // Right-handed: X x Y = Z.
        Assert.Equal(dz, Vector3.Cross(dx, dy));
    }

    [Fact]
    public void Stream_mapper_identity_matches_raw_serialize()
    {
        var cube = new CubeData<CubeDataBuffer16>();
        cube.SetLed(new Point3D(0, 0, 0), true);
        cube.SetLed(new Point3D(1, 2, 3), true);
        cube.SetLed(new Point3D(15, 15, 15), true);

        var raw = new byte[cube.Length / 8];
        cube.Serialize(raw);
        var mapped = new byte[cube.Length / 8];
        new OrientationStreamMapper(Size, CubeOrientation.Default).Serialize(cube, mapped);

        Assert.Equal(raw, mapped);
    }

    [Fact]
    public void Stream_mapper_flip_moves_the_bit()
    {
        var cube = new CubeData<CubeDataBuffer16>();
        cube.SetLed(new Point3D(0, 0, 0), true); // canonical index 0

        var mapped = new byte[cube.Length / 8];
        new OrientationStreamMapper(Size, new CubeOrientation { AxisZ = CubeDirection.Front }).Serialize(cube, mapped);

        const int di = 15 * 16 * 16; // display (0,0,15)
        Assert.True((mapped[di >> 3] & (1 << (di & 7))) != 0);
        Assert.Equal(0, mapped[0] & 1); // no longer at index 0
    }

    [Fact]
    public void Layout_places_led0_per_orientation()
    {
        // Default: LED 0 at the (-,-,-) corner.
        var def = CubeInstanceLayout.Build(Size, CubeOrientation.Default);
        Assert.Equal(new Vector3(-7.5f, -7.5f, -7.5f), def[0]);

        // Flip Z: LED 0 moves to +Z.
        var flipped = CubeInstanceLayout.Build(Size, new CubeOrientation { AxisZ = CubeDirection.Front });
        Assert.Equal(new Vector3(-7.5f, -7.5f, 7.5f), flipped[0]);
    }
}
