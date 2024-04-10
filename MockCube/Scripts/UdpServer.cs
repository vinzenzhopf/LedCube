using System.Net;
using System.Net.Sockets;
using Godot;

namespace MockCube.Scripts;

public partial class UdpServer : Node
{

	private readonly UdpClient _udpClient = new();

	[Export]
	public CubeBuilder? CubeBuilder { get; set; }

	[Export]
	private Material? LedOnMaterial { get; set; }
	
	[Export]
	private Material? LedOffMaterial { get; set; }
	
	[Export]
	public int Port { get; set; } = 4242;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		_udpClient.Connect(new IPEndPoint(IPAddress.Loopback, Port));
		GD.Print($"Connected to {Port}");
	}

		
	private double _lifeTime = 0;


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (CubeBuilder is null)
		{
			return;
		}
		
		_lifeTime += delta;

		var oddSecond = Mathf.PosMod(_lifeTime, 2) > 1;
		foreach (var led in CubeBuilder.Leds)
		{
			if (oddSecond)
			{
				led.GetChild<MeshInstance3D>(0).MaterialOverride = LedOffMaterial;
			}
			else
			{
				led.GetChild<MeshInstance3D>(0).MaterialOverride = LedOnMaterial;
			}
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_udpClient.Close();
		GD.Print($"Disconnected");
	}
}
