using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
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
            Save.Ships.Add(ship);
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
    }
}
