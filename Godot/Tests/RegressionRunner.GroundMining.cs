using Deuteros.Code;
using Deuteros.Code.Objects;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
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
