using Settings = Deuteros.UI.Settings.SettingsScreen;
using System;
using System.IO;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public async Task RunSettingsRegressions()
        {
            await CheckAsync("Settings persist audio choices and reopening reflects them", SettingsAudio);
            await CheckAsync("Settings separate cheats and confirm progression changes", SettingsCheats);
        }

        private async Task RunSettingsAdditionalRegressions()
        {
            await CheckAsync("Settings persist display choices and restore defaults", SettingsDisplay);
            await CheckAsync("Progression presets reject full crew rosters before changing the world", SettingsFullCrew);
            Check("Settings tolerate invalid preference values", SettingsInvalidValues);
            await CheckAsync("Modern settings validate, migrate, revert and report failed saves", SettingsModernLifecycle);
        }

        private async Task SettingsNavigationShortcuts()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            var settings = SettingsManager.Instance;
            async Task KeyPress(Key key, bool echo = false, bool control = false)
            {
                GetViewport().PushInput(new InputEventKey { Keycode = key, PhysicalKeycode = key,
                    Unicode = (uint)char.ToLowerInvariant((char)key),
                    Pressed = true, Echo = echo, CtrlPressed = control }, true);
                GetViewport().PushInput(new InputEventKey { Keycode = key, PhysicalKeycode = key,
                    Pressed = false, CtrlPressed = control }, true);
                await InputFrames();
            }
            void Ground() => core.ChangeScene(Scenes.Earth_Ground,
                new System.Collections.Generic.List<SceneVariables> { SceneVariables.Ground });
            Ground();
            await InputFrames();
            await KeyPress(Key.R);
            Equal(Scenes.Earth_Research, core.currentScene, "default Research key navigates");
            await KeyPress(Key.F);
            Equal(Scenes.Production, core.currentScene, "default Production key navigates");
            Equal(true, core.SceneVariables.Contains(SceneVariables.Ground), "ground production context retained");
            Ground();
            await InputFrames();
            await KeyPress(Key.R, echo: true);
            await KeyPress(Key.R, control: true);
            Equal(Scenes.Earth_Ground, core.currentScene, "echo and modified key do not navigate");
            settings.SetSetting("keybinds/research", (long)Key.T);
            settings.ApplyKeybinds();
            await KeyPress(Key.R);
            Equal(Scenes.Earth_Ground, core.currentScene, "old binding stops working");
            await KeyPress(Key.T);
            Equal(Scenes.Earth_Research, core.currentScene, "new binding navigates");
            Ground();
            await InputFrames();
            var cursor = core.GetNode<Deuteros.Code.Utility.GlobalInput>("GameContainer/GameViewport/VirtualCursorView");
            cursor.LockToRect(new Rect2(0, 0, 320, 200));
            await KeyPress(Key.T);
            Equal(Scenes.Earth_Ground, core.currentScene, "cursor modal owns input");
            cursor.Unlock();
            var entry = new LineEdit();
            core.GetNode<SubViewport>("GameContainer/GameViewport").AddChild(entry);
            entry.GrabFocus();
            await InputFrames();
            await KeyPress(Key.T);
            Equal("t", entry.Text, "focused text entry receives shortcut character");
            Equal(Scenes.Earth_Ground, core.currentScene, "text entry does not navigate");
            entry.QueueFree();
            await InputFrames();
            OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Settings/SettingsScreen.tscn"), true, true);
            await InputFrames();
            await KeyPress(Key.T);
            Equal(Scenes.Earth_Ground, core.currentScene, "settings owns keyboard input");
            OverlayManager.Instance.CloseOverlay();
            await InputFrames();
            GameCore.Earth.Station.BuildParts = 8;
            core.ChangeScene(Scenes.Station, new System.Collections.Generic.List<SceneVariables> { SceneVariables.Orbit });
            await InputFrames();
            await KeyPress(Key.T);
            Equal(Scenes.Station, core.currentScene, "Research unavailable in orbit");
            await KeyPress(Key.F);
            Equal(Scenes.Production, core.currentScene, "orbital Production key navigates");
            Equal(true, core.SceneVariables.Contains(SceneVariables.Orbit), "orbital production context retained");
            settings.SetSetting("keybinds/research", (long)Key.R);
            settings.ApplyKeybinds();
        }

        private async Task SettingsDisplay()
        {
            await WithSettings(async screen =>
            {
                screen.WindowModeRow.StepValue(1);
                screen.VSyncRow.StepValue(-1);
                Press(screen, "%ApplyButton");
                var config = GameCore.SingletonInstance.Config;
                using (var disk = new GameConfig(config.FilePath))
                {
                    Equal(SettingsManager.BorderlessMode, disk.GetValue("display", "window_mode").AsString(), "window mode survives reload");
                    Equal(false, disk.GetValue("display", "vsync").AsBool(), "vsync survives reload");
                }
                Press(screen, "%RestoreButton");
                Press(screen, "%ApplyButton");
                using var defaults = new GameConfig(config.FilePath);
                Equal(SettingsManager.WindowedMode, defaults.GetValue("display", "window_mode").AsString(), "default window mode persisted");
                Equal(true, defaults.GetValue("display", "vsync").AsBool(), "default vsync persisted");
                Press(screen, "%CloseButton");
                await InputFrames();
                Equal(false, GetTree().Paused, "display changes do not trap pause");
            });
        }

        private async Task SettingsFullCrew()
        {
            await WithSettings(async screen =>
            {
                var staff = GameCore.Earth.PlanetResources.Staff;
                for (var i = 0; i < staff.Length; i++)
                    staff[i] = new Staff { Type = StaffType.Production, Count = 5, Leader = "Builder" + i };
                var before = Deuteros.Code.Utility.SaveStorage.Serialize(Save);
                screen.DebugTab.ButtonPressed = true;
                Press(screen.SkipToShuttlesRow, "%ActionButton");
                Press(screen, "%ConfirmApplyButton");
                Equal(before, Deuteros.Code.Utility.SaveStorage.Serialize(Save), "failed preflight preserves entire world");
                Equal("Free a crew slot first.", screen.StatusLabel.Text, "actionable crew feedback");
                Equal(true, OverlayManager.Instance.IsOpen, "failed preset keeps settings available");
                await InputFrames();
            });
        }

        private async Task SettingsMtxFullCrew()
        {
            await WithSettings(async screen =>
            {
                var core = GameCore.SingletonInstance;
                core.SetProcess(false);
                var earth = GameCore.Earth;
                for (var i = 0; i < earth.PlanetResources.Staff.Length; i++)
                    earth.PlanetResources.Staff[i] = new Staff { Type = StaffType.Production, Count = 5, Leader = "Builder" + i };
                var before = Deuteros.Code.Utility.SaveStorage.Serialize(Save);
                ConfirmSettingsPreset(screen, 4);
                Equal(true, before == Deuteros.Code.Utility.SaveStorage.Serialize(Save), "indirect setup rejects full roster before changing the world");
                Equal("Free a crew slot first.", screen.StatusLabel.Text, "MTX gives crew capacity feedback");
                Equal(true, OverlayManager.Instance.IsOpen, "rejected preset keeps settings available");

                // Seven existing parts bypass shuttle setup: this path needs no extra staff slot.
                earth.Station.BuildParts = 7;
                Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver); // Keep this capacity check out of the independent bulletin renderer.
                var nameIndex = Save.NextPersonIndex;
                ConfirmSettingsPreset(screen, 4);
                await InputFrames();
                Equal(false, OverlayManager.Instance.IsOpen, "safe MTX setup proceeds with a full roster");
                Equal(true, earth.Station.Built && earth.Station.MtxInstalled, "existing station receives MTX");
                Equal(nameIndex, Save.NextPersonIndex, "safe setup does not allocate a discarded crew name");
                for (var i = 0; i < earth.PlanetResources.Staff.Length; i++)
                    Equal("Builder" + i, earth.PlanetResources.Staff[i].Leader, "existing teams remain unchanged");
            });
        }

        private void SettingsInvalidValues()
        {
            var path = "user://settings-test-" + Guid.NewGuid().ToString("N") + ".cfg";
            try
            {
                using var config = new GameConfig(path);
                config.SetValue("audio", "enabled", "invalid");
                config.SetValue("audio", "volume", 999);
                config.SetValue("display", "scale", -1);
                config.SetValue("display", "fullscreen", 42);
                Equal(true, config.SoundEnabled, "invalid sound type defaults");
                Equal(100, config.Volume, "volume clamped");
                Equal(2, config.WindowScale, "scale clamped");
                Equal(false, config.Fullscreen, "invalid fullscreen type defaults");
            }
            finally { File.Delete(ProjectSettings.GlobalizePath(path)); }
        }

        private async Task SettingsModernLifecycle()
        {
            await WithSettings(async screen =>
            {
                var manager = SettingsManager.Instance;
                var config = manager.Config;
                var path = config.FilePath;
                config.SetValue("audio", "enabled", false);
                config.SetValue("audio", "volume", 35);
                config.SetValue("display", "scale", 4);
                config.SetValue("display", "fullscreen", true);
                config.Load();
                Equal(0, manager.GetSetting("audio/master_volume").AsInt32(), "legacy mute migrates");
                Equal("1280 x 800", manager.GetSetting("display/resolution").AsString(), "legacy scale migrates");
                Equal(SettingsManager.BorderlessMode, manager.GetSetting("display/window_mode").AsString(), "legacy fullscreen migrates");
                manager.SetSetting("audio/master_volume", 999);
                Equal(10, manager.GetSetting("audio/master_volume").AsInt32(), "modern volume clamped");
                manager.SetSetting("audio/master_volume", "invalid");
                Equal(8, manager.GetSetting("audio/master_volume").AsInt32(), "invalid modern volume defaults");
                manager.SetSetting("display/vsync", 42);
                Equal(true, manager.GetSetting("display/vsync").AsBool(), "invalid boolean defaults");
                manager.SetSetting("modern/bulletin_skip", "invalid");
                Equal(false, manager.GetSetting("modern/bulletin_skip", false).AsBool(), "invalid skip defaults");
                File.Delete(ProjectSettings.GlobalizePath(path));
                manager.Revert();
                Equal(8, manager.GetSetting("audio/master_volume").AsInt32(), "revert clears deleted settings");
                screen.ControlsTab.ButtonPressed = true;
                screen.PauseKeyRow.RebindButton.ButtonPressed = true;
                foreach (var pressed in new[] { true, false })
                    GetViewport().PushInput(new InputEventKey { Keycode = Key.F, Pressed = pressed }, true);
                await InputFrames();
                Equal((long)Key.F, screen.PauseKeyRow.SettingValue.AsInt64(), "rebinding receives the physical key");
                Equal((long)Key.P, screen.ProductionKeyRow.SettingValue.AsInt64(), "conflicting binding swaps to the previous key");
                Equal(Key.F, ((InputEventKey)InputMap.ActionGetEvents("pause")[0]).Keycode, "preview updates the input map");
                screen.AudioTab.ButtonPressed = true;
                screen.MasterVolumeRow.ValueSlider.Value = 2;
                foreach (var pressed in new[] { true, false })
                    GetViewport().PushInput(new InputEventKey { Keycode = Key.Escape, Pressed = pressed }, true);
                await InputFrames();
                Equal(true, screen.ConfirmOverlay.Visible, "Escape keeps unsaved settings until a decision");
                config.FilePath = path + "/missing/settings.cfg";
                Press(screen, "%ConfirmApplyButton");
                Equal(true, OverlayManager.Instance.IsOpen, "failed save keeps settings open");
                Equal(true, screen.StatusLabel.Text.StartsWith("Could not save settings:"), "save failure is visible");
                Equal(false, screen.ApplyButton.Disabled, "failed save remains pending");
                config.FilePath = path;
                Press(screen, "%CancelButton");
                await InputFrames();
                Equal(8, manager.GetSetting("audio/master_volume").AsInt32(), "discard restores default audio");
                Equal(Key.P, ((InputEventKey)InputMap.ActionGetEvents("pause")[0]).Keycode, "discard restores the original key map");
                Equal(false, OverlayManager.Instance.IsOpen, "discard closes settings");
            });
        }

        private Settings OpenSettings() => (Settings)OverlayManager.Instance.ShowOverlay(
            GD.Load<PackedScene>("res://Screens/Settings/SettingsScreen.tscn"), true, true);

        private async Task WithSettings(Func<Settings, Task> test)
        {
            InitializeUi();
            var config = GameCore.SingletonInstance.Config;
            var originalPath = config.FilePath;
            var path = "user://settings-test-" + Guid.NewGuid().ToString("N") + ".cfg";
            config.FilePath = path;
            config.Load();
            var master = AudioServer.GetBusIndex("Master");
            var muted = AudioServer.IsBusMute(master);
            var volume = AudioServer.GetBusVolumeDb(master);
            var oldInfinite = GameCore.SingletonInstance.InfiniteResources;
            SettingsManager.Instance.ApplyAll();
            Settings screen = null;
            try
            {
                screen = OpenSettings();
                Equal(true, GetTree().Paused, "settings pause the game");
                await test(screen);
            }
            finally
            {
                OverlayManager.Instance.CloseOverlay();
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                config.FilePath = originalPath;
                config.Load();
                SettingsManager.Instance.ApplyAll();
                GameCore.SingletonInstance.InfiniteResources = oldInfinite;
                AudioServer.SetBusMute(master, muted);
                AudioServer.SetBusVolumeDb(master, volume);
                var file = ProjectSettings.GlobalizePath(path);
                if (File.Exists(file)) File.Delete(file);
            }
        }

        private async Task SettingsAudio()
        {
            await WithSettings(async screen =>
            {
                screen.AudioTab.ButtonPressed = true;
                screen.MasterVolumeRow.ValueSlider.Value = 0;
                Equal(true, AudioServer.IsBusMute(AudioServer.GetBusIndex("Master")), "sound preview applies immediately");
                screen.MusicVolumeRow.ValueSlider.Value = 3;
                var path = GameCore.SingletonInstance.Config.FilePath;
                Equal(false, File.Exists(ProjectSettings.GlobalizePath(path)), "preview does not save before Apply");
                Press(screen, "%ApplyButton");
                using var disk = new ConfigFile();
                Equal(Godot.Error.Ok, disk.Load(path), "settings file written");
                Equal(0, disk.GetValue("audio", "master_volume").AsInt32(), "mute persisted");
                Equal(3, disk.GetValue("audio", "music_volume").AsInt32(), "volume persisted");
                Press(screen, "%CloseButton");
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Equal(false, GetTree().Paused, "close restores pause state");
                screen = OpenSettings();
                Equal(0, screen.MasterVolumeRow.SettingValue.AsInt32(), "reopened mute state");
                Equal(3, screen.MusicVolumeRow.SettingValue.AsInt32(), "reopened music volume");
                screen.AudioTab.ButtonPressed = true;
                await CaptureDisplayEvidence("modern-settings-audio");

            });
        }

        private async Task SettingsCheats()
        {
            await WithSettings(async screen =>
            {
                Equal(true, screen.DebugTab.Visible, "separate debug page exists");
                screen.DebugTab.ButtonPressed = true;
                Equal(true, screen.DebugList.Visible, "cheat controls shown");
                screen.InfiniteResourcesRow.StepValue(1);
                Equal(true, GameCore.SingletonInstance.InfiniteResources, "cheat enabled");
                screen.InfiniteResourcesRow.StepValue(-1);
                Equal(false, GameCore.SingletonInstance.InfiniteResources, "cheat disabled");
                var originalShips = Save.Ships.Count;
                Press(screen.SkipToShuttlesRow, "%ActionButton");
                Equal(originalShips, Save.Ships.Count, "preset waits for confirmation");
                await CaptureDisplayEvidence("modern-settings-preset-confirmation");
                Press(screen, "%ConfirmKeepButton");
                Equal(originalShips, Save.Ships.Count, "cancel leaves progression unchanged");
                Press(screen.SkipToShuttlesRow, "%ActionButton");
                Press(screen, "%ConfirmApplyButton");
                Equal(originalShips + 1, Save.Ships.Count, "confirmed shuttle preset applied");
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            });
        }
    }
}
