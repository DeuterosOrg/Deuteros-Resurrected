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
            GameCore.SingletonInstance.GetNode<Node>("MainScene").AddChild(screen);
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
