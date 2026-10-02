using System;
using System.Linq;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Objects.Interfaces;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
    public static class SdmSystem
    {
        public static bool CanAccess(SaveFile save, IPlanet planet) => planet != null
            && save.BaseGameData.Planets.TryGetValue(planet.PlanetId, out var current) && ReferenceEquals(current, planet)
            && planet.Station.Built
            && (planet.Station.SdmInstalled || planet.ActiveMethanoid)
            && (!planet.ActiveMethanoid || save.Ships.Any(s => s is InterStellarShip { MethanoidOwned: false }
                && s.PlanetLocation == planet.PlanetId && s.ShipState == Ship_States.Docked));

        public static void Docked(IShip ship)
        {
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            if (!save.AtWar || ship is not InterStellarShip { MethanoidOwned: false }) return;
            var planet = save.BaseGameData.Planets[ship.PlanetLocation];
            if (!planet.ActiveMethanoid || !planet.Station.Built) return;
            planet.Station.SdmInstalled = true;
            planet.Station.SdmCountdown = 16;
        }

        public static bool ApplySwitches(SaveFile save, IPlanet planet, int switches)
        {
            if (!CanAccess(save, planet)) return false;
            if (switches == 0x0002 && planet.Station.SdmCountdown == 0)
                planet.Station.SdmCountdown = 12;
            else if (switches == 0x0200 && planet.Station.SdmCountdown != 0)
            {
                planet.Station.SdmCountdown = 0;
                if (planet.ActiveMethanoid)
                {
                    planet.ActiveMethanoid = false;
                    planet.Station.Type = 8;
                    foreach (var material in planet.PlanetResources.Materials)
                        planet.Station.Resources.Stores[material.MaterialType] = Random.Shared.Next(1024) + 100;
                    planet.PlanetResources.Derricks = Random.Shared.Next(8);
                    if (planet.PlanetId != StellarBodies.earth) planet.BaseDamaged = true;
                    planet.Station.StationOrdinal = save.BaseGameData.Planets.Values
                        .Where(p => p != planet && !p.ActiveMethanoid && p.Station.Built)
                        .Select(p => p.Station.StationOrdinal).DefaultIfEmpty(0).Max() + 1;
                    save.News.AddNews(planet.PlanetId.ToScreenString(" ") + " station captured; SDM defused.");
                }
            }
            return true;
        }

        public static void AdvanceTime(double delta)
        {
            if (!double.IsFinite(delta) || delta <= 0) return;
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            // 50 original timer producer calls per count, using a PAL-equivalent second.
            // This timer is independent of day advancement and continues off the SDM screen.
            var total = save.SdmTimerRemainder + delta;
            var ticks = Math.Floor(total);
            save.SdmTimerRemainder = total - ticks;
            if (ticks == 0) return;
            foreach (var planet in save.BaseGameData.Planets.Values)
            {
                var count = planet.Station.SdmCountdown;
                if (count == 0) continue;
                if (ticks >= count) Destroy(save, planet);
                else planet.Station.SdmCountdown -= (int)ticks;
            }
        }

        public static void DayTick(uint previousDay, uint currentDay)
        {
            if (currentDay <= previousDay) return;
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            foreach (var planet in save.BaseGameData.Planets.Values)
            {
                var count = planet.Station.SdmCountdown;
                if (count == 0) continue;
                if ((count & 0x80) != 0)
                {
                    planet.Station.SdmCountdown = count & 0x7f;
                    continue;
                }
                if (planet.Station.Built && (!planet.ActiveMethanoid || count < 15)) Destroy(save, planet);
            }
        }

        private static void Destroy(SaveFile save, IPlanet planet)
        {
            planet.Station.SdmCountdown = 0;
            if (!planet.Station.Built) return;
            var lost = save.Ships.Where(ship => ship.PlanetLocation == planet.PlanetId && ship is not EnemyFleet
                && (ship is Shuttle shuttle
                    ? planet.PlanetId != StellarBodies.earth || (!shuttle.OnGround
                        && (ship.ShipState == Ship_States.Docking || ship.ShipState == Ship_States.Docked))
                    : ship.ShipState == Ship_States.Docked)).ToList();
            foreach (var ship in lost)
            {
                save.Ships.Remove(ship);
                save.News.AddNews(ship.Name + " lost in station self-destruct.");
            }
            foreach (var fleet in save.Ships.OfType<EnemyFleet>().Where(f => f.DestinationPlanetLocation == planet.PlanetId))
            {
                fleet.Attacking = false;
                fleet.AttackDay = fleet.AttackCount = 0;
                fleet.DestinationPlanetLocation = StellarBodies.none;
            }
            planet.Station = new SpaceStation(planet.PlanetId);
            planet.ActiveMethanoid = false;
            if (planet.PlanetId != StellarBodies.earth)
            {
                planet.BaseDamaged = planet.BaseBuildParts > 0;
                planet.PlanetResources.Stores = new Store();
                planet.PlanetResources.Derricks = 0;
                planet.PlanetResources.RemoveAllStaff();
            }
            save.TimeSkip = save.TimeSkipDay = false;
            save.News.AddNews(planet.PlanetId.ToScreenString(" ") + " orbital station destroyed.");
            GameCore.SingletonInstance.StationDestroyed(planet);
        }
    }
}
