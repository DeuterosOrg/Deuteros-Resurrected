using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Objects.ModuleTextFrame;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void ArtifactsStartHidden()
        {
            Equal(0, Save.BaseGameData.Stars.Values.Count(s => s.ArtifactLocation != StellarBodies.none),
                "new campaigns have no preassigned artifacts, including Earth");
            var ship = new IOS { PlanetLocation = StellarBodies.earth };
            Equal<UnknownItem>(null, UnknownItem.ScanForItems(ship), "Earth scan cannot supply a ninth artifact");
        }

        private void ArtifactCaptureAssignment()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.AtWar = true;
            var star = Save.BaseGameData.Stars[StellarBodies.proxima];
            // Isolate the missing capture trigger from the old startup placement.
            star.ArtifactLocation = StellarBodies.none;
            var planets = Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == star.StarId).ToList();
            foreach (var planet in planets) planet.ActiveMethanoid = false;
            var first = planets.Single(p => p.PlanetId == StellarBodies.atlantic);
            var last = planets.Single(p => p.PlanetId == StellarBodies.pacific);
            foreach (var planet in new[] { first, last })
            {
                planet.ActiveMethanoid = true;
                planet.Station.Built = true;
                planet.Station.SdmInstalled = true;
                planet.Station.SdmCountdown = 16;
            }
            var ship = NavigationShip();
            Save.Ships.Add(ship);
            ship.StarLocation = star.StarId;
            ship.ShipState = Ship_States.Docked;
            ship.PlanetLocation = first.PlanetId;
            Equal(true, SdmSystem.ApplySwitches(Save, first, 0x0200), "first station defused");
            Equal(StellarBodies.none, star.ArtifactLocation, "remaining hostile station defers assignment");
            ship.PlanetLocation = last.PlanetId;
            Equal(true, SdmSystem.ApplySwitches(Save, last, 0x0200), "final station defused");
            Equal(true, planets.Any(p => p.PlanetId == star.ArtifactLocation), "capture reveals an artifact within Proxima");
            var location = star.ArtifactLocation;
            Equal(1, Save.AlienTransmissions.PendingLocations.Count, "one location notice queued");
            Equal(location, Save.AlienTransmissions.PendingLocations[0], "notice records actual revealed location");
            ship.PlanetLocation = location;
            Equal(UnknownItemTypes.AlienArtifact, UnknownItem.ScanForItems(ship).ItemType, "revealed segment is discoverable by the real scanner");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            star = Save.BaseGameData.Stars[StellarBodies.proxima];
            Equal(location, star.ArtifactLocation, "revealed location survives actual save/load");
            Equal(location, Save.AlienTransmissions.PendingLocations.Single(), "undisplayed notice survives save/load");
            star.ArtifactLocation = StellarBodies.none; // Collection removes the location, not its assignment history.
            last = Save.BaseGameData.Planets[last.PlanetId];
            last.ActiveMethanoid = true;
            last.Station.SdmCountdown = 16;
            ship = Save.Ships.OfType<IOS>().Single(s => s.ShipID == ship.ShipID);
            ship.PlanetLocation = last.PlanetId;
            Equal(true, SdmSystem.ApplySwitches(Save, last, 0x0200), "recapture succeeds");
            Equal(StellarBodies.none, star.ArtifactLocation, "recapture cannot duplicate an already collected artifact");
        }

        private void ArtifactCaptureLegacy()
        {
            var ship = UnknownObjectShip();
            ship.Modules[0].HeldItem = new UnknownItem(UnknownItemTypes.AlienArtifact);
            Save.BaseGameData.Stars[StellarBodies.proxima].ArtifactLocation = StellarBodies.baltic;
            Save.BaseGameData.Stars[StellarBodies.centauri].ArtifactLocation = StellarBodies.none;
            var document = JObject.Parse(SaveStorage.Serialize(Save));
            ((JObject)document["Game"]).Property("AlienTransmissions").Remove();
            var original = document.ToString();
            var loaded = SaveStorage.Deserialize(original);
            Equal(StellarBodies.earth, loaded.BaseGameData.Stars[StellarBodies.the_sun].ArtifactLocation, "legacy Earth location retained");
            Equal(StellarBodies.baltic, loaded.BaseGameData.Stars[StellarBodies.proxima].ArtifactLocation, "legacy moon location retained");
            Equal(true, loaded.Ships.Single(s => s.ShipID == ship.ShipID).Modules[0].HeldItem is UnknownItem { ItemType: UnknownItemTypes.AlienArtifact }, "held legacy segment retained");
            foreach (var body in loaded.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.centauri)) body.ActiveMethanoid = false;
            loaded.AlienTransmissions.StationCaptured(loaded, StellarBodies.centauri);
            Equal(StellarBodies.none, loaded.BaseGameData.Stars[StellarBodies.centauri].ArtifactLocation, "legacy collected system cannot respawn");
            Equal(0, loaded.AlienTransmissions.PendingLocations.Count, "migration invents no historical notices");
            var once = SaveStorage.Serialize(loaded);
            Equal(once, SaveStorage.Serialize(SaveStorage.Deserialize(once)), "migration is idempotent");
            Equal(original, document.ToString(), "legacy input remains untouched");
            document["Game"]["AtWar"] = true;
            loaded = SaveStorage.Deserialize(document.ToString());
            Equal(0, loaded.AlienTransmissions.Stage, "legacy war starts an unknown message history at its introduction");
            Equal(10, loaded.AlienTransmissions.Countdown, "legacy war gets the initial delay");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = loaded;
            for (int delivery = 0; delivery < 8; delivery++) GameCore.SingletonInstance.TriggerAlienTechDiscovery(ItemTypes.alien_artifact);
            document = JObject.Parse(SaveStorage.Serialize(Save));
            ((JObject)document["Game"]).Property("AlienTransmissions").Remove();
            loaded = SaveStorage.Deserialize(document.ToString());
            Equal(13, loaded.AlienTransmissions.Stage, "completed legacy recovery takes precedence over war introduction");
            Equal(2, loaded.AlienTransmissions.Countdown, "completed legacy recovery schedules final instructions");
            once = SaveStorage.Serialize(loaded);
            Equal(once, SaveStorage.Serialize(SaveStorage.Deserialize(once)), "completed migration does not restart on repeated load");
        }

        private void ArtifactCaptureInvalidSave()
        {
            var baseline = SaveStorage.Serialize(Save);
            var active = Save;
            var corruptions = new Action<JObject>[]
            {
                state => state["AssignedStars"] = null,
                state => state["AssignedStars"] = new JArray(),
                state => state["AssignedStars"] = new JArray((int)StellarBodies.the_sun, (int)StellarBodies.the_sun),
                state => state["AssignedStars"] = new JArray((int)StellarBodies.the_sun, 999999),
                state => state["PendingLocations"] = null,
                state => state["PendingLocations"] = new JArray((int)StellarBodies.earth),
                state => state["PendingLocations"] = new JArray(999999),
                state => state["PendingLocations"] = new JArray((int)StellarBodies.pacific),
                state => { state["AssignedStars"] = new JArray((int)StellarBodies.the_sun, (int)StellarBodies.proxima);
                    state["PendingLocations"] = new JArray((int)StellarBodies.pacific, (int)StellarBodies.baltic); },
            };
            foreach (var corrupt in corruptions)
            {
                var document = JObject.Parse(baseline);
                corrupt((JObject)document["Game"]["AlienTransmissions"]);
                var rejected = false;
                try { GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(document.ToString())); }
                catch (Exception error) when (error is IOException || error is InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "invalid artifact state rejected before world activation");
                Equal(true, ReferenceEquals(active, Save), "invalid load preserves current world");
            }
        }

        private void ArtifactCaptureAllSystems()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.AtWar = true;
            var ship = NavigationShip(); Save.Ships.Add(ship);
            foreach (var star in Save.BaseGameData.Stars.Values)
            {
                var bodies = Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == star.StarId).ToList();
                foreach (var body in bodies) body.ActiveMethanoid = false;
                var station = bodies.First();
                station.Station.Built = station.Station.SdmInstalled = station.ActiveMethanoid = true;
                station.Station.SdmCountdown = 16;
                ship.PlanetLocation = station.PlanetId;
                ship.StarLocation = star.StarId;
                Equal(true, SdmSystem.ApplySwitches(Save, station, 0x0200), "last hostile station defused in " + star.StarId);
                if (star.StarId == StellarBodies.the_sun)
                    Equal(StellarBodies.none, star.ArtifactLocation, "Sun is excluded even after its last capture");
                else
                    Equal(true, bodies.Any(p => p.PlanetId == star.ArtifactLocation), "revealed segment belongs to its captured system");
            }
            Equal(8, Save.AlienTransmissions.PendingLocations.Count, "exactly eight independent location notices");
            Equal(8, Save.BaseGameData.Stars.Values.Count(s => s.ArtifactLocation != StellarBodies.none), "exactly eight discoverable segments");
            var cygni = Save.BaseGameData.Stars[StellarBodies.cygni].ArtifactLocation;
            Equal(false, cygni == StellarBodies.protos || cygni == StellarBodies.prasios, "five-bit source range excludes Cygni records 32 and 33");
            var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(string.Join(",", Save.AlienTransmissions.PendingLocations), string.Join(",", loaded.AlienTransmissions.PendingLocations), "all notices retain capture order through save/load");
            var queued = loaded.AlienTransmissions.PendingLocations.ToArray();
            loaded.AlienTransmissions.Stage = 4;
            loaded.AlienTransmissions.Ready = true;
            Equal(true, loaded.AlienTransmissions.BeginDisplay(), "trust message opens before queued locations");
            Equal(8, loaded.AlienTransmissions.PendingLocations.Count, "trust message retains early captures");
            foreach (var location in queued)
            {
                loaded = SaveStorage.Deserialize(SaveStorage.Serialize(loaded));
                Equal(false, loaded.AlienTransmissions.Advance(), "queued location waits first update");
                Equal(true, loaded.AlienTransmissions.Advance(), "queued location ready on second update");
                Equal(true, loaded.AlienTransmissions.BeginDisplay(), "queued location displayed");
                Equal(location, loaded.AlienTransmissions.LastLocation, "location notices retain capture order");
            }
            Equal(0, loaded.AlienTransmissions.PendingLocations.Count, "each location is consumed once");
            Equal(0, loaded.AlienTransmissions.Countdown, "empty queue stops scheduling");
            // A separate new campaign's final hostile station is destroyed, not captured.
            CoreData.CreateBaseGameData();
            GameCore.SingletonInstance.GameData.ActiveSaveFile = CoreData.CreateNewSaveFile();
            foreach (var body in Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.proxima)) body.ActiveMethanoid = false;
            var destroyed = Save.BaseGameData.Planets[StellarBodies.pacific];
            destroyed.ActiveMethanoid = true;
            destroyed.Station.Built = true;
            destroyed.Station.SdmCountdown = 1;
            SdmSystem.AdvanceTime(1);
            Equal(false, destroyed.Station.Built, "last hostile station destroyed");
            Equal(StellarBodies.none, Save.BaseGameData.Stars[StellarBodies.proxima].ArtifactLocation, "destruction does not grant a capture segment");
            Equal(0, Save.AlienTransmissions.PendingLocations.Count, "destruction queues no location notice");
        }

        private async Task CaptureDiscoversScg()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.AtWar = true;
            Save.CurrentDay = Save.WarDeclaredDay = 0;
            Save.Ships.Clear();
            foreach (var planet in Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.the_sun))
                planet.ActiveMethanoid = false;
            var earth = GameCore.Earth;
            earth.ActiveMethanoid = earth.Station.Built = earth.Station.SdmInstalled = true;
            earth.Station.SdmCountdown = 16;
            var ship = NavigationShip(); Save.Ships.Add(ship);
            var types = new[] { ItemTypes.g_chassis, ItemTypes.star_drive, ItemTypes.hed_fuel };
            foreach (var type in types) Equal(true, core.GameData.GetItem(type).Research.Locked, "undiscovered before capture: " + type);
            Equal(true, SdmSystem.ApplySwitches(Save, earth, 0x0200), "Sol's final hostile station defused");
            foreach (var type in types) Equal(true, core.GameData.GetItem(type).Research.Locked, "discovery waits for notification dispatch: " + type);
            AdvanceTickDay();
            Equal(BulletinTypes.Drone_Ships, Save.News.LastBulletin, "earlier drone discovery retains priority");
            foreach (var type in types) Equal(true, core.GameData.GetItem(type).Research.Locked, "competing bulletin defers capture discovery: " + type);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            AdvanceTickDay();
            Equal(BulletinTypes.Sol_Cleared, Save.News.LastBulletin, "pending capture discovery survives save and reaches the existing bulletin");
            foreach (var type in types)
            {
                var item = core.GameData.GetItem(type);
                Equal(false, item.Research.Locked, "normal research becomes available: " + type);
                Equal(true, item.Locked, "capture does not grant manufacture before research: " + type);
                Equal(false, item.Research.Researched, "capture does not complete research: " + type);
            }
            Equal(1, Save.Unlocks.Count(u => u == Game_Unlocks.Interstellar_Travel), "galaxy navigation unlocked once");
            Equal(StellarBodies.none, Save.BaseGameData.Stars[StellarBodies.the_sun].ArtifactLocation, "Sol discovery does not create a ninth segment");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            core.ChangeScene(Scenes.Earth_Research, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            var researchScreen = ActiveScreen<Deuteros.Code.Platform.Screens.Research>();
            foreach (var type in types.Reverse())
            {
                var button = researchScreen.Buttons.Single(b => b.ObjectData?.ItemType == type);
                button.EmitSignal(Godot.BaseButton.SignalName.Pressed);
                Equal(type, researchScreen.SelectedButton.ObjectData.ItemType, "discovered project is selectable through Research: " + type);
            }
            await CaptureDisplayEvidence("capture-discovers-scg-research");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            core.GameData.GetItem(ItemTypes.g_chassis).Research.ResearchPercentageComplete = 37;
            earth = GameCore.Earth;
            earth.ActiveMethanoid = true;
            earth.Station.SdmCountdown = 16;
            Equal(true, SdmSystem.ApplySwitches(Save, earth, 0x0200), "later recapture still succeeds");
            AdvanceTickDay();
            Equal(Scenes.SaveScreen, core.currentScene, "discovery not repeated on following days");
            Equal(37, core.GameData.GetItem(ItemTypes.g_chassis).Research.ResearchPercentageComplete, "discovered progress preserved");
        }

        private async Task TradeWarTransmission() => await WithTrade(async (ship, interior) =>
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.MethanoidTradeCount = 16;
            Press(interior, "Modules/01");
            await InputFrames();
            Equal(true, Save.AtWar, "real trade-threshold encounter commits war");
            await FirstTransmissionAfterWar();
        });

        private async Task StationWarTransmission()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.AtWar = false;
            foreach (var planet in Save.BaseGameData.Planets.Values) planet.Station.Built = false;
            foreach (var planet in Save.BaseGameData.Planets.Values.Where(p => p.PlanetId != StellarBodies.earth).Take(5))
            {
                planet.Station.Built = true;
                planet.ActiveMethanoid = false;
            }
            var earth = GameCore.Earth;
            earth.ActiveMethanoid = false;
            earth.Station.BuildParts = 7;
            interior.Ship.ShipState = Ship_States.UnDocked;
            interior.Ship.Pilot = new Staff { Count = 10, Type = StaffType.Marines, Leader = "Construction" };
            interior.Ship.Modules[0].ModuleType = Module_Types.Tool;
            interior.Ship.Modules[0].ItemStored = ItemTypes.of_frame;
            interior.Ship.Modules[0].ItemCount = 1;
            var keys = new[] { ModuleFrameText.Station_Deploy_Complete, ModuleFrameText.Methanoid_DeclareWar };
            var frames = keys.Select(k => Save.BaseGameData.ModuleFrameTexts[k]).ToArray();
            var originalLines = frames.Select(f => f.Lines).ToArray();
            try
            {
                foreach (var frame in frames) frame.Lines = new List<Line>();
                interior.UpdateState();
                Press(interior, "Modules/00");
                await InputFrames();
                Equal(6, Save.BaseGameData.Planets.Values.Count(p => p.Station.Built && !p.ActiveMethanoid), "sixth station actually completed");
                Equal(true, Save.AtWar, "real station completion commits war");
                await FirstTransmissionAfterWar();
            }
            finally
            {
                for (int i = 0; i < frames.Length; i++) frames[i].Lines = originalLines[i];
                core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
                await InputFrames();
            }
        }

        private async Task FirstTransmissionAfterWar()
        {
            var core = GameCore.SingletonInstance;
            // Suppress the separately tested drone discovery so all ten updates here are eligible.
            Save.Unlocks.Add(Game_Unlocks.D_F_C_C);
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            for (int update = 1; update <= 9; update++)
            {
                AdvanceTickDay();
                Equal(Scenes.SaveScreen, core.currentScene, "transmission must not arrive early at update " + update);
                if (update == 5)
                {
                    Save.AlienTransmissions.Start(); // Duplicate war callbacks must not restart the timer.
                    core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                }
            }
            AdvanceTickDay();
            Equal(Scenes.Bulletins, core.currentScene, "tenth eligible update opens department notification");
            var notice = ActiveScreen<Bulletins>();
            var acknowledge = notice.GetNodeOrNull<Godot.Button>("ViewTransmission");
            Equal(true, acknowledge != null, "notification provides a keyboard-accessible acknowledgement");
            Equal(true, acknowledge.Disabled, "acknowledgement waits for notification typing");
            Equal(BulletinTypes.None, Save.News.LastBulletin, "undisplayed transmission has not replaced News replay");
            notice.LetterDelayMs = 0;
            await ToSignal(GetTree().CreateTimer(0.2), Godot.SceneTreeTimer.SignalName.Timeout);
            Equal(false, acknowledge.Disabled, "completed notification enables acknowledgement");
            Press(notice, "ViewTransmission");
            var label = notice.GetNode<Godot.RichTextLabel>("Labels/BulletinLabel");
            Equal(true, label.GetParsedText().StartsWith("greetings human."), "acknowledgement opens the first original message");
            Equal("res://Fonts/deuteros-alien.ttf", label.GetThemeFont("normal_font").ResourcePath, "body renders with the supplied cryptographic font");
            Equal(BulletinTypes.Transmission1, Save.News.LastBulletin, "displayed transmission becomes replayable");
            await CaptureDisplayEvidence("alien-first-message");
            core.ChangeScene(Scenes.News, new List<SceneVariables>());
            await InputFrames();
            var beforeReplay = SaveStorage.Serialize(Save);
            Press(ActiveScreen<Deuteros.Code.Platform.Screens.News>(), "Images/Icon");
            await InputFrames();
            Equal(beforeReplay, SaveStorage.Serialize(Save), "News replay does not advance stage, timer or other saved state");
            Equal(true, ActiveScreen<Bulletins>().GetNode<Godot.RichTextLabel>("Labels/BulletinLabel").GetParsedText().StartsWith("greetings human."), "News replays the message rather than its department notification");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
        }

        private async Task TransmissionStageSequence()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.Ships.Clear();
            Save.AlienTransmissions.Start();
            var delays = new[] { 10, 250, 250, 80, 200 };
            for (int stage = 0; stage < 5; stage++)
            {
                for (int tick = 1; tick < delays[stage]; tick++)
                {
                    AdvanceTickDay();
                    Equal(Scenes.SaveScreen, core.currentScene, "not due before boundary for stage " + stage);
                    if (tick == delays[stage] / 2)
                        core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                }
                AdvanceTickDay();
                Equal(Scenes.Bulletins, core.currentScene, "department notice at exact boundary for stage " + stage);
                Equal(stage, Save.AlienTransmissions.Stage, "notification has not consumed its transmission");
                Equal(stage - 1, Save.AlienTransmissions.LastStage, "last displayed context unchanged before acknowledgement");
                var notice = ActiveScreen<Bulletins>();
                await FinishBulletin(notice);
                Press(notice, "ViewTransmission");
                Equal(stage, Save.AlienTransmissions.LastStage, "acknowledgement records displayed stage");
                Equal(stage + 1, Save.AlienTransmissions.Stage, "display advances once");
                Equal(stage < 4 ? delays[stage + 1] : 0, Save.AlienTransmissions.Countdown, "original next delay");
                var text = notice.GetNode<Godot.RichTextLabel>("Labels/BulletinLabel").GetParsedText();
                if (stage == 1) Equal(true, text.StartsWith("greetings HUMAN."), "fixed stage-one mask reads the original big-endian byte bit");
                await InputFrames();
                var messageLabel = notice.GetNode<Godot.RichTextLabel>("Labels/BulletinLabel");
                for (int line = 1; line < messageLabel.GetLineCount(); line++)
                    Equal(true, messageLabel.GetLineOffset(line) - messageLabel.GetLineOffset(line - 1) >= 8,
                        "alien glyph rows do not overlap");
                Equal(true, messageLabel.GetContentHeight() <= 176, "message stays within the bulletin panel after layout");
                await CaptureDisplayEvidence("alien-stage-" + stage);
                core.ChangeScene(Scenes.News, new List<SceneVariables>());
                await InputFrames();
                core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                var before = SaveStorage.Serialize(Save);
                Press(ActiveScreen<Deuteros.Code.Platform.Screens.News>(), "Images/Icon");
                await InputFrames();
                Equal(text.ToUpperInvariant(), ActiveScreen<Bulletins>().GetNode<Godot.RichTextLabel>("Labels/BulletinLabel").GetParsedText().ToUpperInvariant(), "saved replay retains message words and layout");
                var beforeState = JObject.Parse(before);
                beforeState["Game"]["AlienTransmissions"]["LastMask"] = Save.AlienTransmissions.LastMask;
                Equal(true, JToken.DeepEquals(beforeState, JObject.Parse(SaveStorage.Serialize(Save))), "only the original glyph mask rotates during replay; gameplay progress is unchanged");
                core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
                await InputFrames();
            }
            for (int tick = 0; tick < 5; tick++) AdvanceTickDay();
            Equal(Scenes.SaveScreen, core.currentScene, "zero delay waits for capture instead of repeating the trust message");
        }

        private void TransmissionInvalidState()
        {
            var baseline = SaveStorage.Serialize(Save);
            var active = Save;
            var changes = new Action<JObject>[]
            {
                s => s["Stage"] = -2, s => s["Stage"] = 14,
                s => s["Countdown"] = -1, s => s["Countdown"] = 251,
                s => s["LastStage"] = -2, s => s["LastStage"] = 14,
                s => s["Ready"] = true,
                s => { s["Stage"] = 0; s["Countdown"] = 1; s["Ready"] = true; },
                s => { s["Stage"] = 5; s["Ready"] = true; },
                s => { s["LastStage"] = 5; s["LastLocation"] = (int)StellarBodies.earth; },
                s => { s["LastStage"] = 5; s["LastLocation"] = 999999; },
                s => s["LastLocation"] = (int)StellarBodies.atlantic,
                s => s["LastMask"] = -1,
            };
            foreach (var change in changes)
            {
                var document = JObject.Parse(baseline);
                change((JObject)document["Game"]["AlienTransmissions"]);
                var rejected = false;
                try { GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(document.ToString())); }
                catch (Exception error) when (error is IOException || error is InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "invalid transmission state rejected");
                Equal(true, ReferenceEquals(active, Save), "invalid transmission cannot replace active world");
            }
        }

        private async Task TransmissionNoticeInterruption()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.Ships.Clear();
            Save.AlienTransmissions.Start();
            for (int tick = 0; tick < 10; tick++) AdvanceTickDay();
            Equal(Scenes.Bulletins, core.currentScene, "notice begins");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            GameCore.LockScreen();
            await InputFrames();
            await ToSignal(GetTree().CreateTimer(0.2), Godot.SceneTreeTimer.SignalName.Timeout);
            Equal(true, core.GetNode<Deuteros.Code.Platform.Helpers.InputBlocker>("GameContainer/GameViewport/InputBlocker").Blocked, "cancelled typing preserves another owner's lock");
            Equal(true, Save.AlienTransmissions.Ready, "interrupted notification remains pending");
            Equal(-1, Save.AlienTransmissions.LastStage, "interrupted notification is not a displayed transmission");
            AdvanceTickDay();
            Equal(Scenes.SaveScreen, core.currentScene, "new modal lock suppresses retry");
            GameCore.UnLockScreen();
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            AdvanceTickDay();
            Equal(Scenes.Bulletins, core.currentScene, "pending notice retries after lock release and save/load");
            var notice = ActiveScreen<Bulletins>();
            await FinishBulletin(notice);
            Press(notice, "ViewTransmission");
            Press(notice, "ViewTransmission");
            Equal(1, Save.AlienTransmissions.Stage, "duplicate acknowledgement advances only once");
            Equal(250, Save.AlienTransmissions.Countdown, "duplicate acknowledgement does not reset next stage");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            Save.AlienTransmissions.Ready = true;
            Save.AlienTransmissions.Countdown = 0;
            AdvanceTickDay();
            notice = ActiveScreen<Bulletins>();
            await FinishBulletin(notice);
            var oldSave = Save;
            var beforeOld = SaveStorage.Serialize(oldSave);
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(beforeOld);
            var beforeNew = SaveStorage.Serialize(Save);
            Press(notice, "ViewTransmission");
            Equal(beforeOld, SaveStorage.Serialize(oldSave), "stale acknowledgement cannot consume an old world's message");
            Equal(beforeNew, SaveStorage.Serialize(Save), "stale acknowledgement cannot mutate the replacement world");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
        }

        private async Task TransmissionCaptureToFinal()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.AlienTransmissions.Stage = 5; // The introduction sequence is exercised separately.
            core.GameData.GetItem(ItemTypes.g_chassis).Research.Locked = false;
            var stars = Save.BaseGameData.Stars.Keys.Where(s => s != StellarBodies.the_sun).ToArray();
            for (int index = 0; index < stars.Length; index++)
            {
                var starId = stars[index];
                var bodies = Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == starId).ToList();
                foreach (var body in bodies) body.ActiveMethanoid = false;
                var station = bodies.First();
                station.ActiveMethanoid = station.Station.Built = station.Station.SdmInstalled = true;
                station.Station.SdmCountdown = 16;
                var ship = NavigationShip();
                ship.PlanetLocation = station.PlanetId;
                ship.StarLocation = starId;
                ship.Pilot = new Staff { Count = 10, Type = StaffType.Marines, Leader = "Recovery" };
                ship.Pilot.AddAction(10);
                ship.Modules[0].ModuleType = Module_Types.Tool;
                ship.Modules[0].ItemStored = ItemTypes.grapple;
                ship.Modules[0].ItemCount = 1;
                Save.Ships.Clear(); Save.Ships.Add(ship);
                Equal(true, SdmSystem.ApplySwitches(Save, station, 0x0200), "last station captured for report " + index);
                var location = Save.BaseGameData.Stars[starId].ArtifactLocation;
                AdvanceTickDay();
                Equal(Scenes.SaveScreen, core.currentScene, "capture waits its first update");
                AdvanceTickDay();
                Equal(Scenes.Bulletins, core.currentScene, "capture report is due on second update");
                var notice = ActiveScreen<Bulletins>();
                await FinishBulletin(notice);
                Press(notice, "ViewTransmission");
                Equal(5 + index, Save.AlienTransmissions.LastStage, "correct location-report stage");
                Equal(location, Save.AlienTransmissions.LastLocation, "message records the actual assigned location");
                var label = notice.GetNode<Godot.RichTextLabel>("Labels/BulletinLabel");
                Equal(true, label.GetParsedText().Contains(location.ToScreenString(" ").ToUpperInvariant()), "location is readable inside the encoded message");
                Equal(true, label.Text.Contains("[font=res://Fonts/deuteros.ttf]"), "location uses ordinary font");
                Equal(true, label.GetParsedText().Contains("please attempt to recover"), "location suffix follows its separately selected zero mask");
                if (index == 0) await CaptureDisplayEvidence("alien-location-report");
                core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
                await InputFrames();
                ship.PlanetLocation = location;
                ship.ShipState = Ship_States.UnDocked;
                Save.CurrentPlanet = location;
                ship.ItemScanResults = UnknownItem.ScanForItems(ship);
                CaptureScannedObject(ship);
                Equal(true, ship.Modules[0].HeldItem is UnknownItem { ItemType: UnknownItemTypes.AlienArtifact }, "real grapple captures revealed segment");
                core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                ship = Save.Ships.OfType<IOS>().Single(s => s.ShipID == ship.ShipID);
                Equal(StellarBodies.none, Save.BaseGameData.Stars[starId].ArtifactLocation, "collection clears orbit through save/load");
                ship.PlanetLocation = station.PlanetId;
                ship.ShipState = Ship_States.Docked;
                Save.CurrentPlanet = station.PlanetId;
                core.ShipSelected = ship.ShipID;
                var tweens = GetTree().GetProcessedTweens().ToHashSet();
                var bay = OpenEquippedBay(ship);
                try
                {
                    Press(bay, "Buttons/ShipNav/Nav_Torso1");
                    Press(bay, "ShipContainer/ScrollContainer2/HBoxContainer/Torso1/SpriteHolder/Buttons/ActivatePod");
                    await ToSignal(GetTree().CreateTimer(5.1), Godot.SceneTreeTimer.SignalName.Timeout);
                    Equal<GrappleItem>(null, ship.Modules[0].HeldItem, "real bay analysis removes the delivered segment");
                }
                finally { CloseDismantleBay(bay, tweens); }
                Equal(index == 7 ? 100 : (index + 1) * 12,
                    core.GameData.GetItem(ItemTypes.alien_artifact).Research.ResearchPercentageComplete, "delivery credit follows the original sequence");
            }
            Equal(13, Save.AlienTransmissions.Stage, "eighth real unload selects final instructions");
            AdvanceTickDay();
            Equal(Scenes.SaveScreen, core.currentScene, "final instructions wait the first update");
            AdvanceTickDay();
            var final = ActiveScreen<Bulletins>();
            await FinishBulletin(final);
            Press(final, "ViewTransmission");
            Equal(true, final.GetNode<Godot.RichTextLabel>("Labels/BulletinLabel").GetParsedText().Contains("CONSTRUCT THE TRANSMITTER"), "final original instructions displayed");
            Equal(13, Save.AlienTransmissions.Stage, "final stage stays at thirteen");
            Equal(0, Save.AlienTransmissions.Countdown, "final instructions stop the timer");
            await CaptureDisplayEvidence("alien-final-instructions");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            var before = SaveStorage.Serialize(Save);
            core.TriggerAlienTechDiscovery(ItemTypes.alien_artifact);
            Equal(before, SaveStorage.Serialize(Save), "a ninth delivery cannot restart the final message");
        }

        private void TransmissionFinalOverridesLocations()
        {
            var state = Save.AlienTransmissions;
            state.Stage = 5;
            state.AssignedStars.Add(StellarBodies.proxima);
            state.PendingLocations.Add(StellarBodies.atlantic);
            state.Countdown = 2;
            for (int delivery = 0; delivery < 8; delivery++) GameCore.SingletonInstance.TriggerAlienTechDiscovery(ItemTypes.alien_artifact);
            Equal(13, state.Stage, "completion overrides pending location stage");
            Equal(0, state.PendingLocations.Count, "obsolete location notices do not outlive completed recovery");
            Equal(2, state.Countdown, "completion starts the final two-update delay");
            GameCore.SingletonInstance.TriggerAlienTechDiscovery(ItemTypes.alien_artifact);
            Equal(2, state.Countdown, "duplicate delivery does not postpone final instructions");
        }

        private async Task TransmissionReplayMask()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            Save.Ships.Clear();
            Save.AlienTransmissions.Stage = 1;
            Save.AlienTransmissions.Ready = true;
            AdvanceTickDay();
            var notice = ActiveScreen<Bulletins>();
            await FinishBulletin(notice);
            Press(notice, "ViewTransmission");
            Equal(true, notice.GetNode<Godot.RichTextLabel>("Labels/BulletinLabel").GetParsedText().StartsWith("greetings HUMAN."), "initial fixed-mask reading");
            // The source body has 30 spaces: ROL30(08945949) = 42251652.
            Equal(0x42251652u, Save.AlienTransmissions.LastMask, "first display retains the rotated mask");
            core.ChangeScene(Scenes.News, new List<SceneVariables>());
            await InputFrames();
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Press(ActiveScreen<Deuteros.Code.Platform.Screens.News>(), "Images/Icon");
            await InputFrames();
            var replay = ActiveScreen<Bulletins>();
            Equal(true, replay.GetNode<Godot.RichTextLabel>("Labels/BulletinLabel").GetParsedText().StartsWith("greetings human."), "replay reads the already rotated mask like the original");
            Equal(0x90894594u, Save.AlienTransmissions.LastMask, "replay rotates through the same thirty spaces");
            Equal(2, Save.AlienTransmissions.Stage, "replay does not advance stage");
            Equal(250, Save.AlienTransmissions.Countdown, "replay does not restart or decrement the delay");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
        }
    }
}
