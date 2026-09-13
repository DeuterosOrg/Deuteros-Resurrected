using Deuteros.Code;
using Deuteros.Code.Platform.Helpers;
using Godot;
using System;

public partial class Error : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var errorButton = this.GetNode<Button>("ErrorButton");

		errorButton.Pressed += Error_Pressed;
	}

	private void Error_Pressed()
	{
		OverlayManager.Instance.CloseOverlay();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
		
}
