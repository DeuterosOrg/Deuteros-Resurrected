using System;
using System.Linq;
using static Deuteros.Code.Enums;
using Deuteros.Code.Objects.GameData;

namespace Deuteros.Code.Objects
{
    public sealed class InterstellarFlight
    {
        public enum FlightLeg { Accelerating, Hyperlight, Local, Stranded }
        internal enum Outcome { Travelling, Arrived, Lost }
        public FlightLeg Leg { get; set; }
        public int Remaining { get; set; }
        public int Phase { get; set; }
        public int Fraction { get; set; }
        public long ClockOffset { get; set; }

        // Original $34FE8 star matrix, $34FB8 speed words and $36838 phase fuel costs.
        private static readonly int[,] Distances = {
            {0,4300,4400,6000,8200,8700,11000,11300,11800},
            {4300,0,200,7000,11000,16300,15000,12700,13200},
            {4400,200,0,8000,12000,15300,16000,12800,13300},
            {6000,7000,8000,0,2500,2720,4550,23000,19400},
            {8200,11000,12000,2500,0,1900,5500,7200,16100},
            {8700,16300,15300,2720,1900,0,3800,14200,7600},
            {11000,15000,16000,4550,5500,3800,0,450,6100},
            {11300,12700,12800,23000,7200,14200,450,0,650},
            {11800,13200,13300,19400,16100,7600,6100,650,0}
        };
        private static readonly int[] Speeds = {256,257,268,295,320,358,426,587,821,1816,5689,10000};
        private static readonly int[] FuelCosts = {1,1,2,3,4,6,8,12,15,18,20,20};
        private static readonly int[] StarOffsets = {0,430000,440000,600000,820000,870000,1100000,1130000,1180000};
        private static int StarIndex(StellarBodies star) => (int)star / 100000 - 1;
        internal static int Distance(StellarBodies origin, StellarBodies destination) => Distances[StarIndex(origin), StarIndex(destination)];
        internal static int StarOffset(StellarBodies star) => StarOffsets[StarIndex(star)];
        internal long AbsoluteClock(GameClock clock) => 310000000L + (long)clock.DateCentidays + ClockOffset;

        internal static InterstellarFlight Start(SCG ship)
        {
            if (ship.StarLocation == ship.DestinationStarLocation) return null;
            return new InterstellarFlight {
                Leg = FlightLeg.Accelerating, Phase = 1,
                Remaining = Distance(ship.StarLocation, ship.DestinationStarLocation) * (ship.EngineDamaged ? 2 : 1),
                ClockOffset = StarOffset(ship.StarLocation)
            };
        }

        internal static int LocalDistance(StellarBodies body)
        {
            var planets = CoreData.StaticGameData.Planets;
            var target = planets[body];
            var parent = target.MoonParentPlanetId == StellarBodies.none ? target : planets[target.MoonParentPlanetId];
            // Original Solar groups begin Earth, Mercury, Venus; other groups follow their current order.
            var group = parent.PlanetId switch {
                StellarBodies.earth => 0, StellarBodies.mercury => 1, StellarBodies.venus => 2, _ => parent.Order
            };
            return 4 + (group == 0 ? (target == parent ? 0 : target.Order + 1) : group * 4);
        }

        internal Outcome Advance(SCG ship, SaveFile save)
        {
            var fuel = ship.Fuel;
            var outcome = Step(ref fuel, save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research.Researched,
                310000000L + (long)save.Clock.DateCentidays, StarOffset(ship.DestinationStarLocation),
                LocalDistance(ship.DestinationPlanetLocation) * (ship.EngineDamaged ? 2 : 1));
            if (ship.Fuel > 0 && fuel == 0) ship.EngineEngaged = false;
            ship.Fuel = fuel;
            return outcome;
        }

