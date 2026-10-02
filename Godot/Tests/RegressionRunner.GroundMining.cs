using Deuteros.Code;
using Deuteros.Code.Objects;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void GroundSurveyWithoutDerricks()
        {
            foreach (var body in new[] { StellarBodies.earth, StellarBodies.the_moon })
            {
                var planet = (Planet)Save.BaseGameData.Planets[body];
                PrepareTickMining(planet);
                planet.ActiveMethanoid = false;
                planet.PlanetResources.Derricks = 0;
                var material = planet.PlanetResources.Materials[0];
                material.GroundAmount = 0;
                material.SurveyTicks = 2;
                planet.DayTick(1, 2);
                Equal(1, material.SurveyTicks, "surveys advance without installed derricks " + body);
                foreach (var countdown in new[] { 0, 1 })
                {
                    material.GroundAmount = 0; material.SurveyTicks = countdown;
                    planet.DayTick(3, 4);
                    Equal(true, material.GroundAmount >= 50 && material.GroundAmount <= 32767,
                        "zero and one survey counters both discover a nonempty deposit");
                    Equal(0x32, material.GroundAmount & 0x32, "discovery preserves original forced low bits");
                    Equal(0, planet.PlanetResources.Stores[ItemTypes.iron], "survey completion does not also extract");
                }
            }
        }

        private void GroundMiningEligibility()
        {
            var planet = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            PrepareTickMining(planet);
            var material = planet.PlanetResources.Materials[0];
            foreach (var gate in new[] { "hostile", "damaged", "unfinished", "same update", "backwards update" })
            {
                planet.ActiveMethanoid = gate == "hostile";
                planet.BaseDamaged = gate == "damaged";
                planet.BaseBuildParts = gate == "unfinished" ? 1 : 2;
                material.GroundAmount = 100; material.SurveyTicks = 0;
                planet.PlanetResources.Stores[ItemTypes.iron] = 0;
                var current = gate == "same update" ? 2u : gate == "backwards update" ? 1u : 3u;
                planet.DayTick(2, current);
                Equal(100, material.GroundAmount, gate + " cannot extract");
                Equal(0, planet.PlanetResources.Stores[ItemTypes.iron], gate + " cannot create stock");
            }
        }

        private void GroundMiningBoundaries()
        {
            foreach (var body in new[] { StellarBodies.earth, StellarBodies.the_moon })
            foreach (var mtx in new[] { false, true })
            {
                var planet = (Planet)Save.BaseGameData.Planets[body];
                PrepareTickMining(planet);
                planet.ActiveMethanoid = false;
                planet.Station.Built = true;
                planet.Station.BuildParts = 8;
                planet.Station.Type = 8;
                planet.Station.MtxInstalled = mtx;
                var material = planet.PlanetResources.Materials[0];
                var stores = mtx ? planet.Station.Resources.Stores : planet.PlanetResources.Stores;
                material.GroundAmount = 1;
                stores[ItemTypes.iron] = 100;
                planet.DayTick(1, 2);
                Equal(100, stores[ItemTypes.iron], "insufficient whole batch produces no ore " + body + " MTX=" + mtx);
                Equal(0, material.GroundAmount, "depleted deposit cannot become negative");
                material.GroundAmount = 10; material.SurveyTicks = 0;
                stores[ItemTypes.iron] = 49999;
                planet.DayTick(3, 4);
                Equal(50000, stores[ItemTypes.iron], "full batch clamps at storage limit");
                Equal(8, material.GroundAmount, "original extraction consumes whole batch even when output is capped");
                planet.DayTick(5, 6);
                Equal(50000, stores[ItemTypes.iron], "full stores remain capped");
                Equal(6, material.GroundAmount, "full stores do not stop original extraction");
            }
        }
    }
}
