using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Helpers;
using Godot;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using static Deuteros.Code.Enums;
using static System.Collections.Specialized.BitVector32;

namespace Deuteros.Code.Platform.Screens
{
	public partial class MTX : BaseSubScene
	{
		public List<Enums.ItemTypes> ResourceTypeList = new List<Enums.ItemTypes>() { Enums.ItemTypes.iron, Enums.ItemTypes.titanium, Enums.ItemTypes.aluminium, Enums.ItemTypes.carbon, ItemTypes.copper, ItemTypes.hydrogen, ItemTypes.deuterium, ItemTypes.methane, ItemTypes.helium, ItemTypes.paladium, ItemTypes.platinum, ItemTypes.silver, ItemTypes.gold, ItemTypes.silica, ItemTypes.meh_fuel, ItemTypes.hed_fuel };
		public const string StationSprite = "res://Sprites//Buttons//Overview//Station_0.png";
		public const string SelectedStationSprite = "res://Sprites//Buttons//MTX//Crate.png";

		List<MTXRow> ResourceRows { get; set; }
		List<Button> SystemButtons { get; set; }
		List<TextureButton> StationButtons { get; set; }
		Button RestoreButton { get; set; }
		Button SwitchStoreButton { get; set; }
		Button ClearButton { get; set; }
		Button TransmitAllButton { get; set; }
		Button BalanceButton { get; set; }
		Button ScrollUpButton { get; set; }
		Button ScrollDownButton { get; set; }

		Label SystemName { get; set; }

		Control TemplateResourceRow { get; set; }

		BoxContainer ResourceContainer { get; set; }
		ScrollContainer ResourceList { get; set; }

		Objects.Store CurrentStore { get; set; }
		Objects.MTX CurrentMTX { get; set; }
		Objects.MTX InitialState { get; set; }

		Enums.StellarBodies CurrentStarSystem { get; set; }

		Action ChangeStore { get; set; }

		Tween ScrollTween { get; set; }

		public override void _Ready()
		{
			StationButtons = new List<TextureButton>();
			SystemButtons = new List<Button>();
			ResourceRows = new List<MTXRow>();

			ScrollTween = new Tween();

			SystemName = GetNode<Label>("Destination/SystemName");

			RestoreButton = GetNode<Button>("Config/Buttons/Restore");
			SwitchStoreButton = GetNode<Button>("Config/Buttons/SwitchStore");
			ClearButton = GetNode<Button>("Config/Buttons/Clear");
			TransmitAllButton = GetNode<Button>("Config/Buttons/TransmitAll");
			BalanceButton = GetNode<Button>("Config/Buttons/Balance");
			ScrollUpButton = GetNode<Button>("Config/Buttons/ScrollUp");
			ScrollDownButton = GetNode<Button>("Config/Buttons/ScrollDown");

			for (int i = 0; i < 16; i++)
			{
				StationButtons.Add(GetNode<TextureButton>("Destination/StationButtons/" + i.ToString().PadLeft(2, '0')));
				StationButtons[i].SetMeta("StationId", (int)StellarBodies.none);
				var itemIndex = i;
				StationButtons[i].Pressed += () => StationButton_Pressed(itemIndex);
			}


			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/The_Sun"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.the_sun);
			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/Proxima"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.proxima);
			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/Centauri"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.centauri);
			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/Barnard"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.barnard);
			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/Lalande"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.lalande);
			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/Sirius"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.sirius);
			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/Cygni"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.cygni);
			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/Procyon"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.procyon);
			SystemButtons.Add(GetNode<Button>("Destination/SystemButtons/Tau_Ceti"));
			SystemButtons.Last().SetMeta("StarType", (int)Enums.StellarBodies.tau_ceti);

			ResourceContainer = GetNode<BoxContainer>("Config/ResourceList/ResourceContainer");
			ResourceList = GetNode<ScrollContainer>("Config/ResourceList");

			ResourceList.ScrollVertical = 0;

			//Load the template row, then delete it
			var originalResourceRow = GetNode<MTXRow>("Config/ResourceList/ResourceContainer/00");
			TemplateResourceRow = (MTXRow)originalResourceRow.Duplicate();
			originalResourceRow.GetParent().RemoveChild(originalResourceRow);
			originalResourceRow.QueueFree();

			foreach (var item in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => !T.Locked).OrderBy(T => T.ItemCategory != ItemCategory.resource))
			{
				var newItemRow = (MTXRow)TemplateResourceRow.Duplicate();
				newItemRow.Load(item.ItemType);
				newItemRow.RowName.Text = item.ShortName;
				newItemRow.Count.Text = "0";
				newItemRow.LeftDiamond.Pressed += () => ResourceArrowButton_Pressed(item.ItemType, true);
				newItemRow.RightDiamond.Pressed += () => ResourceArrowButton_Pressed(item.ItemType, false);
				newItemRow.LeftCross.Pressed += () => ResourceArrowButton_Pressed(item.ItemType, true);
				newItemRow.RightCross.Pressed += () => ResourceArrowButton_Pressed(item.ItemType, false);

				ResourceRows.Add(newItemRow);

				ResourceContainer.AddChild(newItemRow);
			}

			RestoreButton.Pressed += RestoreButton_Pressed;
			SwitchStoreButton.Pressed += SwitchStoreButton_Pressed;
			ClearButton.Pressed += ClearButton_Pressed;
			TransmitAllButton.Pressed += TransmitAllButton_Pressed;
			BalanceButton.Pressed += BalanceButton_Pressed;
			ScrollUpButton.Pressed += ScrollUpButton_Pressed;
			ScrollDownButton.Pressed += ScrollDownButton_Pressed;

			foreach (var systemButton in SystemButtons)
			{
				systemButton.Pressed += () => SystemButton_Pressed(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars[(Enums.StellarBodies)((int)systemButton.GetMeta("StarType"))].StarId);
			}

			ScrollTween.Finished += ScrollTween_Finished;

			base._Ready();
		}

		private void ResourceArrowButton_Pressed(Enums.ItemTypes itemType, bool send)
		{
			if (send)
			{
				if (CurrentMTX.SendItems.Contains(itemType))
					CurrentMTX.SendItems.Remove(itemType);
				else
					CurrentMTX.SendItems.Add(itemType);

				if (CurrentMTX.BalanceItems.Contains(itemType))
					CurrentMTX.BalanceItems.Remove(itemType);
			}
			else
			{
				if (CurrentMTX.BalanceItems.Contains(itemType))
				{
					CurrentMTX.BalanceItems.Remove(itemType);
				}
				else
				{
					CurrentMTX.BalanceItems.Add(itemType);

					if (!CurrentMTX.SendItems.Contains(itemType))
						CurrentMTX.SendItems.Add(itemType);
				}
			}

			CurrentMTX.CurrentItem = itemType;

			UpdateState();
		}

		private void SystemButton_Pressed(Enums.StellarBodies systemType)
		{
			if (CurrentStarSystem != systemType)
			{
				CurrentStarSystem = systemType;

				UpdateState();
			}
		}

		private void StationButton_Pressed(int stationButtonIndex)
		{
			var stationButton = StationButtons[stationButtonIndex];

			var stationType = (Enums.StellarBodies)(int)stationButton.GetMeta("StationId");

			if (stationType != Enums.StellarBodies.none)
			{
				var stationsInSystem = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Where(T => T.Value.ParentStar == CurrentStarSystem && T.Value.Station.Built && !T.Value.ActiveMethanoid).Select(T => T.Value).OrderBy(T => T.Station.StationOrdinal).ToList();

				var selectedStation = stationsInSystem[stationButtonIndex];

				if (selectedStation.PlanetId != GameCore.SingletonInstance.GetCurrentPlanet().PlanetId && selectedStation.Station.MtxInstalled)
				{
					if (CurrentMTX.Target != selectedStation.PlanetId)
						CurrentMTX.Target = selectedStation.PlanetId;
				}
			}

			UpdateState();
		}

		public void LoadScene(Objects.Store currentStore, Action changeStore)
		{
			ChangeStore = changeStore;

			CurrentStore = currentStore;

			CurrentMTX = CurrentStore.MTX;

			InitialState = (Deuteros.Code.Objects.MTX)CurrentMTX.Clone();

			if (CurrentMTX.Target == StellarBodies.none)
				CurrentStarSystem = StellarBodies.the_sun;
			else
				CurrentStarSystem = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[CurrentMTX.Target].ParentStar;

			UpdateState();
		}

