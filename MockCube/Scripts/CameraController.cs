using Godot;

namespace MockCube.Scripts;

public partial class CameraController : Node
{
	
	[Export]
	public Node3D? RotateY { get; set; }
	
	[Export]
	public Node3D? RotateX { get; set; }
	
	[Export]
	public Node3D? Distance { get; set; }
	

	private bool _isMouseDown;
	
	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (@event is InputEventMouseMotion mouseMotion)
		{
			if (Input.IsActionPressed("click_to_pan"))
			{
				RotateX?.RotateX(-mouseMotion.Relative.Y / 1000f);
				RotateY?.RotateY(-mouseMotion.Relative.X / 1000f);
			}
			else if (Input.IsActionPressed("click_to_zoom"))
			{
				Distance?.Translate(Vector3.Forward * -mouseMotion.Relative.Y / 100f);
			}
		}

		if (Input.IsActionJustPressed("zoom_in"))
		{
			Distance?.Translate(Vector3.Forward);
		} else if (Input.IsActionJustPressed("zoom_out"))
		{
			Distance?.Translate(Vector3.Back);
		}
	}

}
