using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunSdmAlarmRegressions()
        {
            await CheckAsync("SDM switches play the recovered stereo alarm and disarming stops it", SdmAlarmSwitches);
            await CheckAsync("SDM alarm follows local navigation and loaded station identity", SdmAlarmNavigation);
            await CheckAsync("SDM alarm honors pause mute expiry and tree exit", SdmAlarmLifetime);
            await CheckAsync("SDM mixer emits both original channels with the delayed right onset", SdmAlarmOutput);
        }

        private async Task<AudioStreamPlayer> OpenAlarmStation()
        {
            await OpenInterior(Ship_Types.IOS);
            GameCore.SingletonInstance.SetProcess(false);
            GameCore.Earth.Station.SdmInstalled = true;
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.s__d__m).Research.Locked = false;
            GameCore.SingletonInstance.UpdateMenuButtons(false, true);
            await OpenSdmControl();
            var player = ActiveScreen<MainMenu>().GetNodeOrNull<AudioStreamPlayer>("SdmAlarm");
            Equal(true, player != null, "local station has an alarm player");
            return player;
        }

        private async Task SdmAlarmSwitches()
        {
            var player = await OpenAlarmStation();
            Equal(false, player.Playing, "disarmed station is silent");
            var wav = (AudioStreamWav)player.Stream;
            Equal(true, wav.Stereo, "original channels zero and one play left and right");
            Equal(1773, wav.MixRate, "WAV retains its source sample rate");
            Equal(true, Math.Abs(wav.MixRate * player.PitchScale - 1773.4475) < 0.001, "playback corrects WAV integer rate to PAL period 2000");
            Equal(true, Math.Abs(Mathf.DbToLinear(player.VolumeDb) - 63f / 64) < 0.0001, "original channel volume retained");
            Equal(AudioStreamWav.FormatEnum.Format16Bits, wav.Format, "signed sample conversion retains all bits");
            Equal(AudioStreamWav.LoopModeEnum.Forward, wav.LoopMode, "alarm loops");
            Equal(568, wav.LoopBegin, "right channel startup delay is outside repeat loop");
            Equal(1224, wav.LoopEnd, "both channels repeat the 656 sample waveform");
            var data = wav.Data;
            Equal(1224 * 4, data.Length, "complete stereo frames");
            var original = Enumerable.Range(0, 656).Select(i => data[i * 4 + 1]).ToArray();
            Equal("6f27bb13586049f2694531ba7a8a6b2cd6676e65f261800b907e8facb069ebbc",
                Convert.ToHexString(SHA256.HashData(original)).ToLowerInvariant(), "left channel recovers exact original sample");
            for (var i = 0; i < 1224; i++)
            {
                Equal((byte)0, data[i * 4], "left conversion has no invented low bits");
                Equal(original[i % 656], data[i * 4 + 1], "left channel repeats continuously");
                Equal((byte)0, data[i * 4 + 2], "right conversion has no invented low bits");
                Equal(i < 568 ? (byte)0 : original[(i - 568) % 656], data[i * 4 + 3], "right channel delays then repeats original sample");
            }
            var control = ActiveScreen<SdmScreen>();
            Press(control, "HighSwitch");
            Press(control, "LowSwitch");
            await InputFrames();
            Equal(true, player.Playing, "arming starts alarm");
            Equal(true, AudioServer.IsBusMute(AudioServer.GetBusIndex("Game")), "alarm replaces normal game sounds");
            var finished = 0;
            player.Finished += () => finished++;
            await ToSignal(GetTree().CreateTimer(1), SceneTreeTimer.SignalName.Timeout);
            Equal(true, player.Playing, "alarm plays beyond sample length");
            Equal(0, finished, "loop never finishes during arming");
            Press(control, "LowSwitch");
            Press(control, "HighSwitch");
            await InputFrames();
            Equal(false, player.Playing, "defusing stops alarm");
            Equal(false, AudioServer.IsBusMute(AudioServer.GetBusIndex("Game")), "defusing releases normal sound priority");
        }

        private async Task SdmAlarmNavigation()
        {
            var player = await OpenAlarmStation();
            var core = GameCore.SingletonInstance;
            GameCore.Earth.Station.SdmCountdown = 12;
            await InputFrames();
            var id = player.GetInstanceId();
            core.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
            await InputFrames();
            Equal(true, player.Playing, "local Stores keeps the alarm");
            Equal(id, ActiveScreen<MainMenu>().GetNode<AudioStreamPlayer>("SdmAlarm").GetInstanceId(), "navigation reuses one player");
            foreach (var scene in new[] { Scenes.Overview, Scenes.News, Scenes.SaveScreen, Scenes.Earth_Ground })
            {
                core.ChangeScene(scene, new List<SceneVariables> { SceneVariables.Ground });
                await InputFrames();
                Equal(false, player.Playing, "global or Earth ground view is silent: " + scene);
            }
            core.ChangeScene(Scenes.Station, new List<SceneVariables> { SceneVariables.Orbit });
            await InputFrames();
            Equal(true, player.Playing, "returning to armed orbit restarts warning");
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            GameCore.Earth.Station.SdmCountdown = 0;
            await InputFrames();
            Equal(false, player.Playing, "loading a disarmed world does not retain obsolete alarm");
            GameCore.Earth.Station.SdmCountdown = 12;
            await InputFrames();
            Equal(true, player.Playing, "loaded armed world starts alarm");
            var ship = Save.Ships.First();
            core.ShipSelected = ship.ShipID;
            ship.ShipState = Ship_States.InTransit;
            core.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
            await InputFrames();
            Equal(false, player.Playing, "travelling ship does not hear origin station alarm");
            ship.ShipState = Ship_States.Docked;
            core.UpdateMenuButtons(false, true);
            await InputFrames();
            Equal(true, player.Playing, "returning ship hears local alarm");
            Save.CurrentPlanet = StellarBodies.the_moon;
            core.ChangeScene(Scenes.Station, new List<SceneVariables> { SceneVariables.Orbit });
            await InputFrames();
            Equal(false, player.Playing, "another location does not inherit alarm");
        }

        private async Task SdmAlarmLifetime()
        {
            var player = await OpenAlarmStation();
            var core = GameCore.SingletonInstance;
            var soundEnabled = core.Config.SoundEnabled;
            GameCore.Earth.Station.SdmCountdown = 12;
            await InputFrames();
            try
            {
                GetTree().Paused = true;
                await InputFrames();
                Equal(false, player.CanProcess(), "paused game pauses alarm player");
                Equal(true, player.StreamPaused, "paused tree suspends playback");
                GetTree().Paused = false;
                core.Config.SoundEnabled = false;
                core.Config.ApplyAudio();
                await InputFrames();
                Equal(true, AudioServer.IsBusMute(AudioServer.GetBusIndex(player.Bus)), "alarm respects sound setting");
                Equal(true, player.Playing, "mute does not reset alarm phase");
                core.Config.SoundEnabled = true;
                core.Config.ApplyAudio();
                core._Process(12);
                await InputFrames();
                Equal(false, player.Playing, "station expiry stops alarm");
                GameCore.Earth.Station.Built = true;
                GameCore.Earth.Station.SdmCountdown = 12;
                core.ChangeScene(Scenes.Station, new List<SceneVariables> { SceneVariables.Orbit });
                await InputFrames();
                Equal(true, player.Playing, "rebuilt armed station can warn");
                var menu = ActiveScreen<MainMenu>();
                var parent = menu.GetParent();
                parent.RemoveChild(menu);
                Equal(false, player.Playing, "menu exit stops audio before shutdown drain");
                parent.AddChild(menu);
            }
            finally
            {
                GetTree().Paused = false;
                core.Config.SoundEnabled = soundEnabled;
                core.Config.ApplyAudio();
            }
            // Leave the alarm playing: normal regression teardown must also exit cleanly.
            await InputFrames();
            Equal(true, player.Playing, "tree reentry resumes armed location once");
        }

        private async Task SdmAlarmOutput()
        {
            await OpenAlarmStation();
            var core = GameCore.SingletonInstance;
            var enabled = core.Config.SoundEnabled;
            var volume = core.Config.Volume;
            Equal(true, AudioServer.GetBusIndex("Game") >= 0, "ordinary sounds have a separate bus for alarm priority");
            ActiveScreen<MainMenu>().GetNode<AudioStreamPlayer>("MenuClickSound").Stop();
            await DrainStoppedAudio();
            var bus = AudioServer.GetBusIndex("Master");
            var effectIndex = AudioServer.GetBusEffectCount(bus);
            using var capture = new AudioEffectCapture { BufferLength = 2 };
            using var toneStream = new AudioStreamWav
            {
                Format = AudioStreamWav.FormatEnum.Format16Bits, Stereo = true, MixRate = 48000,
                LoopMode = AudioStreamWav.LoopModeEnum.Forward, LoopEnd = 16,
                Data = Enumerable.Range(0, 64).Select(i => i % 2 == 0 ? (byte)0 : (byte)32).ToArray()
            };
            var tone = new AudioStreamPlayer { Stream = toneStream, Bus = "Game" };
            ActiveScreen<MainMenu>().AddChild(tone);
            AudioServer.AddBusEffect(bus, capture);
            try
            {
                core.Config.SoundEnabled = true;
                core.Config.Volume = 100;
                core.Config.ApplyAudio();
                GameCore.Earth.Station.SdmCountdown = 12;
                ActiveScreen<MainMenu>()._Process(0);
                // A newly requested ordinary sound must not leak into either alarm channel.
                tone.Play();
                await ToSignal(GetTree().CreateTimer(0.6), SceneTreeTimer.SignalName.Timeout);
                var frames = capture.GetBuffer(capture.GetFramesAvailable());
                var left = Array.FindIndex(frames, f => Math.Abs(f.X) > 0.0001);
                var right = Array.FindIndex(frames, f => Math.Abs(f.Y) > 0.0001);
                Equal(true, left >= 0 && right > left, "mixer emits left then right waveform");
                var delay = (right - left) / AudioServer.GetMixRate();
                Equal(true, delay > 0.28 && delay < 0.36, "right onset retains approximately sixteen PAL frames");
                GD.Print($"SDM MIX: {frames.Length} frames; right onset delay {delay:F4}s");
            }
            finally
            {
                AudioServer.RemoveBusEffect(bus, effectIndex);
                tone.Stop();
                tone.Free();
                core.Config.SoundEnabled = enabled;
                core.Config.Volume = volume;
                core.Config.ApplyAudio();
                await DrainStoppedAudio();
            }
        }
    }
}
