using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Platform.Screens.ShipBayScenes;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task PtlCockpitFitting()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            foreach (var hull in new[] { Ship_Types.IOS, Ship_Types.SCG })
            {
                var bay = await DfccFuelBay(hull);
                var ship = (InterStellarShip)bay.Ship;
                ship.Engine = true;
                ship.Fuel = 200;
                ship.Modules[0].ModuleType = Module_Types.Tool;
                ship.Modules[0].ItemStored = ItemTypes.d__f__c__c;
                ship.Modules[0].ItemCount = 1;
                ship.Modules[1].ModuleType = Module_Types.Supply;
                ship.Modules[1].ItemStored = ItemTypes.iron;
                ship.Modules[1].ItemCount = 37;
                ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, Source = StellarBodies.earth,
                    Destination = StellarBodies.the_moon, SourceItems = new() { ItemTypes.iron }, DestinationItems = new(),
                    CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
                ship.DroneCount = 12;
                var item = core.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher);
                item.Research.Locked = item.Locked = false;
                item.Research.Researched = true;
                item.Research.ResearchPercentageComplete = 100;
                var stores = bay.ResourceList.Stores;
                stores[item.ItemType] = 2;
                stores[ItemTypes.a__c__c] = 3;
                GameCore.Earth.PlanetResources.Stores[item.ItemType] = 9;
                var modules = Newtonsoft.Json.JsonConvert.SerializeObject(ship.Modules);
                var acc = ship.ACC;
                bay.GetNode<Cockpit>(ShipParts + "Cockpit").UpdateState();
                var button = bay.GetNode<TextureButton>(ShipParts + "Cockpit/Buttons/AddACC");
                var parent = bay.GetParent();
                var viewport = new SubViewport { Size = new Vector2I(320, 200), GuiDisableInput = false };
                AddChild(viewport);
                bay.Reparent(viewport);
                try
                {
                    await InputFrames();
                    ClickMenu(viewport, button);
                    await InputFrames();
                    Equal(true, ship.PTL, "real cockpit pointer fits the permanent launcher on " + hull);
                    Equal(1, stores[item.ItemType], "exactly one local launcher consumed");
                    Equal(9, GameCore.Earth.PlanetResources.Stores[item.ItemType], "ground stock is not used for orbital fitting");
                    Equal(3, stores[ItemTypes.a__c__c], "PTL fitting cannot spend ACC stock");
                    Equal(acc, ship.ACC, "existing navigation settings and owner survive");
                    Equal(modules, Newtonsoft.Json.JsonConvert.SerializeObject(ship.Modules), "permanent fitting preserves every pod and its cargo");
                    Equal(200, ship.Fuel, "fitting does not spend battle fuel");
                    Equal(12, ship.DroneCount, "fitting does not spend drones");
                    Equal(new Vector2(24, 16), button.TextureNormal.GetSize(), "original cockpit icon is unscaled");
                    Equal(true, button.TextureNormal.ResourcePath.EndsWith("/PTL.png"), "fitting action uses the recovered PTL icon");
                    button.EmitSignal(Control.SignalName.MouseEntered);
                    Equal("Fit Torpedo Launcher", GameCore.HoverText, "hover names the actual fitting action");
                    ClickMenu(viewport, button);
                    Equal(1, stores[item.ItemType], "duplicate input consumes no second launcher");
                    Equal(true, OverlayManager.Instance.IsOpen, "duplicate fitting explains why nothing changed");
                    OverlayManager.Instance.CloseOverlay();
                    await InputFrames();
                }
                finally { bay.Reparent(parent); viewport.Free(); }
                await CaptureDisplayEvidence("ptl-fitted-" + hull);
                core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
                await InputFrames();
                core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                var loaded = Save.Ships.OfType<InterStellarShip>().Single();
                Equal(true, loaded.PTL, "permanent launcher survives reload");
                var logic = new Deuteros.Code.Objects.Battle.BattleLogic(loaded, new EnemyFleet { DroneCount = 30 }, 0, null, null, null, null, null);
                logic.GetType().GetProperty("BattleState", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(logic, BattleState.FleetsInBattle);
                Equal(true, logic.canPTL(), "fitted saved launcher reaches the existing battle action");
                logic.LaunchPTL();
                logic.LaunchPTL();
                Equal(100, loaded.Fuel, "battle launch charges once after real fitting and reload");
            }
            await DrainStoppedAudio();
        }

        private async Task PtlFittingGates()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            var bay = await DfccFuelBay(Ship_Types.IOS);
            var ship = (InterStellarShip)bay.Ship;
            var item = core.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher);
            var stores = bay.ResourceList.Stores;
            stores[item.ItemType] = 2;
            var cockpit = bay.GetNode<Cockpit>(ShipParts + "Cockpit");
            void Rejected(string reason)
            {
                cockpit.AddACC.EmitSignal(BaseButton.SignalName.Pressed);
                Equal(false, ship.PTL, reason);
                Equal(2, stores[item.ItemType], "rejection preserves launcher stock: " + reason);
            }
            item.Research.Researched = false;
            bay.GetNode<Cockpit>(ShipParts + "Cockpit").UpdateState();
            Equal(false, cockpit.AddACC.Visible, "converted hull waits for completed PTL research");
            Rejected("hidden retained research callback cannot install");
            item.Research.Researched = true;
            item.Research.ResearchPercentageComplete = 100;
            item.Research.Locked = item.Locked = false;
            bay.GetNode<Cockpit>(ShipParts + "Cockpit").UpdateState();
            Equal(true, cockpit.AddACC.Visible, "research exposes original cockpit fitting control");
            GlobalInput.LockUi();
            try { Rejected("another UI owner blocks fitting"); }
            finally { GlobalInput.UnlockUi(); }
            GameCore.LockScreen("PTL gate test");
            try { Rejected("screen lock blocks retained fitting input"); }
            finally { GameCore.UnLockScreen(); }
            Cursor.LockToRect(new Rect2(0, 0, 10, 10));
            try { Rejected("cargo cursor lock blocks fitting"); }
            finally { Cursor.Unlock(); }
            GetTree().Paused = true;
            try { Rejected("pause owner blocks fitting"); }
            finally { GetTree().Paused = false; }
            GameCore.ShowError(bay, "Other UI owner");
            Rejected("another overlay blocks fitting");
            OverlayManager.Instance.CloseOverlay();
            await InputFrames();
            ship.ShipState = Ship_States.InTransit;
            Rejected("departed ship cannot use retained bay callback");
            ship.ShipState = Ship_States.Docked;
            Save.Ships.Remove(ship);
            Rejected("removed ship cannot consume current stores");
            Save.Ships.Add(ship);
            GameCore.Earth.ActiveMethanoid = true;
            Rejected("hostile capture invalidates an already-open fitting bay");
            GameCore.Earth.ActiveMethanoid = false;
            GameCore.Earth.Station.Built = false;
            Rejected("destroyed station invalidates its old fitting bay");
            GameCore.Earth.Station.Built = true;
            ship.MethanoidOwned = true;
            Rejected("enemy-owned hull cannot use player fitting controls");
            ship.MethanoidOwned = false;
            stores[item.ItemType] = 0;
            cockpit.AddACC.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(false, ship.PTL, "no stock cannot install");
            Equal(0, stores[item.ItemType], "no stock cannot underflow");
            Equal(true, OverlayManager.Instance.IsOpen, "missing launcher uses existing error feedback");
            OverlayManager.Instance.CloseOverlay();
            await InputFrames();
            ship.DFCC = false;
            bay.GetNode<Cockpit>(ShipParts + "Cockpit").UpdateState();
            stores[ItemTypes.a__c__c] = 1;
            cockpit.AddACC.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(0, stores[ItemTypes.a__c__c], "ordinary ACC fitting remains available on unconverted hull");
            Equal(true, ship.ACC != null && ReferenceEquals(ship.ACC.Ship, ship), "ordinary ACC retains its current owner");
            Equal(false, ship.PTL, "ordinary fitting does not install a launcher");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            await DrainStoppedAudio();
        }
    }
}
