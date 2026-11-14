using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using AccScreen = Deuteros.Code.Platform.Screens.ACC;
using MenuModel = Deuteros.Code.Objects.MenuButton;
using MenuControl = Deuteros.Code.Platform.MenuButton;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private bool uiInitialized;

        private void InitializeUi()
        {
            if (uiInitialized) return;
            // Initialize the persistent menu without starting background audio.
            GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables> { SceneVariables.Ground });
            uiInitialized = true;
        }

        private void CheckUi(string name, Action test)
        {
            Check(name, () =>
            {
                InitializeUi();
                test();
            });
        }

        public void RunUiRegressions()
        {
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                CheckUi($"New {type} owns its automatically installed ACC", () => AutomaticAccOwner(type, false));
                CheckUi($"Replacement {type} ACC does not retain dismantled ship", () => AutomaticAccOwner(type, true));
            }
            CheckUi("Empty menu slot drops old action and can be re-enabled", () => InactiveMenu(false));
            CheckUi("Unavailable menu slot drops old action and can be re-enabled", () => InactiveMenu(true));
            CheckUi("Course selection cancels safely from system view", () => CourseSelection("system", false));
            CheckUi("Course selection cancels safely from galaxy view", () => CourseSelection("galaxy", false));
            CheckUi("Moon selection updates outbound course and ACC destination", () => CourseSelection("moon", false));
            CheckUi("Moon selection updates return course and ACC source", () => CourseSelection("moon", true));
            CheckUi("SCG drone transfer displays and updates star-drone stock", ScgDronePool);
            CheckUi("SCG under attack keeps a visible overview icon", ScgAttackIcon);
            CheckUi("Selecting unavailable equipment does not duplicate the installed AMA", UnavailableEquipmentKeepsAma);
            CheckUi("First IOS course keeps origin on source side and loads the matching stock", FirstIosAccRoute);
        }

        public async Task RunTimedUiRegressions()
        {
            await CheckAsync("Three grapple pods unload consecutively without reopening the bay", async () =>
            {
                InitializeUi();
                await UnloadAllGrapples();
            });
        }

        private ShipBay OpenEquippedBay(IOS ship)
        {
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            return OpenUi<ShipBay>("res://Screens/ShipBay.tscn", new List<SceneVariables> { SceneVariables.Orbit, SceneVariables.Ship });
        }

        private void UnavailableEquipmentKeepsAma()
        {
            var ship = NavigationShip();
            ship.Modules[0].ModuleType = Module_Types.Tool;
            ship.Modules[0].ItemStored = ItemTypes.a__m__a;
            ship.Modules[0].ItemCount = 1;
            var ama = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.a__m__a);
            var grapple = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.grapple);
            ama.Research.Researched = grapple.Research.Researched = true;
            ama.Research.ResearchOrder = 100;
            grapple.Research.ResearchOrder = 101;
            var stores = GameCore.Earth.Station.Resources.Stores;
            stores[ItemTypes.a__m__a] = 2;
            stores[ItemTypes.grapple] = 0;
            var existingTweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenEquippedBay(ship);
            try
            {
                Press(bay, "Buttons/ShipNav/Nav_Torso1");
                Press(bay, "ShipContainer/ScrollContainer2/HBoxContainer/Torso1/SpriteHolder/Buttons/ActivatePod");
                PressEquipmentNamed(bay, grapple.ShortName);
                Equal(2, stores[ItemTypes.a__m__a], "unavailable replacement must not refund AMA");
                Equal(ItemTypes.a__m__a, ship.Modules[0].ItemStored, "AMA remains installed");
                Equal(1, ship.Modules[0].ItemCount, "installed count unchanged");
                PressEquipmentNamed(bay, ama.ShortName);
                Equal(3, stores[ItemTypes.a__m__a], "actual removal refunds exactly one AMA");
                Equal(ItemTypes.none, ship.Modules[0].ItemStored, "pod emptied");
                Equal(0, ship.Modules[0].ItemCount, "pod count cleared");
            }
            finally
            {
                GetTree().CurrentScene.GetNode<GlobalInput>("GameContainer/GameViewport/VirtualCursorView").Unlock();
                foreach (var tween in GetTree().GetProcessedTweens().Where(t => !existingTweens.Contains(t))) tween.Kill();
                bay.Free();
            }
        }

        private static void PressEquipmentNamed(ShipBay bay, string name)
        {
            for (int i = 0; i < 11; i++)
            {
                var index = i.ToString("00");
                if (bay.GetNode<Label>("EquipmentStock/BackgroundBox/PanelContainer/GridContainer/NameColumn/" + index).Text == name)
                {
                    Press(bay, "EquipmentStock/Buttons/" + index);
                    return;
                }
            }
            throw new InvalidOperationException("Equipment row not found: " + name);
        }

        private void FirstIosAccRoute()
        {
            Save.Ships.Clear();
            var ship = NavigationShip();
            ship.Modules[0].ModuleType = Module_Types.Supply;
            ship.ACC = new Deuteros.Code.Objects.ACC
            {
                Ship = ship, Source = StellarBodies.earth, Destination = StellarBodies.earth,
                SourceItems = new List<ItemTypes>(), DestinationItems = new List<ItemTypes>(),
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron
            };
            Save.Ships.Add(ship);
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            var interior = OpenUi<ShipInterior>("res://Screens/ShipInterior.tscn");
            try
            {
                Press(interior, "SetCourse");
                var map = interior.GetNode<Control>("StarMap").GetChildren().OfType<StarMap>().Single();
                Press(map, "PlanetHolder/Moons/Moon02");
                using var rightClick = new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true };
                interior._Input(rightClick);
                Press(interior, "OpenACC");
                var acc = interior.GetNode<Control>("ACCScreen").GetChildren().OfType<AccScreen>().Single();
                Equal("Earth", acc.GetNode<Label>("Window/Text/SourceName").Text, "source label is the origin");
                Equal("The Moon", acc.GetNode<Label>("Window/Text/DestinationName").Text, "destination label is the selected course");
                Press(acc, "Window/SourceButtons/Col01/00");
                Press(acc, "Window/DestinationButtons/Col01/03");
                GameCore.Earth.Station.Resources.Stores[ItemTypes.iron] = 12;
                Save.BaseGameData.Planets[StellarBodies.the_moon].Station.Resources.Stores[ItemTypes.carbon] = 17;
                ship.ACC.LoadSupply();
                Equal(ItemTypes.iron, ship.Modules[0].ItemStored, "source selection loads at Earth");
                Equal(12, ship.Modules[0].ItemCount, "source quantity");
                ship.PlanetLocation = StellarBodies.the_moon;
                ship.DestinationPlanetLocation = StellarBodies.earth;
                ship.ACC.LoadSupply();
                Equal(ItemTypes.carbon, ship.Modules[0].ItemStored, "destination selection loads at Moon");
                Equal(17, ship.Modules[0].ItemCount, "destination quantity");
            }
            finally
            {
                GetTree().CurrentScene.GetNode<GlobalInput>("GameContainer/GameViewport/VirtualCursorView").Unlock();
                interior.Free();
            }
        }

        private async Task UnloadAllGrapples()
        {
            var ship = NavigationShip();
            var stores = GameCore.Earth.Station.Resources.Stores;
            stores[ItemTypes.iron] = 12;
            stores[ItemTypes.carbon] = 49900;
            stores[ItemTypes.platinum] = 0;
            for (int i = 0; i < 3; i++)
            {
                ship.Modules[i].ModuleType = Module_Types.Tool;
                ship.Modules[i].ItemStored = ItemTypes.grapple;
                ship.Modules[i].ItemCount = 1;
                ship.Modules[i].HeldItem = new Asteroid
                {
                    GrappleItemType = GrappleItemTypes.Asteroid,
                    Type = i == 0 ? ItemTypes.iron : i == 1 ? ItemTypes.carbon : ItemTypes.platinum,
                    Mass = i == 0 ? 250 : i == 1 ? 150 : 50
                };
            }
            var viewport = new SubViewport { Size = new Vector2I(320, 200), GuiDisableInput = false };
            AddChild(viewport);
            var bay = OpenEquippedBay(ship);
            bay.Reparent(viewport);
            await InputFrames();
            var window = bay.GetNode<DynamicWindow>("GrappleWindow/GrappleEmptier");
            var blocker = GameCore.SingletonInstance.GetNode<Deuteros.Code.Platform.Helpers.InputBlocker>("GameContainer/GameViewport/InputBlocker");
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    ClickMenu(viewport, bay.GetNode<BaseButton>("Buttons/ShipNav/Nav_Torso" + (i + 1)));
                    await ToSignal(GetTree().CreateTimer(1.1), SceneTreeTimer.SignalName.Timeout);
                    Equal((i + 1) * 224, bay.ScrollContainer.ScrollHorizontal, "selected pod is visible before unloading");
                    AdvanceTickDay();
                    await InputFrames();
                    Equal((i + 1) * 224, bay.ScrollContainer.ScrollHorizontal, "day refresh preserves the selected pod");
                    ClickMenu(viewport, bay.GetNode<BaseButton>("ShipContainer/ScrollContainer2/HBoxContainer/Torso" + (i + 1) + "/SpriteHolder/Buttons/ActivatePod"));
                    await InputFrames();
                    Equal(true, window.IsVisibleInTree(), "unload window visible for pod " + (i + 1));
                    Equal(i == 0 ? "250 Iron 262" : i == 1 ? "150 Carbon 50000" : "50 Platinum 50", window.GetNode<Label>("Quantites").Text,
                        "breakup shows held mass and resulting capped stock");
                    Equal(true, blocker.Blocked, "unload locks input");
                    await ToSignal(GetTree().CreateTimer(5.1), SceneTreeTimer.SignalName.Timeout);
                    Equal<GrappleItem>(null, ship.Modules[i].HeldItem, "pod unloaded");
                    Equal(false, blocker.Blocked, "unload releases input");
                }
                Equal(262, stores[ItemTypes.iron], "first pod stock credited once");
                Equal(50000, stores[ItemTypes.carbon], "second pod stock capped");
                Equal(50, stores[ItemTypes.platinum], "third pod credited once");
            }
            finally
            {
                if (blocker.Blocked) GameCore.UnLockScreen();
                viewport.Free();
            }
        }

        private T OpenUi<T>(string path, List<SceneVariables> variables = null) where T : Node
        {
            var scene = GD.Load<PackedScene>(path).Instantiate<T>();
            if (scene is BaseSubScene subScene)
                subScene.SceneVariables = variables ?? new List<SceneVariables>();
            AddChild(scene);
            return scene;
        }

        private static void Press(Node scene, string path)
        {
            scene.GetNode<BaseButton>(path).EmitSignal(BaseButton.SignalName.Pressed);
        }

        private void AutomaticAccOwner(Ship_Types type, bool replacement)
        {
            Save.Ships.Clear();
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            var ground = type == Ship_Types.Shuttle;
            var stores = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
            var chassis = type == Ship_Types.Shuttle ? ItemTypes.s_chassis : type == Ship_Types.IOS ? ItemTypes.i_chassis : ItemTypes.g_chassis;
            stores[chassis] = 2;
            stores[ItemTypes.a__c__c] = 2;
            var existingTweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenUi<ShipBay>("res://Screens/ShipBay.tscn", new List<SceneVariables>
            {
                ground ? SceneVariables.Ground : SceneVariables.Orbit,
                ground ? SceneVariables.Shuttle : SceneVariables.Ship
            });
            AccScreen acc = null;
            try
            {
                var createPath = "Buttons/Nav_Create_" + type;
                Press(bay, createPath);
                if (replacement)
                {
                    Press(bay, "Buttons/Nav_Dismantle");
                    Press(bay, createPath);
                }
                var ship = Save.Ships.Single();
                Equal(type, ship.ShipType, "created type");
                Equal<IShip>(ship, ship.ACC.Ship, "ACC owner");
                Equal(1, stores[ItemTypes.a__c__c], "one ACC installed; dismantling returns the old controller");

                acc = OpenUi<AccScreen>("res://PreFabs/ACC.tscn");
                acc.SetACC(ship.ACC);
                acc.UpdateState();
                Equal(ground ? "Surface" : StellarBodies.earth.ToScreenString(" "),
                    acc.GetNode<Label>("Window/Text/SourceName").Text, "ACC screen renders owner");
                Equal(ground ? "Orbit" : StellarBodies.earth.ToScreenString(" "),
                    acc.GetNode<Label>("Window/Text/DestinationName").Text, "ACC destination label");
            }
            finally
            {
                acc?.Free();
                // These synchronous tests dispose the bay before its scrolling animation gets a frame.
                foreach (var tween in GetTree().GetProcessedTweens().Where(tween => !existingTweens.Contains(tween)))
                    tween.Kill();
                bay.Free();
            }
        }

        private void InactiveMenu(bool unavailable)
        {
            var menu = GameCore.SingletonInstance.GetNode<Node>("GameContainer/GameViewport/MainScene").GetChildren().OfType<MainMenu>().Single();
            var original = menu.MenuButtons;
            var calls = 0;
            var enabled = new MenuModel(Menu_Buttons.Shuttle, Scenes.ShipInterior, true,
                new Godot.Collections.Array<SceneVariables>(), new List<Action> { () => calls++ });
            try
            {
                menu.MenuButtons = Enumerable.Repeat<MenuModel>(null, 12).ToList();
                menu.MenuButtons[2] = enabled;
                menu.SetupMenus();
                var button = menu.GetNode<MenuControl>("MainButtons/A3");
                button._Pressed();
                Equal(1, calls, "enabled callback");

                menu.MenuButtons[2] = unavailable
                    ? new MenuModel(Menu_Buttons.Shuttle, Scenes.ShipInterior, true,
                        new Godot.Collections.Array<SceneVariables>(), enabled.ClickActions, () => false)
                    : null;
                menu.SetupMenus();
                button._Pressed();
                Equal(1, calls, "inactive callback must not execute");
                Equal(true, button.Disabled, "inactive slot is not clickable");

                menu.MenuButtons[2] = enabled;
                menu.SetupMenus();
                Equal(false, button.Disabled, "available slot is clickable again");
                button._Pressed();
                Equal(2, calls, "new enabled callback");
            }
            finally
            {
                menu.MenuButtons = original;
                menu.SetupMenus();
            }
        }

        private IOS NavigationShip()
        {
            return new IOS
            {
                Name = "Regression IOS", ShipType = Ship_Types.IOS, ShipState = Ship_States.Docked,
                PlanetLocation = StellarBodies.earth, StarLocation = StellarBodies.the_sun,
                DestinationPlanetLocation = StellarBodies.earth, DestinationStarLocation = StellarBodies.the_sun,
                Fuel = 100, FuelType = ItemTypes.meh_fuel,
                Modules = Enumerable.Range(0, 3).Select(_ => new ShipModule()).ToList()
            };
        }

        private void CourseSelection(string view, bool returning)
        {
            Save.Ships.Clear();
            Save.Unlocks.Add(Game_Unlocks.Interstellar_Travel);
            var ship = NavigationShip();
            ship.ACC = new Deuteros.Code.Objects.ACC
            {
                Ship = ship, Source = returning ? StellarBodies.mars : StellarBodies.earth,
                Destination = returning ? StellarBodies.earth : StellarBodies.mars
            };
            Save.Ships.Add(ship);
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            var interior = OpenUi<ShipInterior>("res://Screens/ShipInterior.tscn");
            var cursor = GetTree().CurrentScene.GetNode<GlobalInput>("GameContainer/GameViewport/VirtualCursorView");
            try
            {
                Press(interior, "SetCourse");
                var holder = interior.GetNode<Control>("StarMap");
                var map = holder.GetChildren().OfType<StarMap>().Single();
                Equal(true, cursor.IsLocked, "map captures cursor");
                if (view == "moon")
                    Press(map, "PlanetHolder/Moons/Moon02");
                else
                {
                    Press(map, "PlanetHolder/GoBack");
                    if (view == "galaxy") Press(map, "StarSystemHolder/GoBack");
                }
                Equal(view == "moon" ? StellarBodies.the_moon : view == "system" ? StellarBodies.the_sun : StellarBodies.none,
                    map.CurrentLocation, "navigation selection");
                using var rightClick = new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true };
                interior._Input(rightClick);
                Equal(view == "moon" ? StellarBodies.the_moon : StellarBodies.earth, ship.DestinationPlanetLocation, "ship destination");
                Equal(StellarBodies.the_sun, ship.DestinationStarLocation, "ship destination star");
                Equal(view == "moon" && returning ? StellarBodies.the_moon : returning ? StellarBodies.mars : StellarBodies.earth,
                    ship.ACC.Source, "ACC source");
                Equal(view == "moon" && !returning ? StellarBodies.the_moon : returning ? StellarBodies.earth : StellarBodies.mars,
                    ship.ACC.Destination, "ACC destination");
                Equal(false, cursor.IsLocked, "cursor released");
                Equal(0, holder.GetChildCount(), "map removed on close");
            }
            finally
            {
                cursor.Unlock();
                interior.Free();
            }
        }

        private void ScgDronePool()
        {
            var stores = GameCore.Earth.Station.Resources.Stores;
            stores[ItemTypes.ios_drone] = 3;
            stores[ItemTypes.star_drone] = 7;
            var ship = new SCG { PlanetLocation = StellarBodies.earth, DroneCount = 2 };
            Save.Ships.Add(ship);
            var transfer = OpenUi<global::FleetTransfers>("res://PreFabs/ShipModuleWindows/FleetTransfers.tscn");
            try
            {
                transfer._Process(0);
                transfer.TransferDrones(ship);
                var label = transfer.GetNode<Label>("OrbitalColorRect/OrbitalDronePoolLabel");
                Equal(true, label.Text.EndsWith("  7"), "initial star-drone pool");
                stores[ItemTypes.star_drone] = 9;
                transfer._Process(0.016);
                Equal(true, label.Text.EndsWith("  9"), "new star drones appear without a transfer click");
                Press(transfer, "RepeatingButton1");
                Equal(3, ship.DroneCount, "fleet receives drone");
                Equal(true, label.Text.EndsWith("  8"), "pool after loading");
                Press(transfer, "RepeatingButton2");
                Equal(2, ship.DroneCount, "fleet returns drone");
                Equal(true, label.Text.EndsWith("  9"), "pool after unloading");
                Equal(3, stores[ItemTypes.ios_drone], "IOS pool unchanged");

                var ios = new IOS { PlanetLocation = StellarBodies.earth, DroneCount = 2 };
                Save.Ships.Add(ios);
                transfer.TransferDrones(ios);
                stores[ItemTypes.ios_drone] = 4;
                transfer._Process(0.016);
                Equal(true, label.Text.EndsWith("  4"), "new IOS drones appear without a transfer click");
                stores[ItemTypes.ios_drone] = 0;
                ios.DroneCount = 5;
                transfer._Process(0.016);
                Equal(true, label.Text.EndsWith("  0"), "pool reflects withdrawals elsewhere");
                Equal(true, transfer.GetNode<Label>("FleetColorRect/FleetDronePoolLabel").Text.EndsWith("  5"), "fleet count refreshes");
                Equal(true, transfer.GetNode<Label>("PowerColorRect/PowerLabel").Text.EndsWith("  20"), "fleet power refreshes");
            }
            finally { transfer.Free(); }
        }

        private void ScgAttackIcon()
        {
            Save.Ships.Clear();
            Save.AtWar = true;
            GameCore.Earth.ActiveMethanoid = true;
            Save.Ships.Add(new SCG { ShipType = Ship_Types.SCG, PlanetLocation = StellarBodies.earth });
            var overview = OpenUi<global::Overview>("res://Screens/Overview.tscn");
            try
            {
                var button = overview.GetNode<TextureButton>("SCG/Col0/SCG00");
                Equal(true, button.Visible, "attacked ship remains listed");
                Equal("res://Sprites/Buttons/Overview/Ship_UnderAttack.png", button.TextureNormal?.ResourcePath, "attack icon resource");
            }
            finally { overview.Free(); }
        }
    }
}
