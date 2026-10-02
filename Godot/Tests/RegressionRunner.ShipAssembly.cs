using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Godot;
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
            await CheckAsync("SCG bay exposes exactly five functional module mounts", ScgModuleMounts);
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
            Equal(type == Ship_Types.Shuttle ? 1 : type == Ship_Types.IOS ? 3 : 5, Save.Ships.Single().Modules.Count, "correct usable mounts");
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
            var bay = await EmptyAssemblyBay(Ship_Types.SCG);
            var stores = bay.ResourceList.Stores;
            stores[ItemTypes.g_chassis] = 1;
            stores[ItemTypes.supply_pod] = 5;
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.supply_pod).Locked = false;
            Press(bay, "Buttons/Nav_Create_SCG");
            Equal(false, bay.GetNode<Control>("Buttons/ShipNav/Nav_Torso6").Visible, "no navigation to a nonexistent sixth mount");
            Equal(false, bay.GetNode<Control>(ShipParts + "Torso6").Visible, "no phantom module artwork");
            for (var mount = 1; mount <= 5; mount++)
            {
                Equal(true, bay.GetNode<Control>("Buttons/ShipNav/Nav_Torso" + mount).Visible, "real mount can be selected");
                Press(bay, "Buttons/ShipNav/Nav_Torso" + mount);
                Press(bay, ShipParts + "Torso" + mount + "/SpriteHolder/Buttons/AddSupplyPod");
                Equal(Module_Types.Supply, Save.Ships.Single().Modules[mount - 1].ModuleType, "pod fitted to selected mount");
            }
            Equal(0, stores[ItemTypes.supply_pod], "five pods fitted exactly once");
            await InputFrames();
            await CaptureDisplayEvidence("scg-five-module-mounts");
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
