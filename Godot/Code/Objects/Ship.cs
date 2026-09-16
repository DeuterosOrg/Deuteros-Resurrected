using Deuteros.Code.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
    public class Ship : IShip
    {
        public Guid ShipID { get; set; }
        public Enums.StellarBodies PlanetLocation { get; set; }
        public Enums.StellarBodies StarLocation { get; set; }
        public Enums.StellarBodies DestinationPlanetLocation { get; set; }
        public Enums.StellarBodies DestinationStarLocation { get; set; }
        public uint StartTravelDay { get; set; }
        public uint StartRepairDay { get; set; }
        public Enums.Ship_Types ShipType { get; set; }
        public Enums.Ship_States ShipState { get; set; }
        public bool Engine { get; set; }
        public Staff Pilot { get; set; }
        public string Name { get; set; }
        public int Fuel { get; set; }
        public bool LocationView { get; set; }
        public Enums.ItemTypes FuelType { get; set; }
        public List<ShipModule> Modules { get; set; }
        public ACC ACC { get; set; }
        public bool EngineEngaged { get; set; }
        public int FallingCount { get; set; }

		public void Dock()
        {
            if (ShipState == Ship_States.UnDocked && (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation].Station.Built || PlanetLocation == StellarBodies.asteroids))
            {
                StartTravelDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
                ShipState = Ship_States.Docking;
            }
        }

        public void Land()
        {
            if (ShipType == Ship_Types.Shuttle && ShipState == Ship_States.UnDocked && Fuel>0)
            {
                ShipState = Ship_States.Landing;
                StartTravelDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
            }
        }

        public void TakeOff()
        {
            if (Engine && Fuel > 0 && ShipState != Ship_States.CrewRepairing)
            {
                if (Pilot != null) Pilot.AddAction();

                //clear the Shuttle/Ship State to prevent scrolling in ship bay when ship is not there
                if (ShipType == Ship_Types.Shuttle)
                {
                    if (((Shuttle)this).OnGround == true)
                        GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation].ShuttleState = 0;
                    else
                        GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation].Station.ShuttleState = 0;
                }
                else
                {
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation].Station.StarShipState = 0;
                }

                if (ShipType == Ship_Types.Shuttle && ((Shuttle)this).OnGround == true)
                {
                    ((Shuttle)this).OnGround = false;
                    ShipState = Ship_States.TakingOff;
                    StartTravelDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
                }
                else if (ShipState == Ship_States.Docked)
                {
                    ShipState = Ship_States.Launching;
                    StartTravelDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
                }
            }
        }

        public bool EngageEngine()
        {
            if (ShipState == Ship_States.UnDocked && Engine && DestinationPlanetLocation != PlanetLocation)
            {
                if (Pilot != null) Pilot.AddAction();

                EngineEngaged = true;
                ShipState = Ship_States.InTransit;
                StartTravelDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
                
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DisengageEngine()
        {
            EngineEngaged = false;
        }

        public virtual int TravelTimeRemain()
        {
            return 0;
        }
    }
}