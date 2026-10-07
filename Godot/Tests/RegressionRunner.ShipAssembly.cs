using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Platform.Helpers;
using Godot;
using Deuteros.Code.Utility;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunShipAssemblyRegressions()
        {
            await CheckAsync("Entering any ship bay preserves star drive research locks", BayPreservesResearch);
            await CheckAsync("SCG assembly visibility follows SCG chassis availability", ScgAssemblyVisibility);
            foreach (var hull in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                var type = hull;
                await CheckAsync($"{type} assembly consumes a chassis and cannot duplicate an occupied bay", () => AssemblyConsumesChassis(type));
                await CheckAsync($"{type} engine fitting consumes exactly one drive from the correct store", () => AssemblyConsumesEngine(type));
                await CheckAsync($"{type} build fit dismantle conserves chassis drive and ACC", () => AssemblyRoundTrip(type));
            }
            await CheckAsync("Dismantling waits for room for both the chassis and installed drive", AssemblyReturnCapacity);
            foreach (var hull in new[] { Ship_Types.IOS, Ship_Types.SCG })
            {
                var type = hull;
                await CheckAsync($"Repeated {type} assembly cannot create a second docked ship", () => AssemblyOccupiedBay(type));
            }
            await CheckAsync("SCG bay exposes exactly six functional module mounts", ScgModuleMounts);
        }

        private async Task<ShipBay> EmptyAssemblyBay(Ship_Types type, bool orbitalShuttle = false)
        {
            InitializeUi();
            Save.Ships.Clear();
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.Earth.Station.BuildParts = 8;
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.ActiveMethanoid = false;
            var ground = type == Ship_Types.Shuttle && !orbitalShuttle;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipBay, new List<SceneVariables>
            {
                ground ? SceneVariables.Ground : SceneVariables.Orbit,
                type == Ship_Types.Shuttle ? SceneVariables.Shuttle : SceneVariables.Ship
            });
            await InputFrames();
            return ActiveScreen<ShipBay>();
        }

        private async Task BayUnavailablePods()
        {
            foreach (var orbital in new[] { false, true })
            {
                var bay = await EmptyAssemblyBay(Ship_Types.Shuttle, orbital);
                var stores = bay.ResourceList.Stores;
                stores[ItemTypes.s_chassis] = 1;
                Press(bay, "Buttons/Nav_Create_Shuttle");
                Press(bay, "Buttons/ShipNav/Nav_Torso1");
                foreach (var pod in new[] { (ItemTypes.supply_pod, "Supply"), (ItemTypes.tool_pod, "Tool"), (ItemTypes.cryo_pod, "Cryo") })
                {
                    var item = GameCore.SingletonInstance.GameData.GetItem(pod.Item1);
                    var button = ShipParts + "Torso1/SpriteHolder/Buttons/Add" + pod.Item2 + "Pod";
                    foreach (var locked in new[] { false, true })
                    {
                        item.Locked = locked;
                        stores[pod.Item1] = locked ? 1 : 0;
                        var stock = stores.Items.ToDictionary(x => x.Key, x => x.Value);
                        try
                        {
                            Press(bay, button);
                            Equal(Module_Types.None, bay.Ship.Modules[0].ModuleType, "unavailable pod is not fitted");
                            Equal(true, stock.OrderBy(x => x.Key).SequenceEqual(stores.Items.OrderBy(x => x.Key)), "rejection preserves stores");
                            Equal(true, OverlayManager.Instance.IsOpen, "unavailable pod explains rejection");
                            var overlay = OverlayManager.Instance.GetNode("GlobalOverlay/GameArea/Center").GetChild(0);
                            Equal("Pod Not Available", overlay.GetNode<Label>("ErrorButton/OuterColorRect/InnerColorRect/ErrorLabel").Text, "pod error text");
                            await InputFrames();
                            var panel = overlay.GetNode<Control>("ErrorButton/OuterColorRect").GetGlobalRect();
                            var text = overlay.GetNode<Label>("ErrorButton/OuterColorRect/InnerColorRect/ErrorLabel").GetGlobalRect();
                            Equal(true, panel.Encloses(text.Grow(2 * GameViewportContainer.Instance.Scale.X)), "error box encloses text with padding");
                            Equal(true, panel.GetCenter().DistanceTo(GameViewportContainer.Instance.GetGlobalTransformWithCanvas() * new Vector2(160, 100)) < GameViewportContainer.Instance.Scale.X, "error box stays centred");
                            Press(overlay, "ErrorButton");
                            await InputFrames();
                            Equal(false, OverlayManager.Instance.IsOpen, "error dismisses normally");
                        }
                        finally { if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); }
                    }
                    item.Locked = false;
                    Press(bay, button);
                    Equal(false, OverlayManager.Instance.IsOpen, "available pod fits without error");
                    Equal(0, stores[pod.Item1], "fitting consumes one pod");
                    bay.GetNode<Deuteros.Code.Platform.Screens.ShipBayScenes.Torso>(ShipParts + "Torso1")._Process(1.21);
                    Press(bay, button);
                    Equal(Module_Types.None, bay.Ship.Modules[0].ModuleType, "empty pod removes normally");
                    Equal(1, stores[pod.Item1], "removal returns pod");
                    bay.GetNode<Deuteros.Code.Platform.Screens.ShipBayScenes.Torso>(ShipParts + "Torso1")._Process(1.21);
                }
                foreach (var message in new[] { "Not Enough Space In Stores\nTo Dismantle Ship", "Team Leader\nMust Be Rated\nExpert To\nProduce This\nItem" })
                {
                    GameCore.ShowError(bay, message);
                    await InputFrames();
                    var overlay = OverlayManager.Instance.GetNode("GlobalOverlay/GameArea/Center").GetChild(0);
                    var panel = overlay.GetNode<Control>("ErrorButton/OuterColorRect").GetGlobalRect();
                    var label = overlay.GetNode<Label>("ErrorButton/OuterColorRect/InnerColorRect/ErrorLabel");
                    Equal(true, panel.Encloses(label.GetGlobalRect().Grow(2 * GameViewportContainer.Instance.Scale.X)), "multiline error fits panel");
                    Equal(true, GameViewportContainer.Instance.GetGlobalRect().Encloses(panel), "long error fits viewport");
                    Press(overlay, "ErrorButton");
                    await InputFrames();
                }
            }
        }

        private async Task BayPreservesResearch()
        {
            foreach (var locked in new[] { true, false })
            foreach (var hull in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                var research = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.star_drive).Research;
                research.Locked = locked;
                await EmptyAssemblyBay(hull);
                Equal(locked, research.Locked, "viewing bay must not discover star drive");
            }
        }

        private async Task ScgAssemblyVisibility()
        {
            var chassis = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.g_chassis);
            var shuttle = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.s_chassis);
            foreach (var scgLocked in new[] { true, false })
            foreach (var shuttleLocked in new[] { false, true })
            {
                chassis.Locked = scgLocked;
                shuttle.Locked = shuttleLocked;
                var bay = await EmptyAssemblyBay(Ship_Types.SCG);
                Equal(!scgLocked, bay.GetNode<Control>("Buttons/Nav_Create_SCG").Visible, "SCG selection independent of shuttle technology");
            }
        }

        private static ItemTypes AssemblyChassis(Ship_Types type) => type == Ship_Types.Shuttle ? ItemTypes.s_chassis : type == Ship_Types.IOS ? ItemTypes.i_chassis : ItemTypes.g_chassis;
        private static ItemTypes AssemblyDrive(Ship_Types type) => type == Ship_Types.Shuttle ? ItemTypes.s_drive : type == Ship_Types.IOS ? ItemTypes.i_drive : ItemTypes.star_drive;

        private async Task AssemblyConsumesChassis(Ship_Types type)
        {
            var bay = await EmptyAssemblyBay(type);
            var stores = bay.ResourceList.Stores;
            var chassis = AssemblyChassis(type);
            stores[chassis] = 0;
            stores[ItemTypes.a__c__c] = 2;
            Press(bay, "Buttons/Nav_Create_" + type);
            Equal(0, Save.Ships.Count, "no stock cannot build");
            Equal(2, stores[ItemTypes.a__c__c], "rejection takes no ACC");
            stores[chassis] = 2;
            Press(bay, "Buttons/Nav_Create_" + type);
            Equal(1, Save.Ships.Count, "one hull built");
            Equal(1, stores[chassis], "exactly one chassis fitted");
            Equal(1, stores[ItemTypes.a__c__c], "exactly one ACC fitted");
            Equal(type == Ship_Types.Shuttle ? 1 : type == Ship_Types.IOS ? 3 : 6, Save.Ships.Single().Modules.Count, "correct usable mounts");
            // An already-dispatched callback must not create a second hull in this berth.
            Press(bay, "Buttons/Nav_Create_" + type);
            Equal(1, Save.Ships.Count, "repeated assembly cannot occupy bay twice");
            Equal(1, stores[chassis], "repeated assembly takes no parts");
            Equal(1, stores[ItemTypes.a__c__c], "repeated assembly takes no ACC");
        }

        private async Task AssemblyOccupiedBay(Ship_Types type)
        {
            var bay = await EmptyAssemblyBay(type);
            bay.ResourceList.Stores[AssemblyChassis(type)] = 2;
            bay.ResourceList.Stores[ItemTypes.a__c__c] = 2;
            Press(bay, "Buttons/Nav_Create_" + type);
            Press(bay, "Buttons/Nav_Create_" + type);
            Equal(1, Save.Ships.Count, "occupied berth cannot create another hull");
        }

        private async Task AssemblyConsumesEngine(Ship_Types type)
        {
            foreach (var orbitalShuttle in type == Ship_Types.Shuttle ? new[] { false, true } : new[] { false })
            {
                var bay = await EmptyAssemblyBay(type, orbitalShuttle);
                var stores = bay.ResourceList.Stores;
                var other = ReferenceEquals(stores, GameCore.Earth.PlanetResources.Stores) ? GameCore.Earth.Station.Resources.Stores : GameCore.Earth.PlanetResources.Stores;
                var engine = AssemblyDrive(type);
                stores[AssemblyChassis(type)] = 1;
                stores[ItemTypes.a__c__c] = 0;
                stores[engine] = 0;
                other[engine] = 7;
                Press(bay, "Buttons/Nav_Create_" + type);
                var ship = Save.Ships.Single();
                var path = ShipParts + "Engine/SpriteHolder/Buttons/InstallEngine";
                Press(bay, path);
                Equal(false, ship.Engine, "other location cannot supply an engine");
                stores[engine] = 2;
                Press(bay, path);
                Equal(true, ship.Engine, "drive installed");
                Equal(1, stores[engine], "drive removed from local stock");
                Equal(7, other[engine], "other stock untouched");
                Press(bay, path);
                Equal(1, stores[engine], "repeat fitting does not charge again");
            }
        }

        private async Task AssemblyRoundTrip(Ship_Types type)
        {
            var bay = await EmptyAssemblyBay(type);
            var stores = bay.ResourceList.Stores;
            stores.Items.Clear();
            stores[AssemblyChassis(type)] = 2;
            stores[AssemblyDrive(type)] = 3;
            stores[ItemTypes.a__c__c] = 1;
            for (var cycle = 0; cycle < 2; cycle++)
            {
                Press(bay, "Buttons/Nav_Create_" + type);
                Press(bay, ShipParts + "Engine/SpriteHolder/Buttons/InstallEngine");
                Equal(1, stores[AssemblyChassis(type)], "chassis aboard before dismantle");
                Equal(2, stores[AssemblyDrive(type)], "drive aboard before dismantle");
                Equal(0, stores[ItemTypes.a__c__c], "ACC aboard");
                Press(bay, "Buttons/Nav_Dismantle");
                Equal(0, Save.Ships.Count, "hull removed");
                Equal(2, stores[AssemblyChassis(type)], "chassis recovered exactly once");
                Equal(3, stores[AssemblyDrive(type)], "installed drive recovered exactly once");
                Equal(1, stores[ItemTypes.a__c__c], "ACC recovered exactly once");
                Press(bay, "Buttons/Nav_Dismantle");
                Equal(2, stores[AssemblyChassis(type)], "repeat dismantle cannot add chassis");
            }
        }

        private async Task ScgModuleMounts()
        {
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.g_chassis).Locked = false;
            var bay = await EmptyAssemblyBay(Ship_Types.SCG);
            var stores = bay.ResourceList.Stores;
            stores[ItemTypes.g_chassis] = 1;
            stores[ItemTypes.supply_pod] = 6;
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.supply_pod).Locked = false;
            var point = bay.GetNode<Control>("Buttons/Nav_Create_SCG").GetGlobalRect().GetCenter();
            PushGameInput(new InputEventMouseMotion { Position = point, GlobalPosition = point });
            foreach (var pressed in new[] { true, false })
                PushGameInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed });
            await InputFrames();
            Equal(1, Save.Ships.Count, "SCG builds from the button centre through pointer input");
            Equal(true, bay.GetNode<Control>("Buttons/ShipNav/Nav_Torso6").Visible, "sixth mount navigation");
            Equal(true, bay.GetNode<Control>(ShipParts + "Torso6").Visible, "sixth mount artwork");
            for (var mount = 1; mount <= 6; mount++)
            {
                Equal(true, bay.GetNode<Control>("Buttons/ShipNav/Nav_Torso" + mount).Visible, "real mount can be selected");
                Press(bay, "Buttons/ShipNav/Nav_Torso" + mount);
                Press(bay, ShipParts + "Torso" + mount + "/SpriteHolder/Buttons/AddSupplyPod");
                Equal(Module_Types.Supply, Save.Ships.Single().Modules[mount - 1].ModuleType, "pod fitted to selected mount");
                bay.GetNode<Deuteros.Code.Platform.Screens.ShipBayScenes.Torso>(ShipParts + "Torso" + mount)._Process(1.21);
            }
            Equal(0, stores[ItemTypes.supply_pod], "six pods fitted exactly once");
            stores[ItemTypes.iron] = 300;
            Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/ActivatePod");
            Equal(true, bay.GetNode<Control>("CargoService").Visible, "sixth pod service opens");
            Press(bay, "CargoService/Buttons/" + ItemTypes.iron.ToScreenString());
            Equal(250, Save.Ships.Single().Modules[5].ItemCount, "sixth pod receives cargo");
            Equal(50, stores[ItemTypes.iron], "cargo leaves stores exactly once");
            Press(bay, "CargoService/Buttons/" + ItemTypes.iron.ToScreenString());
            Equal(300, stores[ItemTypes.iron], "sixth pod unload conserves stock");
            await RightClick(bay.GetNode<Control>("CargoService").GetGlobalRect().GetCenter());
            Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/AddSupplyPod");
            Equal(Module_Types.None, Save.Ships.Single().Modules[5].ModuleType, "sixth pod removed");
            Equal(1, stores[ItemTypes.supply_pod], "sixth pod returned once");
            await ToSignal(GetTree().CreateTimer(1.1), SceneTreeTimer.SignalName.Timeout);
            await CaptureDisplayEvidence("scg-six-module-mounts");
            Press(bay, "Buttons/ShipNav/Nav_Engine");
            await ToSignal(GetTree().CreateTimer(1.1), SceneTreeTimer.SignalName.Timeout);
            var viewport = bay.GetNode<ScrollContainer>("ShipContainer/ScrollContainer2").GetGlobalRect();
            var engine = bay.GetNode<Control>(ShipParts + "Engine").GetGlobalRect();
            Equal(true, viewport.HasPoint(engine.GetCenter()), "engine remains reachable beyond sixth pod");
            await CaptureDisplayEvidence("scg-six-module-engine");
        }

        private async Task ScgLegacySixthMount()
        {
            var bay = await EmptyAssemblyBay(Ship_Types.SCG);
            bay.ResourceList.Stores[ItemTypes.g_chassis] = 1;
            Press(bay, "Buttons/Nav_Create_SCG");
            var ship = Save.Ships.Single();
            ship.Modules = Enumerable.Range(0, 5).Select(_ => new ShipModule()).ToList();
            ship.Modules[0].ModuleType = Module_Types.Tool;
            ship.Modules[0].ItemStored = ItemTypes.grapple;
            ship.Modules[0].HeldItem = new UnknownItem(UnknownItemTypes.AlienArtifact);
            ship.Modules[1].ModuleType = Module_Types.Cryo;
            ship.Modules[1].StaffStored = new Staff { Leader = "Legacy crew", Type = StaffType.Marines, Count = 10 };
            ship.Modules[4].ModuleType = Module_Types.Supply;
            ship.Modules[4].ItemStored = ItemTypes.iron;
            ship.Modules[4].ItemCount = 123;
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            for (var reload = 0; reload < 2; reload++)
            {
                var loaded = restored.Ships.Single();
                Equal(6, loaded.Modules.Count, "legacy SCG gains one empty mount");
                Equal(123, loaded.Modules[4].ItemCount, "fifth pod cargo retained");
                Equal(ItemTypes.iron, loaded.Modules[4].ItemStored, "fifth pod material retained");
                Equal("Legacy crew", loaded.Modules[1].StaffStored.Leader, "frozen crew retained");
                Equal(10, loaded.Modules[1].StaffStored.Count, "frozen team retained");
                Equal(true, loaded.Modules[0].HeldItem is UnknownItem, "grapple contents retained");
                Equal(Module_Types.None, loaded.Modules[5].ModuleType, "new mounting has no free pod");
                Equal(0, loaded.Modules[5].ItemCount, "new mounting has no free cargo");
                restored = SaveStorage.Deserialize(SaveStorage.Serialize(restored));
            }
            GameCore.SingletonInstance.LoadSavedGame(restored);
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipBay, new List<SceneVariables> { SceneVariables.Orbit, SceneVariables.Ship });
            await InputFrames();
            bay = ActiveScreen<ShipBay>();
            bay.ResourceList.Stores[ItemTypes.supply_pod] = 1;
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.supply_pod).Locked = false;
            Press(bay, "Buttons/ShipNav/Nav_Torso6");
            Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/AddSupplyPod");
            Equal(Module_Types.Supply, Save.Ships.Single().Modules[5].ModuleType, "migrated sixth mount can fit a pod");
            Equal(0, bay.ResourceList.Stores[ItemTypes.supply_pod], "migrated mount consumes actual stock");
            Save.Ships.Single().Modules[5].ItemStored = ItemTypes.titanium;
            Save.Ships.Single().Modules[5].ItemCount = 250;
            Press(bay, ShipParts + "Torso6/OpenShipInterior");
            await InputFrames();
            var interior = ActiveScreen<ShipInterior>();
            Equal("250 Titanium", interior.GetNode<Label>("TextLayout/CargoValue6").Text, "interior displays sixth cargo");
            await CaptureDisplayEvidence("scg-six-cargo-interior");
            var dialog = await OpenSupplyPods(interior);
            try
            {
                var ditch = dialog.GetNode<Button>("Rows/Pod5/Ditch");
                var close = dialog.GetNode<Button>("Close");
                Equal(false, ditch.GetGlobalRect().Intersects(close.GetGlobalRect()), "sixth cargo row does not overlap Close");
                Equal(true, dialog.GetGlobalRect().Encloses(ditch.GetGlobalRect()), "sixth cargo action remains inside dialog");
                await CaptureDisplayEvidence("scg-six-cargo-dialog");
                var point = ditch.GetGlobalRect().GetCenter();
                GetViewport().PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point }, true);
                foreach (var pressed in new[] { true, false })
                    GetViewport().PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed }, true);
                await InputFrames();
                Equal(0, Save.Ships.Single().Modules[5].ItemCount, "sixth cargo can be ditched by pointer");
                Equal(123, Save.Ships.Single().Modules[4].ItemCount, "ditching sixth cargo preserves fifth");
            }
            finally { Deuteros.Code.Platform.Helpers.OverlayManager.Instance.CloseOverlay(); await InputFrames(); }
        }

        private async Task AssemblyReturnCapacity()
        {
            var bay = await EmptyAssemblyBay(Ship_Types.SCG);
            var stores = bay.ResourceList.Stores;
            stores.Items.Clear();
            stores[ItemTypes.g_chassis] = stores[ItemTypes.star_drive] = 1;
            Press(bay, "Buttons/Nav_Create_SCG");
            var ship = Save.Ships.Single();
            // An empty mounting returns only its chassis, never a drive.
            Press(bay, "Buttons/Nav_Dismantle");
            Equal(1, stores[ItemTypes.star_drive], "unfitted drive remains stock");
            Equal(1, stores[ItemTypes.g_chassis], "unpowered chassis recovered");
            Press(bay, "Buttons/Nav_Create_SCG");
            ship = Save.Ships.Single();
            Press(bay, ShipParts + "Engine/SpriteHolder/Buttons/InstallEngine");
            foreach (var blocked in new[] { ItemTypes.g_chassis, ItemTypes.star_drive })
            {
                stores[ItemTypes.g_chassis] = stores[ItemTypes.star_drive] = 49999;
                stores[blocked] = 50000;
                Press(bay, "Buttons/Nav_Dismantle");
                Equal(true, Save.Ships.Contains(ship), "full part stock blocks dismantle");
                Equal(50000, stores[blocked], "blocked stock not changed");
                Equal(49999, stores[blocked == ItemTypes.g_chassis ? ItemTypes.star_drive : ItemTypes.g_chassis], "other return stays aboard");
                Deuteros.Code.Platform.Helpers.OverlayManager.Instance.CloseOverlay();
                await InputFrames();
            }
            stores[ItemTypes.g_chassis] = stores[ItemTypes.star_drive] = 49999;
            Press(bay, "Buttons/Nav_Dismantle");
            Equal(0, Save.Ships.Count, "exactly sufficient capacity permits dismantle");
            Equal(50000, stores[ItemTypes.g_chassis], "chassis fills capacity");
            Equal(50000, stores[ItemTypes.star_drive], "drive fills capacity");
        }
    }
}
