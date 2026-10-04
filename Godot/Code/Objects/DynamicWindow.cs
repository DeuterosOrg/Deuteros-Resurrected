using Deuteros.Code.Platform.Base;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
	public partial class DynamicWindow : Control
	{
		[Export]
		public int ContentWidth { get; set; }

		[Export]
		public int ContentHeight { get; set; }

		[Export]
		public bool CloseTimer { get; set; }
		[Export]
		public float CloseTimerLength { get; set; }

		public object DataObject { get; set; }

		public Action<object> Closed { get; set; }

		public override void _Ready()
		{
			if (ContentWidth != 0 && ContentHeight != 0)
			{
				var contentWindow = GetNode<Control>("Window");
				contentWindow.Size = new Vector2(ContentWidth, ContentHeight + 8);
			}

			if (CloseTimer)
				GetTree().CreateTimer(CloseTimerLength, false).Timeout += () => { Closed?.Invoke(DataObject); this.Visible = false; };
		}

		public void StartCloseTimer(float timerLength)
		{
			GetTree().CreateTimer(timerLength, false).Timeout += () => { Closed?.Invoke(DataObject); this.Visible = false; };
		}
	}
}