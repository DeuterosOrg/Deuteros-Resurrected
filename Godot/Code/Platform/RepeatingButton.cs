using Godot;
using System;

namespace Deuteros.Code.Platform
{
	public partial class RepeatingButton : Button
	{
		private Timer _repeatTimer;
		private bool _isHeld = false;
		public bool IsRepeating {
			get {
				return _isHeld && !_repeatTimer.OneShot;
			}
		}

		[Export] public double RepeatDelay { get; set; } = 0.5;
		[Export] public double RepeatRate { get; set; } = 0.1;

			   public override void _Ready()
		{
			_repeatTimer = new Timer
			{
				Name = "RepeatTimer",
				OneShot = true,
				Autostart = false,
				ProcessCallback = Timer.TimerProcessCallback.Idle
			};

			AddChild(_repeatTimer);
			_repeatTimer.Timeout += OnRepeatTimeout;

			ButtonDown += OnButtonDown;
			ButtonUp += OnButtonUp;
			MouseExited += OnMouseExited;
		}

		private void OnButtonDown()
		{
			_isHeld = true;
			_repeatTimer.OneShot = true;
			_repeatTimer.WaitTime = RepeatDelay;
			_repeatTimer.Start();
		}

		private void OnButtonUp()
		{
			StopRepeating();
		}

		private void OnMouseExited()
		{
			StopRepeating();
		}

		private void OnRepeatTimeout()
		{
			if (!_isHeld)
				return;

			EmitSignal(SignalName.Pressed);

			// Switch to continuous repeat mode
			_repeatTimer.OneShot = false;
			_repeatTimer.WaitTime = RepeatRate;
			_repeatTimer.Start(); // Restarts with new rate
		}

		private void StopRepeating()
		{
			_isHeld = false;
			_repeatTimer.Stop();
		}
	}
}
