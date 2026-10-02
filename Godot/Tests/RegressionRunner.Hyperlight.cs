using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void PrepareHyperlightDiscovery()
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
        }

        private void HyperlightDiscoveryProgression()
        {
            PrepareHyperlightDiscovery();
            var hyperlight = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.hyperlight);
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

        private void HyperlightDiscoverySavedDelay()
        {
            PrepareHyperlightDiscovery();
            var core = GameCore.SingletonInstance;
            for (var tick = 0; tick < 4; tick++) AdvanceTickDay();
            core.GameData.ActiveSaveFile = Deuteros.Code.Utility.SaveStorage.Deserialize(Deuteros.Code.Utility.SaveStorage.Serialize(Save));
            for (var tick = 0; tick < 5; tick++)
            {
                AdvanceTickDay();
                Equal(true, core.GameData.GetItem(ItemTypes.hyperlight).Research.Locked, "load preserves remaining delay without early discovery");
            }
            AdvanceTickDay();
            Equal(false, core.GameData.GetItem(ItemTypes.hyperlight).Research.Locked, "load does not restart discovery delay");
            Equal(BulletinTypes.Hyperlight_Speed, Save.News.LastBulletin, "discovery reaches the existing Hyperlight bulletin");
        }

        private void HyperlightDiscoverySampledCount()
        {
            PrepareHyperlightDiscovery();
            var core = GameCore.SingletonInstance;
            var recaptured = Save.BaseGameData.Planets.Values.First(p => p.ParentStar == StellarBodies.proxima);
            recaptured.ActiveMethanoid = true;
            AdvanceTickDay(); // Sample eight hostile systems at the enemy scheduling boundary.
            recaptured.ActiveMethanoid = false;
            Save.EnemyBuildDay = Save.CurrentDay + 12;
            for (var tick = 0; tick < 11; tick++) AdvanceTickDay();
            Equal(true, core.GameData.GetItem(ItemTypes.hyperlight).Research.Locked, "capture cannot bypass enemy count sampling");
            AdvanceTickDay(); // Sample seven and start its eight-pass delay.
            for (var tick = 0; tick < 8; tick++)
            {
                AdvanceTickDay();
                Equal(true, core.GameData.GetItem(ItemTypes.hyperlight).Research.Locked, "newly sampled count waits all eight passes");
            }
            AdvanceTickDay();
            Equal(false, core.GameData.GetItem(ItemTypes.hyperlight).Research.Locked, "discovery follows sampled count rather than immediate world count");
        }
    }
}
