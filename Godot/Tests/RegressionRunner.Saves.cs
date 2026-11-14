using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public void RunSaveRegressions()
        {
            Check("Save round trip retains private progress, references and all ship types", SaveRoundTrip);
            Check("Invalid saves fail without replacing the current world", RejectInvalidSaves);
            Check("Save slots replace atomically and retain the previous save", SaveSlots);
        }


        public async Task RunSaveUiRegressions()
        {
            Check("Corrupt save model fields are rejected before activation", RejectCorruptSaveGraph);
            await CheckAsync("Save screen confirms overwrite and load, and preserves the game on corrupt files", SaveScreenActions);
            await CheckAsync("A saved ship in flight arrives after restoring and advancing", ResumeSavedFlight);
        }

        private void RejectCorruptSaveGraph()
        {
            var ship = NavigationShip();
            ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, SourceItems = new(), DestinationItems = new(),
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
            Save.Ships.Add(ship);
            var original = SaveStorage.Serialize(Save);
            var active = Save;
            var corruptions = new Action<JObject>[]
            {
                doc => ((JObject)doc["Game"]["Ships"].Last)["ShipType"] = (int)Ship_Types.Shuttle,
                doc => ((JObject)doc["Game"]["Ships"].Last)["ACC"]["CurrentSource"] = 999,
                doc => ((JObject)doc["Game"]["Ships"].Last)["ACC"]["CurrentDestination"] = 999,
                doc => doc["Game"]["EnemyStarCursor"] = 999,
                doc => doc["Game"]["News"]["NewsItems"] = null,
                doc => ((JObject)doc["Game"]).Property("CurrentDay").Remove(),
                doc => ((JArray)doc["Game"]["BaseGameData"]["Planets"]["earth"]["PlanetResources"]["Materials"])[0] = null,
                doc =>
                {
                    var vessel = (JObject)doc["Game"]["Ships"].Last;
                    vessel["ShipState"] = (int)Ship_States.InTransit;
                    vessel["DestinationPlanetLocation"] = (int)StellarBodies.none;
                },
            };
            foreach (var corrupt in corruptions)
            {
                var document = JObject.Parse(original);
                corrupt(document);
                var rejected = false;
                try { GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(document.ToString())); }
                catch (Exception error) when (error is IOException || error is InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "corrupt graph rejected");
                Equal(true, ReferenceEquals(active, Save), "invalid load preserves current world");
            }
        }

        private async Task ResumeSavedFlight()
        {
            InitializeUi();
            var ship = NavigationShip();
            ship.DestinationPlanetLocation = StellarBodies.the_moon;
            ship.Engine = ship.EngineEngaged = true;
            ship.ShipState = Ship_States.InTransit;
            var duration = ship.TravelTimeRemain();
            ship.StartTravelDay = 100;
            Save.CurrentDay = 100u + (uint)duration - 1;
            Save.Ships.Add(ship);
            var oldWorld = Save;
            GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(SaveStorage.Serialize(Save)));
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var restored = Save.Ships.Single(s => s.ShipID == ship.ShipID);
            Equal(1, restored.TravelTimeRemain(), "remaining journey retained");
            AdvanceTickDay();
            Equal(Ship_States.UnDocked, restored.ShipState, "arrival follows restored schedule");
            Equal(StellarBodies.the_moon, restored.PlanetLocation, "restored ship reaches destination");
            Equal(StellarBodies.earth, restored.DestinationPlanetLocation, "return route prepared");
            Equal(99, restored.Fuel, "one day's fuel charged");
            Equal(100, ship.Fuel, "old ship not updated");
            Equal(Ship_States.InTransit, ship.ShipState, "old world remains detached");
            Equal(oldWorld.CurrentDay + 1, Save.CurrentDay, "date advances once");
        }

        private async Task SaveScreenActions()
        {
            InitializeUi();
            var directory = Path.Combine(Path.GetTempPath(), "deuteros-save-ui-" + Guid.NewGuid());
            var screen = GD.Load<PackedScene>("res://Screens/SaveScreen.tscn")
                .Instantiate<Deuteros.Code.Platform.Screens.SaveScreen>();
            screen.Storage = new SaveStorage(directory);
            GameCore.SingletonInstance.GetNode<Node>("GameContainer/GameViewport/MainScene").AddChild(screen);
            try
            {
                Equal(true, screen.GetNode<Button>("Load1").Disabled, "empty slot cannot load");
                Save.CurrentDay = 11;
                Save.Clock.DateCentidays = 1100;
                Press(screen, "Save1");
                Equal(11u, screen.Storage.Read(1).CurrentDay, "save button writes current game");
                Equal("1: 3100 011.00", screen.GetNode<Label>("Slot1").Text, "saved slot metadata");
                Save.CurrentDay = 22;
                Save.Clock.DateCentidays = 2200;
                Press(screen, "Save1");
                Equal(11u, screen.Storage.Read(1).CurrentDay, "overwrite waits for confirmation");
                Press(screen, "Cancel");
                Equal(11u, screen.Storage.Read(1).CurrentDay, "cancel preserves file");
                Press(screen, "Save1");
                Press(screen, "Confirm");
                Equal(22u, screen.Storage.Read(1).CurrentDay, "confirmed overwrite");
                Save.CurrentDay = 33;
                Save.Clock.DateCentidays = 3300;
                var active = Save;
                Press(screen, "Load1");
                Press(screen, "Cancel");
                Equal(true, ReferenceEquals(active, Save), "cancel preserves active world");
                File.WriteAllText(screen.Storage.SlotPath(2), "{corrupt");
                // Direct callback signal also exercises failure handling for a file corrupted after display refresh.
                Press(screen, "Load2");
                Press(screen, "Confirm");
                Equal(true, ReferenceEquals(active, Save), "failed load preserves active world");
                Equal("Cannot load this save.", screen.GetNode<Label>("Status").Text, "visible error");
                Press(screen, "Load1");
                Press(screen, "Confirm");
                Equal(false, ReferenceEquals(active, Save), "load activates a new world");
                Equal(22u, Save.CurrentDay, "restored date");
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "load recreates overview");
                Equal("3100 022.00", ActiveRecipeScreen<Deuteros.Code.Platform.Screens.MainMenu>().Time.Text, "persistent menu refreshed");
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Save.TimeSkipDay = true;
                GameCore.SingletonInstance._Process(0);
                Equal(23u, Save.CurrentDay, "restored world can advance");
                Equal(33u, active.CurrentDay, "previous world stays unchanged");
            }
            finally
            {
                screen.Free();
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private void SaveRoundTrip()
        {
            var earth = GameCore.Earth;
            Save.CurrentDay = 127;
            Save.EnemyStarCursor = 6;
            Save.News.AddNews("Persistence check");
            Save.News.LastBulletin = BulletinTypes.None;
            earth.ResearchStaff = new Staff { Leader = "Research", Count = 25, Type = StaffType.Research };
            earth.ResearchStaff.AddAction(8);
            earth.TrainingData.ProductionLocked = true;
            earth.TrainingData.ProductionTrainingCount = 17;
            earth.TrainingData.ProductionDayStart = 120;
            earth.PlanetResources.Stores[ItemTypes.iron] = 4321;
            earth.Station.Resources.Stores.MTX.Target = StellarBodies.the_moon;
            earth.Station.Resources.Stores.MTX.BalanceItems.Add(ItemTypes.carbon);
            var product = Save.BaseGameData.ItemList.First(i => i.Research != null);
            earth.CurrentResearchItem = product.Research;
            earth.Factory.ProductionQueue.Add(new ProductionItem(product) { Production_Value = 91, Active = true });
            var ios = NavigationShip();
            ios.ShipState = Ship_States.InTransit;
            ios.StartTravelDay = 123;
            ios.ACC = new Deuteros.Code.Objects.ACC { Ship = ios, SourceItems = new(), DestinationItems = new(), CurrentDestination = ItemTypes.iron };
            ios.ACC.CurrentSource = ItemTypes.carbon;
            ios.ACC.Active = true;
            ios.Modules[0].HeldItem = new UnknownItem(UnknownItemTypes.AlienArtifact);
            ios.ItemScanResults = new Asteroid { Type = ItemTypes.carbon, Mass = 701, Class = 4, DayCount = 3 };
            Save.Ships.Add(ios);
            Save.Ships.Add(new Shuttle { ShipType = Ship_Types.Shuttle, ShipState = Ship_States.Docked, Modules = new(), PlanetLocation = StellarBodies.earth, OnGround = true });
            Save.Ships.Add(new SCG { ShipType = Ship_Types.SCG, ShipState = Ship_States.Docked, Modules = new(), PlanetLocation = StellarBodies.earth });
            Save.TimeSkip = true;
            Save.TimeSkipDay = true;
            var before = Save;
            var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(true, ReferenceEquals(before, Save), "deserialization cannot activate a world");
            Equal(127u, loaded.CurrentDay, "game date");
            Equal(6, loaded.EnemyStarCursor, "enemy scheduling position");
            var restored = (Earth)loaded.BaseGameData.Planets[StellarBodies.earth];
            Equal(4321, restored.PlanetResources.Stores[ItemTypes.iron], "raw stock");
            Equal(17, restored.TrainingData.ProductionTrainingCount, "training queue");
            Equal(true, restored.TrainingData.ProductionLocked, "training state");
            Equal(earth.ResearchStaff.GetLevel(), restored.ResearchStaff.GetLevel(), "private staff experience");
            restored.ResearchStaff.AddAction(1);
            Equal((int)StaffLevel_Researcher.Professor, restored.ResearchStaff.GetLevel(), "exact promotion threshold preserved");
            Equal(Save.News.GetNews(100).Last(), loaded.News.GetNews(100).Last(), "private news history");
            var canonicalProduct = loaded.BaseGameData.ItemList.First(i => i.ItemType == product.ItemType);
            Equal(true, ReferenceEquals(canonicalProduct, restored.Factory.ProductionQueue[0].Product), "production references canonical item");
            Equal(true, ReferenceEquals(canonicalProduct.Research, restored.CurrentResearchItem), "research references canonical item");
            var restoredShip = (IOS)loaded.Ships.Single(s => s.ShipID == ios.ShipID);
            Equal(true, ReferenceEquals(restoredShip, restoredShip.ACC.Ship), "ACC owns restored ship");
            Equal(ItemTypes.carbon, restoredShip.ACC.CurrentSource, "ACC cycle position");
            Equal(701, restoredShip.AsteroidScanResults.Mass, "scan result subtype");
            Equal(true, restoredShip.Modules[0].HeldItem is UnknownItem, "grapple subtype");
            Equal(Save.Ships.Count(s => s is EnemyFleet), loaded.Ships.Count(s => s is EnemyFleet), "enemy fleets");
            Equal(true, loaded.Ships.Any(s => s is Shuttle) && loaded.Ships.Any(s => s is SCG), "other player hulls");
            Equal(false, loaded.TimeSkip || loaded.TimeSkipDay, "load stops automatic advancement");
            Equal(true, Save.TimeSkip && Save.TimeSkipDay, "saving does not mutate active clock");
            Equal(true, ReferenceEquals(CoreData.StaticGameData.BulletinTexts, loaded.BaseGameData.BulletinTexts), "static bulletin definitions restored");
            // Do not leave a synthetic partially equipped world advancing during teardown.
            Save.TimeSkip = Save.TimeSkipDay = false;
        }

        private void RejectInvalidSaves()
        {
            var active = Save;
            var json = SaveStorage.Serialize(Save);
            foreach (var invalid in new[] { "{", json.Replace("\"Version\":1", "\"Version\":999"),
                json.Replace("\"CurrentPlanet\":1", "\"CurrentPlanet\":999999"),
                "{\"Version\":1,\"Game\":{\"$type\":\"System.IO.FileInfo, System.Private.CoreLib\"}}" })
            {
                // Explicit invalid planet mutation below does not depend on enum's numeric value.
                if (invalid == json) continue;
                var rejected = false;
                try { SaveStorage.Deserialize(invalid); } catch (Exception) { rejected = true; }
                Equal(true, rejected, "invalid file rejected");
                Equal(true, ReferenceEquals(active, Save), "active game untouched");
            }
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
            obj["Game"]["CurrentPlanet"] = 999999;
            var failed = false;
            try { SaveStorage.Deserialize(obj.ToString()); } catch (Exception) { failed = true; }
            Equal(true, failed, "unknown current planet rejected");
        }

        private async Task SettingsAutosave()
        {
            await WithSettings(async settings =>
            {
                var core = GameCore.SingletonInstance;
                core.SetProcess(false);
                var manager = Deuteros.Code.Platform.Helpers.SettingsManager.Instance;
                var originalStorage = core.Storage;
                var directory = Path.Combine(Path.GetTempPath(), "deuteros-autosave-" + Guid.NewGuid());
                var storage = new SaveStorage(directory);
                core.Storage = storage;
                try
                {
                    Equal("Off", settings.AutosaveRow.SettingValue.AsString(), "Autosave defaults to Off");
                    settings.AutosaveRow.StepValue(1);
                    Equal("5 minutes", settings.AutosaveRow.SettingValue.AsString(), "Autosave offers an active-play interval");
                    Press(settings, "%ApplyButton");
                    using (var disk = new GameConfig(manager.Config.FilePath))
                        Equal("5 minutes", disk.GetValue("gameplay", "autosave").AsString(), "Autosave preference persists");
                    Press(settings, "%CloseButton");
                    await InputFrames();
                    core.ChangeScene(Scenes.Earth_Ground, new List<SceneVariables> { SceneVariables.Ground });
                    await InputFrames();
                    for (var slot = 1; slot <= 6; slot++) storage.Write(slot, Save);
                    var protectedFiles = Directory.GetFiles(directory).ToDictionary(path => path, File.ReadAllText);
                    core._Process(299);
                    Equal(false, File.Exists(Path.Combine(directory, "autosave.json")), "not saved before interval");
                    var cursor = core.GetNode<GlobalInput>("GameContainer/GameViewport/VirtualCursorView");
                    cursor.LockToRect(new Rect2(0, 0, 320, 200));
                    core._Process(600);
                    Equal(false, File.Exists(Path.Combine(directory, "autosave.json")), "modal time does not trigger saving");
                    cursor.Unlock();
                    core._Process(1);
                    Equal(true, File.Exists(Path.Combine(directory, "autosave.json")), "active interval writes independent autosave");
                    Equal(Save.CurrentDay, storage.Read(7).CurrentDay, "autosave contains the settled world");
                    var first = File.ReadAllText(storage.SlotPath(7));
                    core._Process(300);
                    Equal(first, File.ReadAllText(storage.SlotPath(7) + ".bak"), "previous autosave retained atomically");
                    foreach (var file in protectedFiles)
                        Equal(file.Value, File.ReadAllText(file.Key), "manual and Quick Saves stay byte-identical");
                    var latest = File.ReadAllText(storage.SlotPath(7));
                    core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
                    await InputFrames();
                    core._Process(600);
                    Equal(latest, File.ReadAllText(storage.SlotPath(7)), "save selection cannot be overwritten while choosing recovery");
                    var screen = ActiveRecipeScreen<Deuteros.Code.Platform.Screens.SaveScreen>();
                    Equal(false, screen.GetNode<Button>("Load7").Disabled, "autosave is recoverable through normal controls");
                    Equal(false, screen.HasNode("Save7"), "automatic slot has no manual write control");
                    Equal(true, screen.GetNode<Label>("Slot7").Text.StartsWith("A:"), "autosave date is identified");
                    await CaptureDisplayEvidence("autosave-slots");
                    var savedDay = storage.Read(7).CurrentDay;
                    Save.CurrentDay += 10;
                    var unsavedDay = Save.CurrentDay;
                    Press(screen, "Load7");
                    Press(screen, "Cancel");
                    Equal(unsavedDay, Save.CurrentDay, "cancel keeps current world");
                    Press(screen, "Load7");
                    Press(screen, "Confirm");
                    await InputFrames();
                    Equal(savedDay, Save.CurrentDay, "confirmed load recovers autosave");
                    core._Process(299);
                    Equal(latest, File.ReadAllText(storage.SlotPath(7)), "loaded world gets a fresh interval");
                    manager.SetSetting("gameplay/autosave", "Off");
                    core._Process(600);
                    Equal(latest, File.ReadAllText(storage.SlotPath(7)), "Off preserves the previous recovery file");
                    manager.SetSetting("gameplay/autosave", true);
                    core._Process(600);
                    Equal(latest, File.ReadAllText(storage.SlotPath(7)), "malformed preference falls back to Off");
                    manager.SetSetting("gameplay/autosave", "5 minutes");
                    var validDeadline = Save.EnemyBuildDay;
                    Save.EnemyBuildDay = ulong.MaxValue;
                    core._Process(300);
                    Save.EnemyBuildDay = validDeadline;
                    Equal(latest, File.ReadAllText(storage.SlotPath(7)), "failed write preserves recovery file");
                    Equal(true, Deuteros.Code.Platform.Helpers.OverlayManager.Instance.IsOpen, "autosave failure is visible");
                    Deuteros.Code.Platform.Helpers.OverlayManager.Instance.CloseOverlay();
                    await InputFrames();
                    core._Process(1);
                    Equal(false, Deuteros.Code.Platform.Helpers.OverlayManager.Instance.IsOpen, "failure does not retry every frame");
                    settings = OpenSettings();
                    await InputFrames();
                    Equal("5 minutes", settings.AutosaveRow.SettingValue.AsString(), "reopening restores saved interval");
                    settings.AutosaveRow.StepValue(1);
                    Equal("10 minutes", settings.AutosaveRow.SettingValue.AsString(), "next interval is selectable");
                    Press(settings, "%CancelButton");
                    await InputFrames();
                    Equal("5 minutes", manager.GetSetting("gameplay/autosave").AsString(), "Cancel discards interval preview");
                    foreach (var minutes in new[] { 10, 15 })
                    {
                        var previous = File.ReadAllText(storage.SlotPath(7));
                        manager.SetSetting("gameplay/autosave", $"{minutes} minutes");
                        core._Process(minutes * 60 - 1);
                        Equal(previous, File.ReadAllText(storage.SlotPath(7)), "longer interval starts fresh");
                        core._Process(1);
                        Equal(Save.CurrentDay, storage.Read(7).CurrentDay, "selected longer interval saves current world");
                        Equal(previous, File.ReadAllText(storage.SlotPath(7) + ".bak"), "longer interval retains its predecessor");
                    }
                }
                finally
                {
                    core.Storage = originalStorage;
                    core.SetProcess(true);
                    if (Directory.Exists(directory)) Directory.Delete(directory, true);
                }
            });
        }

        private async Task QuickSaveShortcut()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            var originalStorage = core.Storage;
            var directory = Path.Combine(Path.GetTempPath(), "deuteros-quick-ui-" + Guid.NewGuid());
            var storage = new SaveStorage(directory);
            core.Storage = storage;
            var manager = Deuteros.Code.Platform.Helpers.SettingsManager.Instance;
            var originalKey = manager.GetSetting("keybinds/quick_save");
            manager.SetSetting("keybinds/quick_save", (long)Key.F5);
            manager.ApplyKeybinds();
            try
            {
                core.ChangeScene(Scenes.Earth_Ground, new List<SceneVariables> { SceneVariables.Ground });
                await InputFrames();
                Save.CurrentDay = 11;
                Save.Clock.DateCentidays = 1100;
                await PushGameKey(Key.F5);
                Equal(true, storage.Exists(SaveStorage.QuickSlot), "default Quick Save writes separate slot");
                Equal(11u, storage.Read(SaveStorage.QuickSlot).CurrentDay, "saved world matches active game");
                Equal(Scenes.Earth_Ground, core.currentScene, "saving leaves current screen open");
                Equal("Quick saved.", GameCore.HoverText, "quick save gives visible feedback");
                var first = File.ReadAllText(storage.SlotPath(SaveStorage.QuickSlot));
                Save.CurrentDay = 22;
                Save.Clock.DateCentidays = 2200;
                manager.SetSetting("keybinds/quick_save", (long)Key.F6);
                manager.ApplyKeybinds();
                await PushGameKey(Key.F5);
                await PushGameKey(Key.F6, echo: true);
                await PushGameKey(Key.F6, control: true);
                OpenSettings();
                await InputFrames();
                await PushGameKey(Key.F6);
                Deuteros.Code.Platform.Helpers.OverlayManager.Instance.CloseOverlay();
                await InputFrames();
                Equal(first, File.ReadAllText(storage.SlotPath(SaveStorage.QuickSlot)), "old modified repeated and modal keys leave save unchanged");
                await PushGameKey(Key.F6);
                Equal(22u, storage.Read(SaveStorage.QuickSlot).CurrentDay, "rebound key replaces quick save");
                Equal(first, File.ReadAllText(storage.SlotPath(SaveStorage.QuickSlot) + ".bak"), "previous snapshot backed up");
                core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
                await InputFrames();
                var screen = ActiveRecipeScreen<Deuteros.Code.Platform.Screens.SaveScreen>();
                Equal("Q: 3100 022.00", screen.GetNode<Label>("Slot6").Text, "load screen identifies quick save date");
                var label = screen.GetNode<Label>("Slot6");
                var rowSize = label.Size;
                label.ClipText = false;
                var textWidth = label.GetMinimumSize().X;
                label.ClipText = true;
                label.Size = rowSize;
                Equal(true, textWidth <= rowSize.X, "quick save date fits its visible row");
                await CaptureDisplayEvidence("quick-save-slots");
                Equal(false, screen.GetNode<Button>("Load6").Disabled, "quick save can be loaded through normal controls");
                Save.CurrentDay = 33;
                Save.Clock.DateCentidays = 3300;
                await PushGameKey(Key.F6);
                Equal("Q: 3100 033.00", screen.GetNode<Label>("Slot6").Text, "open save screen refreshes after shortcut");
                var valid = File.ReadAllText(storage.SlotPath(SaveStorage.QuickSlot));
                var elapsed = Save.Clock.NormalElapsed;
                Save.Clock.NormalElapsed = -1;
                await PushGameKey(Key.F6);
                Save.Clock.NormalElapsed = elapsed;
                Equal(true, Deuteros.Code.Platform.Helpers.OverlayManager.Instance.IsOpen, "save failure is shown");
                Equal(valid, File.ReadAllText(storage.SlotPath(SaveStorage.QuickSlot)), "failed quick save preserves previous snapshot");
                Deuteros.Code.Platform.Helpers.OverlayManager.Instance.CloseOverlay();
                await InputFrames();
                Save.CurrentDay = 44;
                Save.Clock.DateCentidays = 4400;
                Press(screen, "Load6");
                Press(screen, "Cancel");
                Equal(44u, Save.CurrentDay, "cancelled quick load keeps active world");
                Press(screen, "Load6");
                Press(screen, "Confirm");
                await InputFrames();
                Equal(33u, Save.CurrentDay, "confirmed quick load restores saved world");
                Equal(Scenes.Overview, core.currentScene, "quick load recreates overview");
            }
            finally
            {
                core.Storage = originalStorage;
                manager.SetSetting("keybinds/quick_save", originalKey);
                manager.ApplyKeybinds();
                core.SetProcess(true);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private void QuickSaveStorage()
        {
            var directory = Path.Combine(Path.GetTempPath(), "deuteros-quick-save-" + Guid.NewGuid());
            try
            {
                var storage = new SaveStorage(directory);
                Save.CurrentDay = 11;
                for (var slot = 1; slot <= 5; slot++) storage.Write(slot, Save);
                var manuals = Directory.GetFiles(directory).ToDictionary(path => path, File.ReadAllText);
                Save.CurrentDay = 22;
                storage.Write(6, Save);
                Equal(22u, storage.Read(6).CurrentDay, "quick save can be read independently");
                Save.CurrentDay = 33;
                storage.Write(6, Save);
                Equal(33u, storage.Read(6).CurrentDay, "quick save replaces its own slot");
                Equal(22u, SaveStorage.Deserialize(File.ReadAllText(storage.SlotPath(6) + ".bak")).CurrentDay,
                    "previous quick save retained in backup");
                foreach (var manual in manuals)
                    Equal(manual.Value, File.ReadAllText(manual.Key), "manual slot remains byte-identical");
                Equal(0, Directory.GetFiles(directory, "*.tmp").Length, "quick save leaves no temporary files");
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }

        private void SaveSlots()
        {
            var directory = Path.Combine(Path.GetTempPath(), "deuteros-save-test-" + Guid.NewGuid());
            try
            {
                var storage = new SaveStorage(directory);
                Save.CurrentDay = 11;
                Save.Clock.DateCentidays = 1100;
                storage.Write(1, Save);
                Save.CurrentDay = 22;
                Save.Clock.DateCentidays = 2200;
                storage.Write(1, Save);
                Equal(22u, storage.Read(1).CurrentDay, "new save");
                Equal(11u, SaveStorage.Deserialize(File.ReadAllText(storage.SlotPath(1) + ".bak")).CurrentDay, "previous save backup");
                var good = File.ReadAllText(storage.SlotPath(1));
                var invalid = Save.BaseGameData;
                try
                {
                    Save.BaseGameData = null;
                    var rejected = false;
                    try { storage.Write(1, Save); } catch (Exception) { rejected = true; }
                    Equal(true, rejected, "invalid world rejected before writing");
                }
                finally { Save.BaseGameData = invalid; }
                Equal(good, File.ReadAllText(storage.SlotPath(1)), "failed save preserves file");
                Equal(0, Directory.GetFiles(directory, "*.tmp").Length, "temporary files cleaned");
                var badSlot = false;
                try { storage.Read(0); } catch (ArgumentOutOfRangeException) { badSlot = true; }
                Equal(true, badSlot, "slot range checked");
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }
    }
}
