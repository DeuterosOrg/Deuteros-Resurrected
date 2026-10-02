using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using MenuControl = Deuteros.Code.Platform.MenuButton;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunSelfDestructRegressions()
        {
            await CheckAsync("Hostile docking requires SDM defusing before station capture", SdmDockAndDefuse);
            await CheckAsync("Installed station SDM arms and disarms through both switches", SdmManualSwitches);
            CheckUi("SDM real-time expiry removes berth occupants and preserves nearby ships", SdmRealtimeLoss);
            CheckUi("SDM simulation expiry happens before display notification", SdmSimulationLoss);
            CheckUi("Hostile SDM is protected at sixteen and fifteen but not fourteen", SdmHostileGrace);
            CheckUi("SDM high bit skips exactly one simulation expiry pass", SdmHighBit);
            CheckUi("Earth SDM preserves grounded shuttle and ground services", SdmEarthException);
            CheckUi("Non-Earth SDM removes even grounded local shuttle", SdmColonyShuttle);
            CheckUi("SDM fractional timer survives save load and old saves default disarmed", SdmTimerSave);
            await CheckAsync("Hostile SDM Hyperlight interlock rejects the wrong switch sequence", SdmInterlock);
            await CheckAsync("First hostile SDM access discovers technology without granting ownership", SdmDiscovery);
            CheckUi("SDM rejects malformed countdown and timer saves", SdmInvalidSave);
            CheckUi("SDM callbacks cannot mutate an obsolete saved world", SdmStaleWorld);
            await CheckAsync("SDM expiration removes its own screen before subsequent display events", SdmScreenExpiry);
            CheckUi("SDM defusal restores captured station resources once and leaves colony repair pending", SdmCaptureResources);
            CheckUi("SDM removes colony ground crews but preserves Earth ground and offsite crews", SdmGroundCrews);


        }

        private int SdmCount(SpaceStation station) =>
            station.SdmCountdown;

        private async Task<Node> OpenSdmControl()
        {
            var button = ActiveScreen<MainMenu>().GetNode("MainButtons").GetChildren().OfType<MenuControl>()
                .FirstOrDefault(b => !b.Disabled && b.HoverText == "Self-destruct");
            Equal(true, button != null, "installed mechanism has an accessible control");
            button.EmitSignal(BaseButton.SignalName.Pressed);
            await InputFrames();
            return GameCore.SingletonInstance.GetNode("MainScene").GetChildren()
                .Last(n => n.Name == "SelfDestruct" && !n.IsQueuedForDeletion());
        }

        private async Task SdmDockAndDefuse()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            var ship = interior.Ship;
            ship.ShipState = Ship_States.UnDocked;
            var planet = GameCore.Earth;
            planet.ActiveMethanoid = true;
            planet.Station.SdmInstalled = true;
            Save.AtWar = true;
            var research = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.s__d__m).Research;
            research.Locked = false;
            research.Researched = true;
            interior.GetNode<Button>("Dock").EmitSignal(BaseButton.SignalName.Pressed);
            Equal(true, planet.ActiveMethanoid, "Dock does not grant ownership before defusing");
            Equal(0, SdmCount(planet.Station), "countdown starts on completed docking");
            ShipInterior.UpdateShips(Save.CurrentDay, ++Save.CurrentDay);
            Equal(Ship_States.Docked, ship.ShipState, "capturing ship reaches berth");
            Equal(true, planet.ActiveMethanoid, "docking completion still needs defusal");
            Equal(16, SdmCount(planet.Station), "hostile dock starts the original countdown");
            GameCore.SingletonInstance.UpdateMenuButtons(false, true);
            var control = await OpenSdmControl();
            Press(control, "LowSwitch");
            Equal(true, planet.ActiveMethanoid, "neutral switch combination cannot capture");
            Press(control, "HighSwitch");
            Equal(false, planet.ActiveMethanoid, "safe switch combination captures the station");
            Equal(0, SdmCount(planet.Station), "defused countdown cleared");
            Equal(true, Save.Ships.Contains(ship), "capturing ship survives defusal");
        }

        private async Task SdmManualSwitches()
        {
            await OpenInterior(Ship_Types.IOS);
            var station = GameCore.Earth.Station;
            station.SdmInstalled = true;
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.s__d__m).Research.Locked = false;
            GameCore.SingletonInstance.UpdateMenuButtons(false, true);
            var control = await OpenSdmControl();
            Press(control, "HighSwitch");
            Equal(0, SdmCount(station), "intermediate state does not arm");
            Press(control, "LowSwitch");
            Equal(12, SdmCount(station), "manual arm starts twelve-count delay");
            await CaptureDisplayEvidence("sdm-armed");
            Equal(12, SdmCount(SaveStorage.Deserialize(SaveStorage.Serialize(Save)).BaseGameData.Planets[StellarBodies.earth].Station),
                "armed state survives save/load");
            Press(control, "LowSwitch");
            Equal(12, SdmCount(station), "neutral state does not disarm");
            Press(control, "HighSwitch");
            Equal(0, SdmCount(station), "safe combination disarms");
        }

        private Planet SdmLossWorld(StellarBodies location = StellarBodies.the_moon)
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.TimeSkip = Save.TimeSkipDay = false;
            Save.Ships.Clear();
            Save.CurrentPlanet = location;
            var planet = (Planet)Save.BaseGameData.Planets[location];
            planet.ActiveMethanoid = false;
            planet.BaseBuildParts = 2;
            planet.BaseDamaged = false;
            planet.Station.Built = true;
            planet.Station.BuildParts = 8;
            planet.Station.SdmInstalled = true;
            planet.Station.SdmCountdown = 1;
            return planet;
        }

        private Ship SdmVictim(Planet planet, Ship_States state, bool shuttle = false)
        {
            Ship ship = shuttle ? new Shuttle { ShipType = Ship_Types.Shuttle, OnGround = true }
                : new IOS { ShipType = Ship_Types.IOS };
            ship.ShipID = Guid.NewGuid();
            ship.PlanetLocation = planet.PlanetId;
            ship.StarLocation = planet.ParentStar;
            ship.DestinationPlanetLocation = StellarBodies.mars;
            ship.ShipState = state;
            ship.Fuel = 100;
            ship.Pilot = AttritionTeam();
            ship.Modules = new List<ShipModule> { new ShipModule { ModuleType = Module_Types.Cryo, StaffStored = AttritionTeam() } };
            Save.Ships.Add(ship);
            return ship;
        }

        private void SdmRealtimeLoss()
        {
            var planet = SdmLossWorld();
            var berth = SdmVictim(planet, Ship_States.Docked);
            var orbit = SdmVictim(planet, Ship_States.UnDocked);
            var approach = SdmVictim(planet, Ship_States.Docking);
            var transit = SdmVictim(planet, Ship_States.InTransit);
            planet.Station.Resources.AddStaff(AttritionTeam());
            planet.Station.Factory.Builder = AttritionTeam(StaffType.Production);
            planet.Station.Resources.Stores[ItemTypes.iron] = 123;
            planet.Station.MtxInstalled = true;
            GameCore.SingletonInstance._Process(1);
            Equal(false, planet.Station.Built, "expired mechanism removes station");
            Equal(false, Save.Ships.Contains(berth), "occupied berth and its pilot/cryo removed");
            Equal(true, Save.Ships.Contains(orbit), "nearby free orbit survives");
            Equal(true, Save.Ships.Contains(approach), "unoccupied approaching hull survives");
            Equal(true, Save.Ships.Contains(transit), "departed hull survives");
            Equal(0, planet.Station.BuildParts, "station can be rebuilt from zero");
            Equal(false, planet.Station.SdmInstalled || planet.Station.MtxInstalled, "lost hardware removed");
            Equal(0, planet.Station.Resources.Stores[ItemTypes.iron], "station stock lost");
            Equal(true, planet.Station.Resources.Staff.All(t => t == null), "station roster removed");
            Equal<Staff>(null, planet.Station.Factory.Builder, "production team lost");
            Equal(true, planet.BaseDamaged, "colony requires repair after rebuilding");
            var news = Save.News.GetNews(100).Count;
            GameCore.SingletonInstance._Process(2);
            Equal(news, Save.News.GetNews(100).Count, "expiry and loss news happen only once");
        }

        private void SdmSimulationLoss()
        {
            var planet = SdmLossWorld();
            planet.Station.SdmCountdown = 12;
            bool? observed = null;
            void Observe(uint before, uint after) { observed = planet.Station.Built; }
            GameCore.SingletonInstance.DayPassed += Observe;
            try { AdvanceTickDay(); }
            finally { GameCore.SingletonInstance.DayPassed -= Observe; }
            Equal<bool?>(false, observed, "display sees explosion from same simulation update");
        }

        private void SdmHostileGrace()
        {
            var planet = SdmLossWorld();
            planet.ActiveMethanoid = true;
            planet.Station.SdmCountdown = 16;
            AdvanceTickDay();
            Equal(true, planet.Station.Built, "sixteen survives simulation step");
            GameCore.SingletonInstance._Process(1);
            AdvanceTickDay();
            Equal(true, planet.Station.Built, "fifteen survives simulation step");
            GameCore.SingletonInstance._Process(1);
            AdvanceTickDay();
            Equal(false, planet.Station.Built, "fourteen expires on simulation step");
        }

        private void SdmHighBit()
        {
            var planet = SdmLossWorld();
            planet.Station.SdmCountdown = 0x96;
            AdvanceTickDay();
            Equal(22, planet.Station.SdmCountdown, "first pass clears high bit only");
            Equal(true, planet.Station.Built, "first pass skips explosion");
            AdvanceTickDay();
            Equal(false, planet.Station.Built, "next pass explodes");
        }

        private void SdmEarthException()
        {
            var earth = SdmLossWorld(StellarBodies.earth);
            var ground = SdmVictim(earth, Ship_States.Docked, true);
            var dock = SdmVictim(earth, Ship_States.Docking, true);
            ((Shuttle)dock).OnGround = false;
            earth.PlanetResources.Stores[ItemTypes.iron] = 543;
            GameCore.SingletonInstance._Process(1);
            Equal(true, Save.Ships.Contains(ground), "Earth landed shuttle survives");
            Equal(false, Save.Ships.Contains(dock), "Earth orbital docking shuttle is lost");
            Equal(543, earth.PlanetResources.Stores[ItemTypes.iron], "Earth ground stock survives");
            Equal(false, earth.BaseDamaged, "Earth services remain intact");
        }

        private void SdmColonyShuttle()
        {
            var planet = SdmLossWorld();
            var shuttle = SdmVictim(planet, Ship_States.Docked, true);
            planet.PlanetResources.Stores[ItemTypes.iron] = 99;
            GameCore.SingletonInstance._Process(1);
            Equal(false, Save.Ships.Contains(shuttle), "non-Earth shuttle has no grounded exception");
            Equal(0, planet.PlanetResources.Stores[ItemTypes.iron], "discarded local record cannot resurrect stocks on rebuild");
        }

        private void SdmGroundCrews()
        {
            foreach (var location in new[] { StellarBodies.earth, StellarBodies.the_moon })
            foreach (var realTime in new[] { true, false })
            {
                GameCore.SingletonInstance.GameData.ActiveSaveFile = CoreData.CreateNewSaveFile();
                var planet = SdmLossWorld(location);
                var groundCrew = AttritionTeam();
                planet.PlanetResources.AddStaff(groundCrew);
                planet.Station.Resources.AddStaff(AttritionTeam());
                planet.Station.Factory.Builder = AttritionTeam(StaffType.Production);
                var other = Save.BaseGameData.Planets[StellarBodies.mars];
                var otherCrew = AttritionTeam();
                other.PlanetResources.AddStaff(otherCrew);
                var travelling = SdmVictim(planet, Ship_States.InTransit);
                var pilot = travelling.Pilot;
                var cryo = travelling.Modules[0].StaffStored;
                if (realTime) GameCore.SingletonInstance._Process(1);
                else AdvanceTickDay();
                Equal(location == StellarBodies.earth, planet.PlanetResources.Staff.Contains(groundCrew),
                    "only Earth has a separately surviving ground roster");
                Equal(true, planet.Station.Resources.Staff.All(t => t == null), "orbital crews lost");
                Equal<Staff>(null, planet.Station.Factory.Builder, "orbital builder lost");
                Equal(true, other.PlanetResources.Staff.Contains(otherCrew), "other colony retains crew");
                Equal(pilot, travelling.Pilot, "travelling pilot survives");
                Equal(cryo, travelling.Modules[0].StaffStored, "travelling cryopod survives");
                Equal(true, Save.Ships.Contains(travelling), "travelling ship survives");
                var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                Equal(location == StellarBodies.earth,
                    restored.BaseGameData.Planets[location].PlanetResources.Staff.Any(t => t != null),
                    "save reload cannot restore lost colony crew");
            }
        }

        private void SdmTimerSave()
        {
            var planet = SdmLossWorld();
            planet.Station.SdmCountdown = 2;
            GameCore.SingletonInstance._Process(0.6);
            var json = SaveStorage.Serialize(Save);
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(json);
            planet = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            GameCore.SingletonInstance._Process(0.4);
            Equal(1, planet.Station.SdmCountdown, "saved subsecond phase resumes");
            GameCore.SingletonInstance._Process(0.99);
            Equal(true, planet.Station.Built, "does not expire early");
            GameCore.SingletonInstance._Process(0.01);
            Equal(false, planet.Station.Built, "expires at boundary after load");
            var document = Newtonsoft.Json.Linq.JObject.Parse(json);
            foreach (var field in document.Descendants().OfType<Newtonsoft.Json.Linq.JProperty>()
                .Where(p => p.Name == "SdmCountdown" || p.Name == "SdmTimerRemainder").ToList()) field.Remove();
            var legacy = SaveStorage.Deserialize(document.ToString());
            Equal(0, legacy.BaseGameData.Planets[StellarBodies.the_moon].Station.SdmCountdown, "old saves default safely disarmed");
        }

        private async Task<Node> HostileSdmScreen(bool discovered)
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            GameCore.SingletonInstance.SetProcess(false);
            interior.Ship.ShipState = Ship_States.Docked;
            GameCore.Earth.ActiveMethanoid = true;
            GameCore.Earth.Station.SdmInstalled = true;
            GameCore.Earth.Station.SdmCountdown = 16;
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.s__d__m).Research.Locked = !discovered;
            GameCore.SingletonInstance.UpdateMenuButtons(false, true);
            return await OpenSdmControl();
        }

        private async Task SdmInterlock()
        {
            var control = await HostileSdmScreen(true);
            var hyperlight = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.hyperlight).Research;
            hyperlight.Locked = false;
            hyperlight.Researched = false;
            Press(control, "HighSwitch");
            Equal(true, control.GetNode<Button>("LowSwitch").Disabled, "discovered Hyperlight locks first switch while second is on");
            Press(control, "LowSwitch");
            Equal(true, GameCore.Earth.ActiveMethanoid, "forced disabled callback cannot capture");
            Equal(16, GameCore.Earth.Station.SdmCountdown, "wrong sequence cannot reset countdown");
            Press(control, "HighSwitch");
            Press(control, "LowSwitch");
            Press(control, "HighSwitch");
            Equal(false, GameCore.Earth.ActiveMethanoid, "correct order defuses with incomplete Hyperlight research");
        }

        private async Task SdmDiscovery()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            GameCore.SingletonInstance.SetProcess(false);
            interior.Ship.ShipState = Ship_States.Docked;
            GameCore.Earth.ActiveMethanoid = true;
            GameCore.Earth.Station.SdmInstalled = true;
            GameCore.Earth.Station.SdmCountdown = 16;
            var research = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.s__d__m).Research;
            research.Locked = true;
            Save.Unlocks.Remove(Game_Unlocks.Self_Destruct);
            GameCore.SingletonInstance.UpdateMenuButtons(false, true);
            var button = ActiveScreen<MainMenu>().GetNode("MainButtons").GetChildren().OfType<MenuControl>()
                .Single(b => !b.Disabled && b.HoverText == "Self-destruct");
            button.EmitSignal(BaseButton.SignalName.Pressed);
            await InputFrames();
            Equal(false, research.Locked, "first inspection discovers the mechanism");
            Equal(true, Save.Unlocks.Contains(Game_Unlocks.Self_Destruct), "technology recorded");
            Equal(BulletinTypes.Self_Destruct, Save.News.LastBulletin, "original discovery bulletin requested");
            Equal(true, GameCore.Earth.ActiveMethanoid, "discovery does not capture");
            Equal(16, GameCore.Earth.Station.SdmCountdown, "discovery does not reset armed countdown");
        }

        private void SdmInvalidSave()
        {
            var planet = SdmLossWorld();
            foreach (var count in new[] { -1, 256 })
            {
                planet.Station.SdmCountdown = count;
                bool rejected = false;
                try { SaveStorage.Serialize(Save); } catch (System.IO.InvalidDataException) { rejected = true; }
                Equal(true, rejected, "out-of-byte countdown rejected");
            }
            planet.Station.SdmCountdown = 1;
            foreach (var value in new[] { -0.1, 1.0, double.NaN, double.PositiveInfinity })
            {
                Save.SdmTimerRemainder = value;
                bool rejected = false;
                try { SaveStorage.Serialize(Save); } catch (System.IO.InvalidDataException) { rejected = true; }
                Equal(true, rejected, "invalid subsecond phase rejected");
            }
        }

        private void SdmStaleWorld()
        {
            var oldPlanet = SdmLossWorld(StellarBodies.earth);
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(false, SdmSystem.ApplySwitches(Save, oldPlanet, 0x0200), "obsolete station callback rejected");
            Equal(1, oldPlanet.Station.SdmCountdown, "obsolete world unchanged");
            Equal(1, GameCore.Earth.Station.SdmCountdown, "active world unchanged");
        }

        private async Task SdmScreenExpiry()
        {
            var control = await HostileSdmScreen(true);
            GameCore.Earth.Station.SdmCountdown = 1;
            GameCore.SingletonInstance._Process(1);
            Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "lost control replaced by overview");
            Equal(0, Save.Ships.Count, "berthed capturing ship lost");
            Equal(false, control.IsInsideTree(), "old control unsubscribes before display");
            AdvanceTickDay();
            await InputFrames();
        }

        private void SdmCaptureResources()
        {
            var planet = SdmLossWorld(StellarBodies.jupiter);
            planet.ActiveMethanoid = true;
            planet.Station.SdmCountdown = 16;
            planet.Station.Factory.AOC = true;
            planet.Station.MtxInstalled = true;
            planet.PlanetResources.Derricks = 99;
            foreach (var material in planet.PlanetResources.Materials) planet.Station.Resources.Stores[material.MaterialType] = 0;
            planet.Station.Resources.Stores[ItemTypes.of_frame] = 7;
            var ship = SdmVictim(planet, Ship_States.Docked);
            Equal(true, SdmSystem.ApplySwitches(Save, planet, 0x0200), "defuse succeeds");
            foreach (var material in planet.PlanetResources.Materials)
            {
                var count = planet.Station.Resources.Stores[material.MaterialType];
                Equal(true, count >= 100 && count <= 1123, "captured mineral stock uses original random range");
            }
            Equal(true, planet.PlanetResources.Derricks >= 0 && planet.PlanetResources.Derricks <= 7, "captured derrick count restored");
            Equal(true, planet.BaseDamaged, "captive colony still requires repair");
            Equal(true, planet.Station.Factory.AOC && planet.Station.MtxInstalled, "captured installed facilities retained");
            Equal(7, planet.Station.Resources.Stores[ItemTypes.of_frame], "unrelated inventory preserved");
            Equal(true, Save.Ships.Contains(ship), "successful defusal preserves crew and cryopod");
            var before = SaveStorage.Serialize(Save);
            SdmSystem.ApplySwitches(Save, planet, 0x0200);
            Equal(before, SaveStorage.Serialize(Save), "duplicate safe switch cannot reroll or recapture");
        }
    }
}