        private Outcome Step(ref int fuel, bool hyperlight, long solClock, int destinationOffset, int localDistance)
        {
            if (Leg == FlightLeg.Stranded) return --Remaining == 0 ? Outcome.Lost : Outcome.Travelling;
            if (Leg != FlightLeg.Accelerating)
            {
                // $239C0: exhausting ordinary fuel doubles the remaining countdown once, then coasts.
                if (fuel > 0 && --fuel == 0) Remaining *= 2;
                if (--Remaining > 0) return Outcome.Travelling;
                if (Leg == FlightLeg.Local) return Outcome.Arrived;
                ClockOffset = destinationOffset;
            }
            else
            {
                var speed = Speeds[Phase - 1];
                Fraction += speed & 255;
                if (Fraction > 255) speed += 256;
                Fraction &= 255;
                var distance = Math.Min(Remaining, (speed >> 8) + 1);
                Remaining -= distance;
                ClockOffset += distance * 100L;
                var nextPhase = Phase;
                if (Remaining >= 47)
                {
                    if (Phase < 11) nextPhase++;
                    else if (hyperlight)
                    {
                        Leg = FlightLeg.Hyperlight;
                        Phase = 0;
                        Remaining = 1;
                        ClockOffset = -solClock;
                        return Outcome.Travelling;
                    }
                }
                else if (Phase >= 2) nextPhase--;
                if (nextPhase != Phase)
                {
                    Phase = nextPhase;
                    if (fuel < FuelCosts[Phase]) { fuel = 0; return Outcome.Lost; }
                    fuel -= FuelCosts[Phase];
                }
                if (Remaining > 0) return Outcome.Travelling;
                if (ClockOffset != destinationOffset) return Outcome.Lost;
            }
            // The remake selects a body directly. Complete the star leg before its local approach.
            Leg = fuel == 0 ? FlightLeg.Stranded : FlightLeg.Local;
            Phase = Fraction = 0;
            Remaining = fuel == 0 ? 6 : localDistance;
            return Outcome.Travelling;
        }

        internal int ProjectedUpdates(SCG ship, SaveFile save)
        {
            var projected = (InterstellarFlight)MemberwiseClone();
            var fuel = ship.Fuel;
            var hyperlight = save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research.Researched;
            for (var updates = 1; updates <= 50000; updates++)
                if (projected.Step(ref fuel, hyperlight, 310000000L + (long)save.Clock.DateCentidays,
                    StarOffset(ship.DestinationStarLocation), LocalDistance(ship.DestinationPlanetLocation) * (ship.EngineDamaged ? 2 : 1)) != Outcome.Travelling)
                    return updates;
            throw new InvalidOperationException("Interstellar flight did not terminate.");
        }

        internal bool IsValid(SCG ship, SaveFile save)
        {
            var planets = save.BaseGameData.Planets;
            if (ship.ShipState != Ship_States.InTransit || !planets.TryGetValue(ship.DestinationPlanetLocation, out var destination)
                || ship.StarLocation != planets[ship.PlanetLocation].ParentStar || ship.DestinationStarLocation != destination.ParentStar
                || ship.StarLocation == ship.DestinationStarLocation || ship.Fuel < 0 || ship.Fuel > 250
                || Fraction < 0 || Fraction > 255 || Remaining <= 0) return false;
            var absolute = AbsoluteClock(save.Clock);
            if (absolute < 0 || absolute > 310000000L + (long)uint.MaxValue * 100 + 6000000) return false;
            if (Leg == FlightLeg.Accelerating)
            {
                var total = Distance(ship.StarLocation, ship.DestinationStarLocation) * (ship.EngineDamaged ? 2 : 1);
                return Phase >= 1 && Phase <= 11 && Remaining <= total
                    && ClockOffset == StarOffset(ship.StarLocation) + (total - Remaining) * 100L;
            }
            if (Leg == FlightLeg.Hyperlight) return Phase == 0 && Remaining <= 2 && absolute <= 200;
            if (Leg == FlightLeg.Stranded) return Phase == 0 && Fraction == 0 && ship.Fuel == 0
                && Remaining <= 6 && ClockOffset == StarOffset(ship.DestinationStarLocation);
            return Leg == FlightLeg.Local && Phase == 0 && Fraction == 0
                && ClockOffset == StarOffset(ship.DestinationStarLocation)
                && Remaining <= LocalDistance(ship.DestinationPlanetLocation) * (ship.EngineDamaged ? 4 : 2);
        }
    }
}
