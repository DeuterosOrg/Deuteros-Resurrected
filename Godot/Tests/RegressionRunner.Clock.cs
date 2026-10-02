using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Deuteros.Code.Objects;
using Deuteros.Code;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task FractionalNewsAndSlot()
        {
            var news = await OpenNews();
            Save.CurrentDay = 7;
            Save.Clock.DateCentidays = 1234;
            ActiveScreen<MainMenu>().DayTick(Save.CurrentDay, Save.CurrentDay);
            Save.News.AddNews("Fractional report");
            news.DrawData();
            Equal("12.34: Fractional report", news.NewsLabels[0].Text, "News stamps displayed date, not simulation counter");
            await CaptureDisplayEvidence("fractional-news");
            var directory = Path.Combine(Path.GetTempPath(), "deuteros-clock-ui-" + Guid.NewGuid());
            var screen = GD.Load<PackedScene>("res://Screens/SaveScreen.tscn").Instantiate<SaveScreen>();
            screen.Storage = new SaveStorage(directory);
            GameCore.SingletonInstance.GetNode<Node>("MainScene").AddChild(screen);
            try
            {
                Press(screen, "Save1");
                Equal("1: 3100 012.34", screen.GetNode<Label>("Slot1").Text, "slot displays saved fractional date");
                var slotLabel = screen.GetNode<Label>("Slot1");
                var availableWidth = slotLabel.Size.X;
                slotLabel.ClipText = false;
                Equal(true, slotLabel.GetMinimumSize().X <= availableWidth,
                    "fractional slot date fits the rendered label without clipping");
                slotLabel.ClipText = true;
                Equal(7u, screen.Storage.Read(1).CurrentDay, "saving keeps independent update count");
                await CaptureDisplayEvidence("fractional-save-slot");
            }
            finally
            {
                screen.Free();
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        private async Task FractionalArrivalDisplay()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            Save.Clock.DateCentidays = 99998;
            Save.CurrentDay = 7;
            ActiveScreen<MainMenu>().DayTick(Save.CurrentDay, Save.CurrentDay);
            interior.Ship.DestinationPlanetLocation = StellarBodies.the_moon;
            Equal(2, interior.Ship.TravelTimeRemain(), "two updates to the Moon");
            interior.UpdateState();
            Equal("ETA:\n3101 000.00", interior.GetNode<Label>("TextLayout/ETA").Text, "normal-mode ETA projects hundredth-day updates across year boundary");
            Save.TimeSkip = true;
            interior.UpdateState();
            Equal("ETA:\n3101 001.98", interior.GetNode<Label>("TextLayout/ETA").Text, "fast-mode ETA projects whole-day updates");
            Save.TimeSkip = false;
            await CaptureDisplayEvidence("fractional-arrival");
        }

        private void LegacyClockSave()
        {
            Save.CurrentDay = 100;
            var ship = NavigationShip();
            ship.DestinationPlanetLocation = StellarBodies.mars;
            ship.ShipState = Ship_States.InTransit;
            ship.StartTravelDay = 99;
            Save.Ships.Add(ship);
            var remaining = ship.TravelTimeRemain();
            var training = GameCore.Earth.TrainingData;
            training.ResearcherTrainingCount = 10;
            training.ResearcherLocked = true;
            training.ResearcherDayStart = 95;
            GameCore.Earth.PlanetResources.Stores[ItemTypes.iron] = 1234;
            var document = JObject.Parse(SaveStorage.Serialize(Save));
            ((JObject)document["Game"]).Property("Clock").Remove();
            var loaded = SaveStorage.Deserialize(document.ToString());
            Equal(10000ul, loaded.Clock.DateCentidays, "missing legacy clock preserves old displayed date");
            Equal(0d, loaded.Clock.NormalElapsed, "legacy save starts a fresh natural interval");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = loaded;
            Equal(remaining, Save.Ships.Single(s => s.ShipID == ship.ShipID).TravelTimeRemain(), "legacy flight keeps remaining updates");
            Equal(95u, GameCore.Earth.TrainingData.ResearcherDayStart, "training timestamp is not scaled");
            Equal(10, GameCore.Earth.TrainingData.ResearcherTrainingCount, "training queue retained");
            Equal(1234, GameCore.Earth.PlanetResources.Stores[ItemTypes.iron], "inventory retained");
        }

        private void RejectMalformedClock()
        {
            var original = SaveStorage.Serialize(Save);
            var active = Save;
            var corruptions = new Action<JObject>[]
            {
                doc => doc["Game"]["Clock"]["DateCentidays"] = ulong.MaxValue,
                doc => doc["Game"]["Clock"] = null,
                doc => ((JObject)doc["Game"]["Clock"]).Property("NormalElapsed").Remove(),
                doc => doc["Game"]["Clock"]["NormalElapsed"] = -1,
                doc => doc["Game"]["Clock"]["NormalElapsed"] = 315.6,
                doc => doc["Game"]["Clock"]["NormalElapsed"] = "NaN",
                doc => doc["Game"]["Clock"]["PendingIncrement"] = 2,
                doc => doc["Game"]["Clock"]["PendingIncrement"] = 1,
            };
            foreach (var corrupt in corruptions)
            {
                var document = JObject.Parse(original);
                corrupt(document);
                var rejected = false;
                try { GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(document.ToString())); }
                catch (Exception error) when (error is IOException || error is InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "malformed clock rejected before activation");
                Equal(true, ReferenceEquals(active, Save), "failed clock load preserves world");
            }
        }

        private void PausedClock()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            GetTree().Paused = true;
            try { core._Process(315.6); }
            finally { GetTree().Paused = false; }
            foreach (var delta in new[] { 0d, -1d, double.NaN, double.PositiveInfinity }) core._Process(delta);
            Equal(0ul, Save.Clock.DateCentidays, "paused and invalid deltas cannot advance the clock");
            Equal(0d, Save.Clock.NormalElapsed, "invalid deltas cannot poison saved interval");
            core._Process(315.6);
            Equal(1ul, Save.Clock.DateCentidays, "valid timing resumes after pause");
        }

        private void ManualAfterPendingClock()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            GameCore.LockScreen();
            try { core._Process(315.6); }
            finally { GameCore.UnLockScreen(); }
            Save.TimeSkipDay = true;
            core._Process(0);
            Equal(1ul, Save.Clock.DateCentidays, "older natural increment is consumed first");
            Equal(true, Save.TimeSkipDay, "manual request survives consumption of the older increment");
            core._Process(0);
            Equal(101ul, Save.Clock.DateCentidays, "manual request adds its whole day on the next frame");
            Equal(2u, Save.CurrentDay, "each producer increment consumes one model update");
            Equal(false, Save.TimeSkipDay, "consumed manual request clears");
        }

        private void MixedClockSave()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            core._Process(150);
            AdvanceTickDay();
            Equal("3100 001.00", ActiveScreen<MainMenu>().Time.Text, "manual advancement adds a whole day");
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            core._Process(165.5);
            Equal("3100 001.00", ActiveScreen<MainMenu>().Time.Text, "loaded interval waits for its remaining time");
            core._Process(0.1);
            Equal("3100 001.01", ActiveScreen<MainMenu>().Time.Text, "manual advancement and load preserve the natural remainder");
            Equal(2u, Save.CurrentDay, "manual and fractional increments each consume one update");
        }

        private void StalledNaturalClock()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            core._Process(2000);
            Equal("3100 000.01", ActiveScreen<MainMenu>().Time.Text, "a stall cannot accumulate multiple pending clock increments");
            Equal(1u, Save.CurrentDay, "one pending update is consumed");
            core._Process(0);
            Equal(1u, Save.CurrentDay, "no backlog is consumed on the next frame");
            core._Process(315.6);
            Equal("3100 000.02", ActiveScreen<MainMenu>().Time.Text, "a fresh full interval follows the stalled update");
        }

        private void PendingNaturalClock()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            GameCore.Earth.TrainingData.ResearcherTrainingCount = 1;
            GameCore.LockScreen();
            try
            {
                core._Process(315.6);
                core._Process(315.6);
                Equal(false, GameCore.Earth.TrainingData.ResearcherLocked, "blocked presentation does not consume the pending update");
                core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            }
            finally { GameCore.UnLockScreen(); }
            core._Process(0);
            Equal("3100 000.01", ActiveScreen<MainMenu>().Time.Text, "one saved pending update is rendered after release");
            Equal(true, GameCore.Earth.TrainingData.ResearcherLocked, "pending update resumes actual training");
            Equal(1u, Save.CurrentDay, "blocked intervals do not queue catch-up simulation");
        }

        private void NaturalFractionalClock()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            var training = GameCore.Earth.TrainingData;
            training.ResearcherTrainingCount = 1;
            var menu = ActiveScreen<MainMenu>();
            var label = menu.Time;
            core._Process(315.5);
            Equal("3100 000.00", label.Text, "natural clock waits for the full normal interval");
            Equal(false, training.ResearcherLocked, "no simulation update before the threshold");
            core._Process(0.1);
            Equal("3100 000.01", label.Text, "normal interval advances one hundredth of a displayed day");
            Equal(true, training.ResearcherLocked, "fractional advance consumes a real training update");
        }
    }
}
