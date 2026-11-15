using Godot;

namespace Deuteros.Code.Platform.Base
{
	public partial class HoverButton : Button
	{
		[Export]
		public string HoverText { get; set; }

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			this.MouseEntered += MouseHover_Enter;
			this.MouseExited += MouseHover_Exit;
		}

		private void MouseHover_Enter()
		{
			if (HoverText != "")
				Deuteros.Code.GameCore.HoverText = HoverText;
		}

		private void MouseHover_Exit()
		{
			if (HoverText != "")
				Deuteros.Code.GameCore.HoverText = "";
		}
	}
}

