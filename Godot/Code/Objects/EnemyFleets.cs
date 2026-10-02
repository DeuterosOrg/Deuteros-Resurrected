using Deuteros.Code.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
    [Serializable]
    public partial class EnemyFleet : IOS
    {
        public int AttackTrigger { get; set; }
        public int AttackDay { get; set; }
        public int AttackCount { get; set; }
        public bool Attacking { get; set; }

        private Enums.StellarBodies FindAttackStation()
        {
            var star = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars[this.StarLocation];
            var planets = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Where(p => p.Value.ParentStar == star.StarId && p.Value.Station != null && p.Value.Station.BuildParts == 8 && !p.Value.ActiveMethanoid).OrderBy(p => p.Value.MethanoidAttackedCount).ThenByDescending(p=>p.Key);
            if (planets.Count()>0)
            {
                return planets.First().Key;
            }
            else
                return Enums.StellarBodies.none;
        }

        private void ChooseAttackTarget()
        {
            if (AttackCount > 0) AttackCount--;

            if (AttackCount == 0)
            {
                //set destinationplanetlocation
                var attackDestination = FindAttackStation();
                if (attackDestination != StellarBodies.none)
                {
                    DestinationPlanetLocation = attackDestination;
                }
            }

            if (DestinationPlanetLocation != StellarBodies.none)
            {
                var r = Random.Shared.Next(3);
                if (r == 0) r = 1;
                AttackCount = r;

                AttackDay = TravelTimeRemain() + Random.Shared.Next(63) + 1;
            }
        }

        public void ProcessAttackTrigger()
        {
            if (DroneCount >= AttackTrigger)
            {
                if (AttackDay == 0)
                {
                    ChooseAttackTarget();
                }
            }

        }
   
        public void CapturePlanet()
        {
            Attacking = false;
            var attackedPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[DestinationPlanetLocation];
            attackedPlanet.Station.MtxInstalled = true;
            attackedPlanet.Station.SdmInstalled = true;
            attackedPlanet.MethanoidAttackedCount = 0;
            var captured = !attackedPlanet.ActiveMethanoid;
            attackedPlanet.ActiveMethanoid = true;

            //todo remove planetary stocks
            attackedPlanet.Station.Resources.Stores.Items.Clear();
            attackedPlanet.PlanetResources.Stores.Items.Clear();

            attackedPlanet.Station.Resources.Stores.Items[ItemTypes.ios_drone] = 50;

            //remove all ships at this location except methanoid fleets
            IShip ship = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.FirstOrDefault(s => s.PlanetLocation == attackedPlanet.PlanetId && s.GetType()!=typeof(EnemyFleet));
            while (ship != null)
            {
                var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
                if (save.Ships.Remove(ship)) save.News.AddShipLoss(ship);
                ship = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.FirstOrDefault(s => s.PlanetLocation == attackedPlanet.PlanetId && s.GetType() != typeof(EnemyFleet));
            }

            //remove all stack from planet and station
            attackedPlanet.Station.Resources.RemoveAllStaff();
            attackedPlanet.PlanetResources.RemoveAllStaff();

            attackedPlanet.PlanetResources.Derricks = Random.Shared.Next(8);

            //ensure base is built and undamaged
            attackedPlanet.BaseBuildParts = 2;
            attackedPlanet.BaseDamaged = false;

            //install AOC
            attackedPlanet.Station.Factory.AOC = true;

            //random amounts of materials in orbit
            foreach (var material in attackedPlanet.PlanetResources.Materials)
            {
                attackedPlanet.Station.Resources.Stores.Items[material.MaterialType] = Random.Shared.Next(1024) + 100;
            }
            AttackCount = 0;
            if (captured) GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(attackedPlanet.PlanetId.ToScreenString(" ") + " CAPTURED !");
        }

        public void CancelAttack(Enums.StellarBodies attackLocation)
        {
            AttackDay = 0;
            Attacking = false;
            AttackTrigger = AttackTrigger * 2;
            if (AttackTrigger > 200) AttackTrigger = 200;

            //all ships at this location are no longer under attack
            foreach (Ship ship in GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Where(s => s.PlanetLocation == attackLocation && s.ShipType != Ship_Types.Shuttle))
            {
                ((InterStellarShip)ship).AttackedCount = 0;
            }

        }
        public void ProcessFleet()
        {
            if (AttackDay > 0)
            {
                AttackDay--;
                if (!Attacking && AttackDay == 0)
                {
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip = false;
                    PlanetLocation = DestinationPlanetLocation;
                    var attackedPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[DestinationPlanetLocation];
                    attackedPlanet.MethanoidAttackedCount++;
                    Attacking = true;
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(attackedPlanet.PlanetId.ToScreenString(" ") + " UNDER ATTACK !");
                    AttackDay = 5;
                }
                else if (Attacking && AttackDay == 0)
                {
                    CapturePlanet();
                }
            }
        }
    }
}
