using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects.Interfaces
{
    public interface IShip
    {
        public Guid ShipID { get; set; }
        public Enums.StellarBodies PlanetLocation { get; set; }
        public Enums.StellarBodies StarLocation { get; set; }
        public Enums.StellarBodies DestinationPlanetLocation { get; set; }
        public Enums.StellarBodies DestinationStarLocation { get; set; }
        public uint StartTravelDay { get; set; }
        public Enums.Ship_Types ShipType { get; set; }
        public Enums.Ship_States ShipState { get; set; }
        public bool Engine { get; set; }
        public Staff Pilot { get; set; }
        public string Name { get; set; }
        public int Fuel { get; set; }
        public bool LocationView { get; set; }
        public Enums.ItemTypes FuelType { get; set; }
        public List<ShipModule> Modules { get; set; }        
        public bool EngineEngaged { get; set; }
        public int FallingCount { get; set; }
        public void Dock();
        public void Land();
        public void TakeOff();
        public bool EngageEngine();
        public bool CanTravelTo(Enums.StellarBodies destination);
        public void DisengageEngine();
        public int TravelTimeRemain();
        public Objects.ACC ACC { get; set; }
    }
}
