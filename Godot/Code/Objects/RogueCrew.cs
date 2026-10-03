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
        private static readonly ItemTypes[] RaidMaterials = { ItemTypes.titanium, ItemTypes.aluminium,
            ItemTypes.paladium, ItemTypes.platinum, ItemTypes.meh_fuel, ItemTypes.hed_fuel };

        public void Advance(SaveFile save, Func<int> random)
        {
            if (!ReferenceEquals(save, GameCore.SingletonInstance.GameData.ActiveSaveFile)) return;
            var ship = save.Ships.OfType<SCG>().FirstOrDefault(s => Controls(s));
            if (ship == null)
            {
                if (OriginalCrewCount.HasValue) { RestoreCrewCount(); Stage = 1; }
                return;
            }
            if (ship.ACC != null) ship.ACC.Active = ship.ACC.CycleMode = ship.ACC.Refuelling = false;
            var planet = save.BaseGameData.Planets[ship.PlanetLocation];
            switch (Stage)
            {
                case 1:
                case 5:
                    ChooseRoute(save, ship);
                    break;
                case 2:
                    if (ship.ShipState == Ship_States.UnDocked
                        && (ship.DestinationPlanetLocation == ship.PlanetLocation || ship.EngageEngine())) Stage++;
                    break;
                case 3:
                    if (ship.ShipState is not (Ship_States.UnDocked or Ship_States.Docked)) break;
                    if (!planet.Station.Built || !planet.ActiveMethanoid) { Stage = 1; break; }
                    if (ship.ShipState == Ship_States.UnDocked) ship.Dock();
                    else if (ship.ShipState == Ship_States.Docked && ship.Modules.All(m => m.StaffStored == null))
                    {
                        ship.Fuel = 250;
                        ship.DFCC = ship.PTL = false;
                        ship.DroneCount = 0;
                        ship.Modules = Enumerable.Range(0, 6).Select(_ => new ShipModule { ModuleType = Module_Types.Supply }).ToList();
                        ship.TakeOff(requireFuel: false);
                        Stage++;
                    }
                    break;
                case 4:
                case 7:
                case 11:
                case 14:
                case 16:
                    if (ship.ShipState == Ship_States.UnDocked) Stage++;
                    break;
                case 6:
                    if (ship.ShipState != Ship_States.UnDocked) break;
                    if (ship.DestinationPlanetLocation == ship.PlanetLocation) Stage = 5;
                    else if (ship.EngageEngine()) Stage++;
                    break;
                case 8:
                    if (ship.ShipState != Ship_States.UnDocked) break;
                    if (!planet.Station.Built) { Stage = 1; break; }
                    ship.Dock();
                    if (ship.ShipState == Ship_States.Docking) Stage++;
                    break;
                case 9:
                    if (ship.ShipState == Ship_States.UnDocked) Stage = 1;
                    else if (ship.ShipState == Ship_States.Docked && planet.Station.Built)
                    {
                        Stage++;
                        RedirectMtx(save, planet);
                    }
                    break;
                case 10:
                    if (ship.ShipState != Ship_States.Docked || !planet.Station.Built) break;
                    Raid(ship, planet.Station.Resources.Stores, 6);
                    var refuel = Math.Min(Math.Max(0, 250 - ship.Fuel), planet.Station.Resources.Stores[ItemTypes.hed_fuel]);
                    planet.Station.Resources.Stores[ItemTypes.hed_fuel] -= refuel;
                    ship.Fuel += refuel;
                    ship.TakeOff(requireFuel: false);
                    Stage++;
                    break;
                case 12:
                case 15:
                case 20:
                    Stage = 1;
                    break;
                case 13:
                    if (ship.ShipState == Ship_States.UnDocked && ship.EngageEngine()) Stage++;
                    break;
                case 17:
                    if (ship.ShipState != Ship_States.UnDocked || !planet.Station.Built) break;
                    OriginalCrewCount = Crew.Count;
                    Crew.Count >>= 3;
                    Stage++;
                    break;
                case 18:
                    if (ship.ShipState != Ship_States.UnDocked || !planet.Station.Built) break;
                    Stage++;
                    if ((random() & 7) != 4) break;
                    planet.Station.SdmCountdown = 150;
                    save.News.AddNews(planet.PlanetId.ToScreenString(" ") + " Self-Destructing.");
                    if (save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.prison_pod).Research.Locked)
                        PrisonCountdown = 5;
                    break;
                case 19:
                    RestoreCrewCount();
                    if (ship.ShipState == Ship_States.UnDocked && planet.Station.Built) Raid(ship, planet.Station.Resources.Stores, 1);
                    Stage = 1;
                    break;
            }
        }

        private void RestoreCrewCount()
        {
            if (Crew != null && OriginalCrewCount.HasValue) Crew.Count = OriginalCrewCount.Value;
            OriginalCrewCount = null;
        }

        public bool RejectCommand(SaveFile save, IShip ship)
        {
            if (!ReferenceEquals(save, GameCore.SingletonInstance.GameData.ActiveSaveFile) || !save.Ships.Contains(ship)) return true;
            if (!Controls(ship)) return false;
            if (ship.ShipState == Ship_States.Docked)
            {
                RestoreCrewCount();
                Stage = 10;
                Advance(save, Random.Shared.Next);
            }
            return true;
        }

        public bool DockingBlocked(SaveFile save, IShip ship)
        {
            if (!ReferenceEquals(save, GameCore.SingletonInstance.GameData.ActiveSaveFile)
                || !save.Ships.Contains(ship) || !Controls(ship) || ship.ShipState != Ship_States.Docking) return false;
            RestoreCrewCount();
            ship.ShipState = Ship_States.UnDocked;
            Stage = 16;
            return true;
        }

        public void EscapeStation(SaveFile save, IPlanet planet)
        {
            var ship = save.Ships.OfType<SCG>().FirstOrDefault(s => Controls(s) && s.PlanetLocation == planet.PlanetId);
            if (ship == null || ship.ShipState is not (Ship_States.Docked or Ship_States.Docking)) return;
            RestoreCrewCount();
            if (ship.ShipState == Ship_States.Docked) ship.TakeOff(requireFuel: false);
            else ship.ShipState = Ship_States.UnDocked;
            Stage = 11;
        }

        private void ChooseRoute(SaveFile save, SCG ship)
        {
            if (ship.ShipState != Ship_States.UnDocked) return;
            FuelRefining.EnsureSlots(save);
            var stations = save.BaseGameData.Planets.Values.Where(p => p.Station.Built).ToList();
            var star = save.BaseGameData.Planets[ship.PlanetLocation].ParentStar;
            if (!stations.Any(p => p.ParentStar == star && p.ActiveMethanoid))
            {
                var stars = save.BaseGameData.Stars.Keys.OrderBy(s => s);
                star = stars.FirstOrDefault(s => stations.Count(p => p.ParentStar == s && p.ActiveMethanoid) >= 2
                    && stations.Any(p => p.ParentStar == s && !p.ActiveMethanoid));
                if (star == StellarBodies.none) star = stars.FirstOrDefault(s => stations.Any(p => p.ParentStar == s && p.ActiveMethanoid));
            }
            var local = stations.Where(p => p.ParentStar == star).OrderBy(p => p.Station.RefiningSlot).ToList();
            var enemy = local.FirstOrDefault(p => p.ActiveMethanoid);
            if (enemy == null) return;
            var target = Stage == 5 ? local.FirstOrDefault(p => !p.ActiveMethanoid
                && p.Station.Resources.Stores[ItemTypes.ios_drone] >= 5) ?? enemy : enemy;
            var crossStar = save.BaseGameData.Planets[ship.PlanetLocation].ParentStar != star;
            // The remake has body destinations: compose the star journey and its local approach.
            ship.DestinationPlanetLocation = crossStar ? enemy.PlanetId : target.PlanetId;
            ship.DestinationStarLocation = star;
            Stage = crossStar ? 13 : Stage + 1;
        }

        private static void Raid(SCG ship, Store stores, int maximumPods)
        {
            var material = 0;
            foreach (var module in ship.Modules.Where(m => m.ModuleType == Module_Types.Supply).Take(maximumPods))
            {
                while (material < RaidMaterials.Length && stores[RaidMaterials[material]] == 0) material++;
                if (material == RaidMaterials.Length) break;
                module.ItemStored = RaidMaterials[material];
                module.ItemCount = Math.Min(250, stores[module.ItemStored]);
                stores[module.ItemStored] -= module.ItemCount;
            }
        }

        private static void RedirectMtx(SaveFile save, IPlanet source)
        {
            if (!source.Station.MtxInstalled) return;
            FuelRefining.EnsureSlots(save);
            var target = save.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.the_sun
                && p.Station.Built && !p.ActiveMethanoid && p.Station.RefiningSlot is >= 0 and < 8
                && p.Station.Resources.Stores[ItemTypes.aluminium] >= 200).OrderBy(p => p.Station.RefiningSlot).FirstOrDefault();
            if (target == null) return;
            var mtx = source.Station.Resources.Stores.MTX;
            mtx.Target = target.PlanetId;
            mtx.SendItems = RaidMaterials.ToList();
            mtx.BalanceItems = RaidMaterials.ToList();
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
