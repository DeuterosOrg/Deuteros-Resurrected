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

        private async System.Threading.Tasks.Task HyperlightPendingResearchFlow()
        {
            InitializeUi();
            PrepareHyperlightDiscovery();
            var core = GameCore.SingletonInstance;
            for (var tick = 0; tick < 9; tick++) AdvanceTickDay();
            Save.AlienTransmissions.Stage = 0;
            Save.AlienTransmissions.Ready = true;
            AdvanceTickDay();
            Equal(true, Save.AlienTransmissions.HyperlightPending, "competing transmission retains pending research discovery");
            Equal(true, core.GameData.GetItem(ItemTypes.hyperlight).Research.Locked, "competing message does not silently unlock research");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            core.GameData.ActiveSaveFile = Deuteros.Code.Utility.SaveStorage.Deserialize(Deuteros.Code.Utility.SaveStorage.Serialize(Save));
            AdvanceTickDay();
            Equal(true, Save.AlienTransmissions.Ready, "unacknowledged transmission keeps priority after reload");
            Equal(true, Save.AlienTransmissions.HyperlightPending, "retry retains Hyperlight pending state");
            var notice = ActiveScreen<Bulletins>();
            await FinishBulletin(notice);
            Press(notice, "ViewTransmission");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            AdvanceTickDay();
            Equal(BulletinTypes.Hyperlight_Speed, Save.News.LastBulletin, "pending discovery survives interruption and reload");
            Equal(false, Save.AlienTransmissions.HyperlightPending, "notice consumed once");
            core.ChangeScene(Scenes.Earth_Research, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            var screen = ActiveScreen<Deuteros.Code.Platform.Screens.Research>();
            screen.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.hyperlight).EmitSignal(Godot.BaseButton.SignalName.Pressed);
            Equal(ItemTypes.hyperlight, GameCore.Earth.CurrentResearchItem.ItemType, "discovered research is selectable through controls");
            await CaptureDisplayEvidence("hyperlight-discovered-research");
            var team = new Staff { Type = StaffType.Research, Count = 250, Leader = "Research test" };
            team.AddAction(9);
            GameCore.Earth.ResearchStaff = team;
            var research = core.GameData.GetItem(ItemTypes.hyperlight).Research;
            for (uint tick = 0; tick < 100 && !research.Researched; tick++)
                Deuteros.Code.Platform.Screens.Research.UpdateResearch(tick, tick + 1);
            Equal(true, research.Researched, "normal research completes Hyperlight");
            Equal(100, research.ResearchPercentageComplete, "research reaches full completion");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            core.GameData.ActiveSaveFile = Deuteros.Code.Utility.SaveStorage.Deserialize(Deuteros.Code.Utility.SaveStorage.Serialize(Save));
            for (var tick = 0; tick < 12; tick++) AdvanceTickDay();
            Equal(Scenes.SaveScreen, core.currentScene, "completed discovery does not replay");
            Equal(100, core.GameData.GetItem(ItemTypes.hyperlight).Research.ResearchPercentageComplete, "completion remains saved");
        }

        private void HyperlightDiscoverySaveValidation()
        {
            PrepareHyperlightDiscovery();
            for (var tick = 0; tick < 4; tick++) AdvanceTickDay();
            var baseline = Deuteros.Code.Utility.SaveStorage.Serialize(Save);
            foreach (var (field, value) in new[] { ("EnemySystems",-1), ("EnemySystems",10), ("HyperlightSystems",10), ("HyperlightCountdown",-1), ("HyperlightCountdown",9) })
            {
                var document = Newtonsoft.Json.Linq.JObject.Parse(baseline);
                document["Game"]["AlienTransmissions"][field] = value;
                var rejected = false;
                try { Deuteros.Code.Utility.SaveStorage.Deserialize(document.ToString()); }
                catch (System.IO.InvalidDataException) { rejected = true; }
                Equal(true, rejected, "invalid discovery " + field + " rejected");
            }
            var legacy = Newtonsoft.Json.Linq.JObject.Parse(baseline);
            var state = (Newtonsoft.Json.Linq.JObject)legacy["Game"]["AlienTransmissions"];
            foreach (var field in new[] { "EnemySystems", "HyperlightSystems", "HyperlightCountdown", "HyperlightPending" })
            {
                var invalid = Newtonsoft.Json.Linq.JObject.Parse(baseline);
                invalid["Game"]["AlienTransmissions"][field] = Newtonsoft.Json.Linq.JValue.CreateNull();
                var rejected = false;
                try { Deuteros.Code.Utility.SaveStorage.Deserialize(invalid.ToString()); }
                catch (Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "null discovery " + field + " rejected");
                state.Remove(field);
            }
            var loaded = Deuteros.Code.Utility.SaveStorage.Deserialize(legacy.ToString());
            Equal(9, loaded.AlienTransmissions.EnemySystems, "legacy save waits for fresh enemy sample");
            Equal(false, loaded.AlienTransmissions.HyperlightPending, "legacy save invents no pending bulletin");
        }

        private void HyperlightDiscoveryCountBoundaries()
        {
            PrepareHyperlightDiscovery();
            var state = Save.AlienTransmissions;
            for (var systems = 0; systems <= 9; systems++)
            {
                state.EnemySystems = systems;
                state.HyperlightSystems = 9;
                state.HyperlightCountdown = 0;
                state.HyperlightPending = false;
                for (var tick = 0; tick < 10; tick++) state.AdvanceHyperlight(Save);
                Equal(systems == 7, state.HyperlightPending, "only original count seven schedules Hyperlight");
            }
            state.EnemySystems = 7; state.HyperlightSystems = 9; state.HyperlightPending = false;
            Save.AtWar = false;
            for (var tick = 0; tick < 12; tick++) state.AdvanceHyperlight(Save);
            Equal(false, state.HyperlightPending, "peace does not advance war progression");
            Save.AtWar = true;
            state.AdvanceHyperlight(Save);
            state.AdvanceHyperlight(Save);
            state.EnemySystems = 8;
            state.AdvanceHyperlight(Save);
            Equal(0, state.HyperlightCountdown, "recapture cancels ineligible countdown");
            Equal(false, state.HyperlightPending, "cancelled countdown does not produce discovery");
        }
    }
}
