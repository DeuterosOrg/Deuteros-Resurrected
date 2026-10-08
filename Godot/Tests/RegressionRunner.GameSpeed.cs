using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Godot;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task SettingsGameSpeed()
        {
            await WithSettings(async screen =>
            {
                var core = GameCore.SingletonInstance;
                core.SetProcess(false);
                var manager = SettingsManager.Instance;
                // Reproduce the missing consumer independently of the row's options.
                manager.SetSetting("gameplay/game_speed", "2x");
                OverlayManager.Instance.CloseOverlay();
                core.ChangeScene(Enums.Scenes.Earth_Ground, new System.Collections.Generic.List<Enums.SceneVariables> { Enums.SceneVariables.Ground });
                await InputFrames();
                Save.TimeSkip = false;
                Save.Clock.NormalElapsed = 0;
                core._Process(10);
                Equal(20d, Save.Clock.NormalElapsed, "2x advances the natural calendar twice as fast");
                await Task.Delay(1100); // Establish enough monotonic uptime for boundary checks.
                foreach (var (option, factor) in new[] { ("0.5x", 0.5), ("1x", 1d), ("2x", 2d) })
                {
                    manager.SetSetting("gameplay/game_speed", option);
                    Save.TimeSkip = false;
                    Save.Clock.NormalElapsed = 0;
                    var date = Save.Clock.DateCentidays;
                    core._Process(GameClock.NormalIntervalSeconds / factor);
                    Equal(date + 1, Save.Clock.DateCentidays, option + " natural interval advances one centiday");
                    Save.TimeSkipDay = true;
                    core._Process(0);
                    Equal(date + 101, Save.Clock.DateCentidays, option + " date click still advances exactly one day");
                    Save.TimeSkip = true;
                    var interval = (ulong)(500 / factor);
                    Save.TimeSkipStart = Time.GetTicksMsec() - interval / 2;
                    core._Process(0);
                    Equal(date + 101, Save.Clock.DateCentidays, option + " fast mode waits for its interval");
                    Save.TimeSkipStart = Time.GetTicksMsec() - interval;
                    core._Process(0);
                    Equal(date + 201, Save.Clock.DateCentidays, option + " fast mode consumes one day when due");
                    Save.TimeSkip = false;
                }
                foreach (var invalid in new Variant[] { "invalid", "0x", 42 })
                {
                    manager.SetSetting("gameplay/game_speed", invalid);
                    Save.Clock.NormalElapsed = 0;
                    core._Process(10);
                    Equal(10d, Save.Clock.NormalElapsed, "invalid speed safely uses 1x");
                }
                manager.SetSetting("gameplay/game_speed", "1x");
                screen = OpenSettings();
                screen.GameplayTab.ButtonPressed = true;
                Equal("1x", screen.GameSpeedRow.SettingValue.AsString(), "normal rate shown on reopen");
                screen.GameSpeedRow.StepValue(1);
                Equal("2x", screen.GameSpeedRow.SettingValue.AsString(), "row previews double rate");
                var elapsed = Save.Clock.NormalElapsed;
                core._Process(100);
                Equal(elapsed, Save.Clock.NormalElapsed, "speed preview cannot advance paused settings");
                Press(screen, "%ApplyButton");
                using (var disk = new GameConfig(manager.Config.FilePath))
                    Equal("2x", disk.GetValue("gameplay", "game_speed").AsString(), "Apply persists speed");
                OverlayManager.Instance.CloseOverlay();
                manager.Revert();
                screen = OpenSettings();
                screen.GameplayTab.ButtonPressed = true;
                Equal("2x", screen.GameSpeedRow.SettingValue.AsString(), "saved rate restored");
                screen.GameSpeedRow.StepValue(1);
                Equal("0.5x", screen.GameSpeedRow.SettingValue.AsString(), "row cycles to half rate");
                Press(screen, "%CancelButton");
                screen = OpenSettings();
                screen.GameplayTab.ButtonPressed = true;
                Equal("2x", screen.GameSpeedRow.SettingValue.AsString(), "Cancel discards speed preview");
                Press(screen, "%RestoreButton");
                Equal("1x", screen.GameSpeedRow.SettingValue.AsString(), "Gameplay defaults restore normal rate");
                await CaptureDisplayEvidence("game-speed-settings");
                Equal(1d, Engine.TimeScale, "calendar speed leaves presentation and real-time timers unchanged");
            });
        }
    }
}
