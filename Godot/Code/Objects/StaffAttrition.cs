using System;
using System.Collections.Generic;
using Deuteros.Code.Objects.GameData;

namespace Deuteros.Code.Objects
{
    public static class StaffAttrition
    {
        public static void DayTick(uint previousDay, uint currentDay) =>
            Advance(GameCore.SingletonInstance.GameData.ActiveSaveFile, previousDay, currentDay, Random.Shared.Next);

        internal static void Advance(SaveFile save, uint previousDay, uint currentDay, Func<int> random)
        {
            if (currentDay <= previousDay) return;
            // Original gate: 10,000 raw units, with 100 units per displayed day.
            var gates = currentDay / 100 - previousDay / 100;
            if (gates == 0) return;

            var active = new HashSet<Staff>();
            var frozen = new HashSet<Staff>();
            void Add(Staff team) { if (team != null) active.Add(team); }
            foreach (var planet in save.BaseGameData.Planets.Values)
            {
                foreach (var team in planet.PlanetResources.Staff) Add(team);
                foreach (var team in planet.Station.Resources.Staff) Add(team);
                Add(planet.Station.Factory.Builder);
                if (planet is Earth earth)
                {
                    Add(earth.ResearchStaff);
                    Add(earth.Factory.Builder);
                }
            }
            foreach (var ship in save.Ships)
            {
                // These synthetic crews are not entries in the original human roster.
                if (ship is EnemyFleet) continue;
                Add(ship.Pilot);
                foreach (var module in ship.Modules)
                    if (module.ModuleType == Enums.Module_Types.Cryo && module.StaffStored != null)
                        frozen.Add(module.StaffStored);
            }
            active.ExceptWith(frozen);

            for (uint gate = 0; gate < gates; gate++)
                foreach (var team in active)
                {
                    if (team.AttritionCountdown > 0)
                    {
                        team.AttritionCountdown--;
                        continue;
                    }
                    var loss = random() & 1;
                    if (team.Count > 0) team.Count -= loss;
                    // Both draws occur even when an eligible team already has zero members.
                    team.AttritionCountdown = random() & 15;
                }
        }
    }
}
