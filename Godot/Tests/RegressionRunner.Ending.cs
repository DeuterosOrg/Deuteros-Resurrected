using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Platform.Helpers;
using Godot;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private Control OpenEndingPlayer()
        {
            const string path = "res://PreFabs/Ending.tscn";
            Equal(true, ResourceLoader.Exists(path), "original ending presentation is available");
            return (Control)OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>(path), false);
        }

        private static object EndingCall(Control player, string name, params object[] arguments)
        {
            var method = player.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new InvalidOperationException("Missing ending behavior: " + name);
            return method.Invoke(player, arguments);
        }

        private async Task EndingPixels()
        {
            var player = OpenEndingPlayer();
            try
            {
                player.SetProcess(false);
                foreach (var pair in new[] {
                    (800, "d5b654b63e66bb05710dc33b09992a23db50a86dd658744d54e67a67b48a2290"),
                    (2385, "962dacd64e91a63bdd34200aeb8bf46238ee994317ae21697be137c04a748231"),
                    (3676, "567d9ebb7e23d201f2a24b83be240b314807b986ce3654413e20c859eafec35d"),
                    (3702, "ea0787f65f73b0013d03b359490e3125211b28ad5c1502ffb1544c0ded4192f5") })
                {
                    EndingCall(player, "DrawFrame", pair.Item1);
                    var sourceImage = (Image)player.GetType().GetField("image", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(player);
                    Equal(pair.Item2, Convert.ToHexString(SHA256.HashData(sourceImage.GetData())).ToLowerInvariant(), "composed pixels at frame " + pair.Item1);
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                    using var bitmap = player.GetNode<TextureRect>("Picture").Texture.GetImage();
                    if (DisplayServer.GetName() != "headless")
                        Equal(pair.Item2, Convert.ToHexString(SHA256.HashData(bitmap.GetData())).ToLowerInvariant(), "native texture pixels at frame " + pair.Item1);
                    await CaptureDisplayEvidence("ending-frame-" + pair.Item1);
                }
            }
            finally { OverlayManager.Instance.CloseOverlay(); }
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        private async Task EndingPauseAndInput()
        {
            var day = Save.CurrentDay;
            var player = OpenEndingPlayer();
            Equal(true, GetTree().Paused, "campaign paused");
            Equal(true, player.GetNode<AudioStreamPlayer>("Music").Playing, "native music running");
            Equal("Master", player.GetNode<AudioStreamPlayer>("Music").Bus.ToString(), "ending respects preferences without inheriting the SDM Game-bus mute");
            using (var cancel = new InputEventKey { Keycode = Key.Escape, Pressed = true }) Input.ParseInputEvent(cancel);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            using (var cancel = new InputEventKey { Keycode = Key.Escape, Pressed = false }) Input.ParseInputEvent(cancel);
            Equal(true, OverlayManager.Instance.IsOpen, "Escape cannot skip original ending");
            Equal(day, Save.CurrentDay, "campaign did not advance");
            Equal<Node>(null, OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://PreFabs/Ending.tscn"), false), "duplicate activation rejected");
            OverlayManager.Instance.CloseOverlay();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(false, GetTree().Paused, "controlled close restores previous pause state");
            Equal(false, GodotObject.IsInstanceValid(player), "player disposed");
        }

        private async Task EndingHeldReplay()
        {
            var player = OpenEndingPlayer();
            var music = player.GetNode<AudioStreamPlayer>("Music");
            using (var press = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true }) Input.ParseInputEvent(press);
            music.Seek((float)(music.Stream.GetLength() - 0.04));
            var deadline = Time.GetTicksMsec() + 3000;
            while (music.Playing && Time.GetTicksMsec() < deadline) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(false, music.Playing, "complete audio reached terminal black");
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(false, music.Playing, "held left mouse delays replay");
            using (var release = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false }) Input.ParseInputEvent(release);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(true, music.Playing, "release restarts original ending");
            Equal(true, music.GetPlaybackPosition() < 1, "replay begins at start");
            OverlayManager.Instance.CloseOverlay();
        }

        private async Task EndingWorldReplacement()
        {
            var player = OpenEndingPlayer();
            GameCore.SingletonInstance.GameData.ActiveSaveFile = CoreData.CreateNewSaveFile();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(false, OverlayManager.Instance.IsOpen, "old ending releases replacement world");
            Equal(false, GetTree().Paused, "replacement world is usable");
        }

        private async Task EndingClockCatchup()
        {
            var player = OpenEndingPlayer();
            var music = player.GetNode<AudioStreamPlayer>("Music");
            // The final labels are stable from2385 until3669; a large audio jump must not run one frame at a time.
            music.Seek(50f);
            for (var i = 0; i < 8; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var bitmap = (Image)player.GetType().GetField("image", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(player);
            Equal("962dacd64e91a63bdd34200aeb8bf46238ee994317ae21697be137c04a748231",
                Convert.ToHexString(SHA256.HashData(bitmap.GetData())).ToLowerInvariant(), "visual clock catches up to music after a long stall");
            OverlayManager.Instance.CloseOverlay();
        }

        private async Task EndingRejectsMalformedTimeline()
        {
            var player = OpenEndingPlayer(); player.SetProcess(false);
            try
            {
                var data = Godot.FileAccess.GetFileAsString("res://Ending/sequence.json");
                foreach (var corrupt in new[] { "{", data.Replace("\"rate\":50", "\"rate\":0"), data.Replace("\"frame_count\":3703", "\"frame_count\":1") })
                {
                    var rejected = false;
                    try { EndingCall(player, "LoadSequence", corrupt); }
                    catch (TargetInvocationException e) when (e.InnerException is Newtonsoft.Json.JsonException || e.InnerException is InvalidOperationException) { rejected = true; }
                    Equal(true, rejected, "malformed playback data rejected before drawing");
                }
            }
            finally { OverlayManager.Instance.CloseOverlay(); }
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(false, GetTree().Paused, "failed presentation releases pause on close");
        }

        private async Task EndingSceneExitAndPriorPause()
        {
            GetTree().Paused = true;
            var player = OpenEndingPlayer();
            GameCore.SingletonInstance.ChangeScene(Deuteros.Code.Enums.Scenes.Earth_Ground,
                new System.Collections.Generic.List<Deuteros.Code.Enums.SceneVariables> { Deuteros.Code.Enums.SceneVariables.Ground });
            for (var i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(false, OverlayManager.Instance.IsOpen, "leaving underlying scene removes ending");
            Equal(true, GetTree().Paused, "pre-existing pause survives teardown");
            GetTree().Paused = false;
        }

        private async Task EndingWindowClose()
        {
            var player = OpenEndingPlayer();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Equal(true, player.GetNode<AudioStreamPlayer>("Music").Playing, "window close exercises live native audio");
            closeWindowAfterTests = true;
        }
    }
}
