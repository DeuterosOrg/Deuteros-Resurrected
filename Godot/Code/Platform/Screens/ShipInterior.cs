using Deuteros.Code.Platform.Base;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Objects.Interfaces;
using static Deuteros.Code.Enums;
using Deuteros.Code.Utility;
using Deuteros.Code.Platform.Screens.ModuleScenes;
using Deuteros.Code.Objects.ModuleTextFrame;
using System.Threading.Tasks;
using Deuteros.Code.Objects.GameData;
using System.Threading;

namespace Deuteros.Code.Platform.Screens
{
	public partial class ShipInterior : BaseSubScene
	{
		public const string SpriteBasePath = "res://Sprites//SceneSprites//Ships//Interior//";

		public IShip Ship { get; set; }
		public IPlanet CurrentPlanet { get; set; }
		private bool sceneReady;
        private bool arrivalTimeSkip;

		Control TextLayout { get; set; }
		Control StarMap { get; set; }
		Control Window { get; set; }
		Control ACC { get; set; }
		Control GrappleHolder { get; set; }
		Control AMAHolder { get; set; }

		TextureRect LandingBlank { get; set; }
		TextureRect EngineControls { get; set; }
		TextureRect BigLocation { get; set; }

		TextureButton SmallLocation { get; set; }
		TextureButton OpenACC { get; set; }
		TextureButton SetCourse { get; set; }
		TextureButton[] Modules { get; set; } = new TextureButton[6];

		Button EngageEngine { get; set; }
		Button DisengageEngine { get; set; }
		Button Dock { get; set; }
		Button TakeOff { get; set; }
		Button Land { get; set; }

		Label ShipName { get; set; }
		Label Status { get; set; }
		Label FuelValue { get; set; }
		Label PilotName { get; set; }
		Label PilotCount { get; set; }
		Label EngineStatusValue { get; set; }
		Label ACCStatus { get; set; }
		Label[] CargoValues { get; set; } = new Label[6];
		Label CourseText { get; set; }
		Label CourseValue { get; set; }
		Label ETA { get; set; }

		StarMap DestinationStarMap { get; set; }
		ModuleTextFrame ModuleTextFrame { get; set; }
		ACC ACCScreen { get; set; }
		Grapple GrappleScreen { get; set; }
		AMA AMAScreen { get; set; }
		FleetTransfers FleetTransfers { get; set; }
		//MethanoidTextFrame MethanoidTextFrame { get; set; }

		public override void _Ready()
		{
			//Setup some flags to make our lives easier
			CurrentPlanet = GameCore.SingletonInstance.GetCurrentPlanet();

			Ship = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Single(T => T.ShipID == GameCore.SingletonInstance.ShipSelected);

			TextLayout = GetNode<Control>("TextLayout");
			StarMap = GetNode<Control>("StarMap");
			Window = GetNode<Control>("Window");
			ACC = GetNode<Control>("ACCScreen");
			GrappleHolder = GetNode<Control>("GrappleHolder");
			AMAHolder = GetNode<Control>("AMAHolder");

			LandingBlank = GetNode<TextureRect>("LandingBlank");
			EngineControls = GetNode<TextureRect>("EngineControls");
			BigLocation = GetNode<TextureRect>("Location/BigLocation");

			OpenACC = GetNode<TextureButton>("OpenACC");
			SetCourse = GetNode<TextureButton>("SetCourse");
			SmallLocation = GetNode<TextureButton>("Location/SmallLocation");

			for (int i = 0; i < 6; i++)
			{
				Modules[i] = GetNode<TextureButton>("Modules/" + i.ToString().PadLeft(2, '0'));
				var modulePressed = i;
				Modules[i].Pressed += () => ShipInterior_Pressed(modulePressed);
				Modules[i].Visible = false;
			}

			EngageEngine = GetNode<Button>("EngineControls/EngageEngine");
			DisengageEngine = GetNode<Button>("EngineControls/DisengageEngine");
			Dock = GetNode<Button>("Dock");
			TakeOff = GetNode<Button>("TakeOff");
			Land = GetNode<Button>("Land");

			GetNode<Button>("TextLayout/CargoActions").Pressed += SupplyPods_Pressed;
			ShipName = GetNode<Label>("TextLayout/ShipName");
			GetNode<Button>("TextLayout/RenameShip").Pressed += RenameShip_Pressed;
			Status = GetNode<Label>("TextLayout/Status");
			FuelValue = GetNode<Label>("TextLayout/FuelValue");
			PilotName = GetNode<Label>("TextLayout/PilotName");
			PilotCount = GetNode<Label>("TextLayout/PilotCount");
			EngineStatusValue = GetNode<Label>("TextLayout/EngineStatusValue");
			ACCStatus = GetNode<Label>("TextLayout/ACCStatus");
			for (int i = 0; i < CargoValues.Length; i++)
				CargoValues[i] = GetNode<Label>("TextLayout/CargoValue" + (i + 1));
			CourseText = GetNode<Label>("TextLayout/CourseText");
			CourseValue = GetNode<Label>("TextLayout/CourseValue");
			ETA = GetNode<Label>("TextLayout/ETA");

			OpenACC.Pressed += ACC_Pressed;
			SetCourse.Pressed += SetCourse_Pressed;
			EngageEngine.Pressed += EngageEngine_Pressed;

			DisengageEngine.Pressed += DisengageEngine_Pressed;
			SmallLocation.Pressed += SmallLocation_Pressed;
			Dock.Pressed += Dock_Pressed;
			TakeOff.Pressed += TakeOff_Pressed;
			Land.Pressed += Land_Pressed;
			GetNode<Button>("Service").Pressed += Service_Pressed;

			foreach (var cargoValue in CargoValues) cargoValue.Text = "";

			UpdateState();

			base._Ready();
			sceneReady = true;
		}

        public override void _Process(double delta)
        {
            if (sceneReady && arrivalTimeSkip != GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip)
                UpdateArrivalDate();
        }

        private void UpdateArrivalDate()
        {
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            arrivalTimeSkip = save.TimeSkip;
            // Projection follows the current mode, including controls and simulation-triggered stops.
            var projection = Ship is SCG scg ? scg.ProjectedArrival()
                : (Updates: Ship.TravelTimeRemain(), ArrivalOffset: (long?)InterstellarFlight.StarOffset(save.BaseGameData.Planets[Ship.PlanetLocation].ParentStar));
            var remaining = (long)Math.Max(0, projection.Updates);
            ETA.Text = projection.ArrivalOffset == null ? "ETA:\nUnavailable" : "ETA:\n" + GameClock.FormatAbsoluteDate(
                310000000L + (long)save.Clock.DateCentidays + projection.ArrivalOffset.Value + remaining * (arrivalTimeSkip ? 100 : 1));
        }

		private void tradeItems(Dictionary<ShipModule,Enums.ItemTypes> olditemlist, Dictionary<ShipModule, Enums.ItemTypes> newitemlist)
		{
			foreach(ShipModule m in olditemlist.Keys)
			{
				switch (olditemlist[m])
				{
					case ItemTypes.iron:
						newitemlist[m] = ItemTypes.silica;
						break;
					case ItemTypes.silica:
                        newitemlist[m] = ItemTypes.iron;
                        break;
                    case ItemTypes.paladium:
                        newitemlist[m] = ItemTypes.gold;
                        break;
                    case ItemTypes.gold:
                        newitemlist[m] = ItemTypes.paladium;
                        break;
                    case ItemTypes.platinum:
                        newitemlist[m] = ItemTypes.silver;
                        break;
                    case ItemTypes.silver:
                        newitemlist[m] = ItemTypes.platinum;
                        break;
                    case ItemTypes.hydrogen:
                        newitemlist[m] = ItemTypes.methane;
                        break;
                    case ItemTypes.methane:
                        newitemlist[m] = ItemTypes.hydrogen;
                        break;
                    case ItemTypes.helium:
                        newitemlist[m] = ItemTypes.deuterium;
                        break;
                    case ItemTypes.deuterium:
                        newitemlist[m] = ItemTypes.helium;
                        break;
                    case ItemTypes.copper:
                        newitemlist[m] = ItemTypes.titanium;
                        break;
                    case ItemTypes.titanium:
                        newitemlist[m] = ItemTypes.copper;
                        break;
                    case ItemTypes.carbon:
                        newitemlist[m] = ItemTypes.aluminium;
                        break;
                    case ItemTypes.aluminium:
                        newitemlist[m] = ItemTypes.carbon;
                        break;
					default:
                        newitemlist[m] = olditemlist[m];
						break;

                }

            }

		}

		private async Task HandleModulePress(int modulePressed)
		{
            if (RejectShipCommand() || modulePressed < 0 || modulePressed >= Ship.Modules.Count) return;
            var mounted = Ship.Modules[modulePressed];
            // The original item action precedes the remake's generic DFCC interception.
            if (Ship.ShipState != Ship_States.Docked && mounted.ModuleType == Module_Types.Tool
                && mounted.ItemStored == ItemTypes.alien_artifact)
            {
                if (Ship.ShipState != Ship_States.UnDocked) return;
                if (Ship.Pilot == null || Ship.Pilot.GetLevel() < 4)
                {
                    var warning = OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/OFPilotWarning.tscn"), false);
                    if (warning != null)
                    {
                        warning.GetNode<Label>("Window/Background/Number").Text = (modulePressed + 1).ToString();
                        warning.GetNode<Label>("Labels/ToolType").Text = "Unknown";
                        warning.GetNode<RichTextLabel>("Labels/WarningBody").Text = "A [color=yellow]Crew[/color] Is Required\nTo Operate This\nEquipment And Must\nBe Ranked [color=yellow]Warlord[/color]";
                        warning.GetNode<Button>("Dismiss").Pressed += OverlayManager.Instance.CloseOverlay;
                    }
                }
                else OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://PreFabs/Ending.tscn"), false);
                return;
            }
			if (Ship.ShipState == Ship_States.Docked && Ship.PlanetLocation != StellarBodies.asteroids)
			{
				var sceneVariables = new List<SceneVariables>();
				var newScene = Enums.Scenes.ShipBay;

				if (Ship.ShipType == Ship_Types.Shuttle && ((Shuttle)Ship).OnGround)
				{
					if (Ship.Modules[modulePressed].ItemStored == ItemTypes.r_frame && CurrentPlanet.BaseBuildParts < 2 && Ship.Pilot?.Count > 0 && Ship.ShipType == Ship_Types.Shuttle && ((Shuttle)Ship).OnGround)
					{
						if (Ship.Pilot != null) Ship.Pilot.AddAction();

						CurrentPlanet.BaseBuildParts++;

						if (CurrentPlanet.BaseBuildParts == 2)
							await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.RFrame_Deploy_Complete], new List<string>() { CurrentPlanet.BaseBuildParts.ToString() }, (modulePressed + 1));
						else
							await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.RFrame_Deploy], new List<string>() { CurrentPlanet.BaseBuildParts.ToString() }, (modulePressed + 1));

						Ship.Modules[modulePressed].ItemStored = ItemTypes.none;
						Ship.Modules[modulePressed].ItemCount = 0;

						GameCore.SingletonInstance.ShipSelected = Ship.ShipID;

						UpdateState();

						newScene = Enums.Scenes.ShipInterior;
					}

					if (Ship.Modules[modulePressed].ItemStored == ItemTypes.bandaid && CurrentPlanet.BaseDamaged && Ship.Pilot?.Count > 0 && Ship.ShipType == Ship_Types.Shuttle && ((Shuttle)Ship).OnGround)
					{
						((Shuttle)Ship).ShipState = Ship_States.CrewRepairing;
						((Shuttle)Ship).StartRepairDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
						UpdateState();
						GameCore.SingletonInstance.ShipSelected = Ship.ShipID;

						newScene = Enums.Scenes.ShipInterior;
					}

