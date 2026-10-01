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
		Label[] CargoValues { get; set; } = new Label[3];
		Label CourseText { get; set; }
		Label CourseValue { get; set; }
		Label ETA { get; set; }

		StarMap DestinationStarMap { get; set; }
		ModuleTextFrame ModuleTextFrame { get; set; }
		ACC ACCScreen { get; set; }
		Grapple GrappleScreen { get; set; }
		AMA AMAScreen { get; set; }
		Battle BattleScreen { get; set; }
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

			ShipName = GetNode<Label>("TextLayout/ShipName");
			Status = GetNode<Label>("TextLayout/Status");
			FuelValue = GetNode<Label>("TextLayout/FuelValue");
			PilotName = GetNode<Label>("TextLayout/PilotName");
			PilotCount = GetNode<Label>("TextLayout/PilotCount");
			EngineStatusValue = GetNode<Label>("TextLayout/EngineStatusValue");
			ACCStatus = GetNode<Label>("TextLayout/ACCStatus");
			CargoValues[0] = GetNode<Label>("TextLayout/CargoValue1");
			CargoValues[1] = GetNode<Label>("TextLayout/CargoValue2");
			CargoValues[2] = GetNode<Label>("TextLayout/CargoValue3");
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

			CargoValues[0].Text = "";
			CargoValues[1].Text = "";
			CargoValues[2].Text = "";

			UpdateState();

			base._Ready();
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

		private async void ShipInterior_Pressed(int modulePressed)
		{
			if (Ship.ShipState == Ship_States.Docked && Ship.PlanetLocation != StellarBodies.asteroids)
			{
				var sceneVariables = new List<SceneVariables>();
				var newScene = Enums.Scenes.ShipBay;

				if (Ship.ShipType == Ship_Types.Shuttle && ((Shuttle)Ship).OnGround)
				{
					if (Ship.Modules[modulePressed].ItemStored == ItemTypes.r_frame && CurrentPlanet.BaseBuildParts < 2 && Ship.Pilot != null && Ship.ShipType == Ship_Types.Shuttle && ((Shuttle)Ship).OnGround)
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

					if (Ship.Modules[modulePressed].ItemStored == ItemTypes.bandaid && CurrentPlanet.BaseDamaged && Ship.Pilot != null && Ship.ShipType == Ship_Types.Shuttle && ((Shuttle)Ship).OnGround)
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
						if (Ship.Modules.Any<ShipModule>(m => m.ItemStored == ItemTypes.commspod))
						{
							if (GameCore.SingletonInstance.GameData.ActiveSaveFile.MethanoidTradeCount == 16)
							{
                                await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_War_Warning], new List<string>(), (modulePressed + 1));
								var commpodModule = Ship.Modules.First<ShipModule>(m => m.ItemStored == ItemTypes.commspod);
								commpodModule.ItemStored = ItemTypes.grapple;
                                commpodModule.HeldItem = new UnknownItem(UnknownItemTypes.Blazer);
                                GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar = true;
                                GameCore.SingletonInstance.GameData.ActiveSaveFile.WarDeclaredDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
                            }
                            else
							{
								var itemlist = new Dictionary<ShipModule,Enums.ItemTypes>();
                                var newitemlist = new Dictionary<ShipModule, Enums.ItemTypes>();

                                foreach (ShipModule m in Ship.Modules)
								{
									if (m.ModuleType == Module_Types.Supply && m.ItemCount > 0)
									{
										itemlist[m] = m.ItemStored;
										newitemlist[m] = m.ItemStored;
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

                                    //this needs to give the option to the user to cancel - not implemented yet

                                    List<string> p = new List<string>();
                                    p.Add(newitemlist.Values.ToList()[0].ToScreenString());
                                    p.Add(itemlist.Values.ToList()[0].ToScreenString());

                                    GameCore.SingletonInstance.GameData.ActiveSaveFile.MethanoidTradeCount++;
                                    if (itemlist.Count == 1)
									{
										await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_Trade1], p, (modulePressed + 1));
									}
									else
									{
                                        p.Add(newitemlist.Values.ToList()[1].ToScreenString());
                                        p.Add(itemlist.Values.ToList()[1].ToScreenString());
										await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_Trade2], p, (modulePressed + 1));
									}

									//update actual items traded
									foreach(ShipModule m in newitemlist.Keys)
									{
										m.ItemStored = newitemlist[m];
									}

								}
                            }
                        }
                        else
						{
							if (GameCore.SingletonInstance.GameData.GetItem(ItemTypes.commspod).Locked)
							{
								if (Ship.Modules.Any<ShipModule>(m => m.ItemStored == ItemTypes.grapple))
								{
									await ShowModuleTextFrame(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ModuleFrameTexts[Enums.ModuleFrameText.Methanoid_Intro_With_Grapple], new List<string>(), (modulePressed + 1));
									Ship.Modules.First<ShipModule>(m => m.ItemStored == ItemTypes.grapple).HeldItem = new UnknownItem(UnknownItemTypes.CommsPod);
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

						EnemyFleet enemyShip;
						if (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].ActiveMethanoid)
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

						await ShowBattleFrame((InterStellarShip)Ship, enemyShip);

						if (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].ActiveMethanoid)
						{
							//move remaining drones back to store
							GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].Station.Resources.Stores[ItemTypes.ios_drone] += enemyShip.DroneCount;
						}
						else if (!((EnemyFleet)enemyShip).Attacking)
						{
							//enemy fleet has fled

							((EnemyFleet)enemyShip).CancelAttack(Ship.PlanetLocation);
						}

						if (((InterStellarShip)Ship).DroneCount == 0)
						{
							//player Ship destroyed
							GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Remove(Ship);

							//todo show ship destroyed page
						}

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

					if ((Ship.ShipType != Ship_Types.Shuttle || (Ship.ShipType == Ship_Types.Shuttle && !((Shuttle)Ship).OnGround)) && Ship.ShipState == Ship_States.UnDocked  && Ship.Modules[modulePressed].ItemStored == ItemTypes.of_frame && CurrentPlanet.Station.Built == false && Ship.Pilot != null && Ship.PlanetLocation!=StellarBodies.asteroids)
					{
						if (Ship.Pilot != null) Ship.Pilot.AddAction();

						if (CurrentPlanet.Station.BuildParts == 0)
							CurrentPlanet.Station.StationOrdinal = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Where(p => !p.ActiveMethanoid && p.Station != null).MaxBy(p => p.Station.StationOrdinal).Station.StationOrdinal + 1;

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

		private async Task ShowBattleFrame(InterStellarShip player, EnemyFleet enemy)
		{
			BattleScreen = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/Battle.tscn").Instantiate<Battle>();

			Window.AddChild(BattleScreen);

			await BattleScreen.DoBattle(player, enemy);

			Window.RemoveChild(BattleScreen);
		}

		private void ShowDroneTrasferFrame(InterStellarShip player)
		{
			FleetTransfers = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/FleetTransfers.tscn").Instantiate<FleetTransfers>();

			Window.AddChild(FleetTransfers);

			FleetTransfers.TransferDrones(player);

		}

		private async Task ShowModuleTextFrame(TextFrame newTextFrame, List<string> dynamicProperties, int windowNumber)
		{
			GameCore.LockScreen();

			ModuleTextFrame = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/ModuleTextFrame.tscn").Instantiate<ModuleTextFrame>();
			Window.AddChild(ModuleTextFrame);

			await ModuleTextFrame.PlayText(newTextFrame, dynamicProperties, windowNumber);

			Window.RemoveChild(ModuleTextFrame);
			ModuleTextFrame = null;

			GameCore.UnLockScreen();
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

		private void Land_Pressed()
		{
			Ship.Land();
			UpdateState();
		}

		private void TakeOff_Pressed()
		{
			Ship.TakeOff();
			UpdateState();
		}

		private void Dock_Pressed()
		{
			if (Ship.PlanetLocation == StellarBodies.asteroids)
				return;
			if (Ship.ShipType == Ship_Types.Shuttle || ((InterStellarShip)Ship).AttackedCount == 0)
			{
				if (Ship.ShipType != Ship_Types.Shuttle &&
					GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar &&
					GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].ActiveMethanoid)
				{
					var planet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation];

					planet.ActiveMethanoid = false;
					planet.Station.StationOrdinal = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Where(p => p != planet && !p.ActiveMethanoid && p.Station != null).MaxBy(p => p.Station.StationOrdinal).Station.StationOrdinal + 1;

				}

				Ship.Dock();
				UpdateState();
			}
		}

		private void SmallLocation_Pressed()
		{
			Ship.LocationView = !Ship.LocationView;

			UpdateState();
		}

		private void DisengageEngine_Pressed()
		{
			Ship.DisengageEngine();
		}

		private void EngageEngine_Pressed()
		{
			if (Ship.EngageEngine())
			{
				CurrentPlanet = null;
                ((InterStellarShip)Ship).ItemScanResults = null;
            }

            UpdateState();
		}

		private void SetCourse_Pressed()
		{
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
			ACCScreen = GD.Load<PackedScene>("res://PreFabs/ACC.tscn").Instantiate<ACC>();
			ACCScreen.SetACC(Ship.ACC);
			ACCScreen.CloseWindow = CloseACC;

			ACC.AddChild(ACCScreen);

			ACCScreen.UpdateState();

			var cursor = GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView");
			cursor.LockToRect(ACC.GetGlobalRect());

			UpdateState();
		}

		private void UpdateState()
		{
			if (CurrentPlanet == null && Ship.ShipState != Ship_States.InTransit)
				CurrentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation];

			if (Ship.GetType() == typeof(Shuttle))
				ShipName.Text = "Shuttle Craft";
			else
				ShipName.Text = Ship.Name;

			if (Ship.GetType() != typeof(Shuttle) && GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar &&
				(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].ActiveMethanoid ||
				GameCore.SingletonInstance.GameData.PlanetUnderAttack(Ship.PlanetLocation)))
				Status.Text = "UNDER ATTACK !\n" + Ship.PlanetLocation.ToScreenString(" ");

			else if (Ship.GetType() == typeof(Shuttle) && Ship.ShipState == Ship_States.CrewRepairing)
				Status.Text = "Crew Active On\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.GetType() == typeof(Shuttle) && Ship.ShipState == Ship_States.Landing)
				Status.Text = "Landing On\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.GetType() == typeof(Shuttle) && Ship.ShipState == Ship_States.TakingOff)
				Status.Text = "Climbing From\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.GetType() == typeof(Shuttle) && ((Shuttle)Ship).OnGround)
				if (Ship.ACC != null && Ship.ACC.Active && Ship.Fuel < 50)
					Status.Text = "Refueling at\n" + Ship.PlanetLocation.ToScreenString(" ");
				else
					Status.Text = "In Ground Bay\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.ShipState == Ship_States.Launching)
				Status.Text = "Launching From\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.ShipState == Ship_States.Docked)
				if (Ship.ACC != null && Ship.ACC.Active && Ship.Fuel < 50)
					Status.Text = "Refueling at\n" + Ship.PlanetLocation.ToScreenString(" ");
				else
					Status.Text = "Docked Above\n" + Ship.PlanetLocation.ToScreenString(" ");
			else if (Ship.ShipState == Ship_States.InTransit)
				if (Ship.EngineEngaged)
					Status.Text = "In Transit To\n" + Ship.DestinationPlanetLocation.ToScreenString(" ");
				else
					Status.Text = "Drifting To\n" + Ship.DestinationPlanetLocation.ToScreenString(" ");
			else if (Ship.ShipState == Ship_States.Docking)
				Status.Text = "Docking With\n" + Ship.PlanetLocation.ToScreenString(" ");
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

				if (new List<Enums.Ship_States>() { Ship_States.Docking, Ship_States.Launching, Ship_States.Landing, Ship_States.TakingOff, Ship_States.InTransit }.Contains(Ship.ShipState))
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

				if (Ship.ACC.Active)
					ACCStatus.Text = "A.C.C Is \nEngaged";
				else if (Ship.ACC.CycleMode)
					ACCStatus.Text = "A.C.C Is \nFinishing";
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

			var currentDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay + Ship.TravelTimeRemain();
			var curDay = (currentDay % 1000).ToString().PadLeft(3, '0');
			var outputYear = (3100 + Math.Floor((decimal)(currentDay / 1000))) + " " + curDay + ".00";

			ETA.Text = "ETA:\n" + outputYear;

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
					BigLocation.Texture = SpriteManager.LoadImage(SpriteBasePath + "SmallLocation_StormDoors.png");
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
					SmallLocation.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "SmallLocation_Planet_" + CurrentPlanet.PlanetColor.ToString() + ((CurrentPlanet != null && CurrentPlanet.Station.BuildParts > 0) ? "_Station" : "") + ".png");
				else
					SmallLocation.TextureNormal = null;
			}

			SetCourse.Visible = Ship.ShipType != Ship_Types.Shuttle;

			//TODO - Set images
			//Click events for modules
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Right && mb.Pressed)
			{
				var cursor = GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView");

				if (cursor.IsLocked)
				{
					//We were locked into the star map, let's read the new destination
					if (DestinationStarMap != null && DestinationStarMap.Visible == true)
					{
						DestinationStarMap.Visible = false;

						// Closing a system or galaxy view cancels selection and preserves the course.
						if (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.TryGetValue(DestinationStarMap.CurrentLocation, out var newDestination))
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
			ACCScreen.Visible = false;
			ACC.RemoveChild(ACCScreen);

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
				GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet = Ship.PlanetLocation;

				var ground = !((Ship.ShipType != Ship_Types.Shuttle) || (!((Shuttle)Ship).OnGround && ((Shuttle)Ship).PlanetLocation!=StellarBodies.earth));

				GameCore.SingletonInstance.UpdateMenuButtons(ground, !ground);
			}
		}

		#region Statics

		public static void UpdateShips(uint previousDay, uint currentDay)
		{

			foreach (var ship in GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships)
			{
				if (ship.ShipType != Ship_Types.Shuttle)
				{

					if (GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar &&
						!((InterStellarShip)ship).MethanoidOwned &&
						ship.ShipState == Ship_States.UnDocked &&
						(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[ship.PlanetLocation].ActiveMethanoid ||
						GameCore.SingletonInstance.GameData.PlanetUnderAttack(ship.PlanetLocation))
					)
					{
						((InterStellarShip)ship).AttackedCount++;
					}
					else
					{

						((InterStellarShip)ship).AttackedCount = 0;
					}
				}

				if ((ship.ShipState == Enums.Ship_States.Launching || ship.ShipState == Enums.Ship_States.Landing || ship.ShipState == Enums.Ship_States.TakingOff || ship.ShipState == Enums.Ship_States.Docking || ship.ShipState == Enums.Ship_States.InTransit) && ship.Fuel > 0)
				{
					ship.Fuel--;
					ship.FallingCount = 0;
				}
				else if (ship.Fuel == 0 && ship.ShipState == Ship_States.UnDocked)
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
						if (ship.TravelTimeRemain() == 0)
						{
							ship.ShipState = Ship_States.UnDocked;

							var tempDestPlanet = ship.PlanetLocation;
							var tempDestStar = ship.StarLocation;

							ship.PlanetLocation = ship.DestinationPlanetLocation;
							ship.StarLocation = ship.DestinationStarLocation;

							ship.DestinationPlanetLocation = tempDestPlanet;
							ship.DestinationStarLocation = tempDestStar;

							ship.ACC?.Update(Ship_States.InTransit);
						}	
					}
					else if (ship.ShipState == Ship_States.Docking)
					{
						if (ship.ShipType == Ship_Types.Shuttle || !GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Any(T => T.PlanetLocation == ship.PlanetLocation && T.ShipType != Ship_Types.Shuttle && T.ShipState == Ship_States.Docked))
						{
							ship.ShipState = Ship_States.Docked;
							ship.ACC?.Update(Ship_States.Docking);
						}
					}
				}
				else if (ship.ShipState == Ship_States.Docked)
				{
					//Were on an asteroid - Assume all is well and we just need to mine
					if (ship.PlanetLocation == StellarBodies.asteroids)
					{
                        //Is it time to generate some ore?
                        var minedAmount = AMA.Mine(((InterStellarShip)ship).AsteroidScanResults, ship.Modules.First(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.a__m__a));
                        
                        //Our cargo hold is full so we should take off
                        //We have to mine something to trigger a takeoff
                        if (minedAmount > 0 && !ship.Modules.Any(T => T.ModuleType == Module_Types.Supply && T.ItemCount < 250))
						{
							ship.Modules.First(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.a__m__a).LastMinedDay = 0;
							((InterStellarShip)ship).AsteroidScanResults.HasBeenMined = true;

							ship.TakeOff();

							ship.ACC?.Update(Ship_States.Docked);
						}
						else if (minedAmount > 0)
						{
							var firstModule = ship.Modules.First(T => T.ModuleType == Module_Types.Supply && (T.ItemStored == ItemTypes.none || (T.ItemStored == ((InterStellarShip)ship).AsteroidScanResults.Type && T.ItemCount < 250)));
							firstModule.ItemCount = Math.Min(firstModule.ItemCount + minedAmount, 250);
							firstModule.ItemStored = ((InterStellarShip)ship).AsteroidScanResults.Type;
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
							((InterStellarShip)ship).ItemScanResults = Asteroid.ScanAsteroids(((InterStellarShip)ship).AsteroidScanResults);

						//The grapple check failed, check the AMA
						else if (ship.Modules.Any(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.a__m__a && ship.Pilot != null && ship.Pilot.GetLevel() > 0))
							((InterStellarShip)ship).ItemScanResults = Asteroid.ScanAsteroids(((InterStellarShip)ship).AsteroidScanResults);
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
			GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.RemoveAll(ship =>
				ship.FallingCount == 5 || (ship.ShipType != Ship_Types.Shuttle && ((InterStellarShip)ship).AttackedCount == 2));
		}
		#endregion
	}
}
