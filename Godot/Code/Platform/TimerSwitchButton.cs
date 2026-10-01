using Godot;
using System;

namespace Deuteros.Code.Platform
{
    public partial class TimerSwitchButton : Base.HoverButton
    {

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            this.Pressed += TimerSwitch_ButtonUp;

            base._Ready();
        }

        private void TimerSwitch_ButtonUp()
        {
            Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip = !Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip;
            Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkipStart = Time.GetTicksMsec();
        }
    }
}