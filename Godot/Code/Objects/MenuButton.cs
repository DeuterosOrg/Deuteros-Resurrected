using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
	public class MenuButton
	{
		public Enums.Menu_Buttons ButtonType { get; set; }
		public Func<Enums.Menu_Buttons> DisabledButtonType { get; set; } = () => Enums.Menu_Buttons.Empty;
		public Enums.Scenes SceneToLoad { get; set; }
		public bool LoadScene { get; set; }
		public Godot.Collections.Array<Enums.SceneVariables> SceneVariables { get; set; }
		public Func<bool> Enabled { get; set; }
		public List<Action> ClickActions { get; set; }

		public string HoverText { get; set; }

		public MenuButton(Enums.Menu_Buttons buttonType, Enums.Scenes sceneToLoad, bool loadScene, Godot.Collections.Array<Enums.SceneVariables> sceneVariables, List<Action> clickActions, Func<bool> enabled = null, string menuButtonText = "")
		{
			ButtonType = buttonType;
			HoverText = menuButtonText;
			SceneToLoad = sceneToLoad;
			LoadScene = loadScene;
			SceneVariables = sceneVariables;
			ClickActions = clickActions;
			Enabled = enabled == null ? () => { return true; } : enabled;
		}
	}
}
