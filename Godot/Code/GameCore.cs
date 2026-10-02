using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.IO;
using Deuteros.Code.Platform.Base;
using System.Linq;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using System.Diagnostics;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform;
using static Deuteros.Code.Enums;
using Deuteros.Code.Objects.GameData;

namespace Deuteros.Code
{
	public partial class GameCore : BaseSubScene
	{
		#region DEBUGVARS

		//TODO DEBUG

		public bool InfiniteResources { get; set; }

		#endregion

		#region menuButtonsConfig
		private List<Objects.MenuButton> EarthMenuButtons
		{
			get
			{
				return new List<Objects.MenuButton>()
				{
					new Objects.MenuButton(Enums.Menu_Buttons.Production, Enums.Scenes.Production, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground
					}, null,null, "Production"),
					null,
					new Objects.MenuButton(Enums.Menu_Buttons.Shuttle, Enums.Scenes.ShipInterior, true,
						new Godot.Collections.Array<Enums.SceneVariables>() {
							Enums.SceneVariables.Ground
						}, new List<Action> { () => GameCore.SingletonInstance.ShipSelected = GameData.ActiveSaveFile.Ships.Single(T => T.ShipType == Enums.Ship_Types.Shuttle && T.PlanetLocation == Enums.StellarBodies.earth).ShipID },
						() => GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Shuttle_Unlock),
						"Shuttle"),
					new Objects.MenuButton(Enums.Menu_Buttons.Training, Enums.Scenes.Earth_Training, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground
					}, null,null,"Training"),
					new Objects.MenuButton(Enums.Menu_Buttons.Shuttle_Bay, Enums.Scenes.ShipBay, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground,
						Enums.SceneVariables.Shuttle
					}, null,null,"Shuttle Bay"),
					null,
					null,
					null,
					null,
					new Objects.MenuButton(Enums.Menu_Buttons.Research, Enums.Scenes.Earth_Research, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground
					}, null, null, "Research"),
					new Objects.MenuButton(Enums.Menu_Buttons.GroundMaterials, Enums.Scenes.GroundMaterials, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground
					}, null, null, "Resource"),
					new Objects.MenuButton(Enums.Menu_Buttons.Store, Enums.Scenes.Store, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground
					}, null, null, "MiningStore")
				};
			}
		}
		private List<Objects.MenuButton> EarthStationMenuButtons
		{
			get
			{
				return new List<Objects.MenuButton>()
				{
					new Objects.MenuButton(Enums.Menu_Buttons.Production, Enums.Scenes.Production, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Orbit
					}, null, null, "Production"),

					new Objects.MenuButton(Enums.Menu_Buttons.Shuttle_Bay, Enums.Scenes.ShipBay, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Orbit,
						Enums.SceneVariables.Shuttle
					}, null, null, "Space Bay"),

					new Objects.MenuButton(Enums.Menu_Buttons.Shuttle, Enums.Scenes.ShipInterior, true,
						new Godot.Collections.Array<Enums.SceneVariables>() {
							Enums.SceneVariables.Orbit
						}, new List<Action> { () => GameCore.SingletonInstance.ShipSelected = GameData.ActiveSaveFile.Ships.Single(T => T.ShipType == Enums.Ship_Types.Shuttle && T.PlanetLocation == GetCurrentPlanet().PlanetId).ShipID }, null,
						"Shuttle"),

					null,
					null,
					null,
					new Objects.MenuButton(Enums.Menu_Buttons.Store, Enums.Scenes.Store, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Orbit
					}, null, null, "Stores"),
					new Objects.MenuButton(Enums.Menu_Buttons.Ship_Bay, Enums.Scenes.ShipBay, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Orbit,
						Enums.SceneVariables.Ship
					}, null, null, "SpaceDock"),
					null,
					null,
					null,
					null
				};
			}
		}
		private List<Objects.MenuButton> StandardMenuButtons
		{
			get
			{
				return new List<Objects.MenuButton>()
				{
					new Objects.MenuButton(Enums.Menu_Buttons.Production, Enums.Scenes.Production, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Orbit,
					}, null, null, "Production"),

					new Objects.MenuButton(Enums.Menu_Buttons.Shuttle_Bay, Enums.Scenes.ShipBay, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Orbit,
						Enums.SceneVariables.Shuttle
					}, null, null, "Space Bay"),

					new Objects.MenuButton(Enums.Menu_Buttons.Shuttle, Enums.Scenes.ShipInterior, true,
						new Godot.Collections.Array<Enums.SceneVariables>() {
							Enums.SceneVariables.Orbit
						},
						new List<Action> { () => GameCore.SingletonInstance.ShipSelected = GameData.ActiveSaveFile.Ships.Single(T => T.ShipType == Enums.Ship_Types.Shuttle && T.PlanetLocation == GetCurrentPlanet().PlanetId).ShipID },
						() => GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Exists(T => T.ShipType == Enums.Ship_Types.Shuttle && T.PlanetLocation == GetCurrentPlanet().PlanetId),


						"Shuttle"),
					null,
					new Objects.MenuButton(Enums.Menu_Buttons.Shuttle_Bay, Enums.Scenes.ShipBay, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground,
						Enums.SceneVariables.Shuttle
					}, null,() => GetCurrentPlanet().BaseBuildParts==2,"Shuttle Bay"),
					null,
					new Objects.MenuButton(Enums.Menu_Buttons.Store, Enums.Scenes.Store, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Orbit
					}, null, null, "Stores"),
					new Objects.MenuButton(Enums.Menu_Buttons.Ship_Bay, Enums.Scenes.ShipBay, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Orbit,
						Enums.SceneVariables.Ship
					}, null, null, "SpaceDock"),
					null,
					null,
					new Objects.MenuButton(Enums.Menu_Buttons.GroundMaterials, Enums.Scenes.GroundMaterials, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground
					}, null, () => GetCurrentPlanet().BaseBuildParts == 2 && !GetCurrentPlanet().BaseDamaged, "Resource")
					{
						DisabledButtonType = () => GetCurrentPlanet().BaseBuildParts == 2 && GetCurrentPlanet().BaseDamaged
							? Enums.Menu_Buttons.GroundMaterials_Damaged : Enums.Menu_Buttons.Empty
					},
					new Objects.MenuButton(Enums.Menu_Buttons.Store, Enums.Scenes.Store, true,
					new Godot.Collections.Array<Enums.SceneVariables>() {
						Enums.SceneVariables.Ground
					}, null, () => GetCurrentPlanet().BaseBuildParts == 2 && !GetCurrentPlanet().BaseDamaged, "MiningStore")
					{
						DisabledButtonType = () => GetCurrentPlanet().BaseBuildParts == 2 && GetCurrentPlanet().BaseDamaged
							? Enums.Menu_Buttons.Store_Damaged : Enums.Menu_Buttons.Empty
					}
				};
			}
		}

		private List<Objects.MenuButton> OverviewMenuButtons
		{
			get
			{
				return new List<Objects.MenuButton>()
				{
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				};
			}
		}

		#endregion

		private static GameCore _instance;
		private Node _currentScreen;
		private MainMenu _menuScreen;
		private Unlocker _unlocker;
		private InputBlocker _screenLocker;
		private int _lockCount;
		public Enums.Scenes currentScene;

		public static GameCore SingletonInstance
		{
			get
			{
				if (_instance == null)
				{
					_instance = GD.Load<Node>("/root/GameCore") as GameCore;

					if (_instance == null)
					{
						GD.PushError("GameCore not found! Did you forget to AutoLoad it?");
					}
				}

				return _instance;
			}
		}

		public CoreData GameData { get; set; }

		public static string HoverText { get; set; }

		public Guid ShipSelected { get; set; }

		public GameConfig Config { get; set; }

		//Data stored in the GameCore is temporary
		public delegate void DayPassedDelegate(uint previousDay, uint currentDay);
		private event DayPassedDelegate SimulationDayPassed;
		// Screen notification: the active world's simulation has finished this day.
		public event DayPassedDelegate DayPassed;

		public delegate void PlanetChangedDelegate(Objects.Interfaces.IPlanet newPlanet);
		public event PlanetChangedDelegate PlanetChanged;

		public delegate void StationPiecePlacedDelegate(Enums.StellarBodies stellarBody);
		public event StationPiecePlacedDelegate StationPiecePlaced;

		public delegate void ProductionFinishedDelegate(Objects.Factory factory);
		public event ProductionFinishedDelegate ProductionFinished;

		public delegate void ResearchFinishedDelegate(Objects.ResearchItem researchItem);
		public event ResearchFinishedDelegate ResearchFinished;

		public delegate void ShipCreatedDelegate(IShip ship);
		public event ShipCreatedDelegate ShipCreated;

		public delegate void AlientTechDiscoveryDelegate(ItemTypes techType);
		public event AlientTechDiscoveryDelegate AlienTechDiscovery;

		public delegate void UnlockAddedDelegate(Enums.Game_Unlocks addedUnlock);
		public event UnlockAddedDelegate UnlockAdded;

		public GameCore()
		{
			HoverText = "";

			Config = new GameConfig();

			Config.ApplyAudio();
			Config.ApplyDisplay();
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			GetTree().AutoAcceptQuit = false;
			// Assign instance when the node is ready
			if (_instance == null)
				_instance = this;

			_unlocker = new Unlocker();

			SpriteManager.ImageCache = new Godot.Collections.Dictionary<string, Texture2D>();

			GameData = new CoreData();

			//TODO - Only for while building the project. Transitition to json config files before go-live.
			CoreData.CreateBaseGameData();
			GameData.ActiveSaveFile = CoreData.CreateNewSaveFile();

			_screenLocker = GetNode<InputBlocker>("/root/Master/InputBlocker");

			// Model updates are registered once; work and attrition precede ships, then refining.
			SimulationDayPassed += _unlocker.DayTick;
			SimulationDayPassed += UpdatePlanets;
			SimulationDayPassed += Code.Platform.Screens.Production.UpdateProduction;
			SimulationDayPassed += Code.Platform.Screens.Research.UpdateResearch;
			SimulationDayPassed += StaffAttrition.DayTick;
			SimulationDayPassed += Code.Platform.Screens.ShipInterior.UpdateShips;
			SimulationDayPassed += FuelRefining.DayTick;
			SimulationDayPassed += Code.Platform.EnemyDroneBuilder.BuildDrones;
			SimulationDayPassed += Code.Platform.Screens.MTX.UpdateMTX;
			SimulationDayPassed += SdmSystem.DayTick;

			Input.MouseMode = Input.MouseModeEnum.Hidden;

			ChangeScene(Enums.Scenes.IntroScreen, new List<Enums.SceneVariables>());
		}

		public override void _Input(InputEvent @event)
		{
			// Master receives input after descendant screens, so their modal dismissals take priority.
			if (@event is not InputEventMouseButton mouse || mouse.ButtonIndex != MouseButton.Right || !mouse.Pressed)
				return;
			if (_screenLocker.Blocked || GlobalInput.UiLocked || GetTree().Paused || OverlayManager.Instance.IsOpen || currentScene == Scenes.Overview)
				return;
			if (GetNode<GlobalInput>("VirtualCursorView").IsLocked)
				return;
			if (!GameData.ActiveSaveFile.BaseGameData.Planets.Values.Any(planet => planet.Station.BuildParts > 0 && !planet.ActiveMethanoid))
				return;

			GetViewport().SetInputAsHandled();
			ChangeScene(Scenes.Overview, new List<SceneVariables>());
		}

		private void TriggerDay(uint previousDay, uint currentDay)
		{
			SimulationDayPassed?.Invoke(previousDay, currentDay);
			DayPassed?.Invoke(previousDay, currentDay);
			PublishPendingStory();
			// Refresh repair-dependent services without replacing the current screen's header.
			_menuScreen?.SetupMenus();
		}

		private bool StoryDisplayBlocked() => _screenLocker.Blocked || GlobalInput.UiLocked || GetTree().Paused || OverlayManager.Instance.IsOpen
				|| GetNode<GlobalInput>("VirtualCursorView").IsLocked
				|| currentScene == Scenes.Bulletins || currentScene == Scenes.IntroScreen;

		private void PublishPendingStory()
		{
			// Existing discoveries and modal owners keep priority; an undisplayed notice stays saved.
			if (StoryDisplayBlocked()) return;
			var save = GameData.ActiveSaveFile;
			if (save.News.PendingBulletins.Count > 0)
			{
				ShowBulletin(save.News.PendingBulletins[0]);
				return;
			}
			save.AlienTransmissions?.AdvanceHyperlight(save);
			if (save.AlienTransmissions?.Advance() == true)
			{
				ChangeScene(Scenes.Bulletins, new List<SceneVariables>());
				_menuScreen.Location.Text = "News Bulletins";
				((Bulletins)_currentScreen).DisplayTransmissionNotice(save);
			}
			else if (save.AlienTransmissions?.DiscoverHyperlight(save) == true) ShowBulletin(BulletinTypes.Hyperlight_Speed);
			else if (save.AlienTransmissions?.DiscoverScg(save) == true) ShowBulletin(BulletinTypes.Sol_Cleared);
		}

		private void UpdatePlanets(uint previousDay, uint currentDay)
		{
			// Resolve the active world each day; training precedes Earth and local extraction.
			var earth = Earth;
			earth.TrainingData.ChildDayTick(previousDay, currentDay);
			earth.DayTick(previousDay, currentDay);
			foreach (Planet planet in GameData.ActiveSaveFile.BaseGameData.Planets.Values)
				if (!ReferenceEquals(planet, earth)) planet.DayTick(previousDay, currentDay);
		}

		private void TriggerPlanetChange(Objects.Interfaces.IPlanet newPlanet)
		{
			PlanetChanged?.Invoke(newPlanet);
		}

		public void TriggerStationPiecePlaced(Enums.StellarBodies stellarBody)
		{
			StationPiecePlaced?.Invoke(stellarBody);
		}

		public void TriggerProductionFinished(Objects.Factory factory)
		{
			ProductionFinished?.Invoke(factory);
		}

		public void TriggerResearchFinished(Objects.ResearchItem researchItem)
		{
			ResearchFinished?.Invoke(researchItem);
		}

		public void TriggerShipCreated(IShip ship)
		{
			ShipCreated?.Invoke(ship);
		}

		public void TriggerUnlockAdded(Enums.Game_Unlocks addedUnlock)
		{
			UnlockAdded?.Invoke(addedUnlock);
		}
		public void TriggerAlienTechDiscovery(Enums.ItemTypes techType)
		{
			AlienTechDiscovery?.Invoke(techType);
		}

		public Deuteros.Code.Objects.Interfaces.IPlanet GetCurrentPlanet()
		{
			return GameData.ActiveSaveFile.BaseGameData.Planets[GameData.ActiveSaveFile.CurrentPlanet];
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			SdmSystem.AdvanceTime(delta);
			UpdateTime(delta);
		}

		public void StationDestroyed(IPlanet planet)
		{
			if (_currentScreen == null || GameData.ActiveSaveFile.CurrentPlanet != planet.PlanetId) return;
			if (_currentScreen is ShipInterior interior && GameData.ActiveSaveFile.Ships.Contains(interior.Ship))
			{
				interior.UpdateState();
				return;
			}
			if (currentScene == Scenes.Overview || currentScene == Scenes.News || currentScene == Scenes.SaveScreen
				|| currentScene == Scenes.Bulletins || currentScene == Scenes.IntroScreen) return;
			// Exit now so the removed ship/station screen cannot observe the following DayPassed.
			_currentScreen.GetParent().RemoveChild(_currentScreen);
			ChangeScene(Scenes.Overview, new List<SceneVariables>());
		}

		public static void ShowError(BaseSubScene scene, string ErrorText)
		{
			var ErrorScreen = GD.Load<PackedScene>("res://Screens/Base/Error.tscn");
			var overlay = OverlayManager.Instance.ShowOverlay(ErrorScreen, false);
			var errorLabel = overlay.GetNode<Label>("ErrorButton/OuterColorRect/InnerColorRect/ErrorLabel"); ;

			errorLabel.Text = ErrorText;
		}

		private void UpdateTime(double delta)
		{
			if (GetTree().Paused) return;
			var save = GameData.ActiveSaveFile;
			var manual = save.TimeSkipDay || (save.TimeSkip && Time.GetTicksMsec() - save.TimeSkipStart >= 500);
			if (manual) save.Clock.QueueManual();
			else if (!save.TimeSkip && currentScene != Scenes.IntroScreen) save.Clock.AdvanceNormal(delta);
			var manualConsumed = save.Clock.PendingIncrement == 100;
			if ((!manual && StoryDisplayBlocked()) || !save.Clock.Consume()) return;
			save.CurrentDay++;
			TriggerDay(save.CurrentDay - 1, save.CurrentDay);
			if (manualConsumed)
			{
				save.TimeSkipStart = Time.GetTicksMsec();
				save.TimeSkipDay = false;
			}
			QueueRedraw();
		}

		private void UpdateLocationText(bool orbit)
		{
			if (_currentScreen.GetType() == typeof(ShipInterior))
			{
				_menuScreen.Location.Text = ((ShipInterior)_currentScreen).Ship.Name;
			}
			//else if ((_currentScreen.GetType() == typeof(ShipBay) || _currentScreen.GetType() == typeof(GroundMaterials) || _currentScreen.GetType() == typeof(Deuteros.Code.Platform.Screens.Store)) && !orbit)
			else if (!orbit)
			{
				if (GetCurrentPlanet().PlanetId==StellarBodies.earth)
					_menuScreen.Location.Text = GetCurrentPlanet().PlanetId.ToScreenString(" ") + " City";
				else
					_menuScreen.Location.Text = GetCurrentPlanet().PlanetId.ToScreenString(" ") + " Colony";
			}
			else
			{
				_menuScreen.Location.Text = GetCurrentPlanet().PlanetId.ToScreenString(" ") + " Orbital";
			}
		}

		public void ShowAlienTransmission()
		{
			var save = GameData.ActiveSaveFile;
			if (save.AlienTransmissions?.LastStage is not >= 0) return;
			ChangeScene(Scenes.Bulletins, new List<SceneVariables>());
			_menuScreen.Location.Text = "News Bulletins";
			((Bulletins)_currentScreen).DisplayAlienTransmission(save, true);
		}

		public void ShowBulletin(BulletinTypes bulletin, bool replay = false)
		{
			var pending = GameData.ActiveSaveFile.News.PendingBulletins;
			if (!replay && !pending.Contains(bulletin)) pending.Add(bulletin);
			if (StoryDisplayBlocked()) return;
			if (!replay)
			{
				bulletin = pending[0];
				pending.RemoveAt(0);
			}
			GameCore.SingletonInstance.ChangeScene(Enums.Scenes.Bulletins, new List<SceneVariables>());
			_menuScreen.Location.Text = "News Bulletins";
			((Bulletins)_currentScreen).DisplayBulletin(bulletin);
			GameCore.SingletonInstance.GameData.ActiveSaveFile.News.LastBulletin = bulletin;
		}

		public void ChangeScene(Enums.Scenes sceneToLoad, List<Enums.SceneVariables> sceneVariables)
		{
			currentScene = sceneToLoad;
			SceneVariables = sceneVariables;

			var newSceneName = sceneToLoad.ToString().Replace("_", "/") + ".tscn";

			//This is a special case for the loading screen, only happens once on a new game
			if (_currentScreen != null && _currentScreen.SceneFilePath.Contains("IntroScreen"))
			{
				var newMenuScene = GD.Load<PackedScene>("res://Screens/Base/MenuBase.tscn").Instantiate<MainMenu>();
				GetNode<Node>("/root/Master/MainScene").AddChild(newMenuScene);
				_menuScreen = newMenuScene;
			}

			if (_currentScreen != null)
				_currentScreen.QueueFree();

			if (sceneToLoad == Scenes.Earth_Ground) GameData.ActiveSaveFile.CurrentPlanet = StellarBodies.earth;

			var newScene = GD.Load<PackedScene>("res://Screens/" + newSceneName).Instantiate<BaseSubScene>();
			newScene.SceneVariables = sceneVariables;
			GetNode<Node>("/root/Master/MainScene").AddChild(newScene);
			_currentScreen = newScene;

			UpdateMenuButtons(sceneVariables.Contains(Enums.SceneVariables.Ground), sceneVariables.Contains(Enums.SceneVariables.Orbit));

			if (_currentScreen.GetType() == typeof(ShipInterior))
			{
				_menuScreen.Location.Text = ((ShipInterior)_currentScreen).Ship.Name;
			}

			if (_currentScreen.SceneFilePath.Contains("ResourceMap"))
			{
				_menuScreen.Location.Text = "Deposit Analysis";
			}

			if (_currentScreen.SceneFilePath.Contains("News.tscn"))
			{
				_menuScreen.Location.Text = "News Bulletins";
			}

			if (_currentScreen.SceneFilePath.Contains("SaveScreen.tscn"))
			{
				_menuScreen.Location.Text = "Disk Access";
			}

			ShipSelected = Guid.Empty;
		}

		public void UpdateMenuButtons(bool ground, bool orbit)
		{
			_menuScreen?.SetMethanoidIndicator(false);
			// The interior stays open while the ship moves; entry flags describe the previous screen.
			if (_currentScreen is ShipInterior interior)
			{
				var ship = interior.Ship;
				GameData.ActiveSaveFile.CurrentPlanet = ship.PlanetLocation;
				ground = ship is Shuttle shuttle && shuttle.OnGround;
				orbit = !ground;
				if (_menuScreen != null && ship.ShipState == Ship_States.InTransit)
				{
					_menuScreen.MenuButtons = OverviewMenuButtons;
					_menuScreen.Star.Text = ship.StarLocation.ToScreenString(" ");
					UpdateLocationText(orbit);
					_menuScreen.SetupMenus();
					return;
				}
				if (!ground && ship.ShipState == Ship_States.UnDocked)
				{
					var planet = GetCurrentPlanet();
					_menuScreen?.SetMethanoidIndicator(planet.ActiveMethanoid && planet.Station.Built);
				}
			}
			if (_menuScreen != null && _currentScreen.SceneFilePath.Contains("Overview") && !orbit)
			{
				_menuScreen.MenuButtons = OverviewMenuButtons;
				_menuScreen.Location.Text = "MASTER CONTROL";
				_menuScreen.Star.Text = GetCurrentPlanet().ParentStar.ToScreenString(" ");
				_menuScreen.SetupMenus();
			}
			else if (_menuScreen != null && GetCurrentPlanet().PlanetId == Enums.StellarBodies.earth && ground)
			{
				_menuScreen.MenuButtons = EarthMenuButtons;
				_menuScreen.Star.Text = "The Sun";
				UpdateLocationText(false);
				_menuScreen.SetupMenus();
				Earth.GroundSelected = true;
			}
			else if (_menuScreen != null && GetCurrentPlanet().PlanetId == Enums.StellarBodies.earth && orbit)
			{
				_menuScreen.MenuButtons = EarthStationMenuButtons;
				_menuScreen.Star.Text = "The Sun";

				UpdateLocationText(true);

				_menuScreen.SetupMenus();
				Earth.GroundSelected = false;
			}
			else if (_menuScreen != null && GetCurrentPlanet().PlanetId != Enums.StellarBodies.earth)
			{
				if (GetCurrentPlanet().Station.Built && !GetCurrentPlanet().ActiveMethanoid)
					_menuScreen.MenuButtons = StandardMenuButtons;
				else
					_menuScreen.MenuButtons = OverviewMenuButtons;

				UpdateLocationText(orbit);
				_menuScreen.Star.Text = GetCurrentPlanet().ParentStar.ToScreenString(" ");
				_menuScreen.SetupMenus();
			}

			if (_menuScreen != null && orbit && !ground && SdmSystem.CanAccess(GameData.ActiveSaveFile, GetCurrentPlanet()))
			{
				_menuScreen.MenuButtons[3] = new Objects.MenuButton(Menu_Buttons.SDM, Scenes.SelfDestruct, true,
					new Godot.Collections.Array<SceneVariables> { Enums.SceneVariables.Orbit }, null,
					() => SdmSystem.CanAccess(GameData.ActiveSaveFile, GetCurrentPlanet()), "Self-destruct");
				_menuScreen.SetupMenus();
			}

		}
		public static void LockScreen(string lockMessage = "")
		{
			lock (SingletonInstance._screenLocker)
			{
				SingletonInstance._lockCount++;
				SingletonInstance._screenLocker.SetBlocked(true);

				GD.PushWarning("Screen locked (" + lockMessage + ") Count " + SingletonInstance._lockCount.ToString());
			}
		}

		public static void UnLockScreen()
		{
			lock (SingletonInstance._screenLocker)
			{
				SingletonInstance._lockCount = Math.Max(0, SingletonInstance._lockCount - 1);

				if (SingletonInstance._lockCount == 0)
				{
					SingletonInstance._screenLocker.SetBlocked(false);
					GD.PushWarning("Screen unlocked");
				}
				else
				{
					GD.PushWarning("Screen unlock denied Count " + SingletonInstance._lockCount.ToString());
				}
			}
		}

		public static PlanetType GetPlanet<PlanetType>(Enums.StellarBodies planet)
		{
			return (PlanetType)SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[planet];
		}

		public static Earth Earth
		{
			get
			{
				return (Earth)SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];
			}

			set
			{
				SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth] = value;
			}
		}
	}
}
