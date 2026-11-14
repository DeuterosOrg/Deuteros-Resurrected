using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task SettingsEventAlerts()
        {
            await WithSettings(async screen =>
            {
                var core = GameCore.SingletonInstance;
                core.SetProcess(false);
                var manager = SettingsManager.Instance;
                var menu = ActiveRecipeScreen<MainMenu>();
                manager.SetSetting("gameplay/event_alerts", "On");
                manager.SetSetting("gameplay/tooltips", false);
                GameCore.HoverText = "";
                Save.News.AddNews("Earth UNDER ATTACK !");
                menu._Process(0);
                Equal("Earth UNDER ATTACK !", menu.HoverInfo.Text, "enabled event alert renders with tooltips disabled");
                Equal(true, GetTree().Paused && OverlayManager.Instance.IsOpen, "event leaves Settings pause ownership intact");
                Equal(true, Save.News.GetNews(1).Single().EndsWith("Earth UNDER ATTACK !"), "alert is also recorded in history");
                var scene = core.currentScene;
                var bulletin = Save.News.LastBulletin;
                var pending = Save.News.PendingBulletins.ToArray();
                foreach (var disabled in new Variant[] { "Off", "invalid", 42 })
                {
                    manager.SetSetting("gameplay/event_alerts", disabled);
                    GameCore.HoverText = "existing text";
                    Save.News.AddNews("Quiet report");
                    Equal("existing text", GameCore.HoverText, "disabled or malformed option does not replace feedback");
                }
                Equal(4, Save.News.GetNews(100).Count, "Off keeps every report");
                Equal(scene, core.currentScene, "alerts do not navigate");
                Equal(bulletin, Save.News.LastBulletin, "last story bulletin retained");
                Equal(true, pending.SequenceEqual(Save.News.PendingBulletins), "queued story bulletins retained");
                manager.SetSetting("gameplay/event_alerts", "On");
                new Deuteros.Code.Objects.News().AddNews("Detached world report");
                Equal("existing text", GameCore.HoverText, "inactive world cannot publish into the active UI");
                var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                Equal(true, Save.News.GetNews(100).SequenceEqual(restored.News.GetNews(100)), "history round-trips without replaying alerts");
                Equal("existing text", GameCore.HoverText, "deserializing history cannot publish old alerts");
                var staff = new Staff { Leader = "Floyd", Type = Enums.StaffType.Marines, Count = 20 };
                staff.AddAction(40);
                staff.PromoteWarlord();
                menu._Process(0);
                Equal(true, menu.HoverInfo.Text.Contains("New Rank:Warlord"), "real promotion producer publishes through the shared path");
                manager.SetSetting("gameplay/event_alerts", "Off");
                OverlayManager.Instance.CloseOverlay();
                screen = OpenSettings();
                screen.GameplayTab.ButtonPressed = true;
                Equal("Off", screen.EventAlertsRow.SettingValue.AsString(), "default row is Off");
                screen.EventAlertsRow.StepValue(1);
                Equal("On", screen.EventAlertsRow.SettingValue.AsString(), "row enables alerts");
                Press(screen, "%ApplyButton");
                using (var disk = new GameConfig(manager.Config.FilePath))
                    Equal("On", disk.GetValue("gameplay", "event_alerts").AsString(), "Apply persists alert preference");
                screen.EventAlertsRow.StepValue(1);
                Press(screen, "%CancelButton");
                screen = OpenSettings();
                screen.GameplayTab.ButtonPressed = true;
                Equal("On", screen.EventAlertsRow.SettingValue.AsString(), "Cancel restores applied alerts");
                Press(screen, "%RestoreButton");
                Equal("Off", screen.EventAlertsRow.SettingValue.AsString(), "Gameplay defaults restore Off");
                Press(screen, "%CancelButton");
                core.ChangeScene(Enums.Scenes.Earth_Ground, new System.Collections.Generic.List<Enums.SceneVariables> { Enums.SceneVariables.Ground });
                await InputFrames();
                Save.News.AddNews("Earth UNDER ATTACK !");
                menu._Process(0);
                await CaptureDisplayEvidence("news-event-alert");
                menu.NewsButton.EmitSignal(Control.SignalName.MouseEntered);
                menu._Process(0);
                Equal(false, GameCore.HoverTextIsStatus, "normal hover resumes ownership of the status line");
                Equal(false, GetTree().Paused, "event alert does not pause gameplay");
            });
        }
    }
}
