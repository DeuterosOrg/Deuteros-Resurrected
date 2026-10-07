using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Newtonsoft.Json.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task StarDroneDiscovery()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            var chassis = core.GameData.GetItem(ItemTypes.g_chassis).Research;
            var drone = core.GameData.GetItem(ItemTypes.star_drone);
            Equal(true, drone.Research.Locked, "Star Drones start undiscovered");
            core.TriggerResearchFinished(core.GameData.GetItem(ItemTypes.star_drive).Research);
            Equal(true, drone.Research.Locked, "drive event is not the chassis discovery trigger");
            GameCore.Earth.ResearchStaff = new Staff { Type = StaffType.Research, Count = 240, Leader = "Science" };
            GameCore.Earth.ResearchStaff.AddAction(9);
            chassis.Locked = false;
            GameCore.Earth.CurrentResearchItem = chassis;
            GameCore.LockScreen();
            try
            {
                for (uint day = 0; day < 100 && !chassis.Researched; day++) Research.UpdateResearch(day, day + 1);
                Equal(true, chassis.Researched, "normal research calculation completes chassis");
                Equal(false, drone.Research.Locked, "chassis completion exposes Star Drone research");
                Equal(false, drone.Research.Researched, "discovery does not complete research");
                Equal(true, drone.Locked, "manufacture still requires Star Drone research");
                Equal(Scenes.SaveScreen, core.currentScene, "discovery respects current modal owner");
                core.TriggerResearchFinished(chassis);
                Equal(1, Save.News.PendingBulletins.Count(b => b == BulletinTypes.SCG_Drone), "repeat completion queues one bulletin");
            }
            finally { GameCore.UnLockScreen(); }
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Save.BaseGameData.BulletinTexts[BulletinTypes.SCG_Drone].BulletinText = "Star Drone research available.";
            AdvanceTickDay();
            Equal(BulletinTypes.SCG_Drone, Save.News.LastBulletin, "saved pending discovery is displayed");
            await FinishBulletin(ActiveScreen<Bulletins>());
            core.ChangeScene(Scenes.Earth_Research, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            drone = core.GameData.GetItem(ItemTypes.star_drone);
            ActiveScreen<Research>().Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.star_drone)
                .EmitSignal(Godot.BaseButton.SignalName.Pressed);
            Equal(drone.Research, GameCore.Earth.CurrentResearchItem, "discovered drone is selectable through Research");
            for (uint day = 0; day < 100 && !drone.Research.Researched; day++) Research.UpdateResearch(day, day + 1);
            Equal(true, drone.Research.Researched, "Star Drone research can finish normally");
            Equal(false, drone.Locked, "completed Star Drone research permits manufacture");
            Equal(0, Save.News.PendingBulletins.Count, "drone completion does not repeat chassis discovery");
        }

        private void StarDroneDiscoveryLegacy()
        {
            var core = GameCore.SingletonInstance;
            var chassis = core.GameData.GetItem(ItemTypes.g_chassis).Research;
            var drone = core.GameData.GetItem(ItemTypes.star_drone).Research;
            Equal(true, SaveStorage.Deserialize(SaveStorage.Serialize(Save)).BaseGameData.ItemList
                .Single(i => i.ItemType == ItemTypes.star_drone).Research.Locked, "new campaign remains locked after load");
            chassis.Researched = true; chassis.ResearchPercentageComplete = 100;
            Save.News.PendingBulletins.Add(BulletinTypes.Matter_Transmitter);
            var before = JObject.Parse(SaveStorage.Serialize(Save));
            var loaded = SaveStorage.Deserialize(before.ToString());
            var after = JObject.Parse(SaveStorage.Serialize(loaded));
            Equal(false, loaded.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.star_drone).Research.Locked,
                "old completed-chassis campaign receives missing research");
            Equal(BulletinTypes.Matter_Transmitter, loaded.News.PendingBulletins[0], "existing notice keeps priority");
            Equal(BulletinTypes.SCG_Drone, loaded.News.PendingBulletins[1], "missing discovery is queued once");
            Equal(SaveStorage.Serialize(loaded), SaveStorage.Serialize(SaveStorage.Deserialize(SaveStorage.Serialize(loaded))),
                "repeat load is idempotent");
            // Compare the whole world after accounting only for the two intended migration fields.
            foreach (var research in before.SelectTokens("$..Research").OfType<JObject>())
                if (research["ItemType"]?.Value<int>() == (int)ItemTypes.star_drone) research["Locked"] = false;
            ((JArray)before["Game"]["News"]["PendingBulletins"]).Add((int)BulletinTypes.SCG_Drone);
            Equal(true, JToken.DeepEquals(before, after), "all campaign assets and progression otherwise preserved");
            drone.Locked = false; drone.Researched = true; drone.ResearchPercentageComplete = 100;
            loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(1, loaded.News.PendingBulletins.Count, "already discovered drones do not invent another notice");
            Equal(true, loaded.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.star_drone).Research.Researched,
                "completed drone research is retained");
        }
    }
}
