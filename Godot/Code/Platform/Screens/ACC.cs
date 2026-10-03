using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Helpers;
using Godot;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Platform.Screens
{
	public partial class ACC : BaseSubScene
	{
		public List<Enums.ItemTypes> ResourceTypeList = new List<Enums.ItemTypes>() { Enums.ItemTypes.iron, Enums.ItemTypes.titanium, Enums.ItemTypes.aluminium, Enums.ItemTypes.carbon, ItemTypes.copper, ItemTypes.hydrogen, ItemTypes.deuterium, ItemTypes.methane, ItemTypes.helium, ItemTypes.paladium, ItemTypes.platinum, ItemTypes.silver, ItemTypes.gold, ItemTypes.silica, ItemTypes.meh_fuel, ItemTypes.hed_fuel };
		public const string NavSpriteBasePath = "res://Sprites//SceneSprites//Ships//Interior//ACC//";

		public Code.Objects.ACC CurrentACC { get; set; }

		List<TextureButton> SourceButtons { get; set; }
		List<TextureButton> DestinationButtons { get; set; }
		List<TextureButton> SourceCycleButtons { get; set; }
		List<TextureButton> DestinationCycleButtons { get; set; }
		TextureButton EngageButton { get; set; }
		TextureButton DisengageButton { get; set; }
		TextureButton CycleButton { get; set; }
		TextureButton ClearButton { get; set; }

		TextureRect HEDBlank { get; set; }

		Label Warning { get; set; }
		Label SourceName { get; set; }
		Label DestinationName { get; set; }

		public Action CloseWindow;

		public override void _Ready()
		{
			GetNode<Node2D>("Window").GetNode<Label>("Background/Number").Text = "1";

			Warning = GetNode<Label>("Window/Text/Warning");
			SourceName = GetNode<Label>("Window/Text/SourceName");
			DestinationName = GetNode<Label>("Window/Text/DestinationName");

			EngageButton = GetNode<TextureButton>("Window/Buttons/Engage");
			DisengageButton = GetNode<TextureButton>("Window/Buttons/Disengage");
			CycleButton = GetNode<TextureButton>("Window/Buttons/Cycle");
			ClearButton = GetNode<TextureButton>("Window/Buttons/Clear");

			HEDBlank = GetNode<TextureRect>("Window/HEDBlank");

			SourceButtons = new List<TextureButton>();
			DestinationButtons = new List<TextureButton>();

			SourceCycleButtons = new List<TextureButton>();
			DestinationCycleButtons = new List<TextureButton>();

			var itemTypeCount = 0;

			for (int i = 1; i < 3; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					SourceButtons.Add(GetNode<TextureButton>("Window/SourceButtons/Col0" + i + "/0" + j));
					SourceCycleButtons.Add(GetNode<TextureButton>("Window/CycleIcon/Source/Col0" + i + "/0" + j));
					DestinationButtons.Add(GetNode<TextureButton>("Window/DestinationButtons/Col0" + i + "/0" + j));
					DestinationCycleButtons.Add(GetNode<TextureButton>("Window/CycleIcon/Destination/Col0" + i + "/0" + j));

					var itemType = ResourceTypeList[itemTypeCount];

					SourceButtons.Last().Pressed += () => ChangeItem(true, itemType);
					DestinationButtons.Last().Pressed += () => ChangeItem(false, itemType);

					itemTypeCount++;
				}
			}

			EngageButton.Pressed += EngageButton_Pressed;
			DisengageButton.Pressed += DisengageButton_Pressed;
			CycleButton.Pressed += CycleButton_Pressed;
			ClearButton.Pressed += ClearButton_Pressed;

			base._Ready();
		}

		private void ClearButton_Pressed()
		{
            if (!Objects.RogueCrew.CanCommand(CurrentACC?.Ship)) return;
			CurrentACC.CycleMode = false;
			CurrentACC.Active = false;
			CurrentACC.CycleMode = false;

			CurrentACC.DestinationItems = new List<ItemTypes>();
			CurrentACC.SourceItems = new List<ItemTypes>();
			CurrentACC.CurrentSource = ItemTypes.iron;
			CurrentACC.CurrentDestination = ItemTypes.iron;
		}

		private void CycleButton_Pressed()
		{
            if (!Objects.RogueCrew.CanCommand(CurrentACC?.Ship)) return;
			CurrentACC.Activate(completeCycle: true);

			DisplayShipInterior();
		}

		private void DisengageButton_Pressed()
		{
            if (!Objects.RogueCrew.CanCommand(CurrentACC?.Ship)) return;
			CurrentACC.Active = false;
			CurrentACC.CycleMode = false;

			DisplayShipInterior();
		}

		private void EngageButton_Pressed()
		{
            if (!Objects.RogueCrew.CanCommand(CurrentACC?.Ship)) return;
			CurrentACC.Activate();

			DisplayShipInterior();
		}

		private void ChangeItem(bool source, ItemTypes itemType)
		{
            if (!Objects.RogueCrew.CanCommand(CurrentACC?.Ship)) return;
			if (source)
			{
				if (CurrentACC.SourceItems.Contains(itemType))
				{
					CurrentACC.SourceItems.Remove(itemType);
				}
				else
				{
					CurrentACC.SourceItems.Add(itemType);

					CurrentACC.CurrentSource = itemType;
				}
			}
			else
			{
				if (CurrentACC.DestinationItems.Contains(itemType))
				{
					CurrentACC.DestinationItems.Remove(itemType);
				}
				else
				{ 
					CurrentACC.DestinationItems.Add(itemType);
					CurrentACC.CurrentDestination = itemType;
				}
			}

			UpdateState();
		}

		private void DisplayShipInterior()
		{
			CloseWindow?.Invoke();
		}

		public void SetACC(Objects.ACC acc)
		{
			CurrentACC = acc;
		}

		public void UpdateState()
		{
            var disabled = !Objects.RogueCrew.CanCommand(CurrentACC?.Ship);
            EngageButton.Disabled = DisengageButton.Disabled = CycleButton.Disabled = ClearButton.Disabled = disabled;
            SourceButtons.ForEach(button => button.Disabled = disabled);
            DestinationButtons.ForEach(button => button.Disabled = disabled);
			HEDBlank.Visible = !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Game_Unlocks.Interstellar_Travel);

			SourceCycleButtons.ForEach(T => T.Modulate = new Color(T.Modulate, 0f));
			DestinationCycleButtons.ForEach(T => T.Modulate = new Color(T.Modulate, 0f));

			if (CurrentACC.Ship.ShipType == Ship_Types.Shuttle)
			{
				SourceName.Text = "Surface";
				DestinationName.Text = "Orbit";
			}
			else
			{
				SourceName.Text = CurrentACC.Source.ToScreenString(" ");
				DestinationName.Text = CurrentACC.Destination.ToScreenString(" ");
			}

			Warning.Visible = !CurrentACC.Ship.Modules.Any(T => T.ModuleType == Module_Types.Supply);

			for (int i = 0; i < 16; i++)
			{
				if (i == 15 && !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Game_Unlocks.Interstellar_Travel))
					continue;

				if (CurrentACC.SourceItems.Contains(ResourceTypeList[i]))
					SourceButtons[i].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "GreenDiamond.png");
				else 
					SourceButtons[i].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Cross.png");

				if (CurrentACC.DestinationItems.Contains(ResourceTypeList[i]))
					DestinationButtons[i].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "BrownDiamond.png");
				else
					DestinationButtons[i].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Cross.png");

				if (CurrentACC.CurrentSource == ResourceTypeList[i])
					SourceCycleButtons[i].Modulate = new Color(SourceCycleButtons[i].Modulate, 1f);

			if (CurrentACC.CurrentDestination == ResourceTypeList[i])
					DestinationCycleButtons[i].Modulate = new Color(DestinationCycleButtons[i].Modulate, 1f);
		}
		}
	}
}
