using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using Newtonsoft.Json.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task OberonCourse()
        {
            foreach (var hull in new[] { Ship_Types.IOS, Ship_Types.SCG })
            {
                var interior = await OpenInterior(hull);
                var ship = interior.Ship;
                ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, Source = StellarBodies.earth, Destination = StellarBodies.mars,
                    SourceItems = new(), DestinationItems = new(), CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
                Press(interior, "SetCourse");
                var map = interior.GetNode("StarMap").GetChildren().OfType<StarMap>().Single();
                map.LoadMap(StellarBodies.uranus);
                await InputFrames();
                var point = map.MoonButtons[10].GetGlobalRect().GetCenter();
                PushGameInput(new InputEventMouseMotion { Position = point, GlobalPosition = point });
                foreach (var pressed in new[] { true, false })
                    PushGameInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed });
                await InputFrames();
                Equal(StellarBodies.oberon, map.CurrentLocation, "rightmost Uranus moon selects Oberon rather than Titania");
                await CaptureDisplayEvidence("oberon-course-" + hull);
                using var close = new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true };
                interior._Input(close);
                await InputFrames();
                Equal(StellarBodies.oberon, ship.DestinationPlanetLocation, "cockpit accepts Oberon course");
                Equal(StellarBodies.the_sun, ship.DestinationStarLocation, "Oberon is a local Sol destination");
                Equal(StellarBodies.earth, ship.ACC.Source, "ACC source preserved");
                Equal(StellarBodies.oberon, ship.ACC.Destination, "ACC endpoint follows course selection");
                var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save)).Ships.Single(s => s.ShipID == ship.ShipID);
                Equal(StellarBodies.oberon, loaded.DestinationPlanetLocation, "Oberon course survives save load");
                Equal(StellarBodies.oberon, loaded.ACC.Destination, "ACC route survives save load");
                Press(interior, "SetCourse");
                map = interior.GetNode("StarMap").GetChildren().OfType<StarMap>().Single();
                Equal(StellarBodies.oberon, map.SelectedMoon, "reopened cockpit selects Oberon");
                Equal(5, map.MoonButtons.Count(b => b.Visible), "all Uranus moons remain available");
                interior._Input(close);
                await InputFrames();
            }
        }

        private void OriginalMoonRoutes()
        {
            // Disk 1 $1C402: parent bit 0, then eleven satellite positions; $356F0 draws low bits first.
            var masks = new[] {
                (StellarBodies.earth, 0x0009), (StellarBodies.mercury, 0x0001), (StellarBodies.venus, 0x0001),
                (StellarBodies.mars, 0x0085), (StellarBodies.asteroids, 0x0001), (StellarBodies.jupiter, 0x0FBD),
                (StellarBodies.saturn, 0x0FD7), (StellarBodies.uranus, 0x0AC9), (StellarBodies.neptune, 0x04A5),
                (StellarBodies.pluto, 0x0009), (StellarBodies.decuria, 0x0001), (StellarBodies.atlantic, 0x0001),
                (StellarBodies.pacific, 0x0085), (StellarBodies.chiron, 0x0001), (StellarBodies.cercops, 0x0013),
                (StellarBodies.cerberus, 0x0269), (StellarBodies.creon, 0x0405), (StellarBodies.mycenae, 0x0001),
                (StellarBodies.tyre, 0x0009), (StellarBodies.thebes, 0x0B7B), (StellarBodies.pompeii, 0x0405),
                (StellarBodies.jericho, 0x0891), (StellarBodies.crete, 0x066B), (StellarBodies.mari, 0x0011),
                (StellarBodies.nero, 0x0001), (StellarBodies.julius, 0x0469), (StellarBodies.romulus, 0x0001),
                (StellarBodies.remus, 0x0001), (StellarBodies.helios, 0x0001), (StellarBodies.lithos, 0x0001),
                (StellarBodies.burah, 0x0203), (StellarBodies.sulfurum, 0x0909), (StellarBodies.titanes, 0x0575),
                (StellarBodies.zargun, 0x0B6D), (StellarBodies.osme, 0x02AB), (StellarBodies.radius, 0x0815),
                (StellarBodies.cambrian, 0x0001), (StellarBodies.cainozoic, 0x09B3), (StellarBodies.paleozoic, 0x0081),
                (StellarBodies.alpha, 0x0001), (StellarBodies.beta, 0x0009), (StellarBodies.gamma, 0x0425),
                (StellarBodies.epsilon, 0x0B71), (StellarBodies.zeta, 0x05BB) };
            InitializeUi();
            var map = OpenDepositMap();
            var visited = new HashSet<StellarBodies>();
            foreach (var (parent, mask) in masks)
            {
                map.LoadMap(parent);
                Equal(true, map.Planet.Texture != null, parent + " chart has its planet artwork");
                var suppliedImage = parent switch { StellarBodies.cercops => "Red_Rings", StellarBodies.creon => "White_Giant",
                    StellarBodies.paleozoic => "Yellow_Rings", _ => null };
                if (suppliedImage != null)
                    Equal("res://Sprites/SceneSprites/Map/Planet_" + suppliedImage + ".png", map.Planet.Texture.ResourcePath,
                        "chart resolves the supplied asset name");
                visited.Add(map.CurrentLocation);
                var moons = Save.BaseGameData.Planets.Values.Where(p => p.MoonParentPlanetId == parent).OrderBy(p => p.Order).ToList();
                Equal(moons.Count, map.MoonButtons.Count(b => b.Visible), parent + " exposes every defined moon");
                var member = 0;
                for (var slot = 0; slot < 11; slot++)
                {
                    var visible = (mask & (2 << slot)) != 0;
                    Equal(visible, map.MoonButtons[slot].Visible, parent + " original chart slot " + slot);
                    if (!visible) continue;
                    var moon = moons[member++];
                    map.MoonButtons[slot].EmitSignal(BaseButton.SignalName.Pressed);
                    Equal(moon.PlanetId, map.CurrentLocation, "moon button reaches its defined body");
                    Equal(moon.PlanetId, map.SelectedMoon, "deposit selection follows the moon");
                    Equal(moon.PlanetResources.Materials.First().MaterialType.ToScreenString(" "), map.DepositLabels[0].Text,
                        "deposit readout belongs to the selected moon");
                    Equal(true, visited.Add(map.CurrentLocation), "each moon has its own button");
                }
            }
            Equal(160, visited.Count, "all original bodies reached");
            Equal(true, visited.SetEquals(Save.BaseGameData.Planets.Keys), "no defined or artifact-eligible body is omitted");
        }

        private void LegacyMoonRoutes()
        {
            var corrections = new[] {
                (StellarBodies.mars, new[] { 1, 8 }, new[] { 1, 6 }),
                (StellarBodies.uranus, new[] { 1, 5, 6, 10 }, new[] { 2, 5, 6, 8, 10 }),
                (StellarBodies.julius, new[] { 1, 4, 5, 9 }, new[] { 2, 4, 5, 9 }) };
            foreach (var (parent, oldSlots, _) in corrections)
                Save.BaseGameData.Planets[parent].MoonList = oldSlots.Reverse().ToList();
            var before = JObject.Parse(SaveStorage.Serialize(Save));
            var loaded = SaveStorage.Deserialize(before.ToString());
            foreach (var (parent, _, slots) in corrections)
            {
                Equal(true, loaded.BaseGameData.Planets[parent].MoonList.SequenceEqual(slots), parent + " old default chart corrected");
                before["Game"]["BaseGameData"]["Planets"][parent.ToString()]["MoonList"] = new JArray(slots);
            }
            Equal(true, JToken.DeepEquals(before, JObject.Parse(SaveStorage.Serialize(loaded))), "only three chart definitions change; all campaign assets preserved");
            Equal(SaveStorage.Serialize(loaded), SaveStorage.Serialize(SaveStorage.Deserialize(SaveStorage.Serialize(loaded))), "chart migration is idempotent");
            foreach (var (parent, _, _) in corrections) Save.BaseGameData.Planets[parent].MoonList.Add(0);
            var custom = SaveStorage.Serialize(Save);
            Equal(custom, SaveStorage.Serialize(SaveStorage.Deserialize(custom)), "nonstandard charts are preserved");
        }
    }
}
