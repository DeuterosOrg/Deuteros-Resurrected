using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using System;
using System.IO;
using System.Linq;
using Deuteros.Code.Utility;
using Deuteros.Code.Platform.Helpers;
using Newtonsoft.Json.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private SCG NewInterstellarRoute()
        {
            var ship = (SCG)HullShip(true);
            ship.Name = "Interstellar regression";
            ship.PlanetLocation = StellarBodies.mercury;
            ship.DestinationPlanetLocation = StellarBodies.atlantic;
            ship.Fuel = 250;
            Save.Clock.DateCentidays = 10000;
            var research = Save.BaseGameData.ItemList.Find(item => item.ItemType == ItemTypes.hyperlight).Research;
            research.Researched = true;
            research.ResearchPercentageComplete = 100;
            research.Locked = false;
            return ship;
        }

        private void InterstellarNotInstant()
        {
            var ship = NewInterstellarRoute();
            Equal(0, Save.BaseGameData.Planets[ship.PlanetLocation].Order, "source orbit index");
            Equal(0, Save.BaseGameData.Planets[ship.DestinationPlanetLocation].Order, "destination shares local index, not location");
            Equal(true, ship.EngageEngine(), "SCG accepts cross-star course");
            var previous = Save.CurrentDay++;
            ShipInterior.UpdateShips(previous, Save.CurrentDay);
            Equal(Ship_States.InTransit, ship.ShipState, "interstellar distance cannot disappear when orbit indices match");
            Equal(StellarBodies.mercury, ship.PlanetLocation, "requested remote body is not reached after one update");
        }

        private void InterstellarPhaseFuel()
        {
            var ship = NewInterstellarRoute();
            Equal(true, ship.EngageEngine(), "SCG accepts accelerated route");
            var previous = Save.CurrentDay++;
            ShipInterior.UpdateShips(previous, Save.CurrentDay);
            Equal(248, ship.Fuel, "first acceleration selects phase2 and charges two units without generic burn");
            previous = Save.CurrentDay++;
            ShipInterior.UpdateShips(previous, Save.CurrentDay);
            Equal(245, ship.Fuel, "next acceleration selects phase3 and charges three units");
        }

        private void AdvanceInterstellar(bool manual = true)
        {
            if (manual) Save.Clock.QueueManual(); else Save.Clock.AdvanceNormal(GameClock.NormalIntervalSeconds);
            Equal(true, Save.Clock.Consume(), "one clock increment consumed");
            var previous = Save.CurrentDay++;
            ShipInterior.UpdateShips(previous, Save.CurrentDay);
        }

        private void InterstellarTables()
        {
            var expected = new[] {
                0,4300,4400,6000,8200,8700,11000,11300,11800,
                4300,0,200,7000,11000,16300,15000,12700,13200,
                4400,200,0,8000,12000,15300,16000,12800,13300,
                6000,7000,8000,0,2500,2720,4550,23000,19400,
                8200,11000,12000,2500,0,1900,5500,7200,16100,
                8700,16300,15300,2720,1900,0,3800,14200,7600,
                11000,15000,16000,4550,5500,3800,0,450,6100,
                11300,12700,12800,23000,7200,14200,450,0,650,
                11800,13200,13300,19400,16100,7600,6100,650,0 };
            var clocks = new[] {0,430000,440000,600000,820000,870000,1100000,1130000,1180000};
            for (var origin = 0; origin < 9; origin++)
            {
                var star = (StellarBodies)((origin + 1) * 100000);
                Equal(clocks[origin], InterstellarFlight.StarOffset(star), "original initial star clock");
                for (var destination = 0; destination < 9; destination++)
                    Equal(expected[origin * 9 + destination], InterstellarFlight.Distance(star,
                        (StellarBodies)((destination + 1) * 100000)), "original star distance");
            }
            Equal(4, InterstellarFlight.LocalDistance(StellarBodies.earth), "Earth is original first Solar body");
            Equal(5, InterstellarFlight.LocalDistance(StellarBodies.the_moon), "first group's moon follows parent");
            Equal(8, InterstellarFlight.LocalDistance(StellarBodies.mercury), "Mercury is original second group");
            Equal(12, InterstellarFlight.LocalDistance(StellarBodies.venus), "Venus is original third group");
            Equal(4, InterstellarFlight.LocalDistance(StellarBodies.atlantic), "Proxima star-to-first-body leg");
        }

        private void InterstellarSaveJourney()
        {
            var ship = NewInterstellarRoute();
            Equal(16, ship.TravelTimeRemain(), "preview includes twelve star updates and four local updates");
            Equal<InterstellarFlight>(null, ship.Flight, "preview cannot create saved progress");
            ship.EngageEngine();
            var id = ship.ShipID;
            for (var update = 1; update <= 16; update++)
            {
                var before = SaveStorage.Serialize(Save);
                Equal(17 - update, ship.TravelTimeRemain(), "projection tracks all remaining legs");
                Equal(before, SaveStorage.Serialize(Save), "projection cannot mutate fuel clock or flight");
                AdvanceInterstellar(update % 2 == 0);
                if (update == 1) { Equal(4298, ship.Flight.Remaining, "first step advances two distance units"); Equal(200L, ship.Flight.ClockOffset, "private clock gains two days"); }
                if (update == 11) { Equal(InterstellarFlight.FlightLeg.Hyperlight, ship.Flight.Leg, "one-update Hyperlight transition"); Equal(0L, ship.Flight.AbsoluteClock(Save.Clock), "original transition clears private clock"); }
                if (update == 12) { Equal(InterstellarFlight.FlightLeg.Local, ship.Flight.Leg, "star arrival begins body approach"); Equal(141, ship.Fuel, "original star-route fuel trace"); Equal(430000L, ship.Flight.ClockOffset, "Hyperlight syncs destination clock"); }
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                ship = (SCG)Save.Ships.Single(s => s.ShipID == id);
                Equal(update == 16 ? Ship_States.UnDocked : Ship_States.InTransit, ship.ShipState, "reload preserves boundary without replay");
            }
            Equal(StellarBodies.atlantic, ship.PlanetLocation, "only final leg reaches requested body");
            Equal(137, ship.Fuel, "body leg costs four additional ordinary units");
            Equal<InterstellarFlight>(null, ship.Flight, "completed flight cleared before save");
            Equal(6, ship.Modules.Count, "loaded SCG has all six original mounts");
            Equal(12, ship.Modules[0].ItemCount, "cargo stays in its original mount across every reload");
            Equal(12, ship.Modules.Sum(module => module.ItemCount), "empty restored mounts create no cargo");
        }

        private void InterstellarOrdinaryClocks()
        {
            foreach (var route in new[] {0, 1, 2})
            {
                var ship = NewInterstellarRoute();
                Save.News = new Deuteros.Code.Objects.News();
                Save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research.Researched = false;
                if (route == 1) { ship.PlanetLocation = StellarBodies.atlantic; ship.DestinationPlanetLocation = StellarBodies.earth; }
                if (route == 2) { ship.PlanetLocation = StellarBodies.atlantic; ship.DestinationPlanetLocation = StellarBodies.chiron; }
                ship.EngageEngine();
                var starUpdates = route == 2 ? 26 : 208;
                for (var update = 1; update <= starUpdates; update++) AdvanceInterstellar(update % 2 == 0);
                Equal(route == 0, Save.Ships.Contains(ship), "ordinary arrival requires exact destination clock, not direction alone");
                if (route == 0)
                {
                    Equal(InterstellarFlight.FlightLeg.Local, ship.Flight.Leg, "matching star clock permits approach");
                    Equal(53, ship.Fuel, "ordinary original fuel trace");
                    for (var update = 0; update < 4; update++) AdvanceInterstellar();
                    Equal(StellarBodies.atlantic, ship.PlanetLocation, "ordinary matching flight arrives");
                }
                else
                {
                    Equal(1, Save.News.GetNews(100).Count(n => n.Contains(ship.Name + " Destroyed.")), "clock loss reported once");
                    AdvanceInterstellar();
                    Equal(1, Save.News.GetNews(100).Count(n => n.Contains(ship.Name + " Destroyed.")), "removed flight cannot repeat loss");
                }
            }
        }

        private void InterstellarFuelBoundaries()
        {
            foreach (var initial in new[] {1, 2, 108, 109})
            {
                var ship = NewInterstellarRoute(); ship.Fuel = initial; ship.EngageEngine();
                AdvanceInterstellar();
                Equal(initial != 1, Save.Ships.Contains(ship), "phase underflow loses immediately; exact phase cost does not");
                if (initial == 2) { Equal(0, ship.Fuel, "exact fuel remains valid at phase boundary"); AdvanceInterstellar(); Equal(false, Save.Ships.Contains(ship), "next unaffordable phase loses ship"); }
                if (initial < 100) continue;
                for (var update = 2; update <= 11; update++) AdvanceInterstellar();
                Equal(initial - 108, ship.Fuel, "ten phase changes cost108");
                AdvanceInterstellar();
                Equal(initial == 109 ? InterstellarFlight.FlightLeg.Hyperlight : InterstellarFlight.FlightLeg.Stranded,
                    ship.Flight.Leg, "burning final ordinary unit doubles pending Hyperlight countdown");
                if (initial == 109) AdvanceInterstellar();
                Equal(InterstellarFlight.FlightLeg.Stranded, ship.Flight.Leg, "zero-fuel transition coasts to star then strands");
            }
        }

        private void InterstellarMalformedSave()
        {
            var ship = NewInterstellarRoute(); ship.EngageEngine(); AdvanceInterstellar();
            var original = SaveStorage.Serialize(Save); var active = Save;
            foreach (var corrupt in new Action<JObject>[] {
                f => f["Leg"] = 9, f => f["Phase"] = 0, f => f["Phase"] = 12,
                f => f["Remaining"] = 0, f => f["Remaining"] = 50000,
                f => f["Fraction"] = -1, f => f["Fraction"] = 256,
                f => f["ClockOffset"] = long.MaxValue, f => f["ClockOffset"] = 100,
                f => f.Remove("Phase"), f => f["Remaining"] = null })
            {
                var document = JObject.Parse(original); corrupt((JObject)document["Game"]["Ships"][0]["Flight"]);
                var rejected = false;
                try { GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(document.ToString())); }
                catch (Exception e) when (e is IOException || e is InvalidDataException || e is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "malformed flight rejected before world activation");
                Equal(true, ReferenceEquals(active, Save), "rejected save cannot replace current world");
            }
        }

        private void InterstellarEmptyStarArrival()
        {
            var ship = NewInterstellarRoute(); ship.Fuel = 108; ship.EngageEngine();
            for (var update = 0; update < 12; update++) AdvanceInterstellar();
            Equal(6, ship.Flight.Remaining, "empty-fuel star arrival starts original six-update loss countdown");
            for (var update = 1; update <= 6; update++)
            {
                AdvanceInterstellar();
                Equal(update < 6, Save.Ships.Contains(ship), "stranded star arrival expires once after six updates");
                Equal(StellarBodies.mercury, ship.PlanetLocation, "empty star arrival cannot invent a body approach");
                if (update < 6)
                {
                    var id = ship.ShipID;
                    GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                    ship = (SCG)Save.Ships.Single(s => s.ShipID == id);
                }
            }
        }

        private void InterstellarDisengagement()
        {
            var ship = NewInterstellarRoute();
            ship.ItemScanResults = new UnknownItem(UnknownItemTypes.AlienArtifact);
            ship.EngageEngine();
            Equal(true, ship.ItemScanResults == null, "SCG departure discards the previous orbit scan");
            ship.DisengageEngine();
            Equal(true, ship.EngineEngaged, "original disengagement rejects accelerated interstellar state");
            for (var update = 0; update < 11; update++) AdvanceInterstellar();
            ship.DisengageEngine();
            Equal(true, ship.EngineEngaged, "Hyperlight transition cannot be interrupted");
            AdvanceInterstellar();
            ship.DisengageEngine();
            Equal(false, ship.EngineEngaged, "ordinary destination approach can drift");
            AdvanceInterstellar();
            Equal(3, ship.Flight.Remaining, "drift still consumes a travel update");
            Equal(140, ship.Fuel, "ordinary drift retains original per-update fuel cost");
            // A save from before scan invalidation may still carry a departure scan.
            ship.ItemScanResults = new UnknownItem(UnknownItemTypes.AlienArtifact);
            Equal(true, ship.EngageEngine(), "drifting local leg may re-engage");
            Equal(true, ship.ItemScanResults == null, "resuming travel discards a retained legacy scan");
            Equal(4, ship.Flight.Remaining, "original re-engagement recalculates the current star-to-body route");
            Equal(true, ship.EngineEngaged, "resume sets engine flag");
            Equal(430000L, ship.Flight.ClockOffset, "resume never resets to origin star clock");
        }

        private void HyperlightWarlord()
        {
            var ship = NewInterstellarRoute(); ship.Pilot.AddAction(31); // Forty ordinary actions: Admiral.
            Equal(3, ship.Pilot.GetLevel(), "Admiral before Hyperlight departure");
            ship.EngageEngine();
            for (var update = 0; update < 11; update++) AdvanceInterstellar();
            Equal(3, ship.Pilot.GetLevel(), "entering Hyperlight does not promote before arrival");
            var id = ship.ShipID;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (SCG)Save.Ships.Single(s => s.ShipID == id);
            AdvanceInterstellar();
            Equal(4, ship.Pilot.GetLevel(), "actual Hyperlight arrival promotes Admiral to Warlord");
            Equal("Warlord", ship.Pilot.GetLevelString(), "rank display uses persisted milestone");
            Equal(41, (int)JObject.Parse(SaveStorage.Serialize(Save))["Game"]["Ships"][0]["Pilot"]["ActionsTaken"],
                "arrival retains ordinary actions and awards exactly one");
            for (var update = 0; update < 4; update++) AdvanceInterstellar();
            ship.Fuel = 250; ship.EngageEngine();
            Equal(StellarBodies.mercury, ship.DestinationPlanetLocation, "return course is Mercury, with eight-update local approach");
            for (var update = 0; update < 20; update++) AdvanceInterstellar();
            Equal(Ship_States.UnDocked, ship.ShipState, "return route fully completed before checking experience");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (SCG)Save.Ships.Single(s => s.ShipID == id);
            Equal(4, ship.Pilot.GetLevel(), "repeat flight and reload retain Warlord");
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains("New Rank:Warlord")), "only first eligible Hyperlight arrival reports promotion");
            Equal(44, (int)JObject.Parse(SaveStorage.Serialize(Save))["Game"]["Ships"][0]["Pilot"]["ActionsTaken"],
                "two composed routes award four completed-leg actions");
        }

        private void HyperlightRankBoundary()
        {
            var ship = NewInterstellarRoute(); ship.Pilot.AddAction(30); // Captain at39.
            ship.EngageEngine();
            Equal(2, ship.Pilot.GetLevel(), "new interstellar departure cannot prematurely award arrival experience");
            for (var update = 0; update < 12; update++) AdvanceInterstellar();
            Equal(3, ship.Pilot.GetLevel(), "Captain earns Admiral only after Hyperlight eligibility check");
            Equal(0, Save.News.GetNews(100).Count(n => n.Contains("New Rank:Warlord")), "same arrival cannot chain Admiral and Warlord");
            ship = NewInterstellarRoute(); ship.Pilot.AddAction(31);
            Save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research.Researched = false;
            ship.EngageEngine();
            for (var update = 0; update < 212; update++) AdvanceInterstellar();
            Equal(3, ship.Pilot.GetLevel(), "ordinary matching-clock arrival cannot award Warlord");
            ship.Pilot.AddAction(1000);
            Equal(3, ship.Pilot.GetLevel(), "ordinary experience remains capped at Admiral");
        }

        private async Task<ShipInterior> OpenInterstellarInterior(SCG ship)
        {
            InitializeUi();
            Save.CurrentPlanet = ship.PlanetLocation;
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
            await InputFrames();
            return ActiveScreen<ShipInterior>();
        }

        private async Task InterstellarClockDisplay()
        {
            InitializeUi();
            var ship = NewInterstellarRoute(); ship.Pilot.AddAction(31); ship.EngageEngine();
            AdvanceInterstellar();
            var interior = await OpenInterstellarInterior(ship);
            var menu = ActiveScreen<MainMenu>();
            menu.DayTick(Save.CurrentDay, Save.CurrentDay);
            Equal("3100 103.00", menu.Time.Text, "SCG header displays private accelerated clock");
            Equal("ETA:\n3104 401.15", interior.GetNode<Label>("TextLayout/ETA").Text, "arrival projects destination clock and all remaining updates");
            for (var update = 2; update <= 11; update++) AdvanceTickDay();
            Equal("0000 000.00", menu.Time.Text, "Hyperlight transition displays cleared private clock");
            AdvanceTickDay();
            Equal("3104 412.00", menu.Time.Text, "arrival synchronizes displayed private clock to Proxima");
            Equal("Approaching\nAtlantic", interior.GetNode<Label>("TextLayout/Status").Text, "body approach is distinguished from star flight");
            Equal("Proxima", menu.Star.Text, "header star follows completed interstellar leg");
            Equal(true, interior.GetNode<Label>("TextLayout/PilotName").Text.StartsWith("Warlord"), "real crew display receives arrival promotion");
            await CaptureDisplayEvidence("hyperlight-warlord-arrival");
            for (var update = 0; update < 4; update++) AdvanceTickDay();
            Equal(StellarBodies.atlantic, ship.PlanetLocation, "actual scene reaches final body");
            Equal("3104 416.00", menu.Time.Text, "completed flight keeps destination star calendar");
            GameCore.SingletonInstance.ChangeScene(Scenes.Earth_Ground, new List<SceneVariables>());
            await InputFrames();
            Equal("3100 116.00", menu.Time.Text, "leaving SCG returns to selected Earth calendar immediately");
        }

        private async Task InterstellarCourseLock()
        {
            InitializeUi(); var ship = NewInterstellarRoute();
            var interior = await OpenInterstellarInterior(ship);
            Press(interior, "SetCourse");
            var map = interior.GetNode<Node>("StarMap").GetChildren().OfType<Deuteros.Code.Platform.Screens.StarMap>().Single();
            ship.EngageEngine();
            map.CurrentLocation = StellarBodies.chiron;
            interior._Input(new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true });
            Equal(StellarBodies.atlantic, ship.DestinationPlanetLocation, "retained map cannot change an already active flight");
            interior.UpdateState();
            Equal(true, interior.GetNode<BaseButton>("SetCourse").Disabled, "active-flight course control communicates unavailable action");
            Press(interior, "SetCourse");
            Equal(0, interior.GetNode<Node>("StarMap").GetChildCount(), "retained callback cannot reopen course selection in flight");
        }

        private async Task InterstellarLossScreen()
        {
            InitializeUi(); var ship = NewInterstellarRoute(); ship.Fuel = 1;
            var interior = await OpenInterstellarInterior(ship);
            Press(interior, "EngineControls/EngageEngine");
            AdvanceTickDay(); await InputFrames();
            Equal(false, Save.Ships.Contains(ship), "real simulation commits phase-fuel loss");
            Equal(false, GameCore.SingletonInstance.currentScene == Scenes.ShipInterior, "destroyed ship leaves its controls");
            Equal(false, Cursor.IsLocked, "loss does not leave the virtual cursor trapped");
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains(ship.Name + " Destroyed.")), "loss news emitted once");
            await CaptureDisplayEvidence("interstellar-fuel-loss");
        }

        private void WarlordSaveAndConsumers()
        {
            var ship = NewInterstellarRoute(); ship.Pilot.AddAction(31); ship.EngageEngine();
            for (var update = 0; update < 16; update++) AdvanceInterstellar();
            var original = SaveStorage.Serialize(Save);
            foreach (var corrupt in new Action<JObject>[] {
                t => t["ActionsTaken"] = 39, t => t["Type"] = (int)StaffType.Research,
                t => t["Warlord"] = null })
            {
                var document = JObject.Parse(original); corrupt((JObject)document["Game"]["Ships"][0]["Pilot"]);
                var rejected = false;
                try { SaveStorage.Deserialize(document.ToString()); }
                catch (Exception e) when (e is InvalidDataException || e is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "ineligible or malformed Warlord rejected");
            }
            var legacy = JObject.Parse(original); ((JObject)legacy["Game"]["Ships"][0]["Pilot"]).Remove("Warlord");
            Equal(3, SaveStorage.Deserialize(legacy.ToString()).Ships[0].Pilot.GetLevel(), "old saves do not invent Warlord from accumulated actions");
            ship.DroneCount = 10;
            var battle = new Deuteros.Code.Objects.Battle.BattleLogic(ship, new IOS { DroneCount = 10 }, 0, null, null, null, null, null);
            Equal(80, battle.Player1Power, "existing battle calculation consumes Warlord level4");
            var module = ship.Modules[0]; module.ModuleType = Module_Types.Cryo; module.ItemStored = ItemTypes.none; module.ItemCount = 0;
            module.StaffStored = ship.Pilot; ship.Pilot = null;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (SCG)Save.Ships.Single(); module = ship.Modules[0];
            Equal(4, module.StaffStored.GetLevel(), "frozen crew retains Warlord across save/load");
            GameCore.Earth.PlanetResources.AddStaff(module.StaffStored); module.StaffStored = null;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(4, GameCore.Earth.PlanetResources.Staff.Single(s => s?.Leader == "Route").GetLevel(), "returned crew retains rank in station/ground roster");
        }

        private void InterstellarDamageAndDiscovery()
        {
            var ship = NewInterstellarRoute(); ship.EngineDamaged = true;
            Equal(20, ship.TravelTimeRemain(), "Hyperlight keeps its phase trigger; damaged local distance doubles");
            ship.EngageEngine(); AdvanceInterstellar();
            Equal(8598, ship.Flight.Remaining, "damage doubles original star distance, not acceleration speed");
            var id = ship.ShipID;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (SCG)Save.Ships.Single(s => s.ShipID == id);
            for (var update = 1; update < 20; update++) AdvanceInterstellar();
            Equal(StellarBodies.atlantic, ship.PlanetLocation, "damaged Hyperlight route completes");
            Equal(133, ship.Fuel, "damaged approach consumes eight units after Hyperlight");
            ship = NewInterstellarRoute();
            var research = Save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research;
            research.Researched = false; ship.EngageEngine();
            for (var update = 0; update < 11; update++) AdvanceInterstellar();
            Equal(InterstellarFlight.FlightLeg.Accelerating, ship.Flight.Leg, "incomplete research cannot trigger Hyperlight");
            research.Researched = true;
            AdvanceInterstellar();
            Equal(InterstellarFlight.FlightLeg.Hyperlight, ship.Flight.Leg, "new research is observed at next eligible phase check");
            AdvanceInterstellar();
            Equal(InterstellarFlight.FlightLeg.Local, ship.Flight.Leg, "late discovery reaches real Hyperlight arrival");
        }

        private void InterstellarPendingClock()
        {
            var core = GameCore.SingletonInstance; core.SetProcess(false);
            var ship = NewInterstellarRoute(); ship.EngageEngine();
            GameCore.LockScreen();
            try
            {
                core._Process(GameClock.NormalIntervalSeconds);
                Equal(4300, ship.Flight.Remaining, "blocked producer cannot advance a flight");
                Equal(310010001L, ship.AbsoluteClock, "private clock follows produced pending increment exactly once");
                core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                ship = (SCG)Save.Ships.Single();
                core._Process(GameClock.NormalIntervalSeconds);
                Equal(310010001L, ship.AbsoluteClock, "saved pending increment cannot be queued twice");
            }
            finally { GameCore.UnLockScreen(); }
            core._Process(0);
            Equal(4298, ship.Flight.Remaining, "release consumes saved update once");
            Equal(310010201L, ship.AbsoluteClock, "private acceleration added after pending clock consumption");
        }

        private void InterstellarAccFinish()
        {
            var ship = NewInterstellarRoute(); ship.FuelType = ItemTypes.meh_fuel;
            ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, Source = ship.PlanetLocation, Destination = ship.DestinationPlanetLocation,
                SourceItems = new List<ItemTypes> {ItemTypes.iron}, DestinationItems = new List<ItemTypes> {ItemTypes.copper},
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron, CycleMode = true };
            var station = Save.BaseGameData.Planets[StellarBodies.atlantic].Station; station.Built = true;
            station.Resources.Stores[ItemTypes.iron] = 0; station.Resources.Stores[ItemTypes.copper] = 99; station.Resources.Stores[ship.FuelType] = 1000;
            ship.EngageEngine();
            for (var update = 0; update < 12; update++) AdvanceInterstellar();
            Equal(Ship_States.InTransit, ship.ShipState, "intermediate star arrival does not notify ACC to dock");
            Equal(0, station.Resources.Stores[ItemTypes.iron], "intermediate arrival cannot credit station cargo");
            for (var update = 0; update < 4; update++) AdvanceInterstellar();
            Equal(Ship_States.Docking, ship.ShipState, "final body arrival notifies ACC once");
            AdvanceInterstellar();
            Equal(Ship_States.Docked, ship.ShipState, "ACC completes docking");
            Equal(false, ship.ACC.CycleMode || ship.ACC.Active, "complete-cycle stops at actual destination");
            Equal(12, station.Resources.Stores[ItemTypes.iron], "cargo credited exactly once");
            Equal(99, station.Resources.Stores[ItemTypes.copper], "finish does not load reverse cargo");
            Equal(1000, station.Resources.Stores[ship.FuelType], "finish does not refuel at destination");
            AdvanceInterstellar();
            Equal(12, station.Resources.Stores[ItemTypes.iron], "later updates do not repeat delivery");
        }

        private void InterstellarAccRoundTrip()
        {
            var ship = NewInterstellarRoute(); ship.FuelType = ItemTypes.meh_fuel;
            ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, Source = ship.PlanetLocation, Destination = ship.DestinationPlanetLocation,
                SourceItems = new List<ItemTypes> {ItemTypes.iron}, DestinationItems = new List<ItemTypes> {ItemTypes.copper},
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron, Active = true };
            Save.GameConfig.IOSRefuelThreshold = 250;
            foreach (var body in new[] {StellarBodies.mercury, StellarBodies.atlantic})
            {
                var station = Save.BaseGameData.Planets[body].Station; station.Built = true;
                station.Resources.Stores[ship.FuelType] = 1000;
                station.Resources.Stores[ItemTypes.iron] = 0;
                station.Resources.Stores[ItemTypes.copper] = body == StellarBodies.atlantic ? 20 : 0;
            }
            ship.EngageEngine();
            for (var update = 0; update < 18; update++) AdvanceInterstellar();
            Equal(StellarBodies.atlantic, ship.PlanetLocation, "ACC actually reached destination before launching return");
            Equal(Ship_States.InTransit, ship.ShipState, "ACC starts corrected return flight through shared engine path");
            Equal(12, Save.BaseGameData.Planets[StellarBodies.atlantic].Station.Resources.Stores[ItemTypes.iron], "outbound cargo delivered once");
            Equal(20, ship.Modules[0].ItemCount, "return cargo loaded at remote station");
            ship.ACC.Active = false; ship.ACC.CycleMode = true;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (SCG)Save.Ships.Single();
            for (var update = 0; update < 21; update++) AdvanceInterstellar();
            Equal(StellarBodies.mercury, ship.PlanetLocation, "saved automatic roundtrip returns to correct source");
            Equal(Ship_States.Docked, ship.ShipState, "finish request docks after return body leg");
            Equal(false, ship.ACC.Active || ship.ACC.CycleMode, "finish request stays stopped");
            Equal(20, Save.BaseGameData.Planets[StellarBodies.mercury].Station.Resources.Stores[ItemTypes.copper], "return cargo conserved");
            Equal(12, Save.BaseGameData.Planets[StellarBodies.atlantic].Station.Resources.Stores[ItemTypes.iron], "return never repeats outbound delivery");
        }

        private void InterstellarRescuedFallCounter()
        {
            var ship = NewInterstellarRoute();
            ship.PlanetLocation = StellarBodies.atlantic;
            ship.DestinationPlanetLocation = StellarBodies.neptune;
            ship.Fuel = 137;
            ship.FallingCount = 4;
            ship.EngageEngine();
            for (var update = 0; update < 100 && ship.ShipState == Ship_States.InTransit; update++) AdvanceInterstellar();
            Equal(StellarBodies.neptune, ship.PlanetLocation, "resupplied SCG completes Hyperlight and long coasting approach");
            Equal(0, ship.Fuel, "long destination approach exhausts remaining fuel");
            for (var update = 1; update <= 5; update++)
            {
                AdvanceInterstellar();
                Equal(update < 5, Save.Ships.Contains(ship), "fuelled flight clears previous fall debt before a new empty-fuel arrival");
            }
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains(ship.Name + " Destroyed.")), "new fall countdown reports one loss");
        }
    }
}
