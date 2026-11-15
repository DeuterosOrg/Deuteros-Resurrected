using Deuteros.Code.Platform.Screens;
using Godot;

namespace Deuteros.Code.Objects
{
	public partial class BattleCanvas : Control
	{
		public BattleLogic BattleLogic;

		public override void _Draw()
		{
			BattleLogic.BattleTick();
		}
	}
}
