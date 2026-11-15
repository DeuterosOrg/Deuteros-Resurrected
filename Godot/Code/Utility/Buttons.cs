using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Base;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Godot.HttpRequest;

namespace Deuteros.Code.Utility
{
	public class Buttons
	{
		public static List<ButtonType> CreateButtons<ButtonType, ObjectDataType>(GridContainer buttonControlNode, Dictionary<int, ObjectDataType> objectDataList, BaseSubScene referenceScene, string clickedEventName, string codePath, string buttonPrefabName) where ButtonType : ButtonAdapter<ObjectDataType>
		{
			// Add +1 to Y on all rows except these:
			HashSet<int> noExtraPixelRows = new() { 0, 5, 8, 12, 14 }; // example 5

			var packedButton = GD.Load<PackedScene>("res://PreFabs/Buttons/" + buttonPrefabName + ".tscn");

			var script = GD.Load<Script>("res://" + codePath);

			var researchButtonControl = buttonControlNode;
			var xHeight = 0;

			var createdButtons = new List<ButtonType>();

			foreach (Node child in researchButtonControl.GetChildren())
			{
				researchButtonControl.RemoveChild(child);
				child.QueueFree();
			}

			for (var i = 1; i <= 16; i++)
			{
				var createdButton = packedButton.Instantiate();

				createdButton.SetScript(script);

				if (createdButton is ButtonType typedButton)
				{
					if (i <= objectDataList.Count())
					{
						typedButton.ObjectData = objectDataList.ElementAt(i-1).Value;
						typedButton.Connect("Clicked", new Callable(referenceScene, clickedEventName));
					}

					createdButtons.Add(typedButton);
					researchButtonControl.AddChild(typedButton);
				}

				var createdButton2 = packedButton.Instantiate();

				createdButton2.SetScript(script);

				if (createdButton2 is ButtonType typedButton2)
				{
					if ((i + 16) <= objectDataList.Count())
					{
						typedButton2.ObjectData = objectDataList.ElementAt(i+15).Value;
						typedButton2.Connect("Clicked", new Callable(referenceScene, clickedEventName));
					}

					createdButtons.Add(typedButton2);
					researchButtonControl.AddChild(typedButton2);
				}
			}

			return createdButtons;
		}

		public static void ClearPressedConnections(BaseButton button)
		{
			var connectionList = button.GetSignalConnectionList(BaseButton.SignalName.Pressed);

			foreach (var obj in connectionList)
			{
				var objectDictionary = (Godot.Collections.Dictionary)obj;

				if (!objectDictionary.ContainsKey("callable"))
					continue;

				var callable = (Callable)objectDictionary["callable"];

				if (button.IsConnected(BaseButton.SignalName.Pressed, callable))
					button.Disconnect(BaseButton.SignalName.Pressed, callable);
			}
		}
	}
}
