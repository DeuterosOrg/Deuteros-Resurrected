using System;
using System.Linq;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Objects.Interfaces;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
    public static class FuelRefining
    {
        // Original record capacities; all system offsets are even, so local parity suffices.
        public static int Capacity(StellarBodies star) => star switch
        {
            StellarBodies.proxima => 4, StellarBodies.centauri => 12,
            StellarBodies.lalande => 6, StellarBodies.sirius => 2,
            StellarBodies.procyon => 10, _ => 16
        };

        public static void EnsureSlots(SaveFile save)
        {
            foreach (var system in save.BaseGameData.Planets.Values.GroupBy(p => p.ParentStar))
            {
                var stations = system.Where(p => p.Station.Built || p.Station.BuildParts > 0).ToList();
                var used = stations.Where(p => p.Station.RefiningSlot >= 0).Select(p => p.Station.RefiningSlot).ToHashSet();
                foreach (var planet in stations.Where(p => p.Station.RefiningSlot < 0)
                    .OrderBy(p => p.ActiveMethanoid).ThenBy(p => p.Station.StationOrdinal).ThenBy(p => p.PlanetId))
                {
                    var slot = planet.ActiveMethanoid
                        ? Enumerable.Range(0, Capacity(system.Key)).Reverse().FirstOrDefault(i => !used.Contains(i), -1)
                        : -1;
                    if (slot < 0) slot = Enumerable.Range(0, system.Count()).First(i => !used.Contains(i));
                    planet.Station.RefiningSlot = slot;
                    used.Add(slot);
                }
            }
        }

        public static void ClaimPlayerSlot(SaveFile save, IPlanet planet)
        {
            EnsureSlots(save);
            var stations = save.BaseGameData.Planets.Values.Where(p => p.ParentStar == planet.ParentStar
                && p != planet && (p.Station.Built || p.Station.BuildParts > 0)).ToList();
            var humanSlots = stations.Where(p => !p.ActiveMethanoid).Select(p => p.Station.RefiningSlot).ToHashSet();
            var slot = Enumerable.Range(0, stations.Count + 1).First(i => !humanSlots.Contains(i));
            var occupant = stations.FirstOrDefault(p => p.Station.RefiningSlot == slot);
            if (occupant != null)
            {
                var used = stations.Select(p => p.Station.RefiningSlot).ToHashSet();
                occupant.Station.RefiningSlot = planet.Station.RefiningSlot >= 0
                    ? planet.Station.RefiningSlot
                    : Enumerable.Range(0, stations.Count + 1).First(i => !used.Contains(i));
            }
            // ponytail: retain remake worlds above the original per-system capacity; enforcing that limit belongs to station construction.
            planet.Station.RefiningSlot = slot;
        }

        public static void DayTick(uint previousDay, uint currentDay)
        {
            if (currentDay <= previousDay) return;
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            bool Available(ItemTypes type) => save.BaseGameData.ItemList.Any(i => i.ItemType == type
                && i.AutoProduce && !i.Locked && i.Research?.Researched == true);
            var meh = Available(ItemTypes.meh_fuel);
            var hed = Available(ItemTypes.hed_fuel);
            if (!meh && !hed) return;
            EnsureSlots(save);
            save.RefiningPhase ^= 1;
            var earth = save.BaseGameData.Planets[StellarBodies.earth];
            if (save.RefiningPhase == 0)
            {
                if (meh) Batch(earth.PlanetResources.Stores, ItemTypes.meh_fuel, 3, 2, 3, 50000);
                if (hed) Batch(earth.PlanetResources.Stores, ItemTypes.hed_fuel, 3, 2, 2, 50000);
            }
            foreach (var planet in save.BaseGameData.Planets.Values)
            {
                if (planet.ActiveMethanoid || !planet.Station.Built
                    || (planet.Station.RefiningSlot & 1) != save.RefiningPhase) continue;
                if (meh) Batch(planet.Station.Resources.Stores, ItemTypes.meh_fuel, 6, 6, 8, 49994);
                if (hed) Batch(planet.Station.Resources.Stores, ItemTypes.hed_fuel, 5, 5, 5, 50000);
                if (meh && planet.PlanetId != StellarBodies.earth && planet.BaseBuildParts == 2 && !planet.BaseDamaged)
                    Batch(planet.PlanetResources.Stores, ItemTypes.meh_fuel, 2, 2, 3, 49999);
            }
        }

        private static void Batch(Store stores, ItemTypes fuel, int minimum, int consumed, int output, int limit)
        {
            var first = fuel == ItemTypes.meh_fuel ? ItemTypes.hydrogen : ItemTypes.deuterium;
            var second = fuel == ItemTypes.meh_fuel ? ItemTypes.methane : ItemTypes.helium;
            if (stores[first] < minimum || stores[second] < minimum || stores[fuel] >= limit) return;
            stores[first] -= consumed;
            stores[second] -= consumed;
            stores[fuel] += output;
        }
    }
}
