using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private bool closeWindowAfterTests;

        private async Task RunAmbienceRegressions()
        {
            foreach (var scene in new[] { Scenes.Store, Scenes.ShipBay })
                foreach (var ground in new[] { true, false })
                    await CheckAsync($"{scene} {(ground ? "ground" : "orbit")} ambience plays once and stops on scene exit",
                        () => SceneAmbience(scene, ground));
            await CheckAsync("Stores ambience continues across MTX toggles and obeys sound preferences", StoresAmbiencePreferences);
            await CheckAsync("Window close drains scene audio after navigation with settings open", AmbienceWindowClose);
        }

        private void OpenAmbienceScene(Scenes scene, bool ground)
        {
            InitializeUi();
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.Earth.ActiveMethanoid = false;
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            GameCore.SingletonInstance.ChangeScene(scene, new List<SceneVariables>
            {
                ground ? SceneVariables.Ground : SceneVariables.Orbit,
                ground ? SceneVariables.Shuttle : SceneVariables.Ship
            });
        }

        private AudioStreamPlayer AmbiencePlayer(Node screen, string track)
        {
            var player = screen.GetNodeOrNull<AudioStreamPlayer>("SoundController/SoundPlayer");
            Equal(true, player != null, "screen has its ambience player");
            Equal("res://Sounds/Background/" + track + ".ogg", player.Stream.ResourcePath, "supplied track selected");
            Equal(true, player.Stream is AudioStreamOggVorbis ogg && ogg.Loop, "ambience loops");
            Equal(true, player.Playing, "ambience is playing");
            Equal("Music", player.Bus.ToString(), "ambience follows music preferences");
            Equal("Game", AudioServer.GetBusSend(AudioServer.GetBusIndex("Music")).ToString(), "music follows game sound priority");
            Equal("Master", AudioServer.GetBusSend(AudioServer.GetBusIndex("Game")).ToString(), "game sounds inherit master preferences");
            Equal(1, GameCore.SingletonInstance.GetNode("GameContainer/GameViewport/MainScene").FindChildren("SoundPlayer", "AudioStreamPlayer", true, false)
                .OfType<AudioStreamPlayer>().Count(p => p.Playing), "one active ambience player");
            return player;
        }

        private async Task SceneAmbience(Scenes scene, bool ground)
        {
            OpenAmbienceScene(scene, ground);
            await InputFrames();
            var screen = scene == Scenes.Store ? (Node)ActiveScreen<Store>() : ActiveScreen<ShipBay>();
            AudioStreamPlayer player = null;
            try
            {
                player = AmbiencePlayer(screen, scene == Scenes.Store ? "Store" : "ShuttleBay");
                var parent = screen.GetParent();
                parent.RemoveChild(screen);
                Equal(false, player.Playing, "leaving the tree stops playback immediately");
            }
            finally
            {
                screen.Free();
                await DrainStoppedAudio();
            }
            Equal(false, GodotObject.IsInstanceValid(player), "screen owns and frees its audio player");
        }

        private async Task StoresAmbiencePreferences()
        {
            OpenAmbienceScene(Scenes.Store, false);
            GameCore.Earth.Station.MtxInstalled = true;
            Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver);
            GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
            var store = ActiveScreen<Store>();
            await InputFrames();
            try
            {
                var player = AmbiencePlayer(store, "Store");
                var playback = player.GetInstanceId();
                for (var i = 0; i < 4; i++)
                {
                    store.SwitchStoreType.EmitSignal(BaseButton.SignalName.ButtonUp);
                    await InputFrames();
                    Equal(i % 2 == 0, store.MTX.Visible, "toggle enters and leaves MTX");
                    Equal(playback, AmbiencePlayer(store, "Store").GetInstanceId(), "toggle retains the same audio player");
                }
                await WithSettings(async settings =>
                {
                    settings.MasterVolumeRow.ValueSlider.Value = 0;
                    Equal(true, AudioServer.IsBusMute(AudioServer.GetBusIndex("Master")), "settings mute reaches ambience through Master");
                    settings.MasterVolumeRow.ValueSlider.Value = 3;
                    Equal(true, AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")) < 0, "settings volume reaches ambience through Master");
                    Equal(false, AudioServer.IsBusMute(AudioServer.GetBusIndex("Master")), "unmuting restores ambience through Master");
                    Press(settings, "%ApplyButton");
                    Press(settings, "%CloseButton");
                    await InputFrames();
                    Equal(true, player.Playing, "ambience resumes after settings");
                });
            }
            finally
            {
                GameCore.SingletonInstance.ChangeScene(Scenes.Overview, new List<SceneVariables>());
                await InputFrames();
                await DrainStoppedAudio();
            }
        }

        private async Task AmbienceWindowClose()
        {
            var core = GameCore.SingletonInstance;
            Equal(false, GetTree().AutoAcceptQuit, "window close uses the game shutdown path");
            for (var i = 0; i < 3; i++)
            {
                OpenAmbienceScene(Scenes.Store, false);
                await InputFrames();
                var storePlayer = AmbiencePlayer(ActiveScreen<Store>(), "Store");
                OpenAmbienceScene(Scenes.ShipBay, false);
                await InputFrames();
                Equal(false, GodotObject.IsInstanceValid(storePlayer), "navigation removes previous ambience");
                AmbiencePlayer(ActiveScreen<ShipBay>(), "ShuttleBay");
                core.ChangeScene(Scenes.Earth_Training, new List<SceneVariables>());
                await InputFrames();
            }
            Deuteros.Code.Platform.Helpers.OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Settings/SettingsScreen.tscn"), true, true);
            Equal(true, GetTree().Paused, "shutdown begins with paused settings open");
            // The runner emits the real close notification after reporting the
            // assertion result; the external validator requires a clean exit.
            closeWindowAfterTests = true;
        }
    }
}
