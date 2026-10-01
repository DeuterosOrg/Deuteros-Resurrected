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

            if (StartTravelDay == 0) return totalJourneyTime;

            return totalJourneyTime - (int)(GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay - StartTravelDay);
        }
    }
}
