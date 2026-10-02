using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Platform;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;
using MenuControl = Deuteros.Code.Platform.MenuButton;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunMenuSoundRegressions()
        {
            await CheckAsync("Menu navigation plays the supplied cue through the persistent player", MenuNavigationSound);
            await CheckAsync("Side-menu sound survives actions and a change to disabled menu context", MenuContextSound);
            await CheckAsync("Disabled hidden and empty menu controls remain silent", MenuUnavailableSound);
            await CheckAsync("Cancelled pointer presses and right clicks do not play menu cues", MenuCancelledSound);
            await CheckAsync("Keyboard and controller menu activation share the click cue", MenuKeyboardSound);
            await CheckAsync("Time toggle and hold play once per activation without release or tick cues", MenuTimeSound);
            await CheckAsync("Menu sound obeys pointer blocking paused settings and master mute", MenuBlockedSound);
            await CheckAsync("Menu refresh and tree re-entry do not accumulate sound connections", MenuSoundConnections);
        }

        private async Task WithMenuSound(Func<MainMenu, SubViewport, AudioStreamPlayer, Task> test)
        {
            InitializeUi();
            var menu = ActiveScreen<MainMenu>();
            var player = menu.GetNodeOrNull<AudioStreamPlayer>("MenuClickSound");
            Equal(true, player != null, "persistent menu has its click player");
            var parent = menu.GetParent();
            var viewport = new SubViewport { Size = new Vector2I(320, 200), GuiDisableInput = false };
            AddChild(viewport);
            menu.Reparent(viewport);
            try
            {
                await InputFrames();
                await test(menu, viewport, player);
            }
            finally
            {
                Save.TimeSkip = Save.TimeSkipDay = false;
                GetTree().Paused = false;
                OverlayManager.Instance.CloseOverlay();
                menu.Reparent(parent);
                viewport.Free();
                player.Stop();
                await DrainStoppedAudio();
            }
        }

        private void MenuPointer(SubViewport viewport, Vector2 point, bool? pressed = null, MouseButton button = MouseButton.Left)
        {
            if (pressed == null)
            {
                var motion = new InputEventMouseMotion { Position = point, GlobalPosition = point };
                viewport.PushInput(motion, true);
            }
            else
            {
                var click = new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = button, Pressed = pressed.Value };
                viewport.PushInput(click, true);
            }
        }

        private void ClickMenu(SubViewport viewport, BaseButton button)
        {
            var point = button.GetGlobalRect().GetCenter();
            MenuPointer(viewport, point);
            MenuPointer(viewport, point, true);
            MenuPointer(viewport, point, false);
        }

        private async Task WaitMenuCue(AudioStreamPlayer player, Func<int> finished, int expected)
        {
            var deadline = Time.GetTicksMsec() + 2000;
            while (finished() < expected && Time.GetTicksMsec() < deadline)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(expected, finished(), "one completed cue for each accepted activation");
            Equal(false, player.Playing, "short cue finishes without looping");
        }

        private async Task MenuNavigationSound() => await WithMenuSound(async (menu, viewport, player) =>
        {
            Equal("res://Sounds/Button/sMainMenu_Button.wav", player.Stream.ResourcePath, "existing supplied sample");
            Equal(AudioStreamWav.LoopModeEnum.Disabled, ((AudioStreamWav)player.Stream).LoopMode, "click never loops");
            Equal(1, player.MaxPolyphony, "rapid activations cannot pile up voices");
            var playerId = player.GetInstanceId();
            var finished = 0;
            player.Finished += () => finished++;
            // Simulate synchronous scene loading that outlasts this 87 ms cue.
            menu.NewsButton.Pressed += () => System.Threading.Thread.Sleep(200);
            ClickMenu(viewport, menu.NewsButton);
            Equal(Scenes.News, GameCore.SingletonInstance.currentScene, "actual pointer navigates");
            await WaitMenuCue(player, () => finished, 1);
            ClickMenu(viewport, menu.SaveButton);
            Equal(Scenes.SaveScreen, GameCore.SingletonInstance.currentScene, "second navigation succeeds");
            Equal(playerId, menu.GetNode<AudioStreamPlayer>("MenuClickSound").GetInstanceId(), "scene changes retain the player");
            await WaitMenuCue(player, () => finished, 2);
        });

        private async Task MenuContextSound() => await WithMenuSound(async (menu, viewport, player) =>
        {
            var button = menu.GetNode<MenuControl>("MainButtons/A1");
            var actions = 0;
            button.TargetScene = Scenes.Overview;
            button.ClickActions = new List<Action> { () => actions++ };
            var finished = 0;
            player.Finished += () => finished++;
            ClickMenu(viewport, button);
            Equal(1, actions, "side action executes once");
            Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "side button navigates");
            Equal(true, button.Disabled, "new context disables the previous button");
            await WaitMenuCue(player, () => finished, 1);
        });

        private async Task MenuUnavailableSound() => await WithMenuSound(async (menu, viewport, player) =>
        {
            var button = menu.NewsButton;
            button.Disabled = true;
            ClickMenu(viewport, button);
            Equal(false, player.Playing, "disabled pointer input is silent");
            button.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(false, player.Playing, "retained signal cannot sound a disabled control");
            button.Disabled = false;
            button.TargetScene = Scenes.None;
            ClickMenu(viewport, button);
            Equal(false, player.Playing, "enabled but empty destination is silent");
            button.Hide();
            button.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(false, player.Playing, "hidden control is silent");
            await InputFrames();
        });

        private async Task MenuCancelledSound() => await WithMenuSound(async (menu, viewport, player) =>
        {
            var point = menu.NewsButton.GetGlobalRect().GetCenter();
            MenuPointer(viewport, point);
            MenuPointer(viewport, point, true);
            Equal(false, player.Playing, "navigation waits for accepted release");
            var outside = new Vector2(300, 180);
            MenuPointer(viewport, outside);
            MenuPointer(viewport, outside, false);
            Equal(false, player.Playing, "release outside cancels cue");
            Equal(Scenes.SaveScreen, GameCore.SingletonInstance.currentScene, "cancelled press does not navigate");
            MenuPointer(viewport, point, true, MouseButton.Right);
            MenuPointer(viewport, point, false, MouseButton.Right);
            Equal(false, player.Playing, "right click does not activate menu sound");
            await InputFrames();
        });

        private async Task MenuKeyboardSound() => await WithMenuSound(async (menu, viewport, player) =>
        {
            var finished = 0;
            player.Finished += () => finished++;
            menu.NewsButton.GrabFocus();
            foreach (var pressed in new[] { true, false })
            {
                using var key = new InputEventKey { Keycode = Key.Enter, Pressed = pressed };
                viewport.PushInput(key, true);
            }
            Equal(Scenes.News, GameCore.SingletonInstance.currentScene, "keyboard activates focused menu");
            await WaitMenuCue(player, () => finished, 1);
            menu.SaveButton.GrabFocus();
            foreach (var pressed in new[] { true, false })
            {
                using var action = new InputEventAction { Action = "ui_accept", Pressed = pressed };
                viewport.PushInput(action, true);
            }
            Equal(Scenes.SaveScreen, GameCore.SingletonInstance.currentScene, "controller accept activates focused menu");
            await WaitMenuCue(player, () => finished, 2);
        });

        private async Task MenuTimeSound() => await WithMenuSound(async (menu, viewport, player) =>
        {
            var finished = 0;
            player.Finished += () => finished++;
            ClickMenu(viewport, menu.TimeButton);
            Equal(true, Save.TimeSkip, "toggle starts time");
            await WaitMenuCue(player, () => finished, 1);
            ClickMenu(viewport, menu.TimeButton);
            Equal(false, Save.TimeSkip, "toggle stops time");
            await WaitMenuCue(player, () => finished, 2);
            var hold = menu.GetNode<BaseButton>("Time/TimeBox/TimerHoldButton");
            var point = hold.GetGlobalRect().GetCenter();
            MenuPointer(viewport, point);
            MenuPointer(viewport, point, true);
            Equal(true, Save.TimeSkip, "hold starts time");
            await WaitMenuCue(player, () => finished, 3);
            for (var day = 0; day < 3; day++) menu.DayTick((uint)day, (uint)day + 1);
            MenuPointer(viewport, point, false);
            await InputFrames();
            Equal(3, finished, "day updates and release do not replay the cue");
            Equal(false, player.Playing, "release does not start a second cue");
        });

        private async Task MenuBlockedSound() => await WithMenuSound(async (menu, viewport, player) =>
        {
            var blocker = new InputBlocker();
            viewport.AddChild(blocker);
            blocker.SetBlocked(true);
            ClickMenu(viewport, menu.NewsButton);
            Equal(false, player.Playing, "blocked pointer is silent");
            Equal(Scenes.SaveScreen, GameCore.SingletonInstance.currentScene, "blocked pointer does not navigate");
            blocker.Free();
            GetTree().Paused = true;
            ClickMenu(viewport, menu.NewsButton);
            Equal(false, player.Playing, "paused menu is silent");
            GetTree().Paused = false;
            await WithSettings(async settings =>
            {
                Press(settings, "Preferences/Sound");
                Equal(true, AudioServer.IsBusMute(AudioServer.GetBusIndex("Master")), "menu inherits muted Master bus");
                Press(settings, "Resume");
                await InputFrames();
                var finished = 0;
                player.Finished += () => finished++;
                ClickMenu(viewport, menu.NewsButton);
                Equal(Scenes.News, GameCore.SingletonInstance.currentScene, "mute does not disable navigation");
                await WaitMenuCue(player, () => finished, 1);
            });
        });

        private async Task MenuSoundConnections() => await WithMenuSound(async (menu, viewport, player) =>
        {
            var connections = menu.NewsButton.GetSignalConnectionList(BaseButton.SignalName.Pressed).Count;
            for (var i = 0; i < 3; i++)
            {
                menu.SetupMenus();
                viewport.RemoveChild(menu);
                viewport.AddChild(menu);
            }
            Equal(connections, menu.NewsButton.GetSignalConnectionList(BaseButton.SignalName.Pressed).Count, "tree re-entry keeps one set of handlers");
            Equal(1, menu.FindChildren("MenuClickSound", "AudioStreamPlayer", true, false).Count, "one persistent cue player");
            var finished = 0;
            player.Finished += () => finished++;
            ClickMenu(viewport, menu.NewsButton);
            await WaitMenuCue(player, () => finished, 1);
        });
    }
}
