using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using StoreScreen = Deuteros.Code.Platform.Screens.Store;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private const string ShipParts = "ShipContainer/ScrollContainer2/HBoxContainer/";
        private GlobalInput Cursor => GameCore.SingletonInstance.GetNode<GlobalInput>("VirtualCursorView");
        private T ActiveScreen<T>() where T : Node => GameCore.SingletonInstance.GetNode("MainScene")
            .GetChildren().OfType<T>().Last(n => !n.IsQueuedForDeletion());
        private async Task InputFrames()
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        public async Task RunNavigationRegressions()
        {
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
                await CheckAsync($"Ship bay hover labels cover every {type} section", () => BayHoverLabels(type));
            await CheckAsync("Empty ship bay hover labels describe new IOS and SCG", EmptyBayHoverLabels);
            await CheckAsync("Ship bay hover labels follow staff and pod state", ContextualBayHover);
            await CheckAsync("Viewport hover works and clears on hide disable and scene exit", PointerBayHover);
            await CheckAsync("Viewport right-click leaves ship bay over interactive controls", () => RightClickNavigation(Scenes.ShipBay));
            await CheckAsync("Viewport right-click leaves orbital stores and MTX", () => RightClickNavigation(Scenes.Store));
            await CheckAsync("Overview shortcut respects station availability and button release", NavigationEligibility);
            await CheckAsync("Overview shortcut respects screen cursor UI and overlay locks", NavigationLocks);
            await CheckAsync("Right-click closes bay panels before navigating to overview", BayPanelNavigation);
            await CheckAsync("Right-click closes interior modals before navigating to overview", InteriorPanelNavigation);
            await CheckAsync("Right-click cannot interrupt timed grapple unloading", GrappleNavigationLock);
        }

        private async Task<ShipBay> NavigationBay(Ship_Types type = Ship_Types.IOS)
        {
            InitializeUi();
            var ship = LoadedDismantleShip(type);
            ship.Name = "Navigation regression";
            ship.StarLocation = ship.DestinationStarLocation = StellarBodies.the_sun;
            ship.DestinationPlanetLocation = StellarBodies.earth;
            ship.ACC = null;
            foreach (var module in ship.Modules)
            {
                module.ModuleType = Module_Types.None;
                module.StaffStored = null;
                module.ItemStored = ItemTypes.none;
                module.ItemCount = 0;
            }
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            GameCore.Earth.ActiveMethanoid = false;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipBay, new List<SceneVariables>
            {
                type == Ship_Types.Shuttle ? SceneVariables.Ground : SceneVariables.Orbit,
                type == Ship_Types.Shuttle ? SceneVariables.Shuttle : SceneVariables.Ship
            });
            await InputFrames();
            return ActiveScreen<ShipBay>();
        }

        private async Task BayModuleBackground()
        {
            async Task Click(Vector2 point)
            {
                GetViewport().PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point }, true);
                foreach (var pressed in new[] { true, false })
                    GetViewport().PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed }, true);
                await InputFrames();
            }

            foreach (var hull in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                var bay = await NavigationBay(hull);
                var ship = bay.Ship;
                var day = Save.CurrentDay;
                var fuel = ship.Fuel;
                for (var mount = 1; mount <= ship.Modules.Count; mount++)
                {
                    Press(bay, "Buttons/ShipNav/Nav_Torso" + mount);
                    await ToSignal(GetTree().CreateTimer(1.1), SceneTreeTimer.SignalName.Timeout);
                    var torso = ShipParts + "Torso" + mount;
                    bay.ResourceList.Stores[ItemTypes.supply_pod] = 1;
                    GameCore.SingletonInstance.GameData.GetItem(ItemTypes.supply_pod).Locked = false;
                    await Click(bay.GetNode<Control>(torso + "/SpriteHolder/Buttons/AddSupplyPod").GetGlobalRect().GetCenter());
                    Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "pod button does not return to cockpit");
                    Equal(Module_Types.Supply, ship.Modules[mount - 1].ModuleType, "pod button still fits pod");
                    Equal(0, bay.ResourceList.Stores[ItemTypes.supply_pod], "fitting consumes one pod");
                    var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker");
                    var deadline = Time.GetTicksMsec() + 3000;
                    while (blocker.Blocked && Time.GetTicksMsec() < deadline) await InputFrames();
                    Equal(false, blocker.Blocked, "pod animation releases input before navigation");
                    var point = new Vector2(114, 82); // Empty area identified in Craig's screenshot.
                    blocker.SetBlocked(true);
                    try { await Click(point); Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "input lock blocks background"); }
                    finally { blocker.SetBlocked(false); }
                    await Click(bay.GetNode<Control>(torso + "/SpriteHolder/Buttons/ActivatePod").GetGlobalRect().GetCenter());
                    Equal(true, Cursor.IsLocked, "cargo control opens its panel");
                    await Click(point);
                    Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "cargo panel prevents cockpit navigation");
                    bay.GetNode<Control>("CargoService").Visible = false;
                    Cursor.Unlock();
                    await Click(point);
                    Equal(Scenes.ShipInterior, GameCore.SingletonInstance.currentScene, "empty module background opens cockpit");
                    Equal(ship, ActiveScreen<ShipInterior>().Ship, "same ship selected");
                    Equal(day, Save.CurrentDay, "navigation preserves day");
                    Equal(fuel, ship.Fuel, "navigation preserves fuel");
                    Press(ActiveScreen<ShipInterior>(), "Service");
                    await InputFrames();
                    bay = ActiveScreen<ShipBay>();
                }
            }
        }

        private void ExpectHover(Control button, string expected)
        {
            GameCore.HoverText = "";
            button.EmitSignal(Control.SignalName.MouseEntered);
            ActiveScreen<MainMenu>()._Process(0);
            Equal(expected, ActiveScreen<MainMenu>().HoverInfo.Text, "hover " + button.GetPath());
            button.EmitSignal(Control.SignalName.MouseExited);
            Equal("", GameCore.HoverText, "hover cleared");
        }

        private async Task BayHoverLabels(Ship_Types type)
        {
            var bay = await NavigationBay(type);
            var labels = new Dictionary<string, string>
            {
                ["Buttons/Nav_Dismantle"] = "Dismantle ship",
                ["Buttons/ShipNav/Nav_Cockpit"] = "Crew section",
                ["Buttons/ShipNav/Nav_Engine"] = "Engine mounting",
                ["Fuel/FuelGauge/Minus/RepeatingButton"] = "Unload fuel",
                ["Fuel/FuelGauge/Plus/RepeatingButton"] = "Fuel ship",
                [ShipParts + "Cockpit/Buttons/AddACC"] = "Fit A.C.C.",
                [ShipParts + "Cockpit/OpenShipInterior"] = "Access ship",
                [ShipParts + "Engine/OpenShipInterior"] = "Access ship"
            };
            for (int i = 1; i <= bay.Ship.Modules.Count; i++)
            {
                labels["Buttons/ShipNav/Nav_Torso" + i] = "Pod mount " + i;
                labels[ShipParts + "Torso" + i + "/OpenShipInterior"] = "Access ship";
                labels[ShipParts + "Torso" + i + "/SpriteHolder/Buttons/AddSupplyPod"] = "Install supply pod";
                labels[ShipParts + "Torso" + i + "/SpriteHolder/Buttons/AddToolPod"] = "Install tool pod";
                labels[ShipParts + "Torso" + i + "/SpriteHolder/Buttons/AddCryoPod"] = "Install team pod";
            }
            var fuel = bay.Ship.Fuel;
            foreach (var entry in labels) ExpectHover(bay.GetNode<Control>(entry.Key), entry.Value);
            Equal(fuel, bay.Ship.Fuel, "hover does not transfer fuel");
            Equal(true, bay.Ship.Modules.All(m => m.ModuleType == Module_Types.None), "hover does not install pods");
        }

        private async Task EmptyBayHoverLabels()
        {
            await NavigationBay();
            Save.Ships.Clear();
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.g_chassis).Locked = false;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipBay, new List<SceneVariables> { SceneVariables.Orbit, SceneVariables.Ship });
            await InputFrames();
            var bay = ActiveScreen<ShipBay>();
            ExpectHover(bay.GetNode<Control>("Buttons/Nav_Create_IOS"), "Build IOS");
            ExpectHover(bay.GetNode<Control>("Buttons/Nav_Create_SCG"), "Build SCG");
            ExpectHover(bay.GetNode<Control>("Buttons/Nav_Dismantle"), "");
        }

        private async Task ContextualBayHover()
        {
            var bay = await NavigationBay(Ship_Types.Shuttle);
            bay.ResourceList.RemoveAllStaff();
            bay.ResourceList.Staff[1] = new Staff { Type = StaffType.Production, Count = 5, Leader = "Builder" };
            bay.ResourceList.Staff[2] = new Staff { Type = StaffType.Marines, Count = 1, Leader = "Pilot" };
            GameCore.Earth.Factory.Builder = null;
            var roster = bay.GetNode<Deuteros.Code.Platform.StaffList>(ShipParts + "Cockpit/StaffList");
            roster.UpdateStaff(bay.ResourceList.Staff);
            ExpectHover(roster.GetNode<Control>("Staff/Buttons/01"), "Remove ship's crew");
            ExpectHover(roster.GetNode<Control>("Staff/Buttons/02"), "Assign crew to production");
            ExpectHover(roster.GetNode<Control>("Staff/Buttons/03"), "Assign ship's crew");
            Press(roster, "Staff/Buttons/02");
            ExpectHover(roster.GetNode<Control>("Staff/Buttons/02"), "Remove ship's crew");
            bay.Ship.Pilot = null;
            ExpectHover(roster.GetNode<Control>("Staff/Buttons/02"), "");
            var pod = bay.GetNode<Control>(ShipParts + "Torso1/SpriteHolder/Buttons/AddSupplyPod");
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.supply_pod).Locked = false;
            bay.ResourceList.Stores[ItemTypes.supply_pod] = 1;
            Press(bay, ShipParts + "Torso1/SpriteHolder/Buttons/AddSupplyPod");
            ExpectHover(pod, "Remove supply pod");
            bay.Ship.Modules[0].ItemStored = ItemTypes.iron;
            bay.Ship.Modules[0].ItemCount = 1;
            ExpectHover(pod, "Unload pod before removal");
            bay = await NavigationBay();
            bay.ResourceList.Staff[0] = new Staff { Type = StaffType.Production, Count = 5, Leader = "Builder" };
            bay.GetNode<Deuteros.Code.Platform.StaffList>(ShipParts + "Cockpit/StaffList").UpdateStaff(bay.ResourceList.Staff);
            var assignment = bay.GetNode<Control>(ShipParts + "Cockpit/StaffList/Staff/Buttons/01");
            var factory = GameCore.Earth.Station.Factory;
            factory.Builder = null;
            factory.AOC = false;
            ExpectHover(assignment, "Assign crew to production");
            factory.AOC = true;
            ExpectHover(assignment, "");
            factory.AOC = false;
            factory.Builder = new Staff { Type = StaffType.Production, Count = 1 };
            ExpectHover(assignment, "");
            factory.Builder = null;
            var staff = bay.ResourceList.Staff[0];
            Press(bay, ShipParts + "Cockpit/StaffList/Staff/Buttons/01");
            Equal(staff, factory.Builder, "orbital hover describes the actual assignment");
            Equal(5, factory.Builder.Count, "assignment preserves transported staff");
            ExpectHover(assignment, "Remove ship's crew");
        }

        private async Task PointAt(Control control)
        {
            var motion = new InputEventMouseMotion { Position = control.GetGlobalRect().GetCenter(), GlobalPosition = control.GetGlobalRect().GetCenter() };
            control.GetViewport().PushInput(motion, true);
            await InputFrames();
        }

        private async Task PointerBayHover()
        {
            // A headless native window has no physical mouse. A SubViewport performs
            // real GUI hit testing from injected positions without DisplayServer state.
            var viewport = new SubViewport { Size = new Vector2I(320, 200), GuiDisableInput = false };
            AddChild(viewport);
            try { await PointerBayHoverInViewport(viewport); }
            finally { viewport.Free(); }
        }

        private async Task PointerBayHoverInViewport(SubViewport viewport)
        {
            var bay = await NavigationBay();
            bay.Reparent(viewport);
            await InputFrames();
            bay.ResourceList.RemoveAllStaff();
            bay.GetNode<Deuteros.Code.Platform.StaffList>(ShipParts + "Cockpit/StaffList").UpdateStaff(bay.ResourceList.Staff);
            foreach (var entry in new[] { ("Buttons/ShipNav/Nav_Cockpit", "Crew section"),
                ("Fuel/FuelGauge/Plus/RepeatingButton", "Fuel ship"),
                (ShipParts + "Cockpit/StaffList/Staff/Buttons/01", "Remove ship's crew") })
            {
                await PointAt(bay.GetNode<Control>(entry.Item1));
                Equal(entry.Item2, ActiveScreen<MainMenu>().HoverInfo.Text, "viewport hover " + entry.Item1);
            }
            var button = bay.GetNode<BaseButton>("Buttons/Nav_Dismantle");
            await PointAt(button);
            Equal("Dismantle ship", GameCore.HoverText, "before disable");
            button.Disabled = true;
            await InputFrames();
            Equal("", GameCore.HoverText, "disabled hovered button clears");
            button.Disabled = false;
            button.EmitSignal(Control.SignalName.MouseEntered);
            button.Hide();
            Equal("", GameCore.HoverText, "hidden hovered button clears");
            bay.GetNode<Control>("Buttons/ShipNav/Nav_Cockpit").EmitSignal(Control.SignalName.MouseEntered);
            GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            Equal("", GameCore.HoverText, "scene exit clears hover");
        }

        private async Task RightClick(Vector2 position, bool press = true)
        {
            // Godot may retain an unhandled event until physics picking. Do not
            // Dispose injected mouse events when this managed call returns.
            var click = new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = press, Position = position, GlobalPosition = position };
            GetViewport().PushInput(click, true);
            if (press)
            {
                var release = new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = false, Position = position, GlobalPosition = position };
                GetViewport().PushInput(release, true);
            }
            await InputFrames();
        }

        private async Task RightClickNavigation(Scenes scene)
        {
            await NavigationBay();
            var paths = scene == Scenes.ShipBay ? new[] { ShipParts + "Cockpit/OpenShipInterior", "Buttons/ShipNav/Nav_Torso1", "Fuel/FuelGauge/Plus/RepeatingButton", "ShipBay_Empty" }
                : new[] { "TradStore/StoreButtons", "TradStore/ResourceList", "TradStore/SwitchStoreImage/SwitchStoreType", "TradStore/ButtonsImage" };
            foreach (var path in paths)
            {
                GameCore.SingletonInstance.ChangeScene(scene, new List<SceneVariables> { SceneVariables.Orbit, SceneVariables.Ship });
                await InputFrames();
                var screen = scene == Scenes.ShipBay ? (Node)ActiveScreen<ShipBay>() : ActiveScreen<StoreScreen>();
                var fuel = Save.Ships[0].Fuel;
                var alternate = GameCore.Earth.Station.Resources.Stores.AlternativeView;
                await RightClick(screen.GetNode<Control>(path).GetGlobalRect().GetCenter());
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "overview from " + path);
                Equal(fuel, Save.Ships[0].Fuel, "right-click does not fuel ship");
                Equal(alternate, GameCore.Earth.Station.Resources.Stores.AlternativeView, "right-click does not switch stores");
            }
            if (scene == Scenes.Store)
            {
                Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver);
                GameCore.Earth.Station.MtxInstalled = true;
                GameCore.Earth.Station.Resources.Stores.AlternativeView = true;
                GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
                await InputFrames();
                await RightClick(ActiveScreen<StoreScreen>().GetNode<Control>("MTX/Imagery/Background").GetGlobalRect().GetCenter());
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "overview from MTX");
            }
        }

        private async Task NavigationEligibility()
        {
            var bay = await NavigationBay();
            var point = bay.GetNode<Control>("Buttons/ShipNav/Nav_Cockpit").GetGlobalRect().GetCenter();
            await RightClick(point, false);
            Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "release alone ignored");
            foreach (var planet in Save.BaseGameData.Planets.Values) planet.Station.BuildParts = 0;
            await RightClick(point);
            Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "no player station");
            GameCore.Earth.Station.BuildParts = 8;
            GameCore.Earth.ActiveMethanoid = true;
            await RightClick(point);
            Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "enemy station excluded");
            GameCore.Earth.ActiveMethanoid = false;
            await RightClick(point);
            Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "eligible station");
            var overview = ActiveScreen<global::Overview>();
            await RightClick(point);
            Equal(overview, ActiveScreen<global::Overview>(), "overview not reconstructed");
        }

        private async Task NavigationLocks()
        {
            var bay = await NavigationBay();
            var point = bay.GetNode<Control>("Buttons/ShipNav/Nav_Cockpit").GetGlobalRect().GetCenter();
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker");
            try
            {
                blocker.SetBlocked(true);
                await RightClick(point);
                Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "screen lock");
                blocker.SetBlocked(false);
                Cursor.LockToRect(new Rect2(point, new Vector2(10, 10)));
                await RightClick(point);
                Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "cursor lock");
                Cursor.Unlock();
                GlobalInput.LockUi();
                await RightClick(point);
                Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "UI lock");
                GlobalInput.UnlockUi();
                OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Base/Settings.tscn"));
                await RightClick(point);
                Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "settings overlay");
                OverlayManager.Instance.CloseOverlay();
                await InputFrames();
                await RightClick(point);
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "unlocked navigation restored");
            }
            finally
            {
                blocker.SetBlocked(false);
                Cursor.Unlock();
                GlobalInput.UnlockUi();
                if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay();
            }
        }

        private async Task BayPanelNavigation()
        {
            foreach (var kind in new[] { Module_Types.Supply, Module_Types.Tool, Module_Types.Cryo })
            {
                var bay = await NavigationBay();
                bay.Ship.Modules[0].ModuleType = kind;
                Press(bay, "Buttons/ShipNav/Nav_Torso1");
                Press(bay, ShipParts + "Torso1/SpriteHolder/Buttons/ActivatePod");
                Equal(true, Cursor.IsLocked, "panel opened");
                var panel = bay.GetNode<Control>(kind == Module_Types.Supply ? "CargoService" : kind == Module_Types.Tool ? "EquipmentStock" : "StaffList");
                var otherMount = bay.GetNode<Control>("Buttons/ShipNav/Nav_Torso2").GetGlobalRect().GetCenter();
                GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true, Position = otherMount, GlobalPosition = otherMount }, true);
                GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false, Position = otherMount, GlobalPosition = otherMount }, true);
                await InputFrames();
                Equal(1, bay.ScreenState, "open " + kind + " panel prevents changing its target mount");
                await RightClick(panel.GetGlobalRect().GetCenter());
                Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "first click only closes panel");
                Equal(false, panel.Visible, "panel closed");
                Equal(false, Cursor.IsLocked, "cursor unlocked");
                await RightClick(bay.GetNode<Control>("Buttons/ShipNav/Nav_Cockpit").GetGlobalRect().GetCenter());
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "second click navigates");
            }
        }

        private async Task GrappleNavigationLock()
        {
            var bay = await NavigationBay();
            bay.Ship.Modules[0].ModuleType = Module_Types.Tool;
            bay.Ship.Modules[0].ItemStored = ItemTypes.grapple;
            bay.Ship.Modules[0].ItemCount = 1;
            bay.Ship.Modules[0].HeldItem = new Asteroid { GrappleItemType = GrappleItemTypes.Asteroid, Type = ItemTypes.iron, Mass = 37 };
            bay.ResourceList.Stores[ItemTypes.iron] = 0;
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker");
            Press(bay, "Buttons/ShipNav/Nav_Torso1");
            Press(bay, ShipParts + "Torso1/SpriteHolder/Buttons/ActivatePod");
            try
            {
                await RightClick(bay.GetNode<Control>("Buttons/ShipNav/Nav_Cockpit").GetGlobalRect().GetCenter());
                Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "unloading stays in bay");
                Equal(true, blocker.Blocked, "right-click cannot release unload lock");
                await ToSignal(GetTree().CreateTimer(5.1), SceneTreeTimer.SignalName.Timeout);
                Equal(false, blocker.Blocked, "normal completion releases lock");
                Equal<GrappleItem>(null, bay.Ship.Modules[0].HeldItem, "salvage unloaded");
                Equal(37, bay.ResourceList.Stores[ItemTypes.iron], "cargo credited once");
                await RightClick(bay.GetNode<Control>("Buttons/ShipNav/Nav_Cockpit").GetGlobalRect().GetCenter());
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "navigation restored after completion");
            }
            finally { if (blocker.Blocked) GameCore.UnLockScreen(); }
        }

        private async Task InteriorPanelNavigation()
        {
            foreach (var panel in new[] { "course", "acc", "grapple", "ama" })
            {
                await NavigationBay();
                var ship = (IOS)Save.Ships[0];
                ship.ACC = new Deuteros.Code.Objects.ACC
                {
                    Ship = ship, Source = StellarBodies.earth, Destination = StellarBodies.the_moon,
                    SourceItems = new List<ItemTypes>(), DestinationItems = new List<ItemTypes>(),
                    CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron
                };
                if (panel == "grapple" || panel == "ama") ship.ShipState = Ship_States.UnDocked;
                var mount = panel == "grapple" ? 1 : panel == "ama" ? 2 : 0;
                ship.Modules[mount].ModuleType = Module_Types.Tool;
                ship.Modules[mount].ItemStored = panel == "ama" ? ItemTypes.a__m__a : ItemTypes.grapple;
                ship.Modules[mount].ItemCount = 1;
                GameCore.SingletonInstance.ShipSelected = ship.ShipID;
                GameCore.SingletonInstance.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
                await InputFrames();
                var interior = ActiveScreen<ShipInterior>();
                if (panel == "course") Press(interior, "SetCourse");
                else if (panel == "acc") Press(interior, "OpenACC");
                else Press(interior, "Modules/" + mount.ToString("00"));
                var holder = interior.GetNode<Control>(panel == "course" ? "StarMap" : panel == "acc" ? "ACCScreen" : panel == "grapple" ? "GrappleHolder" : "AMAHolder");
                Equal(true, holder.GetChildCount() > 0, "modal opened " + panel);
                var modal = holder.GetChild(0);
                if (panel == "grapple" || panel == "ama")
                    Equal(panel == "grapple" ? "2" : "3", modal.GetNode<Label>("Window/Background/Number").Text, "selected mount number " + panel);
                await RightClick(holder.GetGlobalRect().GetCenter());
                Equal(Scenes.ShipInterior, GameCore.SingletonInstance.currentScene, "first click closes only " + panel);
                Equal(0, holder.GetChildCount(), "modal removed " + panel);
                Equal(false, GodotObject.IsInstanceValid(modal), "closed modal freed " + panel);
                await RightClick(interior.GetNode<Control>("SetCourse").GetGlobalRect().GetCenter());
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "second click navigates after " + panel);
            }
        }
    }
}
