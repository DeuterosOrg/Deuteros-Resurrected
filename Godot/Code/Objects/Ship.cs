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
        public bool EngineDamaged { get; set; }
        public Staff Pilot { get; set; }
        public string Name { get; set; }
        public int Fuel { get; set; }
        // Fuel is gauge units; DFCC hulls require ten store units for each one.
        public int FuelUnitCost => this is InterStellarShip { DFCC: true } ? 10 : 1;
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

        public bool CanTravelTo(StellarBodies destination)
        {
            var planets = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets;
            return planets.TryGetValue(PlanetLocation, out var origin)
                && planets.TryGetValue(destination, out var target)
                && (ShipType == Ship_Types.SCG || origin.ParentStar == target.ParentStar);
        }

        public bool EngageEngine() => EngageEngine(() => Random.Shared.Next(2) == 0);

        internal bool EngageEngine(Func<bool> engineDamageRoll)
        {
            if (this is SCG { Flight.Leg: InterstellarFlight.FlightLeg.Local } drifting
                && ShipState == Ship_States.InTransit && !EngineEngaged && Engine && Fuel > 0)
            {
                drifting.Flight.Remaining = InterstellarFlight.LocalDistance(DestinationPlanetLocation) * (EngineDamaged ? 2 : 1);
                EngineEngaged = true;
                return true;
            }
            if (ShipState == Ship_States.UnDocked && Engine && Fuel > 0 && DestinationPlanetLocation != PlanetLocation
                && CanTravelTo(DestinationPlanetLocation))
            {
                // Body records are authoritative; stale save/ACC star fields must not mislabel arrival.
                var planets = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets;
                StarLocation = planets[PlanetLocation].ParentStar;
                DestinationStarLocation = planets[DestinationPlanetLocation].ParentStar;
                if (Pilot != null) Pilot.AddAction();

                // Original danger-state departure rolls for damage only without DFCC.
                // A damaged drive remains usable, so keep it distinct from absence.
                var data = GameCore.SingletonInstance.GameData;
                if (!EngineDamaged && this is InterStellarShip stellar && !stellar.MethanoidOwned
                    && !stellar.DFCC && data.ActiveSaveFile.AtWar
                    && (planets[PlanetLocation].ActiveMethanoid || data.PlanetUnderAttack(PlanetLocation)))
                    EngineDamaged = engineDamageRoll();

                if (this is SCG scg) scg.Flight = InterstellarFlight.Start(scg);

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
            // Original $30FE0 accepts local state9, but rejects accelerated and Hyperlight states.
            if (this is SCG { Flight: not null } scg && scg.Flight.Leg != InterstellarFlight.FlightLeg.Local) return;
            EngineEngaged = false;
        }

        public virtual int TravelTimeRemain()
        {
            return 0;
        }
    }
}
