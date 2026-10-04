using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void PtlDiscoveryProgression()
        {
            PrepareHyperlightDiscovery();
            var core = GameCore.SingletonInstance;
            foreach (var planet in Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.centauri))
                planet.ActiveMethanoid = false;
            Save.AlienTransmissions.SampleEnemySystems(Save);
            Equal(6, Save.AlienTransmissions.EnemySystems, "six hostile systems remain");
            Save.EnemyBuildDay = Save.Clock.DateCentidays + 100000;
            var hyperlight = core.GameData.GetItem(ItemTypes.hyperlight).Research;
            hyperlight.Locked = false;
            hyperlight.Researched = true;
            hyperlight.ResearchPercentageComplete = 100;
            for (var tick = 0; tick < 41; tick++)
            {
                AdvanceTickDay();
                Equal(true, core.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher).Research.Locked,
                    "count change and forty decrement-only visits do not discover early");
                if (tick == 17)
                    core.GameData.ActiveSaveFile = Deuteros.Code.Utility.SaveStorage.Deserialize(
                        Deuteros.Code.Utility.SaveStorage.Serialize(Save));
            }
            AdvanceTickDay();
            var ptl = core.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher).Research;
            Equal(false, ptl.Locked, "original event discovers PTL when no captive colony is selected");
            Equal(false, ptl.Researched, "discovery does not grant completed research");
            Equal(BulletinTypes.Eureka, Save.News.LastBulletin, "PTL discovery reaches its original bulletin");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            var state = Save.AlienTransmissions;
            state.HyperlightCountdown = 2;
            state.EnemySystems = 7;
            state.AdvanceResearch(Save);
            state.AdvanceResearch(Save);
            Equal(6, state.HyperlightSystems, "recapture cannot replace an active six-system delay");
            state.AdvanceResearch(Save);
            Equal(7, state.HyperlightSystems, "changed ownership is observed after the active delay");
            Equal(8, state.HyperlightCountdown, "recapture starts the original seven-system delay");
        }

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

        private async System.Threading.Tasks.Task PtlColonyStockEvent()
        {
            PrepareHyperlightDiscovery();
            var core = GameCore.SingletonInstance;
            var colony = Save.BaseGameData.Planets[StellarBodies.the_moon];
            colony.CaptiveBase = colony.Station.Built = true;
            colony.Station.Type = 8;
            colony.PlanetResources.Materials = new List<Material> { new Material(ItemTypes.iron, 0) { GroundAmount = 123 } };
            colony.PlanetResources.Stores[ItemTypes.iron] = 45000;
            colony.Station.Resources.Stores[ItemTypes.iron] = 7;
            var state = Save.AlienTransmissions;
            state.EnemySystems = state.HyperlightSystems = 6;
            state.AdvanceResearch(Save, () => 0);
            Equal(50000, colony.PlanetResources.Stores[ItemTypes.iron], "mining dump credits ground stock up to its cap");
            Equal(7, colony.Station.Resources.Stores[ItemTypes.iron], "orbital stock is not the destination");
            Equal(123, colony.PlanetResources.Materials[0].GroundAmount, "event does not change the ore vein");
            Equal(true, core.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher).Research.Locked, "successful dump postpones PTL");
            Equal(BulletinTypes.Mining_Dump, Save.News.PendingBulletins.Single(), "one saved notice owns the credited event");
            state.AdvanceResearch(Save, () => throw new System.InvalidOperationException("pending notice cannot reroll"));
            Equal(79, state.ColonyEventCountdown, "pending notice does not consume its cooldown");
            core.GameData.ActiveSaveFile = Deuteros.Code.Utility.SaveStorage.Deserialize(Deuteros.Code.Utility.SaveStorage.Serialize(Save));
            core.ShowBulletin(BulletinTypes.Mining_Dump);
            ActiveScreen<Bulletins>().LetterDelayMs = 0;
            var text = ActiveScreen<Bulletins>().GetNode<Godot.RichTextLabel>("Labels/BulletinLabel").GetParsedText();
            Equal(true, text.Contains("The Moon") && text.Contains("Iron") && !text.Contains("{0}"), "saved bulletin resolves its actual colony and mineral");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            core.ShowBulletin(BulletinTypes.Mining_Dump, true);
            ActiveScreen<Bulletins>().LetterDelayMs = 0;
            Equal(text, ActiveScreen<Bulletins>().GetNode<Godot.RichTextLabel>("Labels/BulletinLabel").GetParsedText(), "replay retains event parameters");
            Equal(50000, Save.BaseGameData.Planets[StellarBodies.the_moon].PlanetResources.Stores[ItemTypes.iron], "replay does not credit stock again");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            state = Save.AlienTransmissions;
            for (var visit = 0; visit < 79; visit++) state.AdvanceResearch(Save, () => throw new System.InvalidOperationException("cooldown cannot reroll"));
            Equal(0, state.ColonyEventCountdown, "all seventy-nine visits only decrement");
            Equal(true, core.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher).Research.Locked, "final decrement cannot discover PTL");
            // The selected colony has no carbon: that original mask miss reaches discovery.
            var rolls = new Queue<int>(new[] { 0, 3 });
            state.AdvanceResearch(Save, () => rolls.Dequeue());
            Equal(false, core.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher).Research.Locked, "unavailable mineral discovers PTL instead");
            Equal(BulletinTypes.Eureka, Save.News.PendingBulletins.Single(), "discovery is queued once");
            core.ShowBulletin(BulletinTypes.Eureka);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            var ptl = core.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher).Research;
            ptl.ResearchPercentageComplete = 57;
            state.ColonyEventCountdown = 0;
            state.AdvanceResearch(Save, () => 31);
            Equal(57, ptl.ResearchPercentageComplete, "ordinal miss preserves research already in progress");
            Equal(0, Save.News.PendingBulletins.Count, "existing discovery is not announced twice");
            colony = Save.BaseGameData.Planets[StellarBodies.the_moon];
            colony.PlanetResources.Stores[ItemTypes.iron] = 12;
            state.ColonyEventCountdown = 0;
            state.AdvanceResearch(Save, () => 0);
            Equal(10012, colony.PlanetResources.Stores[ItemTypes.iron], "stock events continue after PTL discovery and credit exactly ten thousand");
            core.ShowBulletin(BulletinTypes.Mining_Dump);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            colony.CaptiveBase = false;
            colony.BaseDamaged = true;
            state.ColonyEventCountdown = 0;
            state.AdvanceResearch(Save, () => 0);
            Equal(10012, colony.PlanetResources.Stores[ItemTypes.iron], "ordinary damage does not make a colony eligible");
            Equal(0, Save.News.PendingBulletins.Count, "ineligible colony cannot publish a mining dump");
            await DrainStoppedAudio();
        }

        private void PtlCaptiveLifecycle()
        {
            PrepareHyperlightDiscovery();
            var colony = Save.BaseGameData.Planets[StellarBodies.the_moon];
            colony.ActiveMethanoid = colony.Station.Built = colony.Station.SdmInstalled = true;
            colony.Station.SdmCountdown = 16;
            colony.BaseBuildParts = 2;
            var ship = NavigationShip();
            ship.PlanetLocation = colony.PlanetId;
            ship.ShipState = Ship_States.Docked;
            Save.Ships.Add(ship);
            Equal(true, SdmSystem.ApplySwitches(Save, colony, 0x0200), "real defusal captures the station");
            Equal(true, colony.CaptiveBase, "capture retains captive colony eligibility");
            Equal(true, Deuteros.Code.Utility.SaveStorage.Deserialize(Deuteros.Code.Utility.SaveStorage.Serialize(Save))
                .BaseGameData.Planets[colony.PlanetId].CaptiveBase, "captivity survives saving");
            var shuttle = new Shuttle { ShipType = Ship_Types.Shuttle, PlanetLocation = colony.PlanetId,
                ShipState = Ship_States.CrewRepairing, StartRepairDay = Save.CurrentDay - 2,
                Modules = new List<ShipModule> { new ShipModule { ModuleType = Module_Types.Tool, ItemStored = ItemTypes.bandaid, ItemCount = 1 } } };
            Save.Ships.Add(shuttle);
            shuttle.CompleteRepairs();
            Equal(false, colony.CaptiveBase, "completed colony repair ends captive state");
            colony.ActiveMethanoid = true;
            colony.Station.SdmCountdown = 16;
            SdmSystem.ApplySwitches(Save, colony, 0x0200);
            Equal(true, colony.CaptiveBase, "a later capture restores captive state");
            colony.Station.SdmCountdown = 1;
            SdmSystem.AdvanceTime(1);
            Equal(false, colony.CaptiveBase, "station loss changes the original captive state to a rebuilding state");
        }

        private void PtlDiscoverySaveValidation()
        {
            PrepareHyperlightDiscovery();
            var state = Save.AlienTransmissions;
            state.EnemySystems = state.HyperlightSystems = 6;
            state.HyperlightCountdown = 40;
            state.ColonyEventCountdown = 79;
            var ship = NavigationShip(); ship.PTL = true; Save.Ships.Add(ship);
            var baseline = Deuteros.Code.Utility.SaveStorage.Serialize(Save);
            foreach (var (field, value) in new[] { ("ColonyEventCountdown", -1), ("ColonyEventCountdown", 80), ("HyperlightCountdown", 41) })
            {
                var document = Newtonsoft.Json.Linq.JObject.Parse(baseline);
                document["Game"]["AlienTransmissions"][field] = value;
                var rejected = false;
                try { Deuteros.Code.Utility.SaveStorage.Deserialize(document.ToString()); }
                catch (System.IO.InvalidDataException) { rejected = true; }
                Equal(true, rejected, "invalid event countdown rejected: " + field);
            }
            foreach (var (location, mineral) in new[] { (StellarBodies.none, ItemTypes.iron), (StellarBodies.the_moon, ItemTypes.none),
                (StellarBodies.the_moon, ItemTypes.derrick), ((StellarBodies)999, ItemTypes.iron) })
            {
                var document = Newtonsoft.Json.Linq.JObject.Parse(baseline);
                document["Game"]["News"]["MiningDumpLocation"] = (int)location;
                document["Game"]["News"]["MiningDumpResource"] = (int)mineral;
                var rejected = false;
                try { Deuteros.Code.Utility.SaveStorage.Deserialize(document.ToString()); }
                catch (System.IO.InvalidDataException) { rejected = true; }
                Equal(true, rejected, "malformed mining dump parameters rejected");
            }
            foreach (var field in new[] { "CaptiveBase", "ColonyEventCountdown", "MiningDumpLocation", "MiningDumpResource" })
            {
                var document = Newtonsoft.Json.Linq.JObject.Parse(baseline);
                document.Descendants().OfType<Newtonsoft.Json.Linq.JProperty>().First(p => p.Name == field).Value = Newtonsoft.Json.Linq.JValue.CreateNull();
                var rejected = false;
                try { Deuteros.Code.Utility.SaveStorage.Deserialize(document.ToString()); }
                catch (Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "explicit null is not a missing legacy field: " + field);
            }
            var legacy = Newtonsoft.Json.Linq.JObject.Parse(baseline);
            legacy["Game"]["AlienTransmissions"]["HyperlightSystems"] = 7;
            legacy["Game"]["AlienTransmissions"]["HyperlightCountdown"] = 8;
            foreach (var property in legacy.Descendants().OfType<Newtonsoft.Json.Linq.JProperty>()
                .Where(p => p.Name is "CaptiveBase" or "ColonyEventCountdown" or "MiningDumpLocation" or "MiningDumpResource").ToList()) property.Remove();
            var loaded = Deuteros.Code.Utility.SaveStorage.Deserialize(legacy.ToString());
            Equal(8, loaded.AlienTransmissions.HyperlightCountdown, "existing Hyperlight delay survives migration");
            Equal(0, loaded.AlienTransmissions.ColonyEventCountdown, "old saves start the new event clock without replaying gifts");
            Equal(false, loaded.BaseGameData.Planets.Values.Any(p => p.CaptiveBase), "unknown historical captivity is not invented");
            Equal(true, ((InterStellarShip)loaded.Ships.Single()).PTL, "installed launchers survive loading");
            Equal(0, loaded.News.PendingBulletins.Count, "load grants no stock or discoveries");
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
            Save.EnemyBuildDay = Save.Clock.DateCentidays + 1200;
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

        private void HyperlightRecaptureCountdownOrder()
        {
            PrepareHyperlightDiscovery();
            var state = Save.AlienTransmissions;
            state.SampleEnemySystems(Save);
            state.AdvanceResearch(Save); // Count seven schedules eight decrement-only passes.
            state.EnemySystems = 8;
            state.AdvanceResearch(Save);
            Equal(7, state.HyperlightCountdown, "active delay runs before detecting recapture");
            Equal(7, state.HyperlightSystems, "observed story count stays unchanged during active delay");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = Deuteros.Code.Utility.SaveStorage.Deserialize(
                Deuteros.Code.Utility.SaveStorage.Serialize(Save));
            state = Save.AlienTransmissions;
            for (var tick = 0; tick < 7; tick++) state.AdvanceResearch(Save);
            Equal(0, state.HyperlightCountdown, "saved delay runs all remaining decrement passes");
            Equal(7, state.HyperlightSystems, "zero-reaching pass does not also detect recapture");
            state.AdvanceResearch(Save);
            Equal(8, state.HyperlightSystems, "following pass detects the changed count");
            Equal(false, state.HyperlightPending, "recapture prevents old discovery dispatch");
            state.EnemySystems = 7;
            state.AdvanceResearch(Save);
            Equal(8, state.HyperlightCountdown, "new count seven starts a fresh delay");
            for (var tick = 0; tick < 8; tick++) state.AdvanceResearch(Save);
            Equal(false, state.HyperlightPending, "new delay cannot dispatch on its final decrement");
            state.AdvanceResearch(Save);
            Equal(true, state.HyperlightPending, "discovery dispatches after the new delay");
            state.HyperlightPending = false;
            state.HyperlightCountdown = 4;
            state.EnemySystems = 8;
            state.AdvanceResearch(Save);
            state.EnemySystems = 7;
            for (var tick = 0; tick < 3; tick++) state.AdvanceResearch(Save);
            state.AdvanceResearch(Save);
            Equal(true, state.HyperlightPending, "transient ownership changes during delay do not restart it");
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
                for (var tick = 0; tick < 10; tick++) state.AdvanceResearch(Save);
                Equal(systems == 7, state.HyperlightPending, "only original count seven schedules Hyperlight");
            }
            state.EnemySystems = 7; state.HyperlightSystems = 9; state.HyperlightPending = false;
            Save.AtWar = false;
            for (var tick = 0; tick < 12; tick++) state.AdvanceResearch(Save);
            Equal(false, state.HyperlightPending, "peace does not advance war progression");
            Save.AtWar = true;
            state.AdvanceResearch(Save);
            state.AdvanceResearch(Save);
            state.EnemySystems = 8;
            state.AdvanceResearch(Save);
            Equal(6, state.HyperlightCountdown, "recapture waits for the active countdown before count detection");
            for (var tick = 0; tick < 7; tick++) state.AdvanceResearch(Save);
            Equal(8, state.HyperlightSystems, "recapture is observed after the delay");
            Equal(false, state.HyperlightPending, "ineligible count does not produce discovery");
        }
    }
}
