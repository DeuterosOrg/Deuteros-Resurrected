using Deuteros.Code.Objects.Battle;
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
