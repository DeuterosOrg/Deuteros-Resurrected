using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using NewsScreen = Deuteros.Code.Platform.Screens.News;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunNewsRegressions()
        {
            await CheckAsync("News renders dated latest twelve entries and preserves history in saves", NewsHistory);
            await CheckAsync("News replay is unavailable until a bulletin exists", NewsReplayAvailability);
            await CheckAsync("News replays the saved bulletin without duplicating history", NewsReplay);
            await CheckAsync("Leaving a typing bulletin releases only its own input lock", BulletinExit);
            await CheckAsync("Replacing a typing bulletin preserves the new bulletin lock", BulletinReplacement);
        }

        private async Task NewsShipEvents()
        {
            var news = await OpenNews();
            GameCore.SingletonInstance.SetProcess(false);
            Save.Ships.Clear();
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                var ship = LoadedDismantleShip(type);
                ship.Name = "Lost " + type;
                ship.ACC = null;
                ship.ShipState = Ship_States.UnDocked;
                ship.Fuel = 0;
                ship.FallingCount = 4;
                Save.Ships.Add(ship);
            }
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(0, 1);
            Equal(0, Save.Ships.Count, "all three actual fuel losses committed");
            foreach (var name in new[] { "Lost Shuttle", "Lost IOS", "Lost SCG" })
                Equal(1, Save.News.GetNews(100).Count(n => n.Contains(name) && n.Contains("Destroyed")), "each named loss reported once");
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(1, 2);
            Equal(3, Save.News.GetNews(100).Count, "removed ships cannot repeat reports");
            var victim = (InterStellarShip)LoadedDismantleShip(Ship_Types.IOS);
            victim.Name = "Hostile Orbit";
            victim.ACC = null;
            victim.ShipState = Ship_States.UnDocked;
            Save.Ships.Add(victim);
            Save.AtWar = true;
            GameCore.Earth.ActiveMethanoid = true;
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(2, 3);
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains("Hostile Orbit UNDER ATTACK")), "attack transition reported");
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(3, 4);
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains("Hostile Orbit UNDER ATTACK")), "ongoing attack is not reported twice");
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains("Hostile Orbit Destroyed")), "hostile orbit loss reported");
            GameCore.Earth.ActiveMethanoid = false;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            news.DrawData();
            Equal(true, news.NewsLabels[0].Text.Contains("Hostile Orbit Destroyed"), "saved latest loss appears first");
            Equal(Save.News.GetNews(1).Single(), news.NewsLabels[0].TooltipText, "full report is retained for hover when a name overflows");
            Equal(TextServer.OverrunBehavior.TrimEllipsis, news.NewsLabels[0].TextOverrunBehavior, "overflow is indicated instead of drawing outside the panel");
            Equal("", news.NewsLabels[11].TooltipText, "unused rows have no stale hover text");
            await CaptureDisplayEvidence("news-ship-losses");
        }

        private async Task NewsStationEvents()
        {
            var news = await OpenNews();
            GameCore.SingletonInstance.SetProcess(false);
            Save.Ships.Clear();
            var target = Save.BaseGameData.Planets[StellarBodies.mars];
            target.ActiveMethanoid = false;
            target.Station.Built = true;
            target.Station.BuildParts = 8;
            var victim = LoadedDismantleShip(Ship_Types.SCG);
            victim.Name = "Captured Ship";
            victim.PlanetLocation = target.PlanetId;
            Save.Ships.Add(victim);
            var fleet = new EnemyFleet { ShipType = Ship_Types.IOS, MethanoidOwned = true,
                PlanetLocation = StellarBodies.jupiter, StarLocation = StellarBodies.the_sun,
                DestinationPlanetLocation = target.PlanetId, AttackDay = 1, DroneCount = 10,
                Modules = new List<ShipModule>(), Fuel = 100 };
            Save.Ships.Add(fleet);
            fleet.ProcessFleet();
            Equal(true, fleet.Attacking, "actual fleet attack starts");
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains("Mars UNDER ATTACK")), "station attack is reported");
            for (int tick = 0; tick < 5; tick++) fleet.ProcessFleet();
            Equal(true, target.ActiveMethanoid, "actual capture commits");
            Equal(false, Save.Ships.Contains(victim), "capture removes player's ship");
            var reports = Save.News.GetNews(100);
            Equal(3, reports.Count, "one attack, one ship loss, one station capture");
            Equal(true, reports[1].Contains("Captured Ship Destroyed"), "ship loss precedes station capture");
            Equal(true, reports[2].Contains("Mars CAPTURED"), "station capture is the final report");
            fleet.ProcessFleet();
            Equal(3, Save.News.GetNews(100).Count, "inactive fleet cannot repeat capture reports");
            news.DrawData();
            Equal(true, news.NewsLabels[0].Text.Contains("Mars CAPTURED"), "capture is visible newest first");
            await CaptureDisplayEvidence("news-station-capture");
        }

        private async Task<NewsScreen> OpenNews()
        {
            InitializeUi();
            GameCore.SingletonInstance.ChangeScene(Scenes.News, new List<SceneVariables>());
            await InputFrames();
            return ActiveScreen<NewsScreen>();
        }

        private async Task NewsHistory()
        {
            var news = await OpenNews();
            Equal(true, news.NewsLabels.All(label => label.Text == ""), "empty game has no placeholder reports");
            for (uint day = 1; day <= 15; day++)
            {
                Save.CurrentDay = day;
                Save.News.AddNews("Report " + day);
            }
            news.DrawData();
            Equal("15 : Report 15", news.NewsLabels[0].Text, "newest report at top");
            Equal("4  : Report 4", news.NewsLabels[11].Text, "oldest visible report at bottom");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(15, restored.News.GetNews(100).Count, "offscreen history retained in save");
            var researcher = new Staff { Leader = "Scientist", Count = 10, Type = StaffType.Research };
            researcher.AddAction(5);
            researcher.AddAction();
            AdvanceTickDay();
            Equal(true, news.NewsLabels[0].Text.Contains("New Rank:Doctor"), "real promotion appears on next day refresh");
        }

        private async Task NewsReplayAvailability()
        {
            var news = await OpenNews();
            Equal(true, news.ReplayIcon.Disabled, "no bulletin disables replay control");
            news.ReplayIcon.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(Scenes.News, GameCore.SingletonInstance.currentScene, "empty replay cannot navigate");
            Save.News.LastBulletin = BulletinTypes.Matter_Transmitter;
            news.DrawData();
            Equal(false, news.ReplayIcon.Disabled, "available bulletin enables replay");
        }

        private async Task FinishBulletin(Bulletins bulletin)
        {
            bulletin.LetterDelayMs = 0;
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker");
            for (int i = 0; i < 400 && blocker.Blocked; i++)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(false, blocker.Blocked, "typewriter completes and releases input");
            await DrainStoppedAudio();
        }

        private async Task DrainStoppedAudio()
        {
            // Godot 4.2.1 Stop() schedules playback disposal on the audio mixer.
            // Let stopped sounds drain before an isolated test immediately quits.
            var deadline = Time.GetTicksMsec() + 2000;
            var previousMixAge = AudioServer.GetTimeSinceLastMix();
            var observedMixes = 0;
            while (observedMixes < 2 && Time.GetTicksMsec() < deadline)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                var mixAge = AudioServer.GetTimeSinceLastMix();
                if (mixAge < previousMixAge) observedMixes++;
                previousMixAge = mixAge;
            }
            Equal(2, observedMixes, "audio mixer drains stopped playback before teardown");
        }

        private async Task NewsReplay()
        {
            var news = await OpenNews();
            Save.News.AddNews("Existing history");
            Save.News.LastBulletin = BulletinTypes.Matter_Transmitter;
            Save.BaseGameData.BulletinTexts[BulletinTypes.Matter_Transmitter].BulletinText = "Replay message.";
            news.DrawData();
            news.ReplayIcon.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(Scenes.Bulletins, GameCore.SingletonInstance.currentScene, "replay opens bulletin");
            var bulletin = ActiveScreen<Bulletins>();
            await FinishBulletin(bulletin);
            Equal(true, bulletin.GetNode<RichTextLabel>("Labels/BulletinLabel").Text.Contains("Replay message."), "saved bulletin content rendered");
            Equal(1, Save.News.GetNews(100).Count, "replay does not duplicate history");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(BulletinTypes.Matter_Transmitter, restored.News.LastBulletin, "replay identity survives save");
        }

        private async Task BulletinExit()
        {
            await OpenNews();
            GameCore.SingletonInstance.ShowBulletin(BulletinTypes.Matter_Transmitter);
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker");
            Equal(true, blocker.Blocked, "typing owns a screen lock");
            GameCore.LockScreen();
            GameCore.SingletonInstance.ChangeScene(Scenes.Overview, new List<SceneVariables>());
            await InputFrames();
            Equal(true, blocker.Blocked, "unrelated lock survives bulletin exit");
            GameCore.UnLockScreen();
            Equal(false, blocker.Blocked, "bulletin exit released its own lock");
            await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
            Equal(false, blocker.Blocked, "cancelled typing cannot relock or access freed controls");
        }

        private async Task BulletinReplacement()
        {
            await OpenNews();
            var core = GameCore.SingletonInstance;
            core.ShowBulletin(BulletinTypes.Matter_Transmitter);
            core.ShowBulletin(BulletinTypes.Matter_Transmitter);
            await InputFrames();
            Equal(true, core.GetNode<InputBlocker>("InputBlocker").Blocked, "new bulletin remains locked after old one exits");
            await FinishBulletin(ActiveScreen<Bulletins>());
        }
    }
}
