using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using Deuteros.Code.Platform.Screens;
using static Deuteros.Code.Enums;
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
            using (var cancel = new InputEventKey { Keycode = Key.Escape, Pressed = true }) GetViewport().PushInput(cancel, true);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            using (var cancel = new InputEventKey { Keycode = Key.Escape, Pressed = false }) GetViewport().PushInput(cancel, true);
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
            try
            {
                using (var press = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = true }) Input.ParseInputEvent(press);
                // Headless runs do not drain accumulated input on each frame.
                Input.FlushBufferedEvents();
                Equal(true, Input.IsMouseButtonPressed(MouseButton.Left), "left-button press reached Input");
                var completed = false;
                music.Finished += () => completed = true;
                music.Seek((float)(music.Stream.GetLength() - 0.04));
                var deadline = Time.GetTicksMsec() + 3000;
                while (!completed && Time.GetTicksMsec() < deadline) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Equal(true, completed, "audio emitted completion");
                await ToSignal(GetTree().CreateTimer(AudioServer.GetOutputLatency() + 0.1), SceneTreeTimer.SignalName.Timeout);
                Equal(true, Input.IsMouseButtonPressed(MouseButton.Left), "left mouse remains held past output latency");
                Equal(false, music.Playing, "held left mouse delays replay");
                var bitmap = (Image)player.GetType().GetField("image", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(player);
                Equal(true, bitmap.GetData().All(value => value == 0), "ending remains black while held");
                using (var release = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false }) Input.ParseInputEvent(release);
                Input.FlushBufferedEvents();
                Equal(false, Input.IsMouseButtonPressed(MouseButton.Left), "left-button release reached Input");
                await InputFrames();
                Equal(true, music.Playing, "release restarts original ending");
                Equal(true, music.GetPlaybackPosition() < 1, "replay begins at start");
            }
            finally
            {
                using (var release = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = false }) Input.ParseInputEvent(release);
                Input.FlushBufferedEvents();
                OverlayManager.Instance.CloseOverlay();
            }
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

        private SCG NewEndingShip(bool fitted = true)
        {
            InitializeUi(); GameCore.SingletonInstance.SetProcess(false);
            var ship = NewInterstellarRoute(); ship.Pilot.AddAction(31); ship.EngageEngine();
            for (var update = 0; update < 16; update++) AdvanceInterstellar();
            Equal(4, ship.Pilot.GetLevel(), "actual Hyperlight arrival earns Warlord");
            ship.FuelType = ItemTypes.hed_fuel;
            ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, Source = ship.PlanetLocation, Destination = StellarBodies.earth,
                SourceItems = new List<ItemTypes>(), DestinationItems = new List<ItemTypes>(),
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
            while (ship.Modules.Count < 6) ship.Modules.Add(new ShipModule());
            ship.Modules[5].ModuleType = Module_Types.Tool;
            ship.Modules[5].ItemStored = fitted ? ItemTypes.alien_artifact : ItemTypes.none;
            ship.Modules[5].ItemCount = fitted ? 1 : 0;
            return ship;
        }

        private async Task TransmitterActivation(bool dfcc)
        {
            var ship = NewEndingShip(); ship.DFCC = dfcc; ship.Fuel = 0;
            var interior = await OpenInterstellarInterior(ship);
            var before = SaveStorage.Serialize(Save);
            Press(interior, "Modules/05"); await InputFrames();
            Equal(true, OverlayManager.Instance.GetNodeOrNull<Ending>("GlobalOverlay/GameArea/Center/Ending") != null, "fitted sixth-mount transmitter plays ending");
            Press(interior, "Modules/05");
            Equal(true, before == SaveStorage.Serialize(Save), "activation consumes no fuel item crew or campaign time");
            OverlayManager.Instance.CloseOverlay(); await InputFrames();
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(before);
            ship = Save.Ships.OfType<SCG>().Single();
            interior = await OpenInterstellarInterior(ship); Press(interior, "Modules/05"); await InputFrames();
            Equal(true, OverlayManager.Instance.IsOpen, "ordinary save retains replay eligibility");
            OverlayManager.Instance.CloseOverlay();
        }

        private async Task TransmitterRankGate()
        {
            var ship = NewEndingShip(); var crew = ship.Pilot;
            foreach (var absent in new[] { false, true })
            {
                crew.Warlord = false; ship.Pilot = absent ? null : crew;
                var interior = await OpenInterstellarInterior(ship);
                Press(interior, "Modules/05"); await InputFrames();
                Equal(true, OverlayManager.Instance.IsOpen, "low or missing rank receives module warning");
                Equal(true, OverlayManager.Instance.GetNode<RichTextLabel>("GlobalOverlay/GameArea/Center/OFPilotWarning/Labels/WarningBody").GetParsedText().Contains("Warlord"), "warning explains required rank");
                await CaptureDisplayEvidence("transmitter-rank-" + absent);
                OverlayManager.Instance.CloseOverlay(); await InputFrames();
            }
            ship.Pilot = crew; crew.Warlord = true; crew.Count = 0;
            var qualified = await OpenInterstellarInterior(ship); Press(qualified, "Modules/05"); await InputFrames();
            Equal(true, OverlayManager.Instance.GetNodeOrNull<Ending>("GlobalOverlay/GameArea/Center/Ending") != null, "original action checks assigned rank without inventing a crew-count gate");
            OverlayManager.Instance.CloseOverlay();
        }

        private async Task TransmitterStateAndRogueGates()
        {
            var ship = NewEndingShip();
            ship.DestinationPlanetLocation = StellarBodies.earth; ship.Fuel = 250; ship.EngageEngine();
            var interior = await OpenInterstellarInterior(ship); Press(interior, "Modules/05"); await InputFrames();
            Equal(false, OverlayManager.Instance.IsOpen, "travelling transmitter cannot activate");
            for (var i = 0; i < 100 && ship.ShipState != Ship_States.UnDocked; i++) AdvanceInterstellar();
            Equal(StellarBodies.earth, ship.PlanetLocation, "actual return trip reaches Earth");
            GameCore.Earth.Station.Built = true; GameCore.Earth.Station.BuildParts = 8;
            ship.Dock(); for (var i = 0; i < 20 && ship.ShipState != Ship_States.Docked; i++) AdvanceInterstellar();
            Equal(Ship_States.Docked, ship.ShipState, "actual docking completed");
            interior = await OpenInterstellarInterior(ship); Press(interior, "Modules/05"); await InputFrames();
            Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "docked click retains ordinary bay navigation");
            Equal(false, OverlayManager.Instance.IsOpen, "docked transmitter does not play ending");
            ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            ship.Modules[5].ModuleType = Module_Types.Tool; ship.Modules[5].ItemStored = ItemTypes.alien_artifact; ship.Modules[5].ItemCount = 1;
            interior = await OpenInterstellarInterior(ship); Press(interior, "Modules/05"); await InputFrames();
            Equal(false, OverlayManager.Instance.IsOpen, "rogue crew cannot manually activate transmitter");
        }

        private async Task TransmitterRetainedControls()
        {
            var ship = NewEndingShip(); var interior = await OpenInterstellarInterior(ship);
            Press(interior, "Modules/05"); await InputFrames();
            Equal(true, OverlayManager.Instance.IsOpen, "ending starts before retained controls");
            var before = SaveStorage.Serialize(Save);
            foreach (var path in new[] { "EngineControls/EngageEngine", "EngineControls/DisengageEngine", "Dock", "TakeOff", "Land", "Location/SmallLocation", "OpenACC", "Modules/05", "TextLayout/RenameShip", "TextLayout/CargoActions" }) Press(interior, path);
            Equal(true, before == SaveStorage.Serialize(Save), "retained controls cannot mutate paused ending world");
            Equal(0, interior.GetNode("ACCScreen").GetChildCount(), "retained ACC cannot open behind ending");
            OverlayManager.Instance.CloseOverlay(); await InputFrames();
            // Allowed owned overlay callbacks still work after the shared pause guard.
            Press(interior, "TextLayout/RenameShip"); await InputFrames();
            var dialog = OverlayManager.Instance.GetNode<Control>("GlobalOverlay/GameArea/Center/RenameShip");
            dialog.GetNode<LineEdit>("NameEdit").Text = "Transmitter crew";
            Press(dialog, "Confirm"); await InputFrames();
            Equal("Transmitter crew", ship.Name, "owned rename confirmation remains usable");
            Press(interior, "TextLayout/CargoActions"); await InputFrames();
            var cargo = OverlayManager.Instance.GetNode<Control>("GlobalOverlay/GameArea/Center/SupplyPods");
            Press(cargo, "Rows/Pod0/Ditch");
            Equal(0, ship.Modules[0].ItemCount, "owned cargo confirmation remains usable");
            OverlayManager.Instance.CloseOverlay();
        }

        private async Task TransmitterRecoveredCampaign()
        {
            await TransmissionCaptureToFinal();
            var core = GameCore.SingletonInstance; var ship = NewEndingShip(false);
            ship.DestinationPlanetLocation = StellarBodies.earth; ship.Fuel = 250; ship.EngageEngine();
            for (var i = 0; i < 100 && ship.ShipState != Ship_States.UnDocked; i++) AdvanceInterstellar();
            Equal(StellarBodies.earth, ship.PlanetLocation, "Warlord returns to manufacturing station");
            GameCore.Earth.Station.Built = true; GameCore.Earth.Station.BuildParts = 8;
            Save.CurrentPlanet = StellarBodies.earth;
            var factory = GameCore.Earth.Station.Factory; factory.AOC = true;
            var production = OpenMtxProduction();
            try
            {
                production.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.alien_artifact).EmitSignal(BaseButton.SignalName.Pressed);
                ProductionDays(50);
            }
            finally { production.Free(); }
            Equal(1, GameCore.Earth.Station.Resources.Stores[ItemTypes.alien_artifact], "eight real recoveries permit normal manufacture");
            ship.Dock(); for (var i = 0; i < 20 && ship.ShipState != Ship_States.Docked; i++) AdvanceInterstellar();
            Equal(Ship_States.Docked, ship.ShipState, "manufactured tool is fitted in a real docked bay");
            var tweens = GetTree().GetProcessedTweens().ToHashSet(); var bay = OpenUi<ShipBay>("res://Screens/ShipBay.tscn", new List<SceneVariables> { SceneVariables.Orbit, SceneVariables.Ship });
            try
            {
                Press(bay, "Buttons/ShipNav/Nav_Torso6");
                Press(bay, "ShipContainer/ScrollContainer2/HBoxContainer/Torso6/SpriteHolder/Buttons/ActivatePod");
                PressEquipmentNamed(bay, "Unknown");
                Equal(ItemTypes.alien_artifact, ship.Modules[5].ItemStored, "normal equipment controls fit the sixth mount");
            }
            finally { CloseDismantleBay(bay, tweens); }
            ship.TakeOff(); for (var i = 0; i < 20 && ship.ShipState != Ship_States.UnDocked; i++) AdvanceInterstellar();
            var interior = await OpenInterstellarInterior(ship);
            await CaptureDisplayEvidence("transmitter-fitted-warlord");
            var evidence = OS.GetEnvironment("DEUTEROS_SCREENSHOT_DIR");
            if (!string.IsNullOrEmpty(evidence))
                System.IO.File.WriteAllText(System.IO.Path.Combine(evidence, "transmitter-ready.json"), SaveStorage.Serialize(Save));
            Press(interior, "Modules/05"); await InputFrames();
            Equal(true, OverlayManager.Instance.GetNodeOrNull<Ending>("GlobalOverlay/GameArea/Center/Ending") != null, "eight captures recovery manufacture fitting launch and activation reach original ending");
            OverlayManager.Instance.CloseOverlay();
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
