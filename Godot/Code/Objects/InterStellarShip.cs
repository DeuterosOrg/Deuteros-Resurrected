using Deuteros.Code.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
    public class InterStellarShip : Ship, IShip
    {
        public bool InTransit { get; set; }
        public bool DFCC { get; set; }
        public bool Scanning { get; set; }
        public bool Mining { get; set; }
		public Asteroid AsteroidScanResults { get { if (ItemScanResults != null && ItemScanResults is Asteroid) return (Asteroid)ItemScanResults; else return null; } }
        public GrappleItem ItemScanResults { get; set; }
        public bool MethanoidOwned { get; set; }

        public bool PTL { get; set; }
        public int DroneCount { get; set; }
        public int AttackedCount { get; set; }
        // Zero-based local IOS allocation, or global SCG allocation; -1 is not allocated yet.
        public int AutomationSlot { get; set; } = -1;

        public static void EnsureAutomationSlots(GameData.SaveFile save)
        {
            foreach (var pool in save.Ships.OfType<InterStellarShip>().Where(s => s is not EnemyFleet)
                .GroupBy(s => (s.ShipType, s is SCG ? Enums.StellarBodies.none : s.StarLocation)))
            {
                var used = pool.Where(s => s.AutomationSlot >= 0).Select(s => s.AutomationSlot).ToHashSet();
                foreach (var ship in pool.Where(s => s.AutomationSlot < 0))
                {
                    // ponytail: preserve remake fleets above 16 slots; enforce original capacity at construction if required.
                    ship.AutomationSlot = Enumerable.Range(0, pool.Count()).First(slot => !used.Contains(slot));
                    used.Add(ship.AutomationSlot);
                }
            }
        }

        internal bool AsteroidClockPhase(int mask) => AutomationSlot >= 0
            && (AutomationSlot & mask) == (int)(((310000000ul
                + GameCore.SingletonInstance.GameData.ActiveSaveFile.Clock.DateCentidays) >> 7) & (ulong)mask);


        public override int TravelTimeRemain()
        {
            int totalJourneyTime = 0;

            var startPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation];
            var destinationPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[DestinationPlanetLocation];

            if (startPlanet.MoonParentPlanetId != Enums.StellarBodies.none)
                startPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[startPlanet.MoonParentPlanetId];

            if (destinationPlanet.MoonParentPlanetId != Enums.StellarBodies.none)
                destinationPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[destinationPlanet.MoonParentPlanetId];

            //Travelling within the same planetary system
            if (startPlanet == destinationPlanet)
            {
                totalJourneyTime = Math.Max(Math.Abs(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation].Order - GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[DestinationPlanetLocation].Order), 1);
            }
            //We're going to a different planet
            else if (startPlanet != destinationPlanet)
            {
                totalJourneyTime = Math.Abs(destinationPlanet.Order - startPlanet.Order) * 4;
            }

            if (EngineDamaged) totalJourneyTime *= 2;

            if (ShipState != Enums.Ship_States.InTransit || StartTravelDay == 0) return totalJourneyTime;

            return totalJourneyTime - (int)(GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay - StartTravelDay);
        }
    }
}
