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

        public override int TravelTimeRemain()
        {
            var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            if (ShipState == Enums.Ship_States.InTransit)
                return Flight == null ? base.TravelTimeRemain() : Flight.ProjectedUpdates(this, save);
            var planets = save.BaseGameData.Planets;
            if (!planets.TryGetValue(DestinationPlanetLocation, out var destination)
                || planets[PlanetLocation].ParentStar == destination.ParentStar) return base.TravelTimeRemain();
            // Preview uses canonical endpoints without changing this ship or its saved flight.
            var preview = (SCG)MemberwiseClone();
            preview.StarLocation = planets[PlanetLocation].ParentStar;
            preview.DestinationStarLocation = destination.ParentStar;
            return InterstellarFlight.Start(preview).ProjectedUpdates(preview, save);
        }

        public SCG()
        {
            ShipID = Guid.NewGuid();
        }
    }
}