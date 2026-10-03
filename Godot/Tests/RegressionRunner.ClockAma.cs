using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using Newtonsoft.Json.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void NaturalAmaPhase()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            var ship = AmaCargoShip();
            ship.Modules[1].ItemCount = 0;
            Save.Clock.DateCentidays = 126;
            core._Process(315.6);
            Equal(0, ship.Modules[1].ItemCount, "slot zero waits through raw clock phase three");
            core._Process(315.6);
            var first = ship.Modules[1].ItemCount;
            Equal(true, first >= 12 && first <= 43, "phase zero deposits one original-sized batch");
            core._Process(315.6);
            var second = ship.Modules[1].ItemCount - first;
            Equal(true, second >= 12 && second <= 43, "each consumed fractional update in the matching phase mines once");
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(Save.CurrentDay, Save.CurrentDay);
            Equal(first + second, ship.Modules[1].ItemCount, "a nonadvancing callback cannot repeat mining");
        }

        private void ManualAmaPhases()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            var ship = AmaCargoShip();
            var expected = new[] { 2, 7, 12, 17, 22, 23, 27, 28, 32, 33, 38 };
            Save.Clock.DateCentidays = 0;
            for (var day = 1; day <= 40; day++)
            {
                ship.Modules[1].ItemCount = 0;
                AdvanceTickDay();
                Equal(expected.Contains(day), ship.Modules[1].ItemCount > 0, "original slot-zero mining gate at day " + day);
            }
        }

        private void AsteroidScanClockPhase()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            var ship = AmaCargoShip();
            ship.ShipState = Ship_States.UnDocked;
            ship.AsteroidScanResults.DayCount = 9; // Old provisional scanner guarantees replacement here.
            var original = ship.ItemScanResults;
            Save.Clock.DateCentidays = 126;
            core._Process(315.6);
            Equal(true, ReferenceEquals(original, ship.ItemScanResults), "off-phase scan retains the current asteroid");
            Save.Clock.DateCentidays = 639;
            core._Process(315.6);
            Equal(false, ReferenceEquals(original, ship.ItemScanResults), "matching eight-phase gate generates a fresh scan");
            var first = ship.ItemScanResults;
            core._Process(315.6);
            Equal(false, ReferenceEquals(first, ship.ItemScanResults), "next eligible natural update scans again");
            var second = ship.ItemScanResults;
            Save.Clock.DateCentidays = 767;
            core._Process(315.6);
            Equal(true, ReferenceEquals(second, ship.ItemScanResults), "leaving the phase preserves the last scan");
        }

        private int? SavedAutomationSlot(Guid id)
        {
            var document = JObject.Parse(SaveStorage.Serialize(Save));
            return (int?)document["Game"]["Ships"].Single(s => (Guid)s["ShipID"] == id)["AutomationSlot"];
        }

        private void StableAutomationSlots()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.Ships.Clear();
            var first = NavigationShip();
            var survivor = NavigationShip();
            Save.Ships.Add(first); core.TriggerShipCreated(first);
            Save.Ships.Add(survivor); core.TriggerShipCreated(survivor);
            Equal<int?>(0, SavedAutomationSlot(first.ShipID), "first IOS takes the first free slot");
            Equal<int?>(1, SavedAutomationSlot(survivor.ShipID), "second IOS has a distinct saved phase");
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Save.Ships.Reverse();
            Save.Ships.RemoveAll(s => s.ShipID == first.ShipID);
            var replacement = NavigationShip();
            Save.Ships.Add(replacement); core.TriggerShipCreated(replacement);
            Equal<int?>(1, SavedAutomationSlot(survivor.ShipID), "removal reload and reorder preserve the survivor's slot");
            Equal<int?>(0, SavedAutomationSlot(replacement.ShipID), "replacement reuses the freed slot");
            var remote = NavigationShip();
            remote.StarLocation = StellarBodies.proxima;
            remote.PlanetLocation = Save.BaseGameData.Planets.Values.First(p => p.ParentStar == StellarBodies.proxima).PlanetId;
            Save.Ships.Add(remote); core.TriggerShipCreated(remote);
            Equal<int?>(0, SavedAutomationSlot(remote.ShipID), "IOS allocation is per star");
            var scgs = Enumerable.Range(0, 2).Select(_ => new SCG { ShipType = Ship_Types.SCG, ShipState = Ship_States.Docked,
                Modules = new(), PlanetLocation = StellarBodies.earth, StarLocation = StellarBodies.the_sun }).ToArray();
            foreach (var ship in scgs) { Save.Ships.Add(ship); core.TriggerShipCreated(ship); }
            scgs[1].StarLocation = StellarBodies.proxima;
            scgs[1].PlanetLocation = remote.PlanetLocation;
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal<int?>(0, SavedAutomationSlot(scgs[0].ShipID), "SCG allocation is independent of IOS");
            Equal<int?>(1, SavedAutomationSlot(scgs[1].ShipID), "SCG keeps its global slot after changing star");
        }

        private void AutomationSlotSaveValidation()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.Ships.Clear();
            for (var i = 0; i < 2; i++)
            {
                var ship = NavigationShip(); Save.Ships.Add(ship); core.TriggerShipCreated(ship);
            }
            var original = SaveStorage.Serialize(Save);
            var legacy = JObject.Parse(original);
            foreach (JObject ship in legacy["Game"]["Ships"]) ship.Property("AutomationSlot")?.Remove();
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(legacy.ToString());
            Equal<int?>(0, SavedAutomationSlot(Save.Ships[0].ShipID), "legacy ships receive deterministic slots");
            Equal<int?>(1, SavedAutomationSlot(Save.Ships[1].ShipID), "legacy slots remain distinct");
            var active = Save;
            foreach (var value in new JToken[] { JValue.CreateNull(), new JValue(-2), new JValue(0) })
            {
                var corrupt = JObject.Parse(original);
                corrupt["Game"]["Ships"][1]["AutomationSlot"] = value;
                var rejected = false;
                try { core.LoadSavedGame(SaveStorage.Deserialize(corrupt.ToString())); }
                catch (Exception error) when (error is InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "null negative or duplicated allocation is rejected");
                Equal(true, ReferenceEquals(active, Save), "invalid allocation cannot replace the world");
            }
        }

        private void AsteroidApproachCountdown()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            var ship = AmaCargoShip();
            ship.ShipState = Ship_States.UnDocked;
            ship.Modules[1].ItemCount = 0;
            ship.Dock();
            core._Process(315.6);
            Equal(Ship_States.Docking, ship.ShipState, "asteroid approach retains its second consumed update");
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (IOS)Save.Ships.Single();
            core._Process(315.6);
            Equal(Ship_States.Docked, ship.ShipState, "loaded approach completes on the second update");
            ship.TakeOff();
            core._Process(315.6);
            Equal(Ship_States.Launching, ship.ShipState, "asteroid departure retains its second consumed update");
            core._Process(315.6);
            Equal(Ship_States.UnDocked, ship.ShipState, "second departure update resumes scanning");
        }
    }
}
