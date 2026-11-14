using Deuteros.Code.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Objects
{
    public class SCG : InterStellarShip, IShip
    {
        public InterstellarFlight Flight { get; set; }

        internal Enums.StellarBodies ClockStar => Flight?.Leg is InterstellarFlight.FlightLeg.Local or InterstellarFlight.FlightLeg.Stranded
            ? DestinationStarLocation : GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[PlanetLocation].ParentStar;
        internal long AbsoluteClock => Flight?.AbsoluteClock(GameCore.SingletonInstance.GameData.ActiveSaveFile.Clock)
            ?? 310000000L + (long)GameCore.SingletonInstance.GameData.ActiveSaveFile.Clock.DateCentidays + InterstellarFlight.StarOffset(ClockStar);

        public override int TravelTimeRemain() => ProjectedArrival().Updates;

        internal (int Updates, long? ArrivalOffset) ProjectedArrival()
        {
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            if (ShipState == Enums.Ship_States.InTransit && Flight != null) return Flight.Project(this, save);
            var planets = save.BaseGameData.Planets;
            var destination = planets[DestinationPlanetLocation];
            if (ShipState == Enums.Ship_States.InTransit || planets[PlanetLocation].ParentStar == destination.ParentStar)
                return (base.TravelTimeRemain(), InterstellarFlight.StarOffset(destination.ParentStar));
            // Preview uses canonical endpoints without changing this ship or its saved flight.
            var preview = (SCG)MemberwiseClone();
            preview.StarLocation = planets[PlanetLocation].ParentStar;
            preview.DestinationStarLocation = destination.ParentStar;
            return InterstellarFlight.Start(preview).Project(preview, save);
        }

        public SCG()
        {
            ShipID = Guid.NewGuid();
        }
    }
}