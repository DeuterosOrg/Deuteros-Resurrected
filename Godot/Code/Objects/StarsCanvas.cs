using Deuteros.Code.Platform.Screens;
using Godot;
using System;

namespace Deuteros.Code.Objects
{
	public partial class StarsCanvas : Control
	{
		public override void _Draw()
		{
			for (var i = 0; i < 48; i++)
			{
				Color c;

				if (Random.Shared.Next(4) == 3)
				{
					c = Color.Color8(84, 102, 51);
				}
				else
				{
					c = Color.Color8(153, 170, 119);
				}

				var x = Random.Shared.Next((int)this.Size.X);
				var y = Random.Shared.Next(64) + 5;

				DrawLine(new Vector2(x, y), new Vector2(x + 1, y), c);
			}
		}
	}
}
