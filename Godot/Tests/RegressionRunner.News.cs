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
            await CheckAsync("News preserves pointer navigation and enables replay only when a bulletin exists", NewsReplayAvailability);
            await CheckAsync("News replays the saved bulletin without duplicating history", NewsReplay);
            await CheckAsync("Leaving a typing bulletin releases only its own input lock", BulletinExit);
            await CheckAsync("Repeated bulletin requests preserve the active typing screen and lock", BulletinReplacement);
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
            Equal(9, Save.News.GetNews(100).Count, "removed ships and their crews cannot repeat reports");
            news.DrawData();
            Equal("", news.NewsLabels[11].TooltipText, "short history has no stale hover text");
            var victim = (InterStellarShip)LoadedDismantleShip(Ship_Types.IOS);
            victim.Name = "Hostile Orbit";
            victim.ACC = null;
            victim.ShipState = Ship_States.UnDocked;
            Save.Ships.Add(victim);
            Save.AtWar = true;
            GameCore.Earth.ActiveMethanoid = true;
            GameCore.Earth.Station.Resources.Stores[ItemTypes.ios_drone] = 92;
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(2, 3);
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains("Hostile Orbit UNDER ATTACK")), "attack transition reported");
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(3, 4);
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains("Hostile Orbit UNDER ATTACK")), "ongoing attack is not reported twice");
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains("Hostile Orbit Destroyed")), "hostile orbit loss reported");
            GameCore.Earth.ActiveMethanoid = false;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            news.DrawData();
            Equal(true, news.NewsLabels[0].Text.Contains("Hostile Orbit Destroyed"), "saved latest loss appears first");
            Equal(true, news.NewsLabels[1].Text.Contains("Crew Killed."), "passenger loss appears under vessel report");
            Equal(true, news.NewsLabels[2].Text.Contains("Pilot Killed."), "pilot loss remains visible");
            Equal(Save.News.GetNews(1).Single(), news.NewsLabels[0].TooltipText, "full report is retained for hover when a name overflows");
            Equal(TextServer.OverrunBehavior.TrimEllipsis, news.NewsLabels[0].TextOverrunBehavior, "overflow is indicated instead of drawing outside the panel");
            Equal(Save.News.GetNews(12).First(), news.NewsLabels[11].TooltipText, "full history retains the oldest visible report");
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
            Equal(5, reports.Count, "one attack, two crew losses, one ship loss, one station capture");
            Equal(true, reports[3].Contains("Captured Ship Destroyed"), "ship loss precedes station capture");
            Equal(true, reports[4].Contains("Mars CAPTURED"), "station capture is the final report");
            fleet.ProcessFleet();
            Equal(5, Save.News.GetNews(100).Count, "inactive fleet cannot repeat capture reports");
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
                Save.Clock.DateCentidays = (ulong)day * 100;
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

        private async Task NewsReplayAvailability() => await WithMenuSound(async (menu, viewport, player) =>
        {
            GameCore.SingletonInstance.ChangeScene(Scenes.News, new List<SceneVariables>());
            await InputFrames();
            var news = ActiveScreen<NewsScreen>();
            var parent = news.GetParent();
            news.Reparent(viewport);
            try
            {
                await InputFrames();
                Equal(true, news.ReplayIcon.Disabled, "no bulletin disables replay control");
                ClickMenu(viewport, news.ReplayIcon);
                Equal(Scenes.News, GameCore.SingletonInstance.currentScene, "empty replay cannot navigate");
                Save.News.LastBulletin = BulletinTypes.Matter_Transmitter;
                news.DrawData();
                Equal(false, news.ReplayIcon.Disabled, "available bulletin enables replay");
                ClickMenu(viewport, menu.TimeButton);
                Equal(true, Save.TimeSkip, "News background allows pointer to start time");
                ClickMenu(viewport, menu.TimeButton);
                Equal(false, Save.TimeSkip, "News background allows pointer to stop time");
                var hovered = false;
                news.NewsLabels[0].MouseEntered += () => hovered = true;
                MenuPointer(viewport, news.NewsLabels[0].GetGlobalRect().GetCenter());
                Equal(true, hovered, "News labels still receive hover input");
                ClickMenu(viewport, menu.MasterControlButton);
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "News background allows pointer to Master Control");
            }
            finally
            {
                if (GodotObject.IsInstanceValid(news)) news.Reparent(parent);
            }
        });

        private async Task FinishBulletin(Bulletins bulletin)
        {
            bulletin.LetterDelayMs = 0;
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("GameContainer/GameViewport/InputBlocker");
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
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("GameContainer/GameViewport/InputBlocker");
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

        private async Task BulletinSkip()
        {
            await OpenNews();
            var settings = SettingsManager.Instance;
            var original = settings.GetSetting("modern/bulletin_skip", false);
            var core = GameCore.SingletonInstance;
            core.ShowBulletin(BulletinTypes.Matter_Transmitter);
            var screen = ActiveScreen<Bulletins>();
            var label = screen.GetNode<RichTextLabel>("Labels/BulletinLabel");
            var blocker = core.GetNode<InputBlocker>("GameContainer/GameViewport/InputBlocker");
            void Click() => screen.GetViewport().PushInput(new InputEventMouseButton
                { ButtonIndex = MouseButton.Left, Pressed = true, Position = new Vector2(310, 195) }, true);
            try
            {
                settings.SetSetting("modern/bulletin_skip", false);
                Click();
                await ToSignal(GetTree().CreateTimer(0.12), SceneTreeTimer.SignalName.Timeout);
                Equal(true, blocker.Blocked && label.VisibleCharacters < label.GetTotalCharacterCount(), "disabled skip keeps typing");
                GetTree().Paused = true;
                var count = label.VisibleCharacters;
                await ToSignal(GetTree().CreateTimer(0.12), SceneTreeTimer.SignalName.Timeout);
                Equal(count, label.VisibleCharacters, "typing pauses with the game");
                GetTree().Paused = false;
                GameCore.LockScreen();
                settings.SetSetting("modern/bulletin_skip", true);
                Click();
                await ToSignal(GetTree().CreateTimer(0.12), SceneTreeTimer.SignalName.Timeout);
                Equal(-1, label.VisibleCharacters, "enabled skip reveals complete text");
                Equal(true, blocker.Blocked, "skip preserves another owner's lock");
                GameCore.UnLockScreen();
                Equal(false, blocker.Blocked, "skip releases exactly its typing lock");
                Equal(Scenes.Bulletins, core.currentScene, "skip click cannot navigate behind the bulletin");
            }
            finally
            {
                GetTree().Paused = false;
                settings.SetSetting("modern/bulletin_skip", original);
                core.ChangeScene(Scenes.Overview, new List<SceneVariables>());
                await InputFrames();
                await DrainStoppedAudio();
            }
        }

        private async Task BulletinReplacement()
        {
            await OpenNews();
            var core = GameCore.SingletonInstance;
            core.ShowBulletin(BulletinTypes.Matter_Transmitter);
            var first = ActiveScreen<Bulletins>();
            core.ShowBulletin(BulletinTypes.Matter_Transmitter);
            await InputFrames();
            Equal(first, ActiveScreen<Bulletins>(), "new request preserves the active bulletin");
            Equal(true, core.GetNode<InputBlocker>("GameContainer/GameViewport/InputBlocker").Blocked, "active bulletin retains its typing lock");
            await FinishBulletin(ActiveScreen<Bulletins>());
        }
    }
}
