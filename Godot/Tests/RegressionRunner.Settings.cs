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

        private async Task PushGameKey(Key key, bool echo = false, bool control = false, bool shift = false)
        {
            GetViewport().PushInput(new InputEventKey { Keycode = key, PhysicalKeycode = key,
                Unicode = (uint)char.ToLowerInvariant((char)key),
                Pressed = true, Echo = echo, CtrlPressed = control, ShiftPressed = shift }, true);
            GetViewport().PushInput(new InputEventKey { Keycode = key, PhysicalKeycode = key,
                Pressed = false, CtrlPressed = control, ShiftPressed = shift }, true);
            await InputFrames();
        }

        private async Task SettingsNavigationShortcuts()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            var settings = SettingsManager.Instance;
            void Ground() => core.ChangeScene(Scenes.Earth_Ground,
                new System.Collections.Generic.List<SceneVariables> { SceneVariables.Ground });
            Ground();
            await InputFrames();
            await PushGameKey(Key.R);
            Equal(Scenes.Earth_Research, core.currentScene, "default Research key navigates");
            await PushGameKey(Key.F);
            Equal(Scenes.Production, core.currentScene, "default Production key navigates");
            Equal(true, core.SceneVariables.Contains(SceneVariables.Ground), "ground production context retained");
            Ground();
            await InputFrames();
            await PushGameKey(Key.R, echo: true);
            await PushGameKey(Key.R, control: true);
            Equal(Scenes.Earth_Ground, core.currentScene, "echo and modified key do not navigate");
            settings.SetSetting("keybinds/research", (long)Key.T);
            settings.ApplyKeybinds();
            await PushGameKey(Key.R);
            Equal(Scenes.Earth_Ground, core.currentScene, "old binding stops working");
            await PushGameKey(Key.T);
            Equal(Scenes.Earth_Research, core.currentScene, "new binding navigates");
            Ground();
            await InputFrames();
            var cursor = core.GetNode<Deuteros.Code.Utility.GlobalInput>("GameContainer/GameViewport/VirtualCursorView");
            cursor.LockToRect(new Rect2(0, 0, 320, 200));
            await PushGameKey(Key.T);
            Equal(Scenes.Earth_Ground, core.currentScene, "cursor modal owns input");
            cursor.Unlock();
            var entry = new LineEdit();
            core.GetNode<SubViewport>("GameContainer/GameViewport").AddChild(entry);
            entry.GrabFocus();
            await InputFrames();
            await PushGameKey(Key.T);
            Equal("t", entry.Text, "focused text entry receives shortcut character");
            Equal(Scenes.Earth_Ground, core.currentScene, "text entry does not navigate");
            entry.QueueFree();
            await InputFrames();
            OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Settings/SettingsScreen.tscn"), true, true);
            await InputFrames();
            await PushGameKey(Key.T);
            Equal(Scenes.Earth_Ground, core.currentScene, "settings owns keyboard input");
            OverlayManager.Instance.CloseOverlay();
            await InputFrames();
            GameCore.Earth.Station.BuildParts = 8;
            core.ChangeScene(Scenes.Station, new System.Collections.Generic.List<SceneVariables> { SceneVariables.Orbit });
            await InputFrames();
            await PushGameKey(Key.T);
            Equal(Scenes.Station, core.currentScene, "Research unavailable in orbit");
            await PushGameKey(Key.F);
            Equal(Scenes.Production, core.currentScene, "orbital Production key navigates");
            Equal(true, core.SceneVariables.Contains(SceneVariables.Orbit), "orbital production context retained");
            settings.SetSetting("keybinds/research", (long)Key.R);
            settings.ApplyKeybinds();
        }

        private async Task SettingsSpeedShortcuts()
        {
            var menu = TimeMenu(Scenes.Earth_Ground);
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            var day = Save.CurrentDay;
            Save.Clock.NormalElapsed = 47;
            await PushGameKey(Key.Equal, shift: true);
            Equal(false, Save.TimeSkip, "Shift+Equal is not the unmodified Equal binding");
            await PushGameKey(Key.Equal);
            Equal(true, Save.TimeSkip, "Speed Up starts existing fast time");
            Equal("animated", menu.TimeAnimation.Animation.ToString(), "fast time indicator animates");
            var started = Save.TimeSkipStart;
            await PushGameKey(Key.Equal);
            Equal(true, Save.TimeSkip, "repeated Speed Up stays fast");
            Equal(started, Save.TimeSkipStart, "same speed preserves cadence");
            await PushGameKey(Key.Minus, echo: true);
            await PushGameKey(Key.Minus, control: true);
            Equal(true, Save.TimeSkip, "echo and modified Slow Down ignored");
            await PushGameKey(Key.Minus);
            Equal(false, Save.TimeSkip, "Slow Down restores normal time");
            Equal("static", menu.TimeAnimation.Animation.ToString(), "normal indicator is static");
            await PushGameKey(Key.Minus);
            Equal(false, Save.TimeSkip, "repeated Slow Down stays normal");
            var settings = SettingsManager.Instance;
            settings.SetSetting("keybinds/speed_up", (long)Key.U);
            settings.SetSetting("keybinds/slow_down", (long)Key.J);
            settings.ApplyKeybinds();
            await PushGameKey(Key.Equal);
            Equal(false, Save.TimeSkip, "old speed binding ignored");
            var cursor = core.GetNode<Deuteros.Code.Utility.GlobalInput>("GameContainer/GameViewport/VirtualCursorView");
            cursor.LockToRect(new Rect2(0, 0, 320, 200));
            await PushGameKey(Key.U);
            Equal(false, Save.TimeSkip, "modal blocks speed shortcut");
            cursor.Unlock();
            await PushGameKey(Key.U);
            Equal(true, Save.TimeSkip, "rebound Speed Up works");
            OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Settings/SettingsScreen.tscn"), true, true);
            await InputFrames();
            await PushGameKey(Key.J);
            Equal(true, Save.TimeSkip, "Settings retains ownership while fast time is paused");
            OverlayManager.Instance.CloseOverlay();
            await InputFrames();
            await PushGameKey(Key.J);
            Equal(false, Save.TimeSkip, "rebound Slow Down works after closing Settings");
            Equal(false, Save.TimeSkipDay, "speed controls do not queue a manual day");
            Equal(day, Save.CurrentDay, "shortcut dispatch does not itself advance simulation");
            Equal(47.0, Save.Clock.NormalElapsed, "normal remainder retained");
            settings.SetSetting("keybinds/speed_up", (long)Key.Equal);
            settings.SetSetting("keybinds/slow_down", (long)Key.Minus);
            settings.ApplyKeybinds();
            core.SetProcess(true);
        }

        private async Task SettingsPauseShortcut()
        {
            await WithSettings(async initial =>
            {
                var overlays = OverlayManager.Instance;
                var manager = SettingsManager.Instance;
                await PushGameKey(Key.P);
                Equal(false, overlays.IsOpen, "Pause resumes clean Settings");
                Equal(false, GetTree().Paused, "game resumes after owned pause");
                await PushGameKey(Key.P, echo: true);
                await PushGameKey(Key.P, control: true);
                Equal(false, overlays.IsOpen, "echo and modified Pause ignored");
                await PushGameKey(Key.P);
                Equal(true, overlays.IsOpen, "Pause opens existing Settings screen");
                Equal(true, GetTree().Paused, "Pause freezes game tree");
                var screen = overlays.GetNode<Settings>("GlobalOverlay/Center/SettingsScreen");
                screen.AudioTab.ButtonPressed = true;
                screen.MasterVolumeRow.ValueSlider.Value = 2;
                await PushGameKey(Key.P);
                Equal(true, screen.ConfirmOverlay.Visible, "Pause requests a decision for unsaved changes");
                Equal(true, GetTree().Paused, "confirmation remains paused");
                await PushGameKey(Key.P);
                Equal(false, screen.ConfirmOverlay.Visible, "Pause cancels confirmation without discarding");
                Equal(2, manager.GetSetting("audio/master_volume").AsInt32(), "preview remains available to apply");
                await PushGameKey(Key.P);
                Press(screen, "%ConfirmDiscardButton");
                await InputFrames();
                Equal(false, overlays.IsOpen, "explicit discard resumes");
                Equal(8, manager.GetSetting("audio/master_volume").AsInt32(), "discard restores applied preference");
                manager.SetSetting("keybinds/pause", (long)Key.O);
                manager.ApplyKeybinds();
                await PushGameKey(Key.P);
                Equal(false, overlays.IsOpen, "old Pause binding ignored");
                await PushGameKey(Key.O);
                Equal(true, GetTree().Paused, "rebound Pause opens Settings");
                screen = overlays.GetNode<Settings>("GlobalOverlay/Center/SettingsScreen");
                screen.ControlsTab.ButtonPressed = true;
                screen.PauseKeyRow.RebindButton.ButtonPressed = true;
                await PushGameKey(Key.O);
                Equal(true, overlays.IsOpen, "key capture owns the active Pause key");
                Equal(false, screen.ConfirmOverlay.Visible, "key capture does not request close");
                await PushGameKey(Key.O);
                Equal(false, overlays.IsOpen, "rebound Pause resumes after capture ends");
                var error = overlays.ShowOverlay(GD.Load<PackedScene>("res://Screens/Base/Error.tscn"), false);
                await InputFrames();
                await PushGameKey(Key.O);
                Equal(true, overlays.IsShowing(error), "Pause cannot dismiss another overlay");
                overlays.CloseOverlay();
                await InputFrames();
                GetTree().Paused = true;
                await PushGameKey(Key.O);
                Equal(true, overlays.IsOpen, "Settings can be opened over an existing pause");
                await PushGameKey(Key.O);
                Equal(false, overlays.IsOpen, "Settings closes over existing pause");
                Equal(true, GetTree().Paused, "closing Settings preserves previous pause owner");
                GetTree().Paused = false;
            });
        }

        private async Task SettingsAutoPause()
        {
            await WithSettings(async screen =>
            {
                var manager = SettingsManager.Instance;
                var overlays = OverlayManager.Instance;
                void Focus(bool focused) => manager.Notification((int)(focused
                    ? NotificationApplicationFocusIn : NotificationApplicationFocusOut));
                try
                {
                    screen.GameplayTab.ButtonPressed = true;
                    screen.AutoPauseRow.StepValue(1);
                    Press(screen, "%ApplyButton");
                    using var disk = new ConfigFile();
                    Equal(Godot.Error.Ok, disk.Load(GameCore.SingletonInstance.Config.FilePath), "Auto Pause preference written");
                    Equal(true, disk.GetValue("gameplay", "auto_pause").AsBool(), "Auto Pause On persisted");
                    Press(screen, "%CloseButton");
                    await InputFrames();
                    Focus(false);
                    Equal(true, GetTree().Paused, "Auto Pause On freezes the tree on focus loss");
                    Focus(false);
                    var day = Save.CurrentDay;
                    Save.TimeSkipDay = true;
                    await InputFrames();
                    Equal(day, Save.CurrentDay, "background pause prevents a queued simulation update");
                    Focus(true);
                    Equal(false, GetTree().Paused, "returning focus releases the owned pause");
                    await InputFrames();
                    Equal(day + 1, Save.CurrentDay, "queued update resumes exactly once");

                    // Both ownership orders must preserve the overlay until it is closed.
                    foreach (var focusFirst in new[] { true, false })
                    {
                        if (focusFirst) Focus(false);
                        screen = OpenSettings();
                        if (!focusFirst) Focus(false);
                        Focus(true);
                        Equal(true, GetTree().Paused, "focus return preserves the open Settings pause");
                        Press(screen, "%CloseButton");
                        await InputFrames();
                        Equal(false, GetTree().Paused, "last owner closes without leaving a stale pause");
                    }
                    screen = OpenSettings();
                    Focus(false);
                    Press(screen, "%CloseButton");
                    await InputFrames();
                    Equal(true, GetTree().Paused, "closing Settings while unfocused preserves Auto Pause");
                    Focus(true);
                    Equal(false, GetTree().Paused, "focus resumes after the overlay closed");
                    screen = OpenSettings();
                    Press(screen, "%CloseButton");
                    Focus(false); // The overlay still owns its deferred close frame.
                    await InputFrames();
                    Focus(true);
                    Equal(false, GetTree().Paused, "focus loss during deferred close does not capture a stale pause");
                    var error = overlays.ShowOverlay(GD.Load<PackedScene>("res://Screens/Base/Error.tscn"), false);
                    Focus(false);
                    Focus(true);
                    Equal(true, overlays.IsShowing(error) && GetTree().Paused, "focus cannot dismiss an error overlay");
                    overlays.CloseOverlay();
                    await InputFrames();
                    GetTree().Paused = true;
                    Focus(false);
                    Focus(true);
                    Equal(true, GetTree().Paused, "an existing independent pause is retained");
                    GetTree().Paused = false;

                    screen = OpenSettings();
                    Equal(true, screen.AutoPauseRow.Value, "applied On survives reopening");
                    screen.GameplayTab.ButtonPressed = true;
                    screen.AutoPauseRow.StepValue(-1);
                    Press(screen, "%CancelButton");
                    await InputFrames();
                    Focus(false);
                    Equal(true, GetTree().Paused, "Cancel restores applied Auto Pause On");
                    Focus(true);
                    screen = OpenSettings();
                    screen.AutoPauseRow.StepValue(-1);
                    Press(screen, "%ApplyButton");
                    Press(screen, "%CloseButton");
                    await InputFrames();
                    Focus(false);
                    Equal(false, GetTree().Paused, "saved Off keeps background processing enabled");
                    Focus(true);
                    manager.SetSetting("gameplay/auto_pause", "invalid");
                    Focus(false);
                    Equal(false, GetTree().Paused, "malformed preference falls back to opt-in Off");
                }
                finally
                {
                    Focus(true);
                    Save.TimeSkipDay = false;
                    GetTree().Paused = overlays.IsOpen;
                }
            });
        }

        private async Task SettingsNextLocationShortcut()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            foreach (var planet in Save.BaseGameData.Planets.Values)
                planet.Station.BuildParts = 0;
            void Station(StellarBodies body, int order, bool hostile = false, bool built = true)
            {
                var planet = Save.BaseGameData.Planets[body];
                planet.ActiveMethanoid = hostile;
                planet.Station.BuildParts = built ? 8 : 7;
                planet.Station.Built = built;
                planet.Station.StationOrdinal = order;
            }
            Station(StellarBodies.earth, 0);
            Station(StellarBodies.the_moon, 1);
            Station(StellarBodies.mars, 2, hostile: true);
            Station(StellarBodies.jupiter, 3, built: false);
            Station(StellarBodies.pacific, 4);
            core.ChangeScene(Scenes.Earth_Ground, new System.Collections.Generic.List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            await PushGameKey(Key.Tab);
            Equal(StellarBodies.the_moon, Save.CurrentPlanet, "default Next Location reaches next completed friendly station");
            Equal(Scenes.Station, core.currentScene, "shortcut opens station overview");
            Equal(true, core.SceneVariables.Contains(SceneVariables.Orbit), "station menu uses orbital context");
            await PushGameKey(Key.Tab);
            Equal(StellarBodies.pacific, Save.CurrentPlanet, "hostile and unfinished stations skipped across systems");
            await PushGameKey(Key.Tab);
            Equal(StellarBodies.earth, Save.CurrentPlanet, "station cycle wraps to Earth");
            await PushGameKey(Key.Tab, echo: true);
            await PushGameKey(Key.Tab, control: true);
            Equal(StellarBodies.earth, Save.CurrentPlanet, "echo and modified key do not navigate");
            var settings = SettingsManager.Instance;
            settings.SetSetting("keybinds/next_location", (long)Key.N);
            settings.ApplyKeybinds();
            await PushGameKey(Key.Tab);
            Equal(StellarBodies.earth, Save.CurrentPlanet, "old binding no longer changes location");
            var viewport = core.GetNode<SubViewport>("GameContainer/GameViewport");
            viewport.GuiReleaseFocus();
            var entry = new LineEdit();
            viewport.AddChild(entry);
            entry.GrabFocus();
            await PushGameKey(Key.N);
            Equal("n", entry.Text, "focused text entry owns rebound character");
            Equal(StellarBodies.earth, Save.CurrentPlanet, "text entry does not navigate");
            entry.QueueFree();
            await InputFrames();
            var cursor = core.GetNode<Deuteros.Code.Utility.GlobalInput>("GameContainer/GameViewport/VirtualCursorView");
            cursor.LockToRect(new Rect2(0, 0, 320, 200));
            await PushGameKey(Key.N);
            Equal(StellarBodies.earth, Save.CurrentPlanet, "modal blocks location navigation");
            cursor.Unlock();
            OpenSettings();
            await InputFrames();
            await PushGameKey(Key.N);
            Equal(StellarBodies.earth, Save.CurrentPlanet, "Settings owns keyboard input");
            OverlayManager.Instance.CloseOverlay();
            await InputFrames();
            await PushGameKey(Key.N);
            Equal(StellarBodies.the_moon, Save.CurrentPlanet, "rebound location key navigates");
            Save.BaseGameData.Planets[StellarBodies.the_moon].ActiveMethanoid = true;
            await PushGameKey(Key.N);
            Equal(StellarBodies.earth, Save.CurrentPlanet, "lost current station falls back to first accessible station");
            Save.BaseGameData.Planets[StellarBodies.pacific].Station.BuildParts = 0;
            await PushGameKey(Key.N);
            Equal(StellarBodies.earth, Save.CurrentPlanet, "single station remains usable");
            GameCore.Earth.Station.BuildParts = 0;
            await PushGameKey(Key.N);
            Equal(StellarBodies.earth, Save.CurrentPlanet, "empty station list does not navigate or throw");
            settings.SetSetting("keybinds/next_location", (long)Key.Tab);
            settings.ApplyKeybinds();
            core.SetProcess(true);
        }

        private async Task SettingsTooltips()
        {
            await WithSettings(async screen =>
            {
                var core = GameCore.SingletonInstance;
                core.SetProcess(false);
                var manager = SettingsManager.Instance;
                var menu = ActiveRecipeScreen<Deuteros.Code.Platform.Screens.MainMenu>();
                screen.GameplayTab.ButtonPressed = true;
                screen.TooltipsRow.StepValue(1);
                menu.NewsButton.EmitSignal(Control.SignalName.MouseEntered);
                menu._Process(0);
                Equal(true, menu.HoverInfo.Text.Length > 0, "enabled tooltip describes the menu action");
                screen.TooltipsRow.StepValue(-1);
                menu._Process(0);
                Equal("", menu.HoverInfo.Text, "Tooltips Off hides the current hover readout");
                Equal(true, manager.GetDefault("gameplay/tooltips").AsBool(), "existing hover help stays enabled by default");
                Press(screen, "%ApplyButton");
                using (var disk = new GameConfig(manager.Config.FilePath))
                    Equal(false, disk.GetValue("gameplay", "tooltips", true).AsBool(), "disabled tooltips persist");
                Press(screen, "%CloseButton");
                await InputFrames();
                foreach (var scene in new[] { Scenes.Earth_Research, Scenes.Production, Scenes.Overview })
                {
                    core.ChangeScene(scene, new System.Collections.Generic.List<SceneVariables> { SceneVariables.Ground });
                    await InputFrames();
                    GameCore.HoverText = "Shared hover label";
                    menu._Process(0);
                    Equal("", menu.HoverInfo.Text, scene + " also respects the shared preference");
                }
                // Native window hover uses the OS pointer; use the existing GUI-hit-test harness.
                var viewport = new SubViewport { Size = new Vector2I(320, 200), GuiDisableInput = false };
                AddChild(viewport);
                var parent = menu.GetParent();
                menu.Reparent(viewport);
                try
                {
                    await PointAt(menu.EarthButton);
                    await PointAt(menu.NewsButton);
                    Equal("News Bulletins", GameCore.HoverText, "GUI hit testing reaches the actual hover source");
                    Equal("", menu.HoverInfo.Text, "hovered control stays hidden while Off");
                    manager.SetSetting("gameplay/tooltips", true);
                    await InputFrames();
                    Equal("News Bulletins", menu.HoverInfo.Text, "On renders the hovered control");
                    manager.SetSetting("gameplay/tooltips", false);
                }
                finally
                {
                    menu.Reparent(parent);
                    viewport.Free();
                }
                var point = menu.NewsButton.GetGlobalRect().GetCenter();
                foreach (var pressed in new[] { true, false })
                    PushGameInput(new InputEventMouseButton { Position = point, ButtonIndex = MouseButton.Left, Pressed = pressed });
                await InputFrames();
                Equal(Scenes.News, core.currentScene, "hidden tooltip leaves pointer navigation active");
                var originalStorage = core.Storage;
                var directory = Path.Combine(Path.GetTempPath(), "deuteros-tooltip-save-" + Guid.NewGuid());
                try
                {
                    core.Storage = new Deuteros.Code.Utility.SaveStorage(directory);
                    await PushGameKey(Key.F5);
                    menu._Process(0);
                    Equal("Quick saved.", menu.HoverInfo.Text, "save confirmation remains visible with tooltips off");
                    Equal(true, core.Storage.Exists(Deuteros.Code.Utility.SaveStorage.QuickSlot), "visible confirmation accompanies a real save");
                    await CaptureDisplayEvidence("tooltips-off-save-feedback");
                    menu.NewsButton.EmitSignal(Control.SignalName.MouseExited);
                    menu.MasterControlButton.EmitSignal(Control.SignalName.MouseEntered);
                    menu._Process(0);
                    Equal("", menu.HoverInfo.Text, "next hover does not inherit save-notice visibility");
                }
                finally
                {
                    core.Storage = originalStorage;
                    if (Directory.Exists(directory)) Directory.Delete(directory, true);
                }
                screen = OpenSettings();
                await InputFrames();
                Equal(false, screen.TooltipsRow.Value, "reopened row reflects saved Off");
                screen.TooltipsRow.StepValue(1);
                menu._Process(0);
                Equal(true, menu.HoverInfo.Text.Length > 0, "On preview restores current help");
                Press(screen, "%CancelButton");
                await InputFrames();
                menu._Process(0);
                Equal("", menu.HoverInfo.Text, "discard restores saved Off");
                manager.SetSetting("gameplay/tooltips", "invalid");
                menu._Process(0);
                Equal(true, menu.HoverInfo.Text.Length > 0, "malformed preference preserves default help");
                GameCore.HoverText = "";
                core.SetProcess(true);
            });
        }

        private async Task SettingsScanlines()
        {
            await WithSettings(async screen =>
            {
                var core = GameCore.SingletonInstance;
                core.SetProcess(false);
                var manager = SettingsManager.Instance;
                var game = GameViewportContainer.Instance;
                Equal(0, screen.ScanlinesRow.Value, "scanlines default to off");
                screen.ScanlinesRow.StepValue(10);
                Equal(true, game.Material is ShaderMaterial, "scanline slider updates the viewport effect");
                float Strength() => ((ShaderMaterial)game.Material).GetShaderParameter("intensity").AsSingle();
                Equal(0.5f, Strength(), "maximum scanlines preserve half the brightness");
                Press(screen, "%ApplyButton");
                using (var disk = new GameConfig(manager.Config.FilePath))
                    Equal(10, disk.GetValue("display", "scanlines").AsInt32(), "scanline intensity persists");
                Press(screen, "%CloseButton");
                await InputFrames();
                var patch = new ColorRect { Color = Colors.White, Position = new Vector2(100, 80),
                    Size = new Vector2(8, 8), MouseFilter = Control.MouseFilterEnum.Ignore };
                core.GetNode<SubViewport>("GameContainer/GameViewport").AddChild(patch);
                foreach (var scale in new[] { "Integer", "Fit" })
                {
                    manager.SetSetting("display/pixel_scaling", scale);
                    manager.ApplyDisplay();
                    if (DisplayServer.GetName() != "headless")
                    {
                        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                        using var pixels = GetViewport().GetTexture().GetImage();
                        var light = ReadGamePixel(pixels, new Vector2(104, 80.1f));
                        var dark = ReadGamePixel(pixels, new Vector2(104, 80.9f));
                        Equal(true, light.R > 0.98f && Math.Abs(dark.R - 0.5f) < 0.02f,
                            scale + " renders bright and half-dark scanline bands");
                        Equal(true, dark.R == dark.G && dark.G == dark.B, "scanlines preserve neutral colour");
                        manager.SetSetting("display/scanlines", 0);
                        manager.ApplyDisplay();
                        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                        using var off = GetViewport().GetTexture().GetImage();
                        Equal(true, ReadGamePixel(off, new Vector2(104, 80.1f)).R > 0.98f
                            && ReadGamePixel(off, new Vector2(104, 80.9f)).R > 0.98f,
                            scale + " off leaves both bands at full brightness");
                        manager.SetSetting("display/scanlines", 10);
                        manager.ApplyDisplay();
                    }
                }
                patch.QueueFree();
                await InputFrames();
                var menu = ActiveRecipeScreen<Deuteros.Code.Platform.Screens.MainMenu>();
                var point = menu.NewsButton.GetGlobalRect().GetCenter();
                foreach (var pressed in new[] { true, false })
                    PushGameInput(new InputEventMouseButton { Position = point, ButtonIndex = MouseButton.Left, Pressed = pressed });
                await InputFrames();
                Equal(Scenes.News, core.currentScene, "scanlines do not intercept pointer navigation");
                await CaptureDisplayEvidence("scanlines-fit");
                screen = OpenSettings();
                await InputFrames();
                Equal(10, screen.ScanlinesRow.Value, "reopened settings show saved intensity");
                screen.ScanlinesRow.StepValue(-10);
                Equal(0f, Strength(), "off preview removes scanlines");
                Press(screen, "%CancelButton");
                await InputFrames();
                Equal(0.5f, Strength(), "discard restores saved intensity");
                foreach (var value in new[] { -1, 100 })
                {
                    manager.SetSetting("display/scanlines", value);
                    manager.ApplyDisplay();
                    Equal(value < 0 ? 0f : 0.5f, Strength(), "external intensity is bounded");
                }
                manager.SetSetting("display/scanlines", "invalid");
                manager.ApplyDisplay();
                Equal(0f, Strength(), "malformed intensity defaults to off");
                core.SetProcess(true);
            });
        }

        private async Task SettingsPixelScaling()
        {
            await WithSettings(async screen =>
            {
                var core = GameCore.SingletonInstance;
                core.SetProcess(false);
                var manager = SettingsManager.Instance;
                GetWindow().Size = new Vector2I(1280, 720);
                await InputFrames();
                var game = GameViewportContainer.Instance;
                Equal(new Vector2(3, 3), game.Scale, "default integer scaling preserves whole pixels");
                screen.PixelScalingRow.StepValue(1);
                await InputFrames();
                Equal("Fit", screen.PixelScalingRow.SettingValue.AsString(), "pixel scaling offers a working Fit choice");
                Equal(true, Mathf.IsEqualApprox(3.6f, game.Scale.X) && game.Scale.X == game.Scale.Y,
                    "Fit fills available height without changing aspect ratio");
                Equal(new Vector2(64, 0), game.Position, "Fit centers its remaining letterbox space");
                Press(screen, "%ApplyButton");
                using (var disk = new GameConfig(manager.Config.FilePath))
                    Equal("Fit", disk.GetValue("display", "pixel_scaling").AsString(), "scaling choice survives disk reload");
                Press(screen, "%CloseButton");
                await InputFrames();
                var menu = ActiveRecipeScreen<Deuteros.Code.Platform.Screens.MainMenu>();
                var point = menu.NewsButton.GetGlobalRect().GetCenter();
                PushGameInput(new InputEventMouseMotion { Position = point });
                foreach (var pressed in new[] { true, false })
                    PushGameInput(new InputEventMouseButton { Position = point, ButtonIndex = MouseButton.Left, Pressed = pressed });
                await InputFrames();
                Equal(Scenes.News, core.currentScene, "fractionally scaled pointer reaches the visible menu button");
                await CaptureDisplayEvidence("pixel-scaling-fit");
                GameCore.ShowError(null, "Scaling check");
                await InputFrames();
                var area = OverlayManager.Instance.GetNode<Control>("GlobalOverlay/GameArea");
                Equal(game.Scale, area.Scale, "game overlay follows fractional scale");
                Equal(game.Position, area.Position, "game overlay follows letterbox offset");
                GetWindow().Size = new Vector2I(1400, 900);
                await InputFrames();
                Equal(new Vector2(4.375f, 4.375f), game.Scale, "Fit responds to window resize");
                Equal(new Vector2(0, 12), game.Position, "resized Fit stays centered on whole screen pixels");
                Equal(game.Scale, area.Scale, "open overlay follows resizing");
                GetWindow().Size = new Vector2I(1280, 720);
                await InputFrames();
                manager.SetSetting("display/pixel_scaling", "Integer");
                manager.ApplyDisplay();
                await InputFrames();
                Equal(new Vector2(3, 3), game.Scale, "integer preview returns to whole pixels");
                Equal(game.Scale, area.Scale, "open overlay follows preference changes");
                manager.Revert();
                await InputFrames();
                Equal(true, Mathf.IsEqualApprox(3.6f, game.Scale.X), "revert restores saved Fit scale");
                OverlayManager.Instance.CloseOverlay();
                await InputFrames();
                screen = OpenSettings();
                await InputFrames();
                Equal("Fit", screen.PixelScalingRow.SettingValue.AsString(), "reopened settings show applied scale");
                screen.PixelScalingRow.StepValue(1);
                Equal(new Vector2(3, 3), game.Scale, "row previews Integer immediately");
                Press(screen, "%CancelButton");
                await InputFrames();
                Equal(true, Mathf.IsEqualApprox(3.6f, game.Scale.X), "discard restores Fit rendering");
                manager.SetSetting("display/pixel_scaling", 42);
                manager.ApplyDisplay();
                Equal(new Vector2(3, 3), game.Scale, "malformed preference falls back to Integer");
                core.SetProcess(true);
            });
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
