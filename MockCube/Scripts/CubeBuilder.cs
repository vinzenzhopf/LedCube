using System.Collections.Generic;
using Godot;

namespace MockCube.Scripts;

public partial class CubeBuilder : Node
{
	[Export]
	public int Size = 16;

	[Export]
	public float Spacing = 1.0f;

	[Export]
	public PackedScene? Template { get; set; }
	
	[Export]
	public Node3D? CameraStack { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Template is null)
		{
			return;
		}

		var center = (Size - 1) / 2f * Spacing;
		var origin = new Vector3(center, 0, center);

		for (var z = 0; z < Size; z++)
		{
			for (var y = 0; y < Size; y++)
			{
				for (var x = 0; x < Size; x++)
				{
					if (Template.Instantiate(PackedScene.GenEditState.Instance) is Node3D led)
					{
						led.Position = new Vector3(x * Spacing, y * Spacing, z * Spacing) - origin;
						Leds.Add(led);
						AddChild(led);
					}
				}
			}
		}

		GD.Print($"Move CameraStack down by {center}");
		CameraStack?.Translate(new Vector3(0, center, 0));
	}

	public List<Node3D> Leds { get; }= new();

	public override void _ExitTree()
	{
		var children = GetChildren();
		foreach (var child in children)
		{
			RemoveChild(child);
			child.Free();
		}
	}
}
