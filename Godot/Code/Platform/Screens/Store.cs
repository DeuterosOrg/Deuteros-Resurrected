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

			// Bind even before discovery: inspecting captured hardware can unlock it while
			// this screen is open. The installed local module controls access, not the unlock.
			MTX.LoadScene(CurrentStore, SwitchStoreType_ButtonUp);
			UpdateStoreView();
			RefreshButtons();
			DrawData();

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

			UpdateStoreView();
			RefreshButtons();
			DrawData();
		}

		private bool CanUseMtx()
		{
			var planet = GameCore.SingletonInstance.GetCurrentPlanet();
			return !SceneVariables.Contains(Enums.SceneVariables.Ground) && planet.Station.Built
				&& !planet.ActiveMethanoid && planet.Station.MtxInstalled;
		}

		private void UpdateStoreView()
		{
			MTX.Visible = CurrentStore.AlternativeView && CanUseMtx();
			TradStore.Visible = !MTX.Visible;
			if (MTX.Visible) MTX.UpdateState();
		}

		protected override void ResearchFinished(Objects.ResearchItem researchItem)
		{
			RefreshButtons();
			DrawData();
		}

		public void RefreshButtons()
		{
			Buttons = Utility.Buttons.CreateButtons<StoreButton, Item>(StoreButtonsNode,
				GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Production && T.BuildRequirements != null && T.Research != null && !CurrentStore.AlternativeView && T.Research.Researched && !T.AutoProduce).Select(T => T).OrderBy(T => T.Research.ResearchOrder).ToDictionary(obj => obj.Research.ResearchOrder),
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
			if (CurrentStore.AlternativeView && CanUseMtx() && !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
			{
				GameCore.SingletonInstance.TriggerAlienTechDiscovery(Enums.ItemTypes.m__t__x);
				return; // The discovery bulletin replaces this screen.
			}

			UpdateStoreView();
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

				BuildAmountLabel.Text = recipeItem.BuildRequirements.Count == 0
					? "No materials required" : string.Format(recipeText, maxCount, recipeItem.FullName);
			}
		}
	}
}
