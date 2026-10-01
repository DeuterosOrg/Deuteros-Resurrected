using Deuteros.Code.Platform.Base;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Objects.Interfaces;
using System.Resources;
using System.Text;

namespace Deuteros.Code.Platform.Screens
{
	public partial class Store : BaseSubScene
	{
		public List<StoreButton> Buttons { get; set; }
		public StoreButton SelectedButton { get; set; }
		public Button SwitchStoreType { get; set; }
		public RichTextLabel ResourceListLabel { get; set; }
		public Label BuildAmountLabel { get; set; }
		public GridContainer StoreButtonsNode { get; set; }
		public Objects.Store CurrentStore { get; set; }

		public Control TradStore { get; set; }
		public MTX MTX { get; set; }

		public override void _Ready()
		{
			var currentPlanet = Deuteros.Code.GameCore.SingletonInstance.GetCurrentPlanet();
			CurrentStore = SceneVariables.Contains(Enums.SceneVariables.Ground) ? currentPlanet.PlanetResources.Stores : currentPlanet.Station.Resources.Stores;

			SwitchStoreType = (Button)GetNode("TradStore/SwitchStoreImage/SwitchStoreType");
			SwitchStoreType.Connect("button_up", new Callable(this, nameof(SwitchStoreType_ButtonUp)));

			ResourceListLabel = (RichTextLabel)GetNode("TradStore/ResourceList");
			BuildAmountLabel = (Label)GetNode("TradStore/Recipe");

			SelectedButton = null;

			StoreButtonsNode = GetNode<GridContainer>("TradStore/StoreButtons");

			TradStore = GetNode<Control>("TradStore");
			MTX = GetNode<MTX>("MTX");

			//TODO - This is messy, should have planned ahead for the MTX
			//If the MTX is available and active, load it up and display it over the current screen
			if (GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
			{
				MTX.Visible = CurrentStore.AlternativeView;
				TradStore.Visible = !CurrentStore.AlternativeView;

				MTX.LoadScene(CurrentStore, SwitchStoreType_ButtonUp);

				if (CurrentStore.AlternativeView)
				{
					MTX.UpdateState();
				}
				else
				{
					RefreshButtons();
					DrawData();
				}
			}
			else
			{
				MTX.Visible = false;
				TradStore.Visible = true;

				RefreshButtons();
				DrawData();
			}

			base._Ready();
		}

		private void StoreButton_Clicked(int index)
		{
			var clickedButton = Buttons.Single(T => T.ObjectData != null && T.ObjectData.Research.ResearchOrder == index);

			if (!clickedButton.ObjectData.Locked)
			{
				CurrentStore.SelectedRecipe = clickedButton.ObjectData.ItemType;

				RefreshButtons();
				DrawData();
			}
		}

		private void SwitchStoreType_ButtonUp()
		{
			CurrentStore.AlternativeView = !CurrentStore.AlternativeView;

			if (GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
			{
				MTX.Visible = CurrentStore.AlternativeView;
				TradStore.Visible = !CurrentStore.AlternativeView;
				if (CurrentStore.AlternativeView)
					MTX.UpdateState();
			}
			RefreshButtons();
			DrawData();
		}

		protected override void ResearchFinished(Objects.ResearchItem researchItem)
		{
			RefreshButtons();
			DrawData();
		}

		public void RefreshButtons()
		{
			Buttons = Utility.Buttons.CreateButtons<StoreButton, Item>(StoreButtonsNode,
				GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && !CurrentStore.AlternativeView && T.Research.Researched && !T.AutoProduce).Select(T => T).OrderBy(T => T.Research.ResearchOrder).ToDictionary(obj => obj.Research.ResearchOrder),
				this,
				nameof(StoreButton_Clicked),
				"/Code/Platform/StoreButton.cs",
				"StoreButton");
			SelectedButton = Buttons.FirstOrDefault(button => button.ObjectData?.ItemType == CurrentStore.SelectedRecipe);
			if (SelectedButton != null)
				SelectedButton.Selected = true;
		}

		// Called every update.
		public override void _Draw()
		{
		}

		protected override void DayTick(uint previousDay, uint currentDay)
		{
			var currentPlanet = Deuteros.Code.GameCore.SingletonInstance.GetCurrentPlanet();
			
			if (CurrentStore.AlternativeView && currentPlanet.Station.MtxInstalled && !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
			{
				GameCore.SingletonInstance.TriggerAlienTechDiscovery(Enums.ItemTypes.m__t__x);
			}

			DrawData();
		}

		public void DrawData()
		{
			int padlength = 1;

			ResourceListLabel.Text = "";
			BuildAmountLabel.Text = "";

			foreach (var item in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => !T.Locked && (T.ItemCategory == Enums.ItemCategory.item) == CurrentStore.AlternativeView))
			{
				padlength = Math.Max(padlength, CurrentStore[item.ItemType].ToString().Length);
			}

			foreach (var item in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => !T.Locked && (T.ItemCategory == Enums.ItemCategory.item) == CurrentStore.AlternativeView))
			{
				if (SelectedButton != null)
				{
					var recipeItem = SelectedButton.ObjectData as Item;
					if (recipeItem.BuildRequirements.Count(T => T.ItemType == item.ItemType)>0 )
					{
						var material = recipeItem.BuildRequirements.First(T => T.ItemType == item.ItemType);
						if (material.ItemCount <= CurrentStore[material.ItemType])
						{
							ResourceListLabel.Text += (CurrentStore.AlternativeView ? "[color=#ffff00]" : "[color=#008800]") + CurrentStore[item.ItemType].ToString().PadLeft(padlength) + " [color=#ffffff]" + item.ShortName + "\n";
						}
						else
						{
							ResourceListLabel.Text += (CurrentStore.AlternativeView ? "[color=#ffff00]" : "[color=#ff0000]") + CurrentStore[item.ItemType].ToString().PadLeft(padlength) + " [color=#ffffff]" + item.ShortName + "\n";
						}
					}
					else
					{
						ResourceListLabel.Text += (CurrentStore.AlternativeView ? "[color=#ffff00]" : "[color=#556633]") + CurrentStore[item.ItemType].ToString().PadLeft(padlength) + " [color=#ffffff]" + item.ShortName + "\n";
					}
				}
				else
				{
					ResourceListLabel.Text += (CurrentStore.AlternativeView ? "[color=#ffff00]" : "[color=#556633]") + CurrentStore[item.ItemType].ToString().PadLeft(padlength) + " [color=#ffffff]" + item.ShortName + "\n";
				}
			}


			if (SelectedButton != null)
			{
				var recipeText = "Enough supplies for {0} {1}s";
				var recipeItem = SelectedButton.ObjectData as Item;
				var maxCount = recipeItem.BuildRequirements
					.Select(material => CurrentStore[material.ItemType] / material.ItemCount).DefaultIfEmpty(0).Min();

				BuildAmountLabel.Text = string.Format(recipeText, maxCount, recipeItem.FullName);
			}
		}
	}
}
