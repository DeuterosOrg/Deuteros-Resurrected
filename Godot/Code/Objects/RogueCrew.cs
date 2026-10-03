using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Objects.Interfaces;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
    public sealed class RogueCrew
    {
        public bool Occurred { get; set; }
        public Staff Crew { get; set; }
        public int Stage { get; set; }
        public bool MutinyPending { get; set; }
        public int PrisonCountdown { get; set; }
        public int PrisonDivider { get; set; }
        public int? OriginalCrewCount { get; set; }

        public bool Controls(IShip ship) => Crew != null && ship is SCG && ReferenceEquals(ship.Pilot, Crew);

        public bool Contained(SaveFile save) => Crew != null && save.Ships.OfType<SCG>().Any(s =>
            s.Modules.Any(m => m.ModuleType == Module_Types.Tool && m.ItemStored == ItemTypes.prison_pod
                && m.ItemCount == 1 && ReferenceEquals(m.StaffStored, Crew)));

        public void CrewLost(Staff crew)
        {
            if (Crew == null || !ReferenceEquals(crew, Crew)) return;
            Crew = null;
            Stage = 0;
            OriginalCrewCount = null;
        }

        public void TryStart(SaveFile save)
        {
            var planets = save.BaseGameData.Planets.Values;
            var research = save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research;
            if (Occurred || research.Locked || research.ResearchPercentageComplete == 0
                || planets.Where(p => p.ActiveMethanoid).Select(p => p.ParentStar).Distinct().Count() >= 5) return;
            var star = save.BaseGameData.Stars.Keys.OrderBy(s => s).FirstOrDefault(s =>
                planets.Any(p => p.ParentStar == s && p.ActiveMethanoid)
                && planets.Any(p => p.ParentStar == s && !p.ActiveMethanoid && p.Station.Built));
            if (star == StellarBodies.none) return;
            InterStellarShip.EnsureAutomationSlots(save);
            var ship = save.Ships.OfType<SCG>().OrderBy(s => s.AutomationSlot).FirstOrDefault(s =>
                s.ShipState == Ship_States.UnDocked && s.Fuel == 250
                && planets.First(p => p.PlanetId == s.PlanetLocation).ParentStar == star
                && s.Pilot?.GetLevel() == (int)StaffLevel_Marines.Warlord
                && !s.Modules.Any(m => (m.ModuleType == Module_Types.Cryo && m.StaffStored != null)
                    || (m.ModuleType == Module_Types.Tool && m.ItemStored == ItemTypes.pulse_blaster_laser && m.ItemCount > 0)));
            if (ship == null) return;
            Occurred = MutinyPending = true;
            Crew = ship.Pilot;
            Crew.Pirate = true;
            ship.Name = "BOUNTY";
            Stage = 11;
            if (ship.ACC != null) ship.ACC.Active = ship.ACC.CycleMode = ship.ACC.Refuelling = false;
        }
        public void Validate(SaveFile save)
        {
            void Require(bool condition, string field)
            {
                if (!condition) throw new InvalidDataException("Invalid save: rogue " + field + ".");
            }
            Require(Stage >= 0 && Stage <= 20 && PrisonCountdown >= 0 && PrisonCountdown <= 252
                && PrisonDivider >= 0 && PrisonDivider < 4, "stage or discovery clock");
            Require(Occurred || (Crew == null && Stage == 0 && !MutinyPending && PrisonCountdown == 0
                && PrisonDivider == 0 && OriginalCrewCount == null), "inactive state");
            Require(Crew != null ? Occurred && Stage > 0 && Crew.Pirate && Crew.Warlord && Crew.CanBeWarlord
                : Stage == 0 && OriginalCrewCount == null, "selected rank");
            Require(OriginalCrewCount == null ? Stage is not (18 or 19)
                : OriginalCrewCount >= 0 && Crew != null && Stage is 18 or 19, "temporary crew count");
            var positions = new List<(Staff Team, bool Allowed)>();
            foreach (var planet in save.BaseGameData.Planets.Values)
            {
                positions.AddRange(planet.PlanetResources.Staff.Select(t => (t, false)));
                positions.AddRange(planet.Station.Resources.Staff.Select(t => (t, true)));
                positions.Add((planet.Station.Factory.Builder, false));
                if (planet is Earth earth)
                {
                    positions.Add((earth.Factory.Builder, false));
                    positions.Add((earth.ResearchStaff, false));
                }
            }
            foreach (var ship in save.Ships.Where(s => s is not EnemyFleet))
            {
                positions.Add((ship.Pilot, ship is SCG));
                positions.AddRange(ship.Modules.Select(m => (m.StaffStored, ship is SCG
                    && m.ModuleType == Module_Types.Tool && m.ItemStored == ItemTypes.prison_pod && m.ItemCount == 1)));
            }
            Require(positions.All(p => p.Team == null || !p.Team.Pirate || ReferenceEquals(p.Team, Crew)), "unselected pirate");
            if (Crew == null) return;
            var found = positions.Where(p => ReferenceEquals(p.Team, Crew)).ToList();
            Require(found.Count == 1 && found[0].Allowed, "crew identity or location");
        }
    }
}
