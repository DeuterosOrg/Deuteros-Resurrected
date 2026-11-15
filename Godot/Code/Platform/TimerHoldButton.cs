using Godot;
using System;

namespace Deuteros.Code.Platform
{
	public partial class TimerHoldButton : Base.HoverButton
	{
		private bool ButtonDownFlag = false;
		public uint DaysAtStart { get; set; }

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			this.Connect("button_up", new Callable(this, nameof(TimerHold_ButtonUp)));
			this.Connect("button_down", new Callable(this, nameof(TimerHold_ButtonDown)));
			this.Connect("mouse_exited", new Callable(this, nameof(TimerHold_Exit)));

			base._Ready();
		}

		private void TimerHold_Exit()
		{
			if (ButtonDownFlag)
			{
				TimerHold_ButtonUp();
			}
		}

		private void TimerHold_ButtonUp()
		{
			ButtonDownFlag = false;
			Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip = false;
			Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkipStart = Time.GetTicksMsec();

			if (Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay - DaysAtStart == 0 && Time.GetTicksMsec() - Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkipStart < 999)
			{
				Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkipDay = true;
			}
		}

		private void TimerHold_ButtonDown()
		{
			ButtonDownFlag = true;
			Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip = true;
			Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkipStart = Time.GetTicksMsec();
			DaysAtStart = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
		}
	}
}