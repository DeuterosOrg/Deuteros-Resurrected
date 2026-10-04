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
        }

        private async Task SettingsDisplay()
        {
            await WithSettings(async screen =>
            {
                var scale = screen.GetNode<OptionButton>("Preferences/WindowScale");
                scale.Select(2);
                scale.EmitSignal(OptionButton.SignalName.ItemSelected, 2L);
                Press(screen, "Preferences/Fullscreen");
                var config = GameCore.SingletonInstance.Config;
                using (var disk = new GameConfig(config.FilePath))
                {
                    Equal(4, disk.WindowScale, "window scale survives reload");
                    Equal(true, disk.Fullscreen, "fullscreen survives reload");
                }
                Equal(true, scale.Disabled, "fullscreen disables window sizing");
                Press(screen, "Preferences/Sound");
                screen.GetNode<HSlider>("Preferences/Volume").Value = 10;
                Press(screen, "Preferences/Defaults");
                using var defaults = new GameConfig(config.FilePath);
                Equal(3, defaults.WindowScale, "default scale persisted");
                Equal(false, defaults.Fullscreen, "default window mode persisted");
                Equal(100, defaults.Volume, "default volume persisted");
                Equal(true, defaults.SoundEnabled, "default sound persisted");
                Equal(false, scale.Disabled, "window sizing restored");
                Press(screen, "Resume");
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
                Press(screen, "CheatsTab");
                Press(screen, "Cheats/ApplyPreset");
                Press(screen, "ConfirmPreset");
                Equal(before, Deuteros.Code.Utility.SaveStorage.Serialize(Save), "failed preflight preserves entire world");
                Equal("Free a crew slot first.", screen.GetNode<Label>("Status").Text, "actionable crew feedback");
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
                Equal("Free a crew slot first.", screen.GetNode<Label>("Status").Text, "MTX gives crew capacity feedback");
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

        private async Task WithSettings(Func<Settings, Task> test)
        {
            InitializeUi();
            var original = GameCore.SingletonInstance.Config;
            var path = "user://settings-test-" + Guid.NewGuid().ToString("N") + ".cfg";
            var config = new GameConfig(path);
            var master = AudioServer.GetBusIndex("Master");
            var muted = AudioServer.IsBusMute(master);
            var volume = AudioServer.GetBusVolumeDb(master);
            var oldInfinite = GameCore.SingletonInstance.InfiniteResources;
            GameCore.SingletonInstance.Config = config;
            Settings screen = null;
            try
            {
                screen = (Settings)OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Base/Settings.tscn"));
                Equal(true, GetTree().Paused, "settings pause the game");
                await test(screen);
            }
            finally
            {
                OverlayManager.Instance.CloseOverlay();
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                GameCore.SingletonInstance.Config = original;
                GameCore.SingletonInstance.InfiniteResources = oldInfinite;
                AudioServer.SetBusMute(master, muted);
                AudioServer.SetBusVolumeDb(master, volume);
                (config as IDisposable)?.Dispose();
                var file = ProjectSettings.GlobalizePath(path);
                if (File.Exists(file)) File.Delete(file);
            }
        }

        private async Task SettingsAudio()
        {
            await WithSettings(async screen =>
            {
                var sound = screen.GetNodeOrNull<Button>("Preferences/Sound");
                Equal(true, sound != null, "modern sound control exists");
                sound.EmitSignal(BaseButton.SignalName.Pressed);
                var slider = screen.GetNode<HSlider>("Preferences/Volume");
                slider.Value = 35;
                Equal(true, AudioServer.IsBusMute(AudioServer.GetBusIndex("Master")), "sound toggle applies immediately");
                var config = GameCore.SingletonInstance.Config;
                var path = config.FilePath;
                using var disk = new ConfigFile();
                Equal(Godot.Error.Ok, disk.Load(path), "settings file written");
                Equal(false, (bool)disk.GetValue("audio", "enabled"), "sound preference persisted");
                Equal(35, (int)disk.GetValue("audio", "volume"), "volume persisted");
                Press(screen, "Resume");
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Equal(false, GetTree().Paused, "resume restores pause state");
                screen = (Settings)OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Base/Settings.tscn"));
                Equal("Off", screen.GetNode<Button>("Preferences/Sound").Text, "reopened sound state");
                Equal(35d, screen.GetNode<HSlider>("Preferences/Volume").Value, "reopened volume");
            });
        }

        private async Task SettingsCheats()
        {
            await WithSettings(async screen =>
            {
                Equal(true, screen.GetNodeOrNull<Button>("CheatsTab") != null, "separate cheats page exists");
                Press(screen, "CheatsTab");
                Equal(true, screen.GetNode<Control>("Cheats").Visible, "cheat controls shown");
                Press(screen, "Cheats/InfiniteResources");
                Equal(true, GameCore.SingletonInstance.InfiniteResources, "cheat enabled");
                Press(screen, "Cheats/InfiniteResources");
                Equal(false, GameCore.SingletonInstance.InfiniteResources, "cheat disabled");
                var originalShips = Save.Ships.Count;
                Press(screen, "Cheats/ApplyPreset");
                Equal(originalShips, Save.Ships.Count, "preset waits for confirmation");
                Press(screen, "CancelPreset");
                Equal(originalShips, Save.Ships.Count, "cancel leaves progression unchanged");
                Press(screen, "Cheats/ApplyPreset");
                Press(screen, "ConfirmPreset");
                Equal(originalShips + 1, Save.Ships.Count, "confirmed shuttle preset applied");
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            });
        }
    }
}
