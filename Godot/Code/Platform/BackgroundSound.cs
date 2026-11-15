using Godot;
using System;

namespace Deuteros.Code.Platform
{
	public partial class BackgroundSound : Node2D
	{
		[Export]
		public Deuteros.Code.Enums.BackgroundSound SoundToPlay { get; set; }

		private AudioStreamPlayer AudioPlayer { get; set; }

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			AudioPlayer = this.GetNode<AudioStreamPlayer>("SoundPlayer");
			AudioPlayer.Stream = (AudioStream)ResourceLoader.Load("res://Sounds/Background/" + SoundToPlay.ToString() + ".ogg");
			AudioPlayer.VolumeDb = 0.1f;
			AudioPlayer.Play();
		}

		public override void _ExitTree()
		{
			AudioPlayer.Stop();
		}
	}
}