using Deuteros.Code.Platform.Base;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Deuteros.Code.Platform.Helpers;
using System.Security.Cryptography.X509Certificates;
using Deuteros.Code.Objects.Interfaces;
using Newtonsoft.Json.Linq;
using System.Reflection;
using static Deuteros.Code.Enums;
using Deuteros.Code.Utility;
using System.ComponentModel.Design;
using static System.Collections.Specialized.BitVector32;

namespace Deuteros.Code.Platform.Screens
{
	public partial class ShipBay : BaseSubScene
	{
		public const string NavSpriteBasePath = "res://Sprites//Buttons//Shipbay//";
		public bool Ground { get; set; }
		public bool Earth { get; set; }
		public bool Shuttle { get; set; }
		public bool ShipPresent { get; set; }
		public IShip Ship { get; set; }
		public IPlanet CurrentPlanet { get; set; }
		TextureButton Nav_Cockpit { get; set; }
		List<TextureButton> Nav_Torsos { get; set; }
		TextureButton Nav_Engine { get; set; }
		TextureButton Nav_Dismantle { get; set; }
		TextureButton Nav_Create_Shuttle { get; set; }
		TextureButton Nav_Create_IOS { get; set; }
		TextureButton Nav_Create_SCG { get; set; }

		RepeatingButton FuelGaugeMinus { get; set; }
		RepeatingButton FuelGaugePlus { get; set; }

		Control CargoService { get; set; }
		Control EquipmentStock { get; set; }
		Control StaffList { get; set; }
		Control GrappleWindowControl { get; set; }
        Label GrappleWindowTitle { get; set; }
        Label GrappleWindowQuantities { get; set; }
        Label GrappleWindowComplete { get; set; }

        Label[] EquipmentStockNameLabels { get; set; } = new Label[11];
		Label[] EquipmentStockCountLabels { get; set; } = new Label[11];
		Label FuelType { get; set; }
		Label FuelInStock { get; set; }
		Label FuelInShip { get; set; }

		Button[] EquipmentStockButtons { get; set; } = new Button[11];

		StaffList TorsoStaffList { get; set; }
		DynamicWindow GrappleWindow { get; set; }

		ShipBayScenes.Cockpit CockpitInstance { get; set; }
		List<ShipBayScenes.Torso> TorsoInstances { get; set; }
		ShipBayScenes.Engine EngineInstance { get; set; }

		public ScrollContainer ScrollContainer;
		public int ScreenWidth = 224;

		public Resource ResourceList { get; set; }

		public int ScreenState { get; set; }

		private Control hoveredControl;
		private Func<string> hoveredLabel;
		private string lastHoverText;

		public override void _Ready()
		{
			ScrollContainer = GetNode<ScrollContainer>("ShipContainer/ScrollContainer2");

			//Setup some flags to make our lives easier
			CurrentPlanet = GameCore.SingletonInstance.GetCurrentPlanet();
			Earth = CurrentPlanet.PlanetId == Enums.StellarBodies.earth;
			Ground = SceneVariables.Contains(Enums.SceneVariables.Ground);
			Shuttle = SceneVariables.Contains(Enums.SceneVariables.Shuttle);

			ResourceList = (Resource)(Ground ? CurrentPlanet.PlanetResources : CurrentPlanet.Station.Resources);

			CockpitInstance = GetNode<ShipBayScenes.Cockpit>("ShipContainer/ScrollContainer2/HBoxContainer/Cockpit");

			TorsoStaffList = GetNode<StaffList>("StaffList");

			TorsoInstances = new List<ShipBayScenes.Torso>();
			TorsoInstances.Add(GetNode<ShipBayScenes.Torso>("ShipContainer/ScrollContainer2/HBoxContainer/Torso1"));
			TorsoInstances.Add(GetNode<ShipBayScenes.Torso>("ShipContainer/ScrollContainer2/HBoxContainer/Torso2"));
			TorsoInstances.Add(GetNode<ShipBayScenes.Torso>("ShipContainer/ScrollContainer2/HBoxContainer/Torso3"));
			TorsoInstances.Add(GetNode<ShipBayScenes.Torso>("ShipContainer/ScrollContainer2/HBoxContainer/Torso4"));
			TorsoInstances.Add(GetNode<ShipBayScenes.Torso>("ShipContainer/ScrollContainer2/HBoxContainer/Torso5"));
			TorsoInstances.Add(GetNode<ShipBayScenes.Torso>("ShipContainer/ScrollContainer2/HBoxContainer/Torso6"));

			EngineInstance = GetNode<ShipBayScenes.Engine>("ShipContainer/ScrollContainer2/HBoxContainer/Engine");

			Nav_Cockpit = GetNode<TextureButton>("Buttons/ShipNav/Nav_Cockpit");
			Nav_Torsos = new List<TextureButton>();
			Nav_Torsos.Add(GetNode<TextureButton>("Buttons/ShipNav/Nav_Torso1"));
			Nav_Torsos.Add(GetNode<TextureButton>("Buttons/ShipNav/Nav_Torso2"));
			Nav_Torsos.Add(GetNode<TextureButton>("Buttons/ShipNav/Nav_Torso3"));
			Nav_Torsos.Add(GetNode<TextureButton>("Buttons/ShipNav/Nav_Torso4"));
			Nav_Torsos.Add(GetNode<TextureButton>("Buttons/ShipNav/Nav_Torso5"));
			Nav_Torsos.Add(GetNode<TextureButton>("Buttons/ShipNav/Nav_Torso6"));
			Nav_Engine = GetNode<TextureButton>("Buttons/ShipNav/Nav_Engine");
			Nav_Dismantle = GetNode<TextureButton>("Buttons/Nav_Dismantle");
			Nav_Create_Shuttle = GetNode<TextureButton>("Buttons/Nav_Create_Shuttle");
			Nav_Create_IOS = GetNode<TextureButton>("Buttons/Nav_Create_IOS");
			Nav_Create_SCG = GetNode<TextureButton>("Buttons/Nav_Create_SCG");

			FuelGaugeMinus = GetNode<RepeatingButton>("Fuel/FuelGauge/Minus/RepeatingButton");
			FuelGaugePlus = GetNode<RepeatingButton>("Fuel/FuelGauge/Plus/RepeatingButton");

			CargoService = GetNode<Control>("CargoService");
			EquipmentStock = GetNode<Control>("EquipmentStock");
			StaffList = GetNode<Control>("StaffList");
			GrappleWindowControl = GetNode<Control>("GrappleWindow");

			GrappleWindow = GetNode<DynamicWindow>("GrappleWindow/GrappleEmptier");
			GrappleWindowTitle = GetNode<Label>("GrappleWindow/GrappleEmptier/Title");
            GrappleWindowQuantities = GetNode<Label>("GrappleWindow/GrappleEmptier/Quantites");
            GrappleWindowComplete = GetNode<Label>("GrappleWindow/GrappleEmptier/Complete");

            for (int i = 0; i < 11; i++)
			{
				EquipmentStockNameLabels[i] = GetNode<Label>("EquipmentStock/BackgroundBox/PanelContainer/GridContainer/NameColumn/" + i.ToString().PadLeft(2, '0'));
				EquipmentStockCountLabels[i] = GetNode<Label>("EquipmentStock/BackgroundBox/PanelContainer/GridContainer/CountColumn/" + i.ToString().PadLeft(2, '0'));
				EquipmentStockButtons[i] = GetNode<Button>("EquipmentStock/Buttons/" + i.ToString().PadLeft(2, '0'));

				int index = i;
				EquipmentStockButtons[i].Pressed += () => SelectEquipment(index);
			}

			FuelType = GetNode<Label>("Fuel/Type");
			FuelInStock = GetNode<Label>("Fuel/InStock");
			FuelInShip = GetNode<Label>("Fuel/InShip");

			CockpitInstance.GetNode<Button>("OpenShipInterior").Pressed += OpenShipInterior_Pressed;

			foreach (var navTorso in TorsoInstances)
				navTorso.GetNode<Button>("OpenShipInterior").Pressed += OpenShipInterior_Pressed;

			EngineInstance.GetNode<Button>("OpenShipInterior").Pressed += OpenShipInterior_Pressed;

			CockpitInstance.StaffList.PilotChanged += CockpitInstance_PilotChanged;
			CockpitInstance.StaffList.ProductionChanged += CockpitInstance_ProductionChanged;

			TorsoStaffList.StaffClicked += TorsoStaffList_StaffClicked;

			EngineInstance.EngineInstalled += EngineInstance_EngineInstalled;

			foreach (var torso in TorsoInstances)
			{
				torso.ModuleChanged += ShipBay_ModuleChanged;
				torso.ModuleOpened += ShipBay_ModuleOpened;
			}

			foreach (var mineral in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.ItemCategory == ItemCategory.resource))
				GetNode<Button>("CargoService/Buttons/" + mineral.ItemType.ToScreenString()).Pressed += () => SelectMineral(mineral.ItemType);

			Nav_Cockpit.Pressed += NavCockpitPressed;

			Nav_Torsos[0].Pressed += () => NavTorsoPressed(1);
			Nav_Torsos[1].Pressed += () => NavTorsoPressed(2);
			Nav_Torsos[2].Pressed += () => NavTorsoPressed(3);
			Nav_Torsos[3].Pressed += () => NavTorsoPressed(4);
			Nav_Torsos[4].Pressed += () => NavTorsoPressed(5);
			Nav_Torsos[5].Pressed += () => NavTorsoPressed(6);

			Nav_Engine.Pressed += NavEnginePressed;

			Nav_Create_Shuttle.Pressed += CreateShuttle;
			Nav_Create_IOS.Pressed += CreateIOS;
			Nav_Create_SCG.Pressed += CreateSCG;
			Nav_Dismantle.Pressed += DismantleShip;

			CockpitInstance.GetNode<TextureButton>("Buttons/AddACC").Pressed += AddACC_Pressed;

			FuelGaugeMinus.Pressed += FuelGaugeMinus_Pressed;
			FuelGaugePlus.Pressed += FuelGaugePlus_Pressed;

            GrappleWindow.Closed = GrappleClosed;

			CargoService.Visible = false;
			EquipmentStock.Visible = false;
			StaffList.Visible = false;
			GrappleWindowControl.Visible = false;

            ScreenState = GetScreenState();

			UpdateState();

			// Restore the bay position after container layout, without animating from the scene's saved offset.
			ScrollContainer.ScrollHorizontal = 0;
			ScrollContainer.SetDeferred(ScrollContainer.PropertyName.ScrollHorizontal, ScreenState * ScreenWidth);

			RefreshButtons();
			BindHoverLabels();

			base._Ready();
		}

		private void BindHover(Control control, Func<string> label)
		{
			control.MouseEntered += () =>
			{
				if (!control.IsVisibleInTree() || (control is BaseButton button && button.Disabled)) return;
				hoveredControl = control;
				hoveredLabel = label;
				UpdateHover();
			};
			control.MouseExited += () => { if (hoveredControl == control) ClearHover(); };
			control.VisibilityChanged += () => { if (hoveredControl == control && !control.IsVisibleInTree()) ClearHover(); };
			control.TreeExiting += () => { if (hoveredControl == control) ClearHover(); };
		}

		private void ClearHover()
		{
			if (GameCore.HoverText == lastHoverText) GameCore.HoverText = "";
			hoveredControl = null;
			hoveredLabel = null;
			lastHoverText = null;
		}

		private void UpdateHover()
		{
			if (hoveredControl == null) return;
			if (!hoveredControl.IsVisibleInTree() || (hoveredControl is BaseButton button && button.Disabled))
			{
				ClearHover();
				return;
			}
			lastHoverText = hoveredLabel() ?? "";
			GameCore.HoverText = lastHoverText;
		}

		public override void _Process(double delta) => UpdateHover();

		private string StaffHover(int index)
		{
			var staff = ResourceList.Staff[index];
			if (staff == null) return ShipPresent && Ship.Pilot != null ? "Remove ship's crew" : "";
			if (staff.Type == StaffType.Marines) return ShipPresent ? "Assign ship's crew" : "";
			if (staff.Type == StaffType.Production && Ground && Earth && GameCore.Earth.Factory.Builder == null)
				return "Assign crew to production";
			return "";
		}

		private string PodHover(int index, Module_Types type, string name)
		{
			if (!ShipPresent || index >= Ship.Modules.Count) return "";
			var module = Ship.Modules[index];
			if (module.ModuleType != type) return "Install " + name;
			if (module.ItemCount > 0 || module.StaffStored != null || module.HeldItem != null)
				return "Unload pod before removal";
			return "Remove " + name;
		}

		private void BindHoverLabels()
		{
			BindHover(Nav_Dismantle, () => "Dismantle ship");
			BindHover(Nav_Create_Shuttle, () => "Build shuttle");
			BindHover(Nav_Create_IOS, () => "Build IOS");
			BindHover(Nav_Create_SCG, () => "Build SCG");
			BindHover(Nav_Cockpit, () => "Crew section");
			BindHover(Nav_Engine, () => "Engine mounting");
			BindHover(FuelGaugeMinus, () => ShipPresent ? "Unload fuel" : "");
			BindHover(FuelGaugePlus, () => ShipPresent ? "Fuel ship" : "");
			BindHover(CockpitInstance.GetNode<Control>("Buttons/AddACC"), () => "Fit A.C.C.");
			BindHover(CockpitInstance.GetNode<Control>("OpenShipInterior"), () => ShipPresent ? "Access ship" : "");
			BindHover(EngineInstance.GetNode<Control>("OpenShipInterior"), () => ShipPresent ? "Access ship" : "");
			for (int i = 0; i < 4; i++)
			{
				var index = i;
				BindHover(CockpitInstance.StaffList.GetNode<Control>("Staff/Buttons/0" + (i + 1)), () => StaffHover(index));
			}
			for (int i = 0; i < TorsoInstances.Count; i++)
			{
				var index = i;
				BindHover(Nav_Torsos[i], () => ShipPresent && index < Ship.Modules.Count ? "Pod mount " + (index + 1) : "");
				var torso = TorsoInstances[i];
				BindHover(torso.GetNode<Control>("OpenShipInterior"), () => ShipPresent ? "Access ship" : "");
				BindHover(torso.AddSupplyPod, () => PodHover(index, Module_Types.Supply, "supply pod"));
				BindHover(torso.AddToolPod, () => PodHover(index, Module_Types.Tool, "tool pod"));
				BindHover(torso.AddCryoPod, () => PodHover(index, Module_Types.Cryo, "team pod"));
			}
		}

		private void AddACC_Pressed()
		{
			Objects.Store stores;

			if (Ground)
				stores = CurrentPlanet.PlanetResources.Stores;
			else
				stores = CurrentPlanet.Station.Resources.Stores;

			if (stores[ItemTypes.a__c__c] > 0)
			{
				Ship.ACC = new Objects.ACC();
				Ship.ACC.Ship = Ship;
				Ship.ACC.Source = CurrentPlanet.PlanetId;
				Ship.ACC.Destination = CurrentPlanet.PlanetId;
				Ship.ACC.Active = false;
				Ship.ACC.CycleMode = false;
				Ship.ACC.SourceItems = new List<ItemTypes>();
				Ship.ACC.DestinationItems = new List<ItemTypes>();
				Ship.ACC.CurrentSource = ItemTypes.iron;
				Ship.ACC.CurrentDestination = ItemTypes.iron;

				stores[Enums.ItemTypes.a__c__c]--;
			}

			UpdateState();
		}

		private void OpenShipInterior_Pressed()
		{
			if (Ship != null)
			{
				GameCore.SingletonInstance.ShipSelected = Ship.ShipID;

				//Underscores in scene names represent a folder
				Deuteros.Code.GameCore.SingletonInstance.ChangeScene(Enums.Scenes.ShipInterior, new List<SceneVariables>());
			}
		}

		private void FuelGaugePlus_Pressed()
		{
			var planetStores = this.Ground ? CurrentPlanet.PlanetResources.Stores : CurrentPlanet.Station.Resources.Stores;
 
			if (ShipPresent && Ship.Fuel < 250 && planetStores[Ship.FuelType] >= Ship.FuelUnitCost)
			{
				Ship.Fuel++;
				planetStores[Ship.FuelType] -= Ship.FuelUnitCost;

				UpdateState();
			}
		}

		private void FuelGaugeMinus_Pressed()
		{
			var planetStores = this.Ground ? CurrentPlanet.PlanetResources.Stores : CurrentPlanet.Station.Resources.Stores;

			if (ShipPresent && Ship.Fuel > 0 && planetStores[Ship.FuelType] <= 50000 - Ship.FuelUnitCost)
			{
				Ship.Fuel--;
				planetStores[Ship.FuelType] += Ship.FuelUnitCost;

				UpdateState();
			}
		}

		private void DismantleShip()
		{
			if (!ShipPresent || Ship == null)
				return;

			// Grapple unloading also handles alien discoveries; keep that existing workflow intact.
			if (Ship.Modules.Any(module => module.HeldItem != null))
			{
				GameCore.ShowError(this, "Unload Grapples\nBefore Dismantling");
				return;
			}

			var returningStaff = Ship.Modules.Where(module => module.StaffStored != null)
				.Select(module => module.StaffStored).ToList();
			if (Ship.Pilot != null)
				returningStaff.Add(Ship.Pilot);

			if (returningStaff.Count > ResourceList.Staff.Count(staff => staff == null))
			{
				GameCore.ShowError(this, "Not Enough Staff Slots\nTo Dismantle Ship");
				return;
			}

			var returningItems = new Dictionary<ItemTypes, int>();
			void ReturnItem(ItemTypes item, int count)
			{
				if (item == ItemTypes.none || count <= 0) return;
				returningItems.TryGetValue(item, out var current);
				returningItems[item] = current + count;
			}

			ReturnItem(Ship.FuelType, Ship.Fuel * Ship.FuelUnitCost);
			if (Ship.ACC != null) ReturnItem(ItemTypes.a__c__c, 1);
			foreach (var module in Ship.Modules)
			{
				ReturnItem(module.ItemStored, module.ItemCount);
				switch (module.ModuleType)
				{
					case Module_Types.Supply: ReturnItem(ItemTypes.supply_pod, 1); break;
					case Module_Types.Tool: ReturnItem(ItemTypes.tool_pod, 1); break;
					case Module_Types.Cryo: ReturnItem(ItemTypes.cryo_pod, 1); break;
				}
			}
			if (Ship is InterStellarShip stellar)
				ReturnItem(Ship.ShipType == Ship_Types.IOS ? ItemTypes.ios_drone : ItemTypes.star_drone, stellar.DroneCount);

			var chassis = Ship.ShipType == Ship_Types.Shuttle ? ItemTypes.s_chassis : Ship.ShipType == Ship_Types.IOS ? ItemTypes.i_chassis : ItemTypes.g_chassis;
			var drive = Ship.ShipType == Ship_Types.Shuttle ? ItemTypes.s_drive : Ship.ShipType == Ship_Types.IOS ? ItemTypes.i_drive : ItemTypes.star_drive;
			ReturnItem(chassis, 1);
			if (Ship.Engine && !Ship.EngineDamaged) ReturnItem(drive, 1);
			// Check the combined returns first (fuel can also be carried in supply pods).
			if (returningItems.Any(item => item.Value > 50000 - ResourceList.Stores[item.Key]))
			{
				GameCore.ShowError(this, "Not Enough Space In Stores\nTo Dismantle Ship");
				return;
			}

			foreach (var item in returningItems)
				ResourceList.Stores[item.Key] += item.Value;
			foreach (var staff in returningStaff)
				ResourceList.AddStaff(staff);

            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            if (save.Ships.Remove(Ship)) save.News.AddNews(Ship.Name + " Scrapped.");
			Ship = null;
			UpdateScreenState(0);
			UpdateState();
			RefreshButtons();
		}

		private void CreateShuttle()
		{
			if ((Ground && CurrentPlanet.PlanetResources.Stores[Enums.ItemTypes.s_chassis] > 0) || (!Ground && CurrentPlanet.Station.Resources.Stores[Enums.ItemTypes.s_chassis] > 0))
			{
				if (GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Any(s => s.ShipType == Ship_Types.Shuttle && s.PlanetLocation == CurrentPlanet.PlanetId))
				{
					GameCore.ShowError(this, "\nShuttle Is\nAlready Active\nHere !");
					return;
				}


				var newShuttle = new Shuttle();
				newShuttle.StartTravelDay = 0;
				newShuttle.StarLocation = CurrentPlanet.ParentStar;
				newShuttle.Modules = new List<ShipModule>();
				newShuttle.Modules.Add(new ShipModule());
				newShuttle.ShipState = Ship_States.Docked;
				newShuttle.Fuel = 0;
				newShuttle.Engine = false;
				newShuttle.FuelType = Enums.ItemTypes.meh_fuel;
				newShuttle.OnGround = Ground;
				newShuttle.Pilot = null;
				newShuttle.PlanetLocation = CurrentPlanet.PlanetId;
				newShuttle.ShipType = Enums.Ship_Types.Shuttle;
				newShuttle.LocationView = false;
				newShuttle.Name = CurrentPlanet.PlanetId.ToScreenString(" ")+" Shuttle";

				Objects.Store stores;

				if (Ground)
					stores = CurrentPlanet.PlanetResources.Stores;
				else
					stores = CurrentPlanet.Station.Resources.Stores;

				if (stores[ItemTypes.a__c__c] > 0)
				{
					newShuttle.ACC = new Objects.ACC();
					newShuttle.ACC.Ship = newShuttle;
					newShuttle.ACC.Source = CurrentPlanet.PlanetId;
					newShuttle.ACC.Destination = CurrentPlanet.PlanetId;
					newShuttle.ACC.Active = false;
					newShuttle.ACC.CycleMode = false;
					newShuttle.ACC.SourceItems = new List<ItemTypes>();
					newShuttle.ACC.DestinationItems = new List<ItemTypes>();
					newShuttle.ACC.CurrentSource = ItemTypes.iron;
					newShuttle.ACC.CurrentDestination = ItemTypes.iron;

					stores[Enums.ItemTypes.a__c__c]--;
				}

				stores[ItemTypes.s_chassis]--;

				GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Add(newShuttle);

				GameCore.SingletonInstance.TriggerShipCreated(newShuttle);

				UpdateState();
				RefreshButtons();
			}
		}

		private void CreateIOS()
		{
			if (!Ground && !ShipPresent && CurrentPlanet.Station.Resources.Stores[Enums.ItemTypes.i_chassis] > 0)
			{
				var newIOS = new IOS();
				newIOS.StartTravelDay = 0;
				newIOS.StarLocation = CurrentPlanet.ParentStar;
				newIOS.Modules = new List<ShipModule>();
				newIOS.Modules.AddRange(Enumerable.Range(0, 3).Select(_ => new ShipModule()));
				newIOS.ShipState = Ship_States.Docked;
				newIOS.Fuel = 0;
				newIOS.Engine = false;
				newIOS.FuelType = Enums.ItemTypes.meh_fuel;
				newIOS.Pilot = null;
				newIOS.PlanetLocation = CurrentPlanet.PlanetId;
				newIOS.ShipType = Enums.Ship_Types.IOS;
				newIOS.DestinationPlanetLocation = CurrentPlanet.PlanetId;
				newIOS.DestinationStarLocation = CurrentPlanet.ParentStar;
				newIOS.LocationView = false;
				newIOS.Name = "IOS3" + GameCore.SingletonInstance.GameData.ActiveSaveFile.IOSCount.ToString().PadLeft(5, '0');

				Objects.Store stores;

				if (Ground)
					stores = CurrentPlanet.PlanetResources.Stores;
				else
					stores = CurrentPlanet.Station.Resources.Stores;

				if (stores[ItemTypes.a__c__c] > 0)
				{
					newIOS.ACC = new Objects.ACC();
					newIOS.ACC.Ship = newIOS;
					newIOS.ACC.Source = CurrentPlanet.PlanetId;
					newIOS.ACC.Destination = CurrentPlanet.PlanetId;
					newIOS.ACC.Active = false;
					newIOS.ACC.CycleMode = false;
					newIOS.ACC.SourceItems = new List<ItemTypes>();
					newIOS.ACC.DestinationItems = new List<ItemTypes>();
					newIOS.ACC.CurrentSource = ItemTypes.iron;
					newIOS.ACC.CurrentDestination = ItemTypes.iron;

					stores[Enums.ItemTypes.a__c__c]--;
				}

				stores[ItemTypes.i_chassis]--;

				GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Add(newIOS);

				GameCore.SingletonInstance.TriggerShipCreated(newIOS);

				UpdateState();
				RefreshButtons();
			}
		}

		private void CreateSCG()
		{
			if (!Ground && !ShipPresent && CurrentPlanet.Station.Resources.Stores[Enums.ItemTypes.g_chassis] > 0)
			{
				var newSCG = new SCG();
				newSCG.StartTravelDay = 0;
				newSCG.StarLocation = CurrentPlanet.ParentStar;
				newSCG.Modules = new List<ShipModule>();
				newSCG.Modules.AddRange(Enumerable.Range(0, 6).Select(_ => new ShipModule()));
				newSCG.ShipState = Ship_States.Docked;
				newSCG.Fuel = 0;
				newSCG.Engine = false;
				newSCG.FuelType = Enums.ItemTypes.hed_fuel;
				newSCG.Pilot = null;
				newSCG.PlanetLocation = CurrentPlanet.PlanetId;
				newSCG.ShipType = Enums.Ship_Types.SCG;
				newSCG.DestinationPlanetLocation = CurrentPlanet.PlanetId;
				newSCG.DestinationStarLocation = CurrentPlanet.ParentStar;
				newSCG.LocationView = false;
				newSCG.Name = "SCG3" + GameCore.SingletonInstance.GameData.ActiveSaveFile.SCGCount.ToString().PadLeft(5, '0');

				Objects.Store stores;

				if (Ground)
					stores = CurrentPlanet.PlanetResources.Stores;
				else
					stores = CurrentPlanet.Station.Resources.Stores;

				if (stores[ItemTypes.a__c__c] > 0)
				{
					newSCG.ACC = new Objects.ACC();
					newSCG.ACC.Ship = newSCG;
					newSCG.ACC.Source = CurrentPlanet.PlanetId;
					newSCG.ACC.Destination = CurrentPlanet.PlanetId;
					newSCG.ACC.Active = false;
					newSCG.ACC.CycleMode = false;
					newSCG.ACC.SourceItems = new List<ItemTypes>();
					newSCG.ACC.DestinationItems = new List<ItemTypes>();
					newSCG.ACC.CurrentSource = ItemTypes.iron;
					newSCG.ACC.CurrentDestination = ItemTypes.iron;

					stores[Enums.ItemTypes.a__c__c]--;
				}

				stores[ItemTypes.g_chassis]--;

				GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Add(newSCG);

				GameCore.SingletonInstance.TriggerShipCreated(newSCG);

				UpdateState();
				RefreshButtons();
			}
		}

		private int GetScreenState()
		{
			var returnState = 0;

			if (Ground)
				returnState = CurrentPlanet.ShuttleState;
			else if (!Ground && CurrentPlanet.Station != null && CurrentPlanet.Station.Built && Shuttle)
				returnState = CurrentPlanet.Station.ShuttleState;
			else if (!Ground && CurrentPlanet.Station != null && CurrentPlanet.Station.Built && !Shuttle)
				returnState = CurrentPlanet.Station.StarShipState;

			return returnState;
		}

		private void UpdateScreenState(int newScreenState)
		{
			if (Ground)
				CurrentPlanet.ShuttleState = newScreenState;
			else if (!Ground && CurrentPlanet.Station != null && CurrentPlanet.Station.Built && Shuttle)
				CurrentPlanet.Station.ShuttleState = newScreenState;
			else if (!Ground && CurrentPlanet.Station != null && CurrentPlanet.Station.Built && !Shuttle)
				CurrentPlanet.Station.StarShipState = newScreenState;
		}

		private void NavCockpitPressed()
		{
			if (ScreenState != 0)
			{
				ScreenState = 0;
				ScrollToScreen();
				UpdateScreenState(ScreenState);
				RefreshButtons();
			}
		}

		private void NavTorsoPressed(int torsoId)
		{
			if (ScreenState != torsoId)
			{
				ScreenState = torsoId;
				ScrollToScreen();
				UpdateScreenState(ScreenState);

				RefreshButtons();
			}
		}

		private void NavEnginePressed()
		{
			if (ScreenState != 7)
			{
				ScreenState = 7;
				ScrollToScreen();
				UpdateScreenState(ScreenState);
				RefreshButtons();
			}
		}

		private void GrappleClosed(object DataObject)
		{
			var currentModule = Ship.Modules[(int)DataObject];
			var heldItem = currentModule.HeldItem;
			currentModule.HeldItem = null;
			if (heldItem == null)
				return;
			if (heldItem.GrappleItemType == GrappleItemTypes.Asteroid)
			{
				var heldAsteroid = ((Asteroid)heldItem);

				ResourceList.Stores[heldAsteroid.Type] = Math.Min(50000, ResourceList.Stores[heldAsteroid.Type] + heldAsteroid.Mass);
			}
			else if (heldItem.GrappleItemType == GrappleItemTypes.UnknownItem)
			{
                var unknownItem = ((UnknownItem)heldItem);
				switch (unknownItem.ItemType)
				{
                    case UnknownItemTypes.AlienArtifact:
                        GameCore.SingletonInstance.TriggerAlienTechDiscovery(Enums.ItemTypes.alien_artifact);
                        break;

                    case UnknownItemTypes.CommsPod:
                        GameCore.SingletonInstance.TriggerAlienTechDiscovery(Enums.ItemTypes.commspod);
						break;

                    case UnknownItemTypes.Blazer:
                        GameCore.SingletonInstance.TriggerAlienTechDiscovery(Enums.ItemTypes.m__f__l);
                        break;
                }
            }

			GameCore.UnLockScreen();
		}

		private void ScrollToScreen()
		{
			//TODO - Does not work backwards
			int targetScrollX = ScreenState * ScreenWidth;

			// Smooth scrolling
			var tween = GetTree().CreateTween();

			// Bounce distances
			float bounce1 = 20f;
			float bounce2 = 8f;

			// Scroll limits
			float minScroll = 0f;
			float maxScroll = (float)ScrollContainer.GetHScrollBar().MaxValue;

			// Clamp target within scroll bounds
			float target = Mathf.Clamp(targetScrollX, minScroll, maxScroll);

			// Get current scroll position
			float current = ScrollContainer.ScrollHorizontal;

			// Direction: +1 if scrolling forward (increasing), -1 if backward
			int direction = (target > current) ? 1 : -1;

			// Compute bounce positions (moving *away* from target)
			float bounceTarget1 = Mathf.Clamp(target - (bounce1 * direction), minScroll, maxScroll);
			float bounceTarget2 = Mathf.Clamp(target - (bounce2 * direction), minScroll, maxScroll);

			// Begin scroll
			tween.TweenProperty(ScrollContainer, "scroll_horizontal", target, 0.6)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.In);

			// First bounce
			if (!Mathf.IsEqualApprox(bounceTarget1, target))
			{
				tween.TweenProperty(ScrollContainer, "scroll_horizontal", bounceTarget1, 0.12)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.Out);

				tween.TweenProperty(ScrollContainer, "scroll_horizontal", target, 0.12)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.InOut);
			}

			// Second, smaller bounce
			if (!Mathf.IsEqualApprox(bounceTarget2, target))
			{
				tween.TweenProperty(ScrollContainer, "scroll_horizontal", bounceTarget2, 0.07)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.Out);

				tween.TweenProperty(ScrollContainer, "scroll_horizontal", target, 0.07)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.InOut);
			}
		}

		private void UpdateState()
		{
			//Detect if there is a ship present
			ShipPresent = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Any(T => T.PlanetLocation == CurrentPlanet.PlanetId && T.ShipState == Ship_States.Docked && 
			(
			(T.ShipType == Ship_Types.Shuttle && SceneVariables.Contains(Enums.SceneVariables.Shuttle) && (Ground == ((Shuttle)T).OnGround))
			|| (T.ShipType != Ship_Types.Shuttle && SceneVariables.Contains(Enums.SceneVariables.Ship))
			)
			);

			CockpitInstance.UpdateStaff(Ground ? CurrentPlanet.PlanetResources.Staff : CurrentPlanet.Station.Resources.Staff);
			TorsoStaffList.UpdateStaff(Ground ? CurrentPlanet.PlanetResources.Staff : CurrentPlanet.Station.Resources.Staff);

			//There is no ship, reset buttons and reset state
			if (!ShipPresent)
			{
				ScreenState = 0;

				Nav_Dismantle.Visible = false;
				Nav_Cockpit.Visible = false;
				Nav_Torsos.ForEach(T => T.Visible = false);
				Nav_Engine.Visible = false;

				CockpitInstance.LoadShip(null);
				TorsoStaffList.UpdateShip(false);

				FuelInShip.Text = "";
				FuelInStock.Text = "";
				FuelType.Text = "";

				TorsoInstances.ForEach(T => T.SpriteHolder.Visible = false);
				EngineInstance.SpriteHolder.Visible = false;

				ScrollContainer.ScrollHorizontal = 0;

				if (Shuttle)
				{
					Nav_Create_Shuttle.Visible = true;
					Nav_Create_IOS.Visible = false;
					Nav_Create_SCG.Visible = false;
				}
				else
				{
					Nav_Create_Shuttle.Visible = false;
					Nav_Create_IOS.Visible = true;
					Nav_Create_SCG.Visible = !GameCore.SingletonInstance.GameData.GetItem(ItemTypes.g_chassis).Locked;
				}
			}
			else
			{
				Nav_Create_Shuttle.Visible = false;
				Nav_Create_IOS.Visible = false;
				Nav_Create_SCG.Visible = false;

				Nav_Dismantle.Visible = true;
				Nav_Cockpit.Visible = true;
				Nav_Engine.Visible = true;

				EngineInstance.SpriteHolder.Visible = true;

				Ship = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Single(T => T.PlanetLocation == CurrentPlanet.PlanetId && T.ShipState == Ship_States.Docked &&
				(
				(T.ShipType == Ship_Types.Shuttle && SceneVariables.Contains(Enums.SceneVariables.Shuttle))
				|| (T.ShipType != Ship_Types.Shuttle && SceneVariables.Contains(Enums.SceneVariables.Ship)
				)
				)
				);

				FuelInShip.Text = Ship.Fuel.ToString();
				FuelInStock.Text = Ground ? CurrentPlanet.PlanetResources.Stores[Ship.FuelType].ToString() : CurrentPlanet.Station.Resources.Stores[Ship.FuelType].ToString();
				FuelType.Text = GameCore.SingletonInstance.GameData.GetItem(Ship.FuelType).ShortName.Substring(0,3);

				CockpitInstance.LoadShip(Ship);
				TorsoStaffList.UpdateShip(Ship != null);

				Nav_Torsos.ForEach(T => T.Visible = false);
				TorsoInstances.ForEach(T => T.Visible = false);
				TorsoInstances.ForEach(T => T.SpriteHolder.Visible = false);

				EngineInstance.Installed = Ship.Engine;
				EngineInstance.Damaged = Ship.EngineDamaged;
				EngineInstance.Shiptype = Ship.ShipType;
				EngineInstance.Ground = Ground;

				if (Ship.ShipType == Ship_Types.Shuttle)
					EngineInstance.EngineType = ItemTypes.s_drive;
				else if (Ship.ShipType == Ship_Types.IOS)
					EngineInstance.EngineType = ItemTypes.i_drive;
				else if (Ship.ShipType == Ship_Types.SCG)
					EngineInstance.EngineType = ItemTypes.star_drive;

				EngineInstance.UpdateState();

				if (Ship.ShipType == Enums.Ship_Types.Shuttle)
				{
					Nav_Torsos[0].Visible = true;
					TorsoInstances[0].Visible = true;
					TorsoInstances[0].SpriteHolder.Visible = true;

					TorsoInstances[0].ChangeModule(Ship.Modules[0]);
					TorsoInstances[0].TorsoSection = 0;
					TorsoInstances[0].UpdateState();
				}
				else if (Ship.ShipType == Enums.Ship_Types.IOS)
				{
					Nav_Torsos[0].Visible = true;
					Nav_Torsos[1].Visible = true;
					Nav_Torsos[2].Visible = true;
					TorsoInstances[0].Visible = true;
					TorsoInstances[1].Visible = true;
					TorsoInstances[2].Visible = true;
					TorsoInstances[0].SpriteHolder.Visible = true;
					TorsoInstances[1].SpriteHolder.Visible = true;
					TorsoInstances[2].SpriteHolder.Visible = true;

					TorsoInstances[0].ChangeModule(Ship.Modules[0]);
					TorsoInstances[0].TorsoSection = 0;
					TorsoInstances[0].UpdateState();

					TorsoInstances[1].ChangeModule(Ship.Modules[1]);
					TorsoInstances[1].TorsoSection = 1;
					TorsoInstances[1].UpdateState();

					TorsoInstances[2].ChangeModule(Ship.Modules[2]);
					TorsoInstances[2].TorsoSection = 2;
					TorsoInstances[2].UpdateState();
				}
				else if (Ship.ShipType == Enums.Ship_Types.SCG)
				{
					for (var index = 0; index < Math.Min(TorsoInstances.Count, Ship.Modules.Count); index++)
					{
						Nav_Torsos[index].Visible = true;
						TorsoInstances[index].Visible = true;
						TorsoInstances[index].SpriteHolder.Visible = true;
						TorsoInstances[index].TorsoSection = index;
						TorsoInstances[index].ChangeModule(Ship.Modules[index]);
					}
				}
			}
		}

		private Staff[] CockpitInstance_ProductionChanged(Staff staff)
		{
			if (Ground && CurrentPlanet.PlanetId == Enums.StellarBodies.earth && ((Earth)CurrentPlanet).Factory.Builder == null)
			{
				((Earth)CurrentPlanet).Factory.Builder = staff;
				((Earth)CurrentPlanet).PlanetResources.RemoveStaff(staff);
				return ((Earth)CurrentPlanet).PlanetResources.Staff;
			}
			else if (!Ground && !CurrentPlanet.Station.Factory.AOC && CurrentPlanet.Station.Factory.Builder == null)
			{
				CurrentPlanet.Station.Factory.Builder = staff;
				CurrentPlanet.Station.Resources.RemoveStaff(staff);
				return CurrentPlanet.Station.Resources.Staff;
			}
			else
			{
				return CurrentPlanet.Station.Resources.Staff;
			}
		}

		private Staff[] CockpitInstance_PilotChanged(Staff staff)
		{
			//Detect if there is a ship present
			if (Ship != null)
			{
				if (staff != null && Ship.Pilot != null)
					Ship.Pilot = ResourceList.SwapStaff(staff, Ship.Pilot);

				if (staff != null && Ship.Pilot == null)
				{
					Ship.Pilot = staff;
					ResourceList.RemoveStaff(staff);
				}

				if (staff == null && Ship.Pilot != null)
				{
					ResourceList.AddStaff(Ship.Pilot);
					Ship.Pilot = null;
				}

				CockpitInstance.UpdateState();
			}

			return ResourceList.Staff;
		}

		private Staff[] TorsoStaffList_StaffClicked(Staff staff)
		{
			if (staff != null && Ship.Modules[ScreenState - 1].StaffStored != null)
			{
				Ship.Modules[ScreenState - 1].StaffStored = ResourceList.SwapStaff(staff, Ship.Modules[ScreenState - 1].StaffStored);
			}
			else if (staff == null && Ship.Modules[ScreenState - 1].StaffStored != null)
			{
				ResourceList.AddStaff(Ship.Modules[ScreenState - 1].StaffStored);
				Ship.Modules[ScreenState - 1].StaffStored = null;
			}
			else if (staff != null && Ship.Modules[ScreenState - 1].StaffStored == null)
			{
				Ship.Modules[ScreenState - 1].StaffStored = staff;
				ResourceList.RemoveStaff(staff);
			}

			TorsoStaffList.UpdateState();
			CockpitInstance.UpdateStaff(ResourceList.Staff);
			CockpitInstance.UpdateState();

			TorsoInstances[ScreenState - 1].UpdateState();

			return ResourceList.Staff;
		}

		private bool ShipBay_ModuleChanged(Enums.Module_Types moduleType, int torsoSection)
		{
			var currentModule = Ship.Modules[torsoSection];
			var oldType = currentModule.ModuleType;

			var currentStore = Ground ? CurrentPlanet.PlanetResources.Stores : CurrentPlanet.Station.Resources.Stores;

			if (currentModule.ModuleType == Enums.Module_Types.Supply && (currentModule.ItemCount > 0 || moduleType == Module_Types.Supply))
				return false;
			else if (currentModule.ModuleType == Enums.Module_Types.Tool && (currentModule.ItemStored != Enums.ItemTypes.none || moduleType == Module_Types.Tool))
				return false;
			else if (currentModule.ModuleType == Enums.Module_Types.Cryo && (currentModule.StaffStored != null || moduleType == Module_Types.Cryo))
				return false;
			else if (moduleType == Module_Types.Supply && (GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.supply_pod).Locked || currentStore[Enums.ItemTypes.supply_pod] == 0))
				return false;
			else if (moduleType == Module_Types.Tool && (GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.tool_pod).Locked || currentStore[Enums.ItemTypes.tool_pod] == 0))
				return false;
			else if (moduleType == Module_Types.Cryo && (GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.cryo_pod).Locked || currentStore[Enums.ItemTypes.cryo_pod] == 0))
				return false;
			else
				Ship.Modules[torsoSection].ModuleType = moduleType;


			switch (moduleType)
			{
				case Module_Types.Supply:
					currentStore[ItemTypes.supply_pod] -= 1;
					break;
				case Module_Types.Tool:
					currentStore[ItemTypes.tool_pod] -= 1;
					break;
				case Module_Types.Cryo:
					currentStore[ItemTypes.cryo_pod] -= 1;
					break;
			}

			switch (oldType)
			{
				case Module_Types.Supply:
					currentStore[ItemTypes.supply_pod] += 1;
					break;
				case Module_Types.Tool:
					currentStore[ItemTypes.tool_pod] += 1;
					break;
				case Module_Types.Cryo:
					currentStore[ItemTypes.cryo_pod] += 1;
					break;
			}

			return true;
		}

		private void ShipBay_ModuleOpened(int torsoSection)
		{
			var currentModule = Ship.Modules[torsoSection];

			var cursor = GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView");

			if (currentModule.ModuleType == Enums.Module_Types.Supply)
			{
				UpdateCargoService();

				CargoService.Visible = true;

				cursor.LockToRect(CargoService.GetGlobalRect());
			}
			else if (currentModule.ModuleType == Enums.Module_Types.Tool)
			{
				//We have a grapple with an item in it
				if (currentModule.ItemStored == ItemTypes.grapple && currentModule.HeldItem != null)
				{
					GrappleWindowControl.Visible = true;
					GrappleWindow.Visible = true;
					if (currentModule.HeldItem.GrappleItemType == GrappleItemTypes.Asteroid)
					{
						var asteroidtype = ((Asteroid)currentModule.HeldItem).Type;
                        GrappleWindowTitle.Text = "Asteroid Break-Up\r\nSequence.";
                        GrappleWindowQuantities.Text = "100 "+asteroidtype.ToScreenString()+" 50000";
                        GrappleWindowComplete.Text = "Sequence Complete.";

                    }
                    else
					{
                        GrappleWindowTitle.Text = "Object Removed For\r\nResearch Analysis...";
                        GrappleWindowQuantities.Text = "";
                        GrappleWindowComplete.Text = "";

                    }
                    GrappleWindow.DataObject = torsoSection;
					GrappleWindow.StartCloseTimer(5f);

					GameCore.LockScreen();
				}
				else
				{
					UpdateEquipmentStock();

					EquipmentStock.Visible = true;

					cursor.LockToRect(EquipmentStock.GetGlobalRect());
				}
			}
			else if (currentModule.ModuleType == Enums.Module_Types.Cryo)
			{
				TorsoStaffList.UpdateState();

				StaffList.Visible = true;

				cursor.LockToRect(StaffList.GetGlobalRect());
			}
		}

		private void EngineInstance_EngineInstalled()
		{
			Ship.Engine = true;
			Ship.EngineDamaged = false;
			Ship.EngineEngaged = false;
		}

		#region EquipmentStock

		private Item[] EquipmentForShip()
		{
			// Original $32B1E hull masks, expressed by item identity rather than saved research indices.
			bool Fits(ItemTypes type) => type switch
			{
				ItemTypes.derrick or ItemTypes.of_frame or ItemTypes.a__c__c or ItemTypes.bandaid or ItemTypes.r_frame => true,
				ItemTypes.pulse_blaster_laser or ItemTypes.a__m__a => Ship.ShipType == Ship_Types.IOS,
				ItemTypes.alien_artifact or ItemTypes.prison_pod => Ship.ShipType == Ship_Types.SCG,
				ItemTypes.grapple or ItemTypes.d__f__c__c or ItemTypes.commspod or ItemTypes.sonic_blaster => Ship.ShipType is Ship_Types.IOS or Ship_Types.SCG,
				_ => false
			};
			return GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList
				.Where(item => item.ItemCategory == ItemCategory.item && item.Research?.Researched == true && Fits(item.ItemType))
				.OrderBy(item => item.Research.Index).ToArray();
		}

		private void UpdateEquipmentStock()
		{
			var equipmentList = EquipmentForShip();

			for (int i = 0; i < 11; i++)
			{
				EquipmentStockNameLabels[i].RemoveThemeColorOverride("font_color");
				EquipmentStockCountLabels[i].RemoveThemeColorOverride("font_color");

				if (equipmentList.Count() > i)
				{
					EquipmentStockNameLabels[i].Visible = true;
					EquipmentStockCountLabels[i].Visible = true;
					EquipmentStockButtons[i].Visible = true;

					EquipmentStockNameLabels[i].Text = equipmentList[i].ShortName;
					EquipmentStockCountLabels[i].Text = ResourceList.Stores[equipmentList[i].ItemType].ToString();
				}
				else
				{
					EquipmentStockNameLabels[i].Visible = false;
					EquipmentStockCountLabels[i].Visible = false;
					EquipmentStockButtons[i].Visible = false;

					EquipmentStockNameLabels[i].Text = "";
					EquipmentStockCountLabels[i].Text = "";
				}
			}

			if (Ship.Modules[ScreenState - 1].ItemCount > 0)
			{
				var itemIndex = Array.FindIndex<Item>(equipmentList, T => T.ItemType == Ship.Modules[ScreenState - 1].ItemStored);

				if (itemIndex >= 0)
				{
					EquipmentStockNameLabels[itemIndex].AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);
					EquipmentStockCountLabels[itemIndex].AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);
				}
			}
		}

		private void SelectEquipment(int itemIndex)
		{
			var equipmentList = EquipmentForShip();
			if (itemIndex < 0 || itemIndex >= equipmentList.Length) return;

			var itemType = equipmentList[itemIndex].ItemType;

			// Keep the installed equipment when the requested replacement is unavailable.
			if (Ship.Modules[ScreenState - 1].ItemStored != itemType && ResourceList.Stores[itemType] <= 0)
				return;

			// Empty the old tank before DFCC changes its stock value. Reject the
            // entire fitting if its fuel cannot be returned without loss.
            if (itemType == ItemTypes.d__f__c__c && Ship is InterStellarShip { DFCC: false }
                && Ship.Modules[ScreenState - 1].ItemStored != itemType)
            {
                var returningFuel = Ship.Fuel * Ship.FuelUnitCost;
                if (returningFuel > 50000 - ResourceList.Stores[Ship.FuelType])
                {
                    GameCore.ShowError(this, "Not Enough Fuel Storage\nTo Fit D.F.C.C.");
                    return;
                }
                ResourceList.Stores[Ship.FuelType] += returningFuel;
                Ship.Fuel = 0;
            }

			if (Ship.Modules[ScreenState - 1].ItemCount > 0)
			{
				var storedItemIndex = Array.FindIndex<Item>(equipmentList, T => T.ItemType == Ship.Modules[ScreenState - 1].ItemStored);

				ResourceList.Stores[Ship.Modules[ScreenState - 1].ItemStored] += Ship.Modules[ScreenState - 1].ItemCount;

				if (storedItemIndex >= 0)
				{
					EquipmentStockNameLabels[storedItemIndex].RemoveThemeColorOverride("font_color");
					EquipmentStockCountLabels[storedItemIndex].RemoveThemeColorOverride("font_color");
				}
			}

			if (Ship.Modules[ScreenState - 1].ItemStored == itemType)
			{
				Ship.Modules[ScreenState - 1].ItemStored = ItemTypes.none;
				Ship.Modules[ScreenState - 1].ItemCount = 0;
			}
			else if (Ship.Modules[ScreenState - 1].ItemStored != itemType && ResourceList.Stores[itemType] > 0)
			{
				Ship.Modules[ScreenState - 1].ItemStored = itemType;

				if (Ship.ShipType!=Ship_Types.Shuttle && itemType == ItemTypes.d__f__c__c)
				{
					//todo show dfcc assembly animation

					((InterStellarShip)Ship).DFCC = true;
				}

				// Original $32FD4 stacks only Derricks; all other equipment occupies one slot.
				if (itemType != ItemTypes.derrick)
				{
					Ship.Modules[ScreenState - 1].ItemCount = 1;
					ResourceList.Stores[itemType]--;
				}
				else
				{
					Ship.Modules[ScreenState - 1].ItemCount = ResourceList.Stores[itemType];
					ResourceList.Stores[itemType] = 0;
				}
			}

			UpdateState();
			UpdateEquipmentStock();
		}

		#endregion

		#region CargoService

		private void UpdateCargoService()
		{
			if (Ship.Modules[ScreenState - 1].ItemCount > 0)
				GetNode<Label>("CargoService/Labels/MineralName" + Ship.Modules[ScreenState - 1].ItemStored.ToScreenString()).AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);

			foreach (var mineral in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.ItemCategory == ItemCategory.resource))
				GetNode<Label>("CargoService/Labels/MineralCount" + mineral.ItemType.ToScreenString()).Text = ResourceList.Stores[mineral.ItemType].ToString();
		}

		private void SelectMineral(ItemTypes itemType)
		{
			if (Ship.Modules[ScreenState - 1].ItemCount > 0)
			{
				ResourceList.Stores[Ship.Modules[ScreenState - 1].ItemStored] += Ship.Modules[ScreenState - 1].ItemCount;
				GetNode<Label>("CargoService/Labels/MineralName" + Ship.Modules[ScreenState - 1].ItemStored.ToScreenString()).RemoveThemeColorOverride("font_color");
			}

			if (Ship.Modules[ScreenState - 1].ItemStored == itemType)
			{
				Ship.Modules[ScreenState - 1].ItemStored = ItemTypes.none;
				Ship.Modules[ScreenState - 1].ItemCount = 0;
			}
			else if (Ship.Modules[ScreenState - 1].ItemStored != itemType && ResourceList.Stores[itemType] > 0)
			{
				Ship.Modules[ScreenState - 1].ItemStored = itemType;
				Ship.Modules[ScreenState - 1].ItemCount = ResourceList.Stores[itemType] >= 250 ? 250 : ResourceList.Stores[itemType];

				ResourceList.Stores[itemType] = Math.Max(0, ResourceList.Stores[itemType] - 250);
			}

			TorsoInstances[ScreenState - 1].UpdateState();

			UpdateCargoService();
		}

		#endregion

		private void RefreshButtons()
		{
			Nav_Cockpit.TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Nav_Cockpit_" + (ScreenState == 0 ? "On" : "Off") + ".png");
			Nav_Torsos[0].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Nav_Torso_" + (ScreenState == 1 ? "On" : "Off") + ".png");
			Nav_Torsos[1].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Nav_Torso_" + (ScreenState == 2 ? "On" : "Off") + ".png");
			Nav_Torsos[2].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Nav_Torso_" + (ScreenState == 3 ? "On" : "Off") + ".png");
			Nav_Torsos[3].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Nav_Torso_" + (ScreenState == 4 ? "On" : "Off") + ".png");
			Nav_Torsos[4].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Nav_Torso_" + (ScreenState == 5 ? "On" : "Off") + ".png");
			Nav_Torsos[5].TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Nav_Torso_" + (ScreenState == 6 ? "On" : "Off") + ".png");
			Nav_Engine.TextureNormal = SpriteManager.LoadImage(NavSpriteBasePath + "Nav_Engine_" + (ScreenState == 7 ? "On" : "Off") + ".png");
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Right && mb.Pressed)
			{
				var cursor = GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView");

				if (cursor.IsLocked)
				{
					if (CargoService.Visible == true)
						CargoService.Visible = false;
					else if (EquipmentStock.Visible == true)
						EquipmentStock.Visible = false;
					else if (StaffList.Visible == true)
						StaffList.Visible = false;

					cursor.Unlock();

					GetViewport().SetInputAsHandled();
				}
			}
		}

		//Triggered from gamecore
		protected override void DayTick(uint previousDay, uint currentDay)
		{
			UpdateState();
		}
	}
}
