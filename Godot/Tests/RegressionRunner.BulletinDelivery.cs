using System.Collections.Generic;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using Godot;
using Newtonsoft.Json.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task BlockedBulletins()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            GameCore.LockScreen();
            try
            {
                core.TriggerAlienTechDiscovery(ItemTypes.m__t__x);
                core.ShowBulletin(BulletinTypes.Matter_Transmitter);
                Equal(Scenes.SaveScreen, core.currentScene, "discovery cannot replace a screen owned by another lock");
                Equal(BulletinTypes.None, Save.News.LastBulletin, "queued notice is not falsely recorded as displayed");
            }
            finally { GameCore.UnLockScreen(); }
            var document = JObject.Parse(SaveStorage.Serialize(Save));
            foreach (var value in new JToken[] { JValue.CreateNull(), new JArray(-1), new JArray(999),
                new JArray((int)BulletinTypes.None), new JArray((int)BulletinTypes.IOS, (int)BulletinTypes.IOS) })
            {
                var invalid = (JObject)document.DeepClone();
                invalid["Game"]["News"]["PendingBulletins"] = value;
                var rejected = false;
                try { SaveStorage.Deserialize(invalid.ToString()); }
                catch (System.Exception error) when (error is System.IO.InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "invalid pending bulletin state is rejected");
            }
            var legacy = (JObject)document.DeepClone();
            ((JObject)legacy["Game"]["News"]).Property("PendingBulletins")?.Remove();
            var restoredLegacy = JObject.Parse(SaveStorage.Serialize(SaveStorage.Deserialize(legacy.ToString())));
            Equal(0, ((JArray)restoredLegacy["Game"]["News"]["PendingBulletins"]).Count, "legacy save invents no queued notices");
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(document.ToString());
            Save.BaseGameData.BulletinTexts[BulletinTypes.Matter_Transmitter].BulletinText = "Queued discovery.";
            Save.BaseGameData.BulletinTexts[BulletinTypes.Self_Destruct].BulletinText = "New discovery.";
            core.TriggerAlienTechDiscovery(ItemTypes.s__d__m);
            Equal(BulletinTypes.Matter_Transmitter, Save.News.LastBulletin, "new discovery cannot overtake an older queued notice");
            AdvanceTickDay();
            Equal(BulletinTypes.Matter_Transmitter, Save.News.LastBulletin, "unblocked saved discovery is delivered");
            await FinishBulletin(ActiveScreen<Bulletins>());
            core.ChangeScene(Scenes.News, new List<SceneVariables>());
            await InputFrames();
            ActiveScreen<Deuteros.Code.Platform.Screens.News>().ReplayIcon.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(BulletinTypes.Matter_Transmitter, Save.News.LastBulletin, "News replay must not consume a different pending discovery");
            await FinishBulletin(ActiveScreen<Bulletins>());
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            AdvanceTickDay();
            Equal(BulletinTypes.Self_Destruct, Save.News.LastBulletin, "new discovery follows the older queued notice");
            await FinishBulletin(ActiveScreen<Bulletins>());
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            AdvanceTickDay();
            Equal(Scenes.SaveScreen, core.currentScene, "duplicate queued requests produce one notice");
        }

        private async Task SimultaneousBulletins()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            Save.Ships.Clear();
            Save.BaseGameData.BulletinTexts[BulletinTypes.IOS_Attachments].BulletinText = "First discovery.";
            Save.BaseGameData.BulletinTexts[BulletinTypes.Methanoid_Laser].BulletinText = "Second discovery.";
            var earth = GameCore.Earth;
            var factory = earth.Station.Factory;
            factory.Builder = new Staff { Type = StaffType.Production, Count = 250, Leader = "Builder" };
            factory.Builder.AddAction(12);
            factory.ProductionQueue.Clear();
            factory.ProductionQueue.Add(new ProductionItem(core.GameData.GetItem(ItemTypes.i_chassis))
                { Active = true, Production_Value = 255, Production_Complete = 3 });
            earth.ResearchStaff = new Staff { Type = StaffType.Research, Count = 250, Leader = "Science" };
            earth.ResearchStaff.AddAction(9);
            var research = core.GameData.GetItem(ItemTypes.m__f__l).Research;
            research.Researched = false; research.Locked = false;
            research.ResearchLimit = 100; research.ResearchPercentageComplete = 99; research.ResearchValue = 255;
            earth.CurrentResearchItem = research;
            AdvanceTickDay();
            Equal(true, Save.Unlocks.Contains(Game_Unlocks.IOS_Attachments), "production really discovers attachments");
            Equal(true, Save.Unlocks.Contains(Game_Unlocks.D_F_C_C), "research really discovers drones in the same update");
            Equal(BulletinTypes.IOS_Attachments, Save.News.LastBulletin, "second discovery cannot replace the first bulletin");
            var first = ActiveScreen<Bulletins>();
            await FinishBulletin(first);
            Equal(true, first.GetNode<RichTextLabel>("Labels/BulletinLabel").Text.Contains("First discovery."), "first discovery is readable");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Save.BaseGameData.BulletinTexts[BulletinTypes.Methanoid_Laser].BulletinText = "Second discovery.";
            AdvanceTickDay();
            Equal(BulletinTypes.Methanoid_Laser, Save.News.LastBulletin, "saved second discovery is delivered on the next available update");
            var second = ActiveScreen<Bulletins>();
            await FinishBulletin(second);
            Equal(true, second.GetNode<RichTextLabel>("Labels/BulletinLabel").Text.Contains("Second discovery."), "second discovery is readable");
            await CaptureDisplayEvidence("queued-discovery-bulletin");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            AdvanceTickDay();
            Equal(Scenes.SaveScreen, core.currentScene, "delivered notices do not repeat");
        }
    }
}
