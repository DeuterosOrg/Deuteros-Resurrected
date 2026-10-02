using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void HyperlightDiscoveryProgression()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.AtWar = true;
            Save.CurrentDay = 100;
            Save.WarDeclaredDay = 0;
            Save.EnemyBuildDay = 0;
            Save.Ships.Clear();
            Save.AlienTransmissions = new AlienTransmissions();
            foreach (var planet in Save.BaseGameData.Planets.Values)
                if (planet.ParentStar == StellarBodies.the_sun || planet.ParentStar == StellarBodies.proxima)
                    planet.ActiveMethanoid = false;
            Equal(7, Save.BaseGameData.Planets.Values.Where(p => p.ActiveMethanoid).Select(p => p.ParentStar).Distinct().Count(),
                "seven systems retain hostile stations");
            var hyperlight = core.GameData.GetItem(ItemTypes.hyperlight);
            Equal(true, hyperlight.Research.Locked, "Hyperlight initially unknown");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            for (var tick = 0; tick < 9; tick++)
            {
                AdvanceTickDay();
                Equal(true, hyperlight.Research.Locked, "count-change and eight delay passes do not discover early");
            }
            AdvanceTickDay();
            Equal(false, hyperlight.Research.Locked, "seven surviving hostile systems discover Hyperlight after progression delay");
            Equal(false, hyperlight.Research.Researched, "discovery does not complete research");
            Equal(true, hyperlight.Research.ResearchPercentageComplete < 100, "player must still perform research");
        }
    }
}
