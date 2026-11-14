using System;
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
        private void EnableRefining()
        {
            DisableFuelRefining();
            foreach (var type in new[] { ItemTypes.meh_fuel, ItemTypes.hed_fuel })
            {
                var fuel = Save.BaseGameData.ItemList.Single(i => i.ItemType == type);
                fuel.Locked = fuel.Research.Locked = false;
                fuel.Research.Researched = fuel.AutoProduce = true;
            }
        }

        private void RefiningBatchBoundaries()
        {
            EnableRefining();
            var moon = Save.BaseGameData.Planets[StellarBodies.the_moon];
            moon.Station.Built = true;
            moon.Station.BuildParts = 8;
            moon.BaseBuildParts = 2;
            moon.BaseDamaged = false;
            FuelRefining.EnsureSlots(Save);
            var batches = new[]
            {
                (GameCore.Earth.PlanetResources.Stores, ItemTypes.meh_fuel, 3, 2, 3, 50000),
                (GameCore.Earth.PlanetResources.Stores, ItemTypes.hed_fuel, 3, 2, 2, 50000),
                (moon.Station.Resources.Stores, ItemTypes.meh_fuel, 6, 6, 8, 49994),
                (moon.Station.Resources.Stores, ItemTypes.hed_fuel, 5, 5, 5, 50000),
                (moon.PlanetResources.Stores, ItemTypes.meh_fuel, 2, 2, 3, 49999)
            };
            foreach (var (stores, fuel, minimum, consumed, output, limit) in batches)
            {
                var a = fuel == ItemTypes.meh_fuel ? ItemTypes.hydrogen : ItemTypes.deuterium;
                var b = fuel == ItemTypes.meh_fuel ? ItemTypes.methane : ItemTypes.helium;
                void Tick() { FuelRefining.DayTick(0, 1); FuelRefining.DayTick(1, 2); }
                stores[a] = minimum - 1; stores[b] = minimum; stores[fuel] = 0;
                Tick(); Equal(0, stores[fuel], "first input must meet minimum");
                stores[a] = minimum; stores[b] = minimum - 1;
                Tick(); Equal(0, stores[fuel], "second input must meet minimum");
                stores[a] = stores[b] = minimum; stores[fuel] = limit;
                Tick(); Equal(minimum, stores[a], "full output does not consume ingredients");
                Equal(limit, stores[fuel], "exact output threshold blocks refining");
                stores[fuel] = limit - 1;
                Tick(); Equal(limit - 1 + output, stores[fuel], "original threshold is checked before addition, without clamping");
                Equal(minimum - consumed, stores[a], "first original batch charge");
                Equal(minimum - consumed, stores[b], "second original batch charge");
            }
            var ground = moon.PlanetResources.Stores;
            ground[ItemTypes.helium] = ground[ItemTypes.deuterium] = 100;
            ground[ItemTypes.hed_fuel] = 0;
            foreach (var gate in new[] { "damaged", "unfinished base", "unfinished station", "hostile", "unresearched", "locked" })
            {
                moon.BaseDamaged = gate == "damaged";
                moon.BaseBuildParts = gate == "unfinished base" ? 1 : 2;
                moon.Station.Built = gate != "unfinished station";
                moon.ActiveMethanoid = gate == "hostile";
                var item = Save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.meh_fuel);
                item.Research.Researched = gate != "unresearched";
                item.Locked = gate == "locked";
                ground[ItemTypes.hydrogen] = ground[ItemTypes.methane] = 100;
                ground[ItemTypes.meh_fuel] = 0;
                FuelRefining.DayTick(0, 1); FuelRefining.DayTick(1, 2);
                Equal(0, ground[ItemTypes.meh_fuel], gate + " rejects ground refining");
                Equal(100, ground[ItemTypes.hydrogen], gate + " preserves inputs");
            }
            Equal(0, ground[ItemTypes.hed_fuel], "other ground never refines HeD");
        }

        private void RefiningSavedPhase()
        {
            EnableRefining();
            var earth = GameCore.Earth.PlanetResources.Stores;
            earth[ItemTypes.hydrogen] = earth[ItemTypes.methane] = earth[ItemTypes.meh_fuel] = 0;
            Save.RefiningPhase = 0;
            FuelRefining.DayTick(0, 1);
            Equal(1, Save.RefiningPhase, "empty stores still consume their global phase");
            var json = SaveStorage.Serialize(Save);
            Equal(json, SaveStorage.Serialize(Save), "serializing never reallocates stations");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(json);
            earth = GameCore.Earth.PlanetResources.Stores;
            earth[ItemTypes.hydrogen] = earth[ItemTypes.methane] = 3;
            FuelRefining.DayTick(1, 2);
            Equal(3, earth[ItemTypes.meh_fuel], "refill after load uses the next even phase");
            DisableFuelRefining();
            FuelRefining.DayTick(2, 3);
            Equal(0, Save.RefiningPhase, "no discovered enabled fuel leaves phase unchanged");
            var document = JObject.Parse(json);
            ((JObject)document["Game"]).Remove("RefiningPhase");
            foreach (var property in document.Descendants().OfType<JProperty>().Where(p => p.Name == "RefiningSlot").ToList()) property.Remove();
            var legacy = SaveStorage.Deserialize(document.ToString());
            Equal(0, legacy.RefiningPhase, "legacy phase starts at zero");
            Equal(true, legacy.BaseGameData.Planets.Values.All(p => p.Station.RefiningSlot == -1), "legacy missing slots remain unassigned until simulation");
            FuelRefining.EnsureSlots(legacy);
            var once = SaveStorage.Serialize(legacy);
            legacy = SaveStorage.Deserialize(once);
            FuelRefining.EnsureSlots(legacy);
            Equal(once, SaveStorage.Serialize(legacy), "legacy slot migration is stable across load");
            foreach (var field in new[] { "phase", "negative slot", "large slot", "duplicate slot", "null slot" })
            {
                var invalid = JObject.Parse(json);
                var slots = invalid.Descendants().OfType<JProperty>().Where(p => p.Name == "RefiningSlot").ToList();
                if (field == "phase") invalid["Game"]["RefiningPhase"] = 2;
                else if (field == "negative slot") slots[0].Value = -2;
                else if (field == "large slot") slots[0].Value = 160;
                else if (field == "null slot") slots[0].Value = JValue.CreateNull();
                else { slots[0].Value = 0; slots[1].Value = 0; }
                var rejected = false;
                try { SaveStorage.Deserialize(invalid.ToString()); }
                catch (Exception error) when (error is InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "invalid " + field + " rejected");
            }
        }

        private void RefiningSlotLifecycle()
        {
            var planets = Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.the_sun).ToList();
            foreach (var planet in planets) { planet.Station = new SpaceStation(planet.PlanetId); planet.ActiveMethanoid = false; }
            var alien = Save.BaseGameData.Planets[StellarBodies.jupiter];
            var other = Save.BaseGameData.Planets[StellarBodies.uranus];
            var earth = GameCore.Earth;
            alien.ActiveMethanoid = other.ActiveMethanoid = true;
            alien.Station.Built = other.Station.Built = true;
            alien.Station.RefiningSlot = 0; other.Station.RefiningSlot = 1;
            alien.Station.Resources.Stores[ItemTypes.iron] = 123;
            FuelRefining.ClaimPlayerSlot(Save, earth);
            earth.Station.BuildParts = 1;
            Equal(0, earth.Station.RefiningSlot, "new human frame takes first human slot");
            Equal(2, alien.Station.RefiningSlot, "construction relocates occupied alien record to first empty slot");
            Equal(123, alien.Station.Resources.Stores[ItemTypes.iron], "relocation preserves location inventory");
            alien.Station.SdmCountdown = 16;
            var ship = NavigationShip(); ship.PlanetLocation = alien.PlanetId; ship.ShipState = Ship_States.Docked; Save.Ships.Add(ship);
            Equal(true, SdmSystem.ApplySwitches(Save, alien, 0x0200), "actual defuse captures station");
            Equal(1, alien.Station.RefiningSlot, "capture takes next human slot");
            Equal(2, other.Station.RefiningSlot, "capture swaps occupant to captured record's old slot");
            earth.Station = new SpaceStation(earth.PlanetId);
            var moon = Save.BaseGameData.Planets[StellarBodies.the_moon];
            FuelRefining.ClaimPlayerSlot(Save, moon); moon.Station.BuildParts = 1;
            Equal(0, moon.Station.RefiningSlot, "station loss frees its slot for reuse");
            SaveStorage.Validate(Save);
        }

        private void RefiningInitialSlotsAndOverflow()
        {
            var sol = new[] { StellarBodies.uranus, StellarBodies.titania, StellarBodies.neptune,
                StellarBodies.triton, StellarBodies.pluto, StellarBodies.jupiter };
            for (var i = 0; i < sol.Length; i++)
                Equal(10 + i, Save.BaseGameData.Planets[sol[i]].Station.RefiningSlot, "original Sol record for " + sol[i]);
            foreach (var system in Save.BaseGameData.Planets.Values.Where(p => p.ParentStar != StellarBodies.the_sun)
                .GroupBy(p => p.ParentStar))
            {
                var slots = system.Where(p => p.ActiveMethanoid).Select(p => p.Station.RefiningSlot).OrderBy(i => i).ToArray();
                Equal(string.Join(",", Enumerable.Range(FuelRefining.Capacity(system.Key) - slots.Length, slots.Length)),
                    string.Join(",", slots), "original occupied record range for " + system.Key);
            }
            // Existing remake worlds can have more stations than the original table allowed.
            var sites = Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.the_sun && !p.ActiveMethanoid).Take(18).ToList();
            Equal(18, sites.Count, "overflow fixture has enough sites");
            foreach (var site in sites)
            {
                FuelRefining.ClaimPlayerSlot(Save, site);
                site.Station.BuildParts = 8;
                site.Station.Built = true;
            }
            Equal(17, sites.Last().Station.RefiningSlot, "existing remake capacity is preserved without slot aliasing");
            SaveStorage.Validate(Save);
            var before = SaveStorage.Serialize(Save);
            var loaded = SaveStorage.Deserialize(before);
            FuelRefining.EnsureSlots(loaded);
            Equal(before, SaveStorage.Serialize(loaded), "overflow world retains every allocation on load");
        }

        private void RefiningAfterAccFuelWait()
        {
            EnableRefining();
            var ship = AccBacklogShip(true);
            Save.GameConfig.ShuttleRefuelThreshold = 1;
            var stores = AccEndpointStore(ship, false);
            stores[ItemTypes.hydrogen] = stores[ItemTypes.methane] = 3;
            Save.RefiningPhase = 1;
            ship.ACC.Activate();
            Equal(true, ship.ACC.Refuelling, "ACC starts waiting with no fuel");
            AdvanceTickDay();
            Equal(Ship_States.Docked, ship.ShipState, "ship phase cannot consume later refining output");
            Equal(3, stores[ItemTypes.meh_fuel], "post-ship phase refines Earth batch");
            AdvanceTickDay();
            Equal(Ship_States.TakingOff, ship.ShipState, "ACC resumes on following simulation update");
            Equal(3, ship.Fuel, "refined fuel loaded once");
            Equal(0, stores[ItemTypes.meh_fuel], "station fuel charged once");
        }
    }
}