					if (newScene == Scenes.ShipBay)
					{
						CurrentPlanet.ShuttleState = modulePressed + 1;

						sceneVariables.Add(Enums.SceneVariables.Ground);
						sceneVariables.Add(Enums.SceneVariables.Shuttle);
					}
				}
				else if (Ship.ShipType == Ship_Types.Shuttle)
				{
					CurrentPlanet.Station.ShuttleState = modulePressed + 1;
					sceneVariables.Add(Enums.SceneVariables.Orbit);
					sceneVariables.Add(Enums.SceneVariables.Shuttle);
				}
				else if (Ship.ShipType != Ship_Types.Shuttle)
				{
					if (CurrentPlanet.ActiveMethanoid)
					{
						if (GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar) return;
						if (Ship.Modules.Any<ShipModule>(m => m.ItemStored == ItemTypes.commspod))
						{
							if (GameCore.SingletonInstance.GameData.ActiveSaveFile.MethanoidTradeCount >= 16)
							{
                                await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_War_Warning], new List<string>(), (modulePressed + 1));
								var commpodModule = Ship.Modules.First<ShipModule>(m => m.ItemStored == ItemTypes.commspod);
								commpodModule.ItemStored = ItemTypes.grapple;
                                commpodModule.HeldItem = new UnknownItem(UnknownItemTypes.Blazer);
                                GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar = true;
                                GameCore.SingletonInstance.GameData.ActiveSaveFile.WarDeclaredDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
                                GameCore.SingletonInstance.GameData.ActiveSaveFile.AlienTransmissions.Start();
                            }
                            else
							{
								var itemlist = new Dictionary<ShipModule,Enums.ItemTypes>();
                                var newitemlist = new Dictionary<ShipModule, Enums.ItemTypes>();
                                var offeredCounts = new Dictionary<ShipModule, int>();

                                // Original $7BE6C scans three positions, including non-supply pods.
                                foreach (ShipModule m in Ship.Modules.Take(3))
								{
									if (m.ModuleType == Module_Types.Supply && m.ItemCount > 0
										&& m.ItemStored >= ItemTypes.iron && m.ItemStored <= ItemTypes.hed_fuel)
									{
										itemlist[m] = m.ItemStored;
										newitemlist[m] = m.ItemStored;
                                        offeredCounts[m] = m.ItemCount;
                                    }
                                }

								//calculate list of new items
								this.tradeItems(itemlist, newitemlist);

								if (itemlist.Count == 0)
								{
									await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_No_Cargo], new List<string>(), (modulePressed + 1));

								}
								else
								{
                                    await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_TradeQuestion], new List<string>(), (modulePressed + 1));

                                    var decision = await AskTradeDecision(itemlist, newitemlist, offeredCounts);
                                    if (decision == null || !IsInsideTree() || IsQueuedForDeletion()) return;

                                    if (!decision.Value)
                                    {
                                        var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
                                        save.MethanoidTradeCount = Math.Max(0, save.MethanoidTradeCount - 1);
                                        await ShowModuleTextFrame(save.BaseGameData.ModuleFrameTexts[ModuleFrameText.Methanoid_No_Trade], new List<string>(), modulePressed + 1);
                                    }
                                    else
                                    {

                                        List<string> p = new List<string>
                                        {
                                            newitemlist.Values.First().ToScreenString(),
                                            itemlist.Values.First().ToScreenString()
                                        };

                                        // Commit together before response playback can be interrupted.
                                        foreach (var module in newitemlist.Keys) module.ItemStored = newitemlist[module];
                                        Ship.Fuel = 250; // Original accepted-trade gift, in gauge units.
                                        GameCore.SingletonInstance.GameData.ActiveSaveFile.MethanoidTradeCount++;
                                        var response = ModuleFrameText.Methanoid_Trade1;
                                        if (itemlist.Count > 1)
                                        {
                                            p.Add(newitemlist.Values.ElementAt(1).ToScreenString());
                                            p.Add(itemlist.Values.ElementAt(1).ToScreenString());
                                            response = ModuleFrameText.Methanoid_Trade2;
                                        }
                                        await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[response], p, modulePressed + 1);
                                    }

								}
                            }
                        }
                        else
						{
							if (GameCore.SingletonInstance.GameData.GetItem(ItemTypes.commspod).Locked)
							{
								var emptyGrapple = Ship.Modules.FirstOrDefault(m => m.ItemStored == ItemTypes.grapple && m.HeldItem == null);
								if (emptyGrapple != null)
								{
									await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_Intro_With_Grapple], new List<string>(), (modulePressed + 1));
									emptyGrapple.HeldItem = new UnknownItem(UnknownItemTypes.CommsPod);
								}
								else
									await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_Intro], new List<string>(), (modulePressed + 1));
							}
							else
							{
                                await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_Intro], new List<string>(), (modulePressed + 1));
                            }

                        }
                        Ship.TakeOff();

                        UpdateState();
                        GameCore.SingletonInstance.ShipSelected = Ship.ShipID;
                        newScene = Scenes.ShipInterior;
                    }

                    GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet = Ship.PlanetLocation;

					CurrentPlanet.Station.StarShipState = modulePressed + 1;
					sceneVariables.Add(Enums.SceneVariables.Orbit);
					sceneVariables.Add(Enums.SceneVariables.Ship);
				}

				//Underscores in scene names represent a folder
				if (newScene != Scenes.None)
					Deuteros.Code.GameCore.SingletonInstance.ChangeScene(newScene, sceneVariables);
			}
			else if (Ship.ShipState != Ship_States.Docked)
			{
				if ((Ship.ShipState == Ship_States.UnDocked && Ship.ShipType != Ship_Types.Shuttle && ((InterStellarShip)Ship).DFCC))
				{
					if (GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar &&
						(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].ActiveMethanoid ||
							GameCore.SingletonInstance.GameData.PlanetUnderAttack(Ship.PlanetLocation)))
					{

                        var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
                        var battlePlanet = save.BaseGameData.Planets[Ship.PlanetLocation];
                        var stationBattle = battlePlanet.ActiveMethanoid;
                        EnemyFleet enemyShip;
                        if (stationBattle)
						{
							enemyShip = new EnemyFleet();

							//pull upto 200 drones from the planet store
							enemyShip.DroneCount = Math.Min(200, GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].Station.Resources.Stores[ItemTypes.ios_drone]);
							GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].Station.Resources.Stores[ItemTypes.ios_drone] -= enemyShip.DroneCount;
						}
						else
						{
							enemyShip = (EnemyFleet)GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.FirstOrDefault(s => s.PlanetLocation == Ship.PlanetLocation && s.ShipType != Ship_Types.Shuttle && ((InterStellarShip)s).MethanoidOwned);
						}

                        void FinishBattle()
                        {
                            if (!ReferenceEquals(save, GameCore.SingletonInstance.GameData.ActiveSaveFile)) return;
                            if (!stationBattle && !enemyShip.Attacking) enemyShip.CancelAttack(Ship.PlanetLocation);
                            if (((InterStellarShip)Ship).DroneCount == 0 && save.Ships.Remove(Ship)) save.News.AddShipLoss(Ship);
                            if (IsInstanceValid(this) && IsInsideTree() && !IsQueuedForDeletion()
                                && GameCore.SingletonInstance.currentScene == Scenes.ShipInterior)
                                DayTick(save.CurrentDay, save.CurrentDay);
                        }
                        await ShowBattleFrame((InterStellarShip)Ship, enemyShip, stationBattle ? battlePlanet : null, FinishBattle);
                        if (!IsInstanceValid(this) || !IsInsideTree() || IsQueuedForDeletion()
                            || !ReferenceEquals(save, GameCore.SingletonInstance.GameData.ActiveSaveFile)
                            || !save.Ships.Contains(Ship)) return;

						UpdateState();
					}
					else if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].ActiveMethanoid)
					{
						ShowDroneTrasferFrame((InterStellarShip)Ship);
						UpdateState();
					}
				}
				else if (Ship.Modules[modulePressed].ModuleType == Module_Types.Tool)
				{

					if ((Ship.ShipType != Ship_Types.Shuttle || (Ship.ShipType == Ship_Types.Shuttle && !((Shuttle)Ship).OnGround)) && Ship.ShipState == Ship_States.UnDocked  && Ship.Modules[modulePressed].ItemStored == ItemTypes.of_frame && CurrentPlanet.Station.Built == false && Ship.PlanetLocation!=StellarBodies.asteroids)
					{
						if (Ship.Pilot == null || Ship.Pilot.Count <= 0)
						{
							if (!OverlayManager.Instance.IsOpen)
							{
								var warning = OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/OFPilotWarning.tscn"), false);
								warning.GetNode<Label>("Window/Background/Number").Text = (modulePressed + 1).ToString();
								warning.GetNode<Button>("Dismiss").Pressed += OverlayManager.Instance.CloseOverlay;
							}
							return;
						}

						Ship.Pilot.AddAction();

						if (CurrentPlanet.Station.BuildParts == 0)
                        {
                            FuelRefining.ClaimPlayerSlot(GameCore.SingletonInstance.GameData.ActiveSaveFile, CurrentPlanet);
							CurrentPlanet.Station.StationOrdinal = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Where(p => !p.ActiveMethanoid && p.Station != null).MaxBy(p => p.Station.StationOrdinal).Station.StationOrdinal + 1;
                        }

						CurrentPlanet.Station.BuildParts++;

						if (CurrentPlanet.Station.BuildParts == 8)
							await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Station_Deploy_Complete], new List<string>() { CurrentPlanet.Station.BuildParts.ToString() }, (modulePressed + 1));
						else
							await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Station_Deploy], new List<string>() { CurrentPlanet.Station.BuildParts.ToString() }, (modulePressed + 1));

						Ship.Modules[modulePressed].ItemStored = ItemTypes.none;
						Ship.Modules[modulePressed].ItemCount = 0;

						if (CurrentPlanet.Station.BuildParts == 8)
							CurrentPlanet.Station.Built = true;

						GameCore.SingletonInstance.TriggerStationPiecePlaced(CurrentPlanet.PlanetId);

						//6 stations completed means war
						if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar &&
							GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Count(p => p.Station.Built && !p.ActiveMethanoid) == 6)
						{
							await ShowMethanoidTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_DeclareWar], new List<string>());

							GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar = true;
							GameCore.SingletonInstance.GameData.ActiveSaveFile.WarDeclaredDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
                                GameCore.SingletonInstance.GameData.ActiveSaveFile.AlienTransmissions.Start();
						}

						UpdateState();
					}
					else if (Ship.Modules[modulePressed].ItemStored == ItemTypes.grapple)
					{
						GrappleScreen = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/Grapple.tscn").Instantiate<Grapple>();

						GrappleHolder.AddChild(GrappleScreen);

						GrappleScreen.Load((InterStellarShip)Ship, Ship.Modules[modulePressed]);

						UpdateState();
					}
					else if (Ship.Modules[modulePressed].ItemStored == ItemTypes.a__m__a)
					{
						AMAScreen = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/AMA.tscn").Instantiate<AMA>();

						AMAHolder.AddChild(AMAScreen);

						AMAScreen.Load((InterStellarShip)Ship, Ship.Modules[modulePressed]);

						AMAScreen.CloseWindow = CloseAMA;

						UpdateState();
					}
				}
			}
		}

        private async Task ShowBattleFrame(InterStellarShip player, EnemyFleet enemy, IPlanet stationOwner, Action onCompleted)
        {
            var frame = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/Battle.tscn").Instantiate<Battle>();
            Window.AddChild(frame);
            var battle = frame.DoBattle(player, enemy, onCompleted);
            if (stationOwner != null)
            {
                var station = stationOwner.Station;
                // DoBattle registers its result settlement first. Return the reservation on normal or cancelled exit.
                frame.TreeExiting += () =>
                {
                    if (stationOwner.ActiveMethanoid && ReferenceEquals(stationOwner.Station, station))
                        station.Resources.Stores[ItemTypes.ios_drone] += enemy.DroneCount;
                };
            }
            try { await battle; }
            finally { if (IsInstanceValid(frame)) frame.QueueFree(); }
        }

		private void ShowDroneTrasferFrame(InterStellarShip player)
		{
			FleetTransfers = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/FleetTransfers.tscn").Instantiate<FleetTransfers>();

			Window.AddChild(FleetTransfers);

			FleetTransfers.TransferDrones(player);

		}

		private async Task ShowModuleTextFrame(TextFrame newTextFrame, List<string> dynamicProperties, int windowNumber)
		{
			var core = GameCore.SingletonInstance;
			GameCore.LockScreen();
			var frame = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/ModuleTextFrame.tscn").Instantiate<ModuleTextFrame>();
			ModuleTextFrame = frame;
			Window.AddChild(frame);
			try
			{
				await frame.PlayText(newTextFrame, dynamicProperties, windowNumber);
			}
			finally
			{
				if (IsInstanceValid(frame)) frame.QueueFree();
				if (ModuleTextFrame == frame) ModuleTextFrame = null;
				if (IsInstanceValid(core) && core.IsInsideTree()) GameCore.UnLockScreen();
			}
		}

		private async Task ShowMethanoidTextFrame(TextFrame newTextFrame, List<string> dynamicProperties)
		{
			//for now we use the existing text frame
			await ShowModuleTextFrame(newTextFrame, dynamicProperties, 1);

			/*
			GameCore.LockScreen();
			MethanoidTextFrame = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/MethanoidTextFrame.tscn").Instantiate<MethanoidTextFrame>();
			Window.AddChild(MethanoidTextFrame);

			await MethanoidTextFrame.PlayText(newTextFrame, dynamicProperties);

			Window.RemoveChild(MethanoidTextFrame);
			MethanoidTextFrame = null;

			GameCore.UnLockScreen();
			*/
		}

        private bool CanService
        {
            get
            {
                if (Ship.ShipState != Ship_States.Docked || Ship.PlanetLocation == StellarBodies.asteroids) return false;
                var planet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation];
                return !planet.ActiveMethanoid && (Ship is Shuttle { OnGround: true }
                    ? planet.PlanetId == StellarBodies.earth || planet.BaseBuildParts == 2 : planet.Station.Built);
            }
        }

        private void Service_Pressed()
        {
            if (RejectShipCommand() || !CanService) return;
            var core = GameCore.SingletonInstance;
            if (GlobalInput.UiLocked || core.GetNode<InputBlocker>("InputBlocker").Blocked
                || core.GetNode<GlobalInput>("VirtualCursorView").IsLocked) return;
            var ground = Ship is Shuttle { OnGround: true };
            core.GameData.ActiveSaveFile.CurrentPlanet = Ship.PlanetLocation;
            core.ShipSelected = Ship.ShipID;
            if (ground) CurrentPlanet.ShuttleState = 0;
            else if (Ship is Shuttle) CurrentPlanet.Station.ShuttleState = 0;
            else CurrentPlanet.Station.StarShipState = 0;
            core.ChangeScene(Scenes.ShipBay, new List<SceneVariables>
            {
                ground ? Enums.SceneVariables.Ground : Enums.SceneVariables.Orbit,
                Ship is Shuttle ? Enums.SceneVariables.Shuttle : Enums.SceneVariables.Ship
            });
        }

        private bool RejectShipCommand(Node owningOverlay = null)
        {
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            if (!IsInsideTree() || IsQueuedForDeletion()) return true;
            if (OverlayManager.Instance.IsOpen && !OverlayManager.Instance.IsShowing(owningOverlay)) return true;
            return save.RogueCrew.RejectCommand(save, Ship);
        }

		private void RenameShip_Pressed()
		{
            if (RejectShipCommand()) return;
			var core = GameCore.SingletonInstance;
			if (GlobalInput.UiLocked || core.GetNode<InputBlocker>("InputBlocker").Blocked ||
				core.GetNode<GlobalInput>("VirtualCursorView").IsLocked || OverlayManager.Instance.IsOpen)
				return;

			var dialog = OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Base/RenameShip.tscn"));
			var nameEdit = dialog.GetNode<LineEdit>("NameEdit");
			var validation = dialog.GetNode<Label>("Validation");
			// Validate on submission rather than truncating names from existing saves when opening.
			nameEdit.Text = Ship.Name;
			nameEdit.GrabFocus();
			nameEdit.SelectAll();
			GameCore.HoverText = "";

			void ConfirmName()
			{
                if (RejectShipCommand(dialog)) return;
				var name = nameEdit.Text.Trim();
				if (name.Length == 0 || name.Length > 24 || name.Any(char.IsControl))
				{
					validation.Text = "Use 1 to 24 characters.";
					nameEdit.GrabFocus();
					return;
				}
				Ship.Name = name;
				UpdateState();
				OverlayManager.Instance.CloseOverlay();
			}

			dialog.GetNode<Button>("Confirm").Pressed += ConfirmName;
			nameEdit.TextSubmitted += _ => ConfirmName();
			dialog.GetNode<Button>("Cancel").Pressed += OverlayManager.Instance.CloseOverlay;
		}

		private void Land_Pressed()
		{
            if (RejectShipCommand()) return;
			Ship.Land();
			UpdateState();
		}

		private void TakeOff_Pressed()
		{
            if (RejectShipCommand()) return;
			Ship.TakeOff();
			UpdateState();
		}

		private void Dock_Pressed()
		{
            if (RejectShipCommand()) return;
			if (Ship.PlanetLocation == StellarBodies.asteroids)
				return;
			if (Ship.ShipType == Ship_Types.Shuttle || ((InterStellarShip)Ship).AttackedCount == 0)
			{

				Ship.Dock();
				UpdateState();
			}
		}

		private void SmallLocation_Pressed()
		{
            if (RejectShipCommand()) return;
			Ship.LocationView = !Ship.LocationView;

			UpdateState();
		}

		private void DisengageEngine_Pressed()
		{
            if (RejectShipCommand()) return;
			Ship.DisengageEngine();
			UpdateState();
		}

		private void EngageEngine_Pressed()
		{
            if (RejectShipCommand()) return;
			if (Ship.EngageEngine())
			{
				CurrentPlanet = null;
            }

            UpdateState();
		}

        private bool CanSetCourse => Ship is not SCG { Flight: not null }
            && RogueCrew.CanCommand(Ship);

		private void SetCourse_Pressed()
		{
            if (RejectShipCommand()) return;
            if (!CanSetCourse) return;
			DestinationStarMap = GD.Load<PackedScene>("res://PreFabs/StarMap.tscn").Instantiate<StarMap>();
			DestinationStarMap.ShowResources = false;

			StarMap.AddChild(DestinationStarMap);

			DestinationStarMap.LoadMap(Ship.DestinationPlanetLocation);

			var cursor = GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView");
			cursor.LockToRect(StarMap.GetGlobalRect());

			UpdateState();
		}

		private void ACC_Pressed()
		{
            if (RejectShipCommand()) return;
			ACCScreen = GD.Load<PackedScene>("res://PreFabs/ACC.tscn").Instantiate<ACC>();
			ACCScreen.SetACC(Ship.ACC);
			ACCScreen.CloseWindow = CloseACC;

			ACC.AddChild(ACCScreen);

			ACCScreen.UpdateState();

			var cursor = GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView");
			cursor.LockToRect(ACC.GetGlobalRect());

			UpdateState();
		}

		internal void UpdateState()
		{
			if (Ship.ShipState != Ship_States.InTransit)
				CurrentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation];

			ShipName.Text = Ship.Name;
			GetNode<Button>("TextLayout/RenameShip").TooltipText = Ship.Name;

			if (Ship.GetType() != typeof(Shuttle) && Ship.ShipState == Ship_States.UnDocked && GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar &&
				(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].ActiveMethanoid ||
				GameCore.SingletonInstance.GameData.PlanetUnderAttack(Ship.PlanetLocation)))
				Status.Text = "UNDER ATTACK !\n" + Ship.PlanetLocation.ToScreenString(" ");

			else if (Ship.GetType() == typeof(Shuttle) && Ship.ShipState == Ship_States.CrewRepairing)
				Status.Text = "Repairing On\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.GetType() == typeof(Shuttle) && Ship.ShipState == Ship_States.Landing)
				Status.Text = "Landing On\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.GetType() == typeof(Shuttle) && Ship.ShipState == Ship_States.TakingOff)
				Status.Text = "Climbing From\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.GetType() == typeof(Shuttle) && ((Shuttle)Ship).OnGround)
				if (Ship.ACC != null && (Ship.ACC.Active || Ship.ACC.CycleMode) && Ship.ACC.Refuelling)
					Status.Text = "Refueling at\n" + Ship.PlanetLocation.ToScreenString(" ");
				else
					Status.Text = "In Ground Bay\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.ShipState == Ship_States.Launching)
				Status.Text = "Launching\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.ShipState == Ship_States.Docked)
				if (Ship.ACC != null && (Ship.ACC.Active || Ship.ACC.CycleMode) && Ship.ACC.Refuelling)
					Status.Text = "Refueling at\n" + Ship.PlanetLocation.ToScreenString(" ");
				else
					Status.Text = "Docked Above\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship is SCG { Flight.Leg: InterstellarFlight.FlightLeg.Stranded })
                Status.Text = "Stranded At\n" + Ship.DestinationStarLocation.ToScreenString(" ");
            else if (Ship is SCG { Flight.Leg: InterstellarFlight.FlightLeg.Local })
                Status.Text = (Ship.EngineEngaged ? "Approaching\n" : "Drifting To\n") + Ship.DestinationPlanetLocation.ToScreenString(" ");
            else if (Ship.ShipState == Ship_States.InTransit)
				if (Ship.EngineEngaged)
					Status.Text = "In Transit To\n" + Ship.DestinationPlanetLocation.ToScreenString(" ");
				else
					Status.Text = "Drifting To\n" + Ship.DestinationPlanetLocation.ToScreenString(" ");
			else if (Ship.ShipState == Ship_States.Docking)
				Status.Text = "Docking With\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship is InterStellarShip && Ship.PlanetLocation == StellarBodies.asteroids)
                Status.Text = (Ship.FallingCount > 0 ? "Stranded At\n" : "Scanning\n") + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.Fuel == 0)
				Status.Text = "Falling To\n" + Ship.PlanetLocation.ToScreenString(" ");
			else
				Status.Text = "Orbitting\n" + Ship.PlanetLocation.ToScreenString(" ");

			FuelValue.Text = Ship.Fuel.ToString();
			if (Ship.Fuel > 0)
				FuelValue.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Yellow);
			else
				FuelValue.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);

			PilotName.Text = Ship.Pilot == null ? "None" : Ship.Pilot.GetLevelString() + "\n" + Ship.Pilot.Leader;
			PilotCount.Text = Ship.Pilot == null ? "" : Ship.Pilot.Count.ToString();

			EngineStatusValue.RemoveThemeColorOverride("font_color");

			if (!Ship.Engine)
			{
				EngineStatusValue.Text = "Not Installed";
				EngineStatusValue.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);
				EngageEngine.Visible = false;
				DisengageEngine.Visible = false;
			}
			else if (Ship.Engine)
			{
				EngageEngine.Visible = true;
				DisengageEngine.Visible = true;

                if (Ship.EngineDamaged)
                {
                    EngineStatusValue.Text = "Damaged !";
                    EngineStatusValue.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);
                }
                else if (Ship.ShipState is Ship_States.Docking or Ship_States.Launching or Ship_States.Landing or Ship_States.TakingOff
                    || (Ship.ShipState == Ship_States.InTransit && Ship.EngineEngaged))
				{
					EngineStatusValue.Text = "Engaged";
					EngineStatusValue.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Green);
				}
				else
				{
					EngineStatusValue.Text = "Disengaged";
					EngineStatusValue.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);
				}
			}

			if (Ship.ACC != null)
			{
				OpenACC.Visible = true;

				if (Ship.ACC.CycleMode)
					ACCStatus.Text = "A.C.C Is \nFinishing";
				else if (Ship.ACC.Active)
					ACCStatus.Text = "A.C.C Is \nEngaged";
				else if (!Ship.ACC.Active)
					ACCStatus.Text = "A.C.C Is \nDisengaged";
			}
			else
			{
				OpenACC.Visible = false;

				ACCStatus.Text = "";
			}

			for (int i = 0; i < Ship.Modules.Count(); i++)
			{
				if (Ship.Modules[i].ModuleType == Module_Types.None)
				{
					CargoValues[i].AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Green);
					CargoValues[i].Text = "free";
					Modules[i].Visible = false;
				}
				else if (Ship.Modules[i].ModuleType == Module_Types.Supply)
				{
					CargoValues[i].AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Beige);
					if (Ship.Modules[i].ItemCount > 0)
						CargoValues[i].Text = Ship.Modules[i].ItemCount + " " + Ship.Modules[i].ItemStored.ToScreenString(" ");
					else
						CargoValues[i].Text = "Empty";

					Modules[i].Visible = true;
					Modules[i].TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Module_Supply.png");
				}
				else if (Ship.Modules[i].ModuleType == Module_Types.Tool)
				{
					CargoValues[i].AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Green);

					if (Ship.Modules[i].ItemStored != ItemTypes.none)
						CargoValues[i].Text = Ship.Modules[i].ItemStored.ToScreenString(" ");
					else
						CargoValues[i].Text = "Empty";

					Modules[i].Visible = true;
					Modules[i].TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Module_Tool.png");
				}
				else if (Ship.Modules[i].ModuleType == Module_Types.Cryo)
				{
					CargoValues[i].AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Yellow);

					if (Ship.Modules[i].StaffStored != null)
						CargoValues[i].Text = Ship.Modules[i].StaffStored.GetTypeText2();
					else
						CargoValues[i].Text = "Empty";

					Modules[i].Visible = true;
					Modules[i].TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Module_Cryo.png");
				}
			}

			if (Ship.ShipType == Ship_Types.Shuttle)
			{
				CourseText.Text = "";
				CourseValue.Text = "";
			}
			else
			{
				CourseValue.Text = Ship.PlanetLocation.ToScreenString(" ") + " To\n" + Ship.DestinationPlanetLocation.ToScreenString(" ");
			}

            UpdateArrivalDate();

			if (Ship.ShipType == Ship_Types.Shuttle)
			{
				LandingBlank.Visible = false;
				EngineControls.Visible = false;
			}
			else
			{
				LandingBlank.Visible = true;
				EngineControls.Visible = true;
			}

			if (Ship.LocationView)
			{
				BigLocation.Visible = true;
				TextLayout.Visible = false;

				if (Ship.ShipState == Ship_States.Docked)
					BigLocation.Texture = SpriteManager.LoadImage(SpriteBasePath + "BigLocation_Docked.png");
				else if (Ship.ShipState == Ship_States.InTransit)
					BigLocation.Texture = SpriteManager.LoadImage(SpriteBasePath + "BigLocation_Travel.png");
				else if (Ship.ShipState == Ship_States.Launching)
					BigLocation.Texture = SpriteManager.LoadImage(SpriteBasePath + "BigLocation_StormDoors.png");
				else if (Ship.ShipState == Ship_States.UnDocked || Ship.ShipState == Ship_States.Docking)
					BigLocation.Texture = LocationTexture(false);
				else if (Ship.ShipState == Ship_States.TakingOff)
					BigLocation.Texture = null;
				else if (Ship.ShipState == Ship_States.Landing)
					BigLocation.Texture = null;
				else
					BigLocation.Texture = null;
			}
			else
			{
				BigLocation.Visible = false;
				TextLayout.Visible = true;

				if (Ship.ShipState == Ship_States.Docked)
					SmallLocation.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "SmallLocation_Docked.png");
				else if (Ship.ShipState == Ship_States.InTransit)
					SmallLocation.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "SmallLocation_Travel.png");
				else if (Ship.ShipState == Ship_States.Launching)
					SmallLocation.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "SmallLocation_StormDoors.png");
				else if (Ship.ShipState == Ship_States.TakingOff)
					SmallLocation.TextureNormal = null;
				else if (Ship.ShipState == Ship_States.Landing)
					SmallLocation.TextureNormal = null;
				else if (Ship.ShipState == Ship_States.UnDocked || Ship.ShipState == Ship_States.Docking)
				{
					SmallLocation.TextureNormal = LocationTexture(true);
				}
				else
					SmallLocation.TextureNormal = null;
			}

            SetCourse.Disabled = !CanSetCourse;
            var rogue = GameCore.SingletonInstance.GameData.ActiveSaveFile.RogueCrew.Controls(Ship);
            GetNode<Button>("Service").Disabled = rogue || !CanService;
            GetNode<Button>("TextLayout/RenameShip").Disabled = GetNode<Button>("TextLayout/CargoActions").Disabled = rogue;
            OpenACC.Disabled = EngageEngine.Disabled = Dock.Disabled = TakeOff.Disabled = Land.Disabled = rogue;
            foreach (var button in Modules) button.Disabled = rogue;
            DisengageEngine.Disabled = rogue || Ship is SCG { Flight: not null } flying && flying.Flight.Leg != InterstellarFlight.FlightLeg.Local;
			SetCourse.Visible = Ship.ShipType != Ship_Types.Shuttle;

			if (sceneReady)
				GameCore.SingletonInstance.UpdateMenuButtons(false, false);
		}


        private Texture2D LocationTexture(bool small)
        {
            var body = CurrentPlanet.IsMoon ? CurrentPlanet.MoonParentPlanetId : CurrentPlanet.PlanetId;
            if (!small && body == StellarBodies.asteroids)
                return SpriteManager.LoadImage("res://Sprites/Scenes/Ship_View_Asteroids.png");
            // Original $353F5 parent-group table and $410DC RGB4 rows; see the palette evidence.
            var palette = body switch
            {
                StellarBodies.mars or StellarBodies.cercops or StellarBodies.epsilon => (0x0500, 0x0800, 0x0A00),
                StellarBodies.uranus or StellarBodies.mycenae or StellarBodies.zargun => (0x0050, 0x0080, 0x00A0),
                StellarBodies.neptune or StellarBodies.atlantic or StellarBodies.cainozoic => (0x0005, 0x0008, 0x000A),
                StellarBodies.jupiter or StellarBodies.chiron or StellarBodies.tyre or StellarBodies.lithos
                    or StellarBodies.radius or StellarBodies.cambrian or StellarBodies.gamma => (0x0860, 0x0A80, 0x0CA0),
                StellarBodies.venus or StellarBodies.saturn or StellarBodies.jericho or StellarBodies.burah
                    or StellarBodies.titanes or StellarBodies.paleozoic => (0x0A80, 0x0CA0, 0x0FD0),
                StellarBodies.earth or StellarBodies.cerberus or StellarBodies.thebes or StellarBodies.mari
                    or StellarBodies.romulus or StellarBodies.remus or StellarBodies.sulfurum or StellarBodies.beta => (0x0048, 0x008A, 0x0AFF),
                StellarBodies.crete or StellarBodies.helios or StellarBodies.alpha or StellarBodies.zeta => (0x0FFF, 0x0684, 0x0462),
                StellarBodies.julius => (0x0ACE, 0x0468, 0x068A),
                _ => (0x0AAA, 0x0CCC, 0x0EEE)
            };
            var station = CurrentPlanet.Station.BuildParts > 0;
            var path = small
                ? SpriteBasePath + "SmallLocation_Planet_White" + (station ? "_Station" : "") + ".png"
                : "res://Sprites/Scenes/Ship_View_" + (station ? "Station" : "Planet") + ".png";
            var key = path + ":" + palette;
            if (SpriteManager.ImageCache.TryGetValue(key, out var cached)) return cached;
            // Headless Godot returns a shared image; mutate and dispose only our copy.
            using var image = (Image)SpriteManager.LoadImage(path).GetImage().Duplicate();
            var first = small ? "aaaaaa" : "004080";
            var second = small ? "cccccc" : "0080a0";
            var third = small ? "eeeeee" : "a0e0e0";
            for (var y = 0; y < image.GetHeight(); y++)
                for (var x = 0; x < image.GetWidth(); x++)
                {
                    var color = image.GetPixel(x, y);
                    var hex = color.ToHtml(false);
                    var rgb4 = hex == first ? palette.Item1 : hex == second ? palette.Item2 : hex == third ? palette.Item3 : -1;
                    if (rgb4 >= 0)
                        image.SetPixel(x, y, new Color(((rgb4 >> 8) & 15) / 15f, ((rgb4 >> 4) & 15) / 15f, (rgb4 & 15) / 15f, color.A));
                }
            var texture = ImageTexture.CreateFromImage(image);
            SpriteManager.ImageCache[key] = texture;
            return texture;
        }

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Right && mb.Pressed)
			{
				var cursor = GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView");

				if (cursor.IsLocked)
				{
					var courseRejected = false;
					//We were locked into the star map, let's read the new destination
					if (DestinationStarMap != null && DestinationStarMap.Visible == true)
					{
						DestinationStarMap.Visible = false;

						// Closing a system or galaxy view cancels selection and preserves the course.
						if (RogueCrew.CanCommand(Ship) && GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.TryGetValue(DestinationStarMap.CurrentLocation, out var newDestination))
						{
							if (!CanSetCourse || !Ship.CanTravelTo(newDestination.PlanetId))
								courseRejected = true;
							else
							{
								// On the return leg, update the ACC source instead of its destination.
								if (Ship.ACC != null)
								{
									if (Ship.ACC.Destination == Ship.PlanetLocation && Ship.ACC.Source != Ship.ACC.Destination)
										Ship.ACC.Source = newDestination.PlanetId;
									else
										Ship.ACC.Destination = newDestination.PlanetId;
								}

								Ship.DestinationPlanetLocation = newDestination.PlanetId;
								Ship.DestinationStarLocation = newDestination.ParentStar;
							}
						}

						StarMap.RemoveChild(DestinationStarMap);
						DestinationStarMap.QueueFree();
						DestinationStarMap = null;

					} 
					//We has the ACC open - No need to do anything, just close it
					else if (ACCScreen != null && ACCScreen.Visible == true)
					{
						CloseACC();
					}

					cursor.Unlock();

					GetViewport().SetInputAsHandled();

					UpdateState();
					if (courseRejected) GameCore.ShowError(this, "Interstellar\ntravel needs\nan SCG.");
				}
				//Catch right-clicks for windows without cursor lock
				else
				{
					//We has the Grapple open - No need to do anything, just close it
					if (GrappleScreen != null && GrappleScreen.Visible == true)
					{
						GrappleHolder.RemoveChild(GrappleScreen);

						GrappleScreen.QueueFree();
						GrappleScreen.Visible = false;
						GrappleScreen = null;

						GetViewport().SetInputAsHandled();
					}
					//We has the AMAopen - No need to do anything, just close it
					else if (AMAScreen != null && AMAScreen.Visible == true)
					{
						AMAHolder.RemoveChild(AMAScreen);

						AMAScreen.QueueFree();
						AMAScreen.Visible = false;
						AMAScreen = null;

						GetViewport().SetInputAsHandled();
					}
				}
			}
		}

		public void CloseAMA()
		{
			AMAHolder.RemoveChild(AMAScreen);

			AMAScreen.QueueFree();
			AMAScreen.Visible = false;
			AMAScreen = null;

			UpdateState();
		}

		public void CloseACC()
		{
			if (ACCScreen == null) return;
			ACCScreen.Visible = false;
			ACC.RemoveChild(ACCScreen);
			ACCScreen.QueueFree();
			ACCScreen = null;
			GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView").Unlock();

			UpdateState();
		}

		//Triggered from gamecore
		protected override void DayTick(uint previousDay, uint currentDay)
		{
			//The ship was destroyed - exit scene left
			if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Contains(Ship))
			{
				if (Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Where(T => T.Station.Built && !T.ActiveMethanoid).ToList().Count() == 0)
					GameCore.SingletonInstance.ChangeScene(Scenes.Earth_Ground, new List<SceneVariables>());
				else
					GameCore.SingletonInstance.ChangeScene(Scenes.Overview, new List<SceneVariables>());
			}
			else
			{
				UpdateState();
			}
		}

		#region Statics

		public static void UpdateShips(uint previousDay, uint currentDay)
		{
            if (currentDay <= previousDay) return;
            InterStellarShip.EnsureAutomationSlots(GameCore.SingletonInstance.GameData.ActiveSaveFile);
            var flightLosses = new HashSet<Guid>();

			foreach (var ship in GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships)
			{
                var asteroidActivity = ship is InterStellarShip && ship.PlanetLocation == StellarBodies.asteroids
                    && ship.ShipState is Ship_States.Docked or Ship_States.UnDocked or Ship_States.Docking or Ship_States.Launching;
                if (asteroidActivity && !((InterStellarShip)ship).AdvanceAsteroidActivity(previousDay, currentDay)) continue;
				if (ship.ShipType != Ship_Types.Shuttle)
				{

					if (GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar &&
						!((InterStellarShip)ship).MethanoidOwned &&
                        !GameCore.SingletonInstance.GameData.ActiveSaveFile.RogueCrew.Controls(ship) &&
						ship.ShipState == Ship_States.UnDocked &&
						(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[ship.PlanetLocation].ActiveMethanoid ||
						GameCore.SingletonInstance.GameData.PlanetUnderAttack(ship.PlanetLocation))
					)
					{
						((InterStellarShip)ship).AttackedCount++;
                        if (((InterStellarShip)ship).AttackedCount == 1)
                            GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(ship.Name + " UNDER ATTACK !");
					}
					else
					{

						((InterStellarShip)ship).AttackedCount = 0;
					}
				}

				if (!asteroidActivity && (ship.ShipState == Enums.Ship_States.Launching || ship.ShipState == Enums.Ship_States.Landing || ship.ShipState == Enums.Ship_States.TakingOff || ship.ShipState == Enums.Ship_States.Docking || ship.ShipState == Enums.Ship_States.InTransit) && ship.Fuel > 0)
				{
					if (ship is not SCG { Flight: not null }) ship.Fuel--;
					ship.FallingCount = 0;
				}
				else if (!asteroidActivity && ship.Fuel == 0 && ship.ShipState == Ship_States.UnDocked)
				{
					ship.FallingCount++;
				}

				if (ship.ShipType == Ship_Types.Shuttle && ship.ShipState == Ship_States.CrewRepairing)
				{
					((Shuttle)ship).CompleteRepairs();
				}
				else if (ship.ShipState != Ship_States.Docked && ship.ShipState != Ship_States.UnDocked)
				{
					if (ship.ShipState == Ship_States.Launching)
					{
						ship.ShipState = Ship_States.UnDocked;
						ship.ACC?.Update(Ship_States.Launching);
					}
					else if (ship.ShipState == Ship_States.TakingOff)
					{
						if (ship.TravelTimeRemain() == 0)
						{
							ship.ShipState = Ship_States.UnDocked;
							ship.ACC?.Update(Ship_States.TakingOff);
						}
					}
					else if (ship.ShipState == Ship_States.Landing)
					{
						if (ship.TravelTimeRemain() == 0)
						{
							ship.ShipState = Ship_States.Docked;
							((Shuttle)ship).OnGround = true;
							ship.ACC?.Update(Ship_States.Landing);
						}
					}
					else if (ship.ShipState == Ship_States.InTransit)
					{
						var arrived = false;
                        if (ship is SCG { Flight: not null } scg)
                        {
                            var result = scg.Flight.Advance(scg, GameCore.SingletonInstance.GameData.ActiveSaveFile);
                            if (result == InterstellarFlight.Outcome.Lost) { flightLosses.Add(ship.ShipID); continue; }
                            arrived = result == InterstellarFlight.Outcome.Arrived;
                            if (arrived) scg.Flight = null;
                        }
                        else arrived = ship.TravelTimeRemain() <= 0;
                        if (arrived)
						{
							ship.ShipState = Ship_States.UnDocked;

							var tempDestPlanet = ship.PlanetLocation;
							var tempDestStar = ship.StarLocation;

							ship.PlanetLocation = ship.DestinationPlanetLocation;
							ship.StarLocation = ship.DestinationStarLocation;

							ship.DestinationPlanetLocation = tempDestPlanet;
							ship.DestinationStarLocation = tempDestStar;

                            if (ship is InterStellarShip asteroidShip && ship.PlanetLocation == StellarBodies.asteroids)
                            {
                                ship.EngineEngaged = false;
                                if (ship.Fuel == 0) { asteroidShip.StrandAtAsteroids(); continue; }
                            }
							ship.ACC?.Update(Ship_States.InTransit);
						}	
					}
					else if (ship.ShipState == Ship_States.Docking)
					{
						if (ship.ShipType == Ship_Types.Shuttle || ship.PlanetLocation == StellarBodies.asteroids
                            || !GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Any(T => T.PlanetLocation == ship.PlanetLocation && T.ShipType != Ship_Types.Shuttle && T.ShipState == Ship_States.Docked))
						{
							ship.ShipState = Ship_States.Docked;
                            SdmSystem.Docked(ship);
							ship.ACC?.Update(Ship_States.Docking);
						}
                        else GameCore.SingletonInstance.GameData.ActiveSaveFile.RogueCrew.DockingBlocked(GameCore.SingletonInstance.GameData.ActiveSaveFile, ship);
					}
				}
				else if (ship.ShipState == Ship_States.Docked)
				{
					//Were on an asteroid - Assume all is well and we just need to mine
					if (ship.PlanetLocation == StellarBodies.asteroids)
					{
						var asteroid = ((InterStellarShip)ship).AsteroidScanResults;
						var ama = ship.Modules.First(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.a__m__a);
						var minedAmount = AMA.Mine((InterStellarShip)ship);
						if (minedAmount > 0)
						{
							// A partial pod of another mineral cannot accept this ore.
							var cargo = ship.Modules.FirstOrDefault(T => T.ModuleType == Module_Types.Supply &&
								T.ItemCount < 250 && (T.ItemCount == 0 || T.ItemStored == asteroid.Type));
							if (cargo == null)
							{
								ama.LastMinedDay = 0;
								asteroid.HasBeenMined = true;
								ship.TakeOff();
								ship.ACC?.Update(Ship_States.Docked);
							}
							else
							{
								cargo.ItemCount = Math.Min(cargo.ItemCount + minedAmount, 250);
								cargo.ItemStored = asteroid.Type;
							}
						}
					}
					else
					{
						//The ship is docked, and we're not updating it - Might be waiting for fuel, so notify anyway
						ship.ACC?.Update(Ship_States.Docked);
					}
				}
				else if (ship.ShipState == Ship_States.UnDocked)
				{
					//If we're at the asteroids and properly equipped we can scan asteroids
					if (ship.PlanetLocation == StellarBodies.asteroids && ship.Modules.Any(T => T.ModuleType == Module_Types.Tool && (T.ItemStored == ItemTypes.grapple || T.ItemStored == ItemTypes.a__m__a)))
					{
						//Check grapple first
						if (ship.Modules.Any(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.grapple && ship.Pilot != null && ship.Pilot.GetLevel() > 1))
							((InterStellarShip)ship).ItemScanResults = Asteroid.ScanAsteroids((InterStellarShip)ship);

						//The grapple check failed, check the AMA
						else if (ship.Modules.Any(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.a__m__a && ship.Pilot != null && ship.Pilot.GetLevel() > 0))
							((InterStellarShip)ship).ItemScanResults = Asteroid.ScanAsteroids((InterStellarShip)ship);
					}
					else
					{
                        //Check grapple
                        if (ship.Modules.Any(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.grapple && ship.Pilot != null && ship.Pilot.GetLevel() > 1))
                            ((InterStellarShip)ship).ItemScanResults = UnknownItem.ScanForItems((InterStellarShip)ship);
                    }

                    //Let the ACC know we are still undocked
                    ship.ACC?.Update(Ship_States.UnDocked);
				}
			}
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            save.Ships.RemoveAll(ship =>
            {
                var fallLimit = ship is InterStellarShip && ship.PlanetLocation == StellarBodies.asteroids ? 7 : 5;
                var lost = flightLosses.Contains(ship.ShipID) || ship.FallingCount >= fallLimit || (ship.ShipType != Ship_Types.Shuttle && ((InterStellarShip)ship).AttackedCount == 2);
                if (lost) save.News.AddShipLoss(ship);
                return lost;
            });
		}
		#endregion
	}
}