		//Triggered from gamecore
		protected override void DayTick(uint previousDay, uint currentDay)
		{
			//Don't tick if the MTX is not even unlocked
			if (GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
				UpdateState();
		}

		private void ScrollDownButton_Pressed()
		{
			if (CurrentStore.MTX.CurrentScroll + 15 < ResourceRows.Count)
			{
				CurrentStore.MTX.CurrentScroll++;

				GameCore.LockScreen();

				ScrollTween?.Kill();
				ScrollTween = CreateTween();
				ScrollTween.Finished += ScrollTween_Finished;
				ScrollTween.SetTrans(Tween.TransitionType.Linear);
				ScrollTween.TweenProperty(ResourceList, "scroll_vertical", ResourceList.ScrollVertical + 8, 0.3);
			}
		}

		private void ScrollUpButton_Pressed()
		{
			if (CurrentStore.MTX.CurrentScroll > 0)
			{
				CurrentStore.MTX.CurrentScroll--;

				GameCore.LockScreen();

				ScrollTween?.Kill();
				ScrollTween = CreateTween();
				ScrollTween.Finished += ScrollTween_Finished;
				ScrollTween.SetTrans(Tween.TransitionType.Linear);
				ScrollTween.TweenProperty(ResourceList, "scroll_vertical", ResourceList.ScrollVertical - 8, 0.3);
			}
		}

		private void ScrollTween_Finished()
		{
			GameCore.UnLockScreen();
		}

		private void BalanceButton_Pressed()
		{
			CurrentMTX.SendItems = new List<ItemTypes>();
			CurrentMTX.BalanceItems = new List<ItemTypes>();

			foreach (var item in ResourceRows)
			{
				CurrentMTX.SendItems.Add(item.ItemType);
				CurrentMTX.BalanceItems.Add(item.ItemType);
			}

			UpdateState();
		}

		private void TransmitAllButton_Pressed()
		{
			CurrentMTX.SendItems = new List<ItemTypes>();
			CurrentMTX.BalanceItems = new List<ItemTypes>();

			foreach (var item in ResourceRows)
			{
				CurrentMTX.SendItems.Add(item.ItemType);
			}

			UpdateState();
		}

		private void ClearButton_Pressed()
		{
			CurrentMTX.SendItems = new List<ItemTypes>();
			CurrentMTX.BalanceItems = new List<ItemTypes>();

			UpdateState();
		}

		private void SwitchStoreButton_Pressed()
		{
			ChangeStore();
		}

		private void RestoreButton_Pressed()
		{
			var scrollStore = CurrentMTX.CurrentScroll;
			CurrentMTX = (Deuteros.Code.Objects.MTX)InitialState.Clone();
			CurrentMTX.CurrentScroll = scrollStore;
			CurrentStore.MTX = CurrentMTX;
			UpdateState();
		}

		public void UpdateState()
		{
			var stationsInSystem = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Where(T => T.Value.ParentStar == CurrentStarSystem && T.Value.Station.Built && !T.Value.ActiveMethanoid).Select(T => T.Value).OrderBy(T => T.Station.StationOrdinal).ToList();

			foreach (var systemButton in SystemButtons)
			{
				var stationType = (Enums.StellarBodies)((int)systemButton.GetMeta("StarType"));

				//Check if the button is currently selected
				if (stationType == CurrentStarSystem)
				{
					//If not, highlight it so you can tell it is selected
				}
			}

			var stationIndex = 0;

			foreach (var stationButton in StationButtons)
			{
				//We're out of planets
				if (stationIndex >= stationsInSystem.Count)
				{
					stationButton.SetMeta("StationId", (int)StellarBodies.none);
					stationButton.TextureNormal = null;
					continue;
				}

				//The station is not correctly set
				if ((Enums.StellarBodies)(int)stationButton.GetMeta("StationId") != stationsInSystem[stationIndex].PlanetId)
				{
					stationButton.SetMeta("StationId", (int)stationsInSystem[stationIndex].PlanetId);
				}

				if ((Enums.StellarBodies)(int)stationButton.GetMeta("StationId") == CurrentMTX.Target)
					stationButton.TextureNormal = SpriteManager.LoadImage(SelectedStationSprite);
				else
					stationButton.TextureNormal = SpriteManager.LoadImage(StationSprite);

				stationIndex++;
			}

			foreach (var item in ResourceRows)
			{
				item.LeftArrow.Visible = CurrentStore.MTX.CurrentItem == item.ItemType;

				item.LeftDiamond.Visible = CurrentStore.MTX.SendItems.Contains(item.ItemType);
				item.RightDiamond.Visible = CurrentStore.MTX.BalanceItems.Contains(item.ItemType);

				item.LeftCross.Visible = !item.LeftDiamond.Visible;
				item.RightCross.Visible = !item.RightDiamond.Visible;

				item.Count.Text = CurrentStore[item.ItemType].ToString();
			}
		}

		#region Statics

		public static void UpdateMTX(uint previousDay, uint currentDay)
		{
			if (GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
			{
				var currentItemList = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => !T.Locked).OrderBy(T => T.ItemCategory != ItemCategory.resource).ToList();

				foreach (var planet in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Where(T => T.Value.Station.Built && !T.Value.ActiveMethanoid && T.Value.Station.MtxInstalled))
				{
					var currentMTX = planet.Value.Station.Resources.Stores.MTX;
					int index = 0;

					//Check the target exists, and check we actually have a move to action
					if (currentMTX.Target != StellarBodies.none && !(currentMTX.SendItems.Count == 0 && currentMTX.BalanceItems.Count == 0))
					{
						var targetStation = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[currentMTX.Target].Station;
						var currentStation = planet.Value.Station;

						//If the currently selected item is not set to perform any actions, move to the next item to process it
						//Keep looping until we find an active object
						//The additional index change at the bototom of this foreach then moves to the next item - It's a quirk
						while (!currentMTX.BalanceItems.Contains(currentMTX.CurrentItem) && !currentMTX.SendItems.Contains(currentMTX.CurrentItem))
						{
							//Find next item in list, and wrap if necessary
							index = currentItemList.FindIndex(x => x.ItemType == currentMTX.CurrentItem);
							currentMTX.CurrentItem = ((index + 1) >= currentItemList.Count ? currentItemList[0] : currentItemList[index + 1]).ItemType;
						}

						if (currentMTX.BalanceItems.Contains(currentMTX.CurrentItem))
						{
							if (targetStation.Resources.Stores[currentMTX.CurrentItem] != currentStation.Resources.Stores[currentMTX.CurrentItem])
							{
								var targetBalanceAmount = (targetStation.Resources.Stores[currentMTX.CurrentItem] + currentStation.Resources.Stores[currentMTX.CurrentItem] / 2);
								targetStation.Resources.Stores[currentMTX.CurrentItem] = targetBalanceAmount;
								currentStation.Resources.Stores[currentMTX.CurrentItem] = targetBalanceAmount;
							}
						}
						else if(currentMTX.SendItems.Contains(currentMTX.CurrentItem))
						{
							if (targetStation.Resources.Stores[currentMTX.CurrentItem] < 50000)
							{
								var spaceAvailable = 50000 - targetStation.Resources.Stores[currentMTX.CurrentItem];
								var transferAmount = spaceAvailable > currentStation.Resources.Stores[currentMTX.CurrentItem] ? currentStation.Resources.Stores[currentMTX.CurrentItem] : spaceAvailable;
								targetStation.Resources.Stores[currentMTX.CurrentItem] += transferAmount;
								currentStation.Resources.Stores[currentMTX.CurrentItem] -= transferAmount;
							}
						}
					}

					//Find next item in list, and wrap if necessary
					index = currentItemList.FindIndex(x => x.ItemType == currentMTX.CurrentItem);
					currentMTX.CurrentItem = ((index + 1) >= currentItemList.Count ? currentItemList[0] : currentItemList[index + 1]).ItemType;
				}
			}
		}

		#endregion
	}
}