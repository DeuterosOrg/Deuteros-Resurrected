using System;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Utility;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunPlanetViewRegressions()
        {
            await CheckAsync("Orbital location view renders the supplied planet and station with original palette colours", OrbitalLocationPixels);
            await CheckAsync("Location previews inherit moon palettes and clear orbital art across ship transitions", OrbitalLocationTransitions);
        }

        private async Task OrbitalLocationPixels()
        {
            var screen = await OpenInterior(Ship_Types.IOS);
            GameCore.SingletonInstance.SetProcess(false);
            var ship = screen.Ship;
            ship.ShipState = Ship_States.UnDocked;
            ship.LocationView = true;
            var scenarios = new[]
            {
                (StellarBodies.earth, 0x0048, 0x008A, 0x0AFF),
                (StellarBodies.the_moon, 0x0048, 0x008A, 0x0AFF),
                (StellarBodies.mars, 0x0500, 0x0800, 0x0A00),
                (StellarBodies.uranus, 0x0050, 0x0080, 0x00A0),
                (StellarBodies.neptune, 0x0005, 0x0008, 0x000A),
                (StellarBodies.jupiter, 0x0860, 0x0A80, 0x0CA0),
                (StellarBodies.venus, 0x0A80, 0x0CA0, 0x0FD0),
                (StellarBodies.mercury, 0x0AAA, 0x0CCC, 0x0EEE),
                (StellarBodies.crete, 0x0FFF, 0x0684, 0x0462),
                (StellarBodies.julius, 0x0ACE, 0x0468, 0x068A)
            };
            foreach (var (body, first, second, third) in scenarios)
            {
                ship.PlanetLocation = body;
                var planet = Save.BaseGameData.Planets[body];
                planet.ActiveMethanoid = false;
                foreach (var station in new[] { false, true })
                {
                    planet.Station.Built = station;
                    planet.Station.BuildParts = station ? 8 : 0;
                    screen.UpdateState();
                    var display = screen.GetNode<TextureRect>("Location/BigLocation");
                    Equal(true, display.Visible && display.Texture != null, "orbit view has artwork: " + body);
                    var path = "res://Sprites/Scenes/Ship_View_" + (station ? "Station" : "Planet") + ".png";
                    using var source = (Image)GD.Load<Texture2D>(path).GetImage().Duplicate();
                    using var actual = (Image)display.Texture.GetImage().Duplicate();
                    Equal(source.GetSize(), actual.GetSize(), "supplied artwork dimensions retained");
                    var changed = 0;
                    for (var y = 0; y < source.GetHeight(); y++)
                        for (var x = 0; x < source.GetWidth(); x++)
                        {
                            var color = source.GetPixel(x, y);
                            var expected = color;
                            var hex = color.ToHtml(false);
                            var rgb4 = hex switch { "004080" => first, "0080a0" => second, "a0e0e0" => third, _ => -1 };
                            if (rgb4 >= 0)
                            {
                                expected = new Color(((rgb4 >> 8) & 15) / 15f, ((rgb4 >> 4) & 15) / 15f, (rgb4 & 15) / 15f);
                                changed++;
                            }
                            Equal(expected.ToHtml(), actual.GetPixel(x, y).ToHtml(), "only original palette pixels change");
                        }
                    Equal(true, changed > 0, "check includes palette pixels");
                    if ((body == StellarBodies.the_moon && station) || (body == StellarBodies.mars && !station))
                        await CheckLocationRender(display, actual, body.ToString());
                }
            }
        }
        private async Task OrbitalLocationTransitions()
        {
            foreach (var hull in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                var screen = await OpenInterior(hull, true);
                GameCore.SingletonInstance.SetProcess(false);
                var ship = screen.Ship;
                ship.ShipState = Ship_States.UnDocked;
                screen.UpdateState();
                var small = screen.GetNode<TextureButton>("Location/SmallLocation");
                var large = screen.GetNode<TextureRect>("Location/BigLocation");
                var earth = small.TextureNormal;
                Equal("004488", earth.GetImage().GetPixel(23, 0).ToHtml(false), "Earth preview uses original first sky colour");
                ship.PlanetLocation = StellarBodies.the_moon;
                var moon = Save.BaseGameData.Planets[StellarBodies.the_moon];
                moon.Station.Built = true;
                moon.Station.BuildParts = 8;
                screen.UpdateState();
                Equal(earth.GetInstanceId(), small.TextureNormal.GetInstanceId(), "moon shares cached parent palette with local station");
                moon.Station.Built = false;
                moon.Station.BuildParts = 0;
                screen.UpdateState();
                Equal(false, earth.GetInstanceId() == small.TextureNormal.GetInstanceId(), "moon station presence is local, not inherited");
                Press(screen, "Location/SmallLocation");
                Equal(true, ship.LocationView && large.Visible && large.Texture != null, "location control opens orbital view");
                var cacheSize = Deuteros.Code.Platform.Helpers.SpriteManager.ImageCache.Count;
                screen.UpdateState();
                Equal(cacheSize, Deuteros.Code.Platform.Helpers.SpriteManager.ImageCache.Count, "refresh reuses converted texture");
                var saved = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                Equal(true, saved.Ships.Single(s => s.ShipID == ship.ShipID).LocationView, "view choice survives save round trip");
                ship.ShipState = Ship_States.Docking;
                screen.UpdateState();
                Equal(true, large.Texture != null, "docking retains orbital view");
                foreach (var (state, suffix) in new[]
                {
                    (Ship_States.Docked, "Docked"), (Ship_States.InTransit, "Travel"), (Ship_States.Launching, "StormDoors")
                })
                {
                    ship.ShipState = state;
                    screen.UpdateState();
                    Equal(true, large.Texture.ResourcePath.EndsWith("BigLocation_" + suffix + ".png"), "large view clears orbit palette: " + state);
                    ship.LocationView = false;
                    screen.UpdateState();
                    Equal(true, small.TextureNormal.ResourcePath.EndsWith("SmallLocation_" + suffix + ".png"), "preview clears orbit palette: " + state);
                    ship.LocationView = true;
                }
                ship.ShipState = Ship_States.UnDocked;
                ship.PlanetLocation = StellarBodies.asteroids;
                screen.UpdateState();
                Equal(true, large.Texture.ResourcePath.EndsWith("Ship_View_Asteroids.png"), "asteroid field uses supplied dedicated artwork");
            }
        }

        private async Task CheckLocationRender(TextureRect display, Image expected, string name)
        {
            if (DisplayServer.GetName() == "headless") return;
            Cursor.Hide();
            // Texture changes also queue minimum-size/layout updates before drawing.
            await InputFrames();
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var rendered = GetViewport().GetTexture().GetImage();
            var rect = display.GetGlobalRect();
            var foreground = new[] { "Background", "EngineControls", "LandingBlank" }
                .Select(path => ActiveScreen<ShipInterior>().GetNode<TextureRect>(path))
                .Where(control => control.Visible)
                .Select(control => (Rect: control.GetGlobalRect(), Image: (Image)control.Texture.GetImage().Duplicate())).ToArray();
            GD.Print($"ORBIT RECT: {rect}; viewport {rendered.GetSize()}; source {expected.GetSize()}");
            await CaptureDisplayEvidence("orbit-before-check-" + name);
            for (var y = 0; y < expected.GetHeight(); y++)
                for (var x = 0; x < expected.GetWidth(); x++)
                {
                    var want = expected.GetPixel(x, y);
                    var point = rect.Position + new Vector2((x + 0.5f) * rect.Size.X / expected.GetWidth(), (y + 0.5f) * rect.Size.Y / expected.GetHeight());
                    foreach (var layer in foreground)
                        if (layer.Rect.HasPoint(point))
                        {
                            var local = (point - layer.Rect.Position) / layer.Rect.Size * layer.Image.GetSize();
                            want = want.Blend(layer.Image.GetPixel((int)local.X, (int)local.Y));
                        }
                    var actual = ReadGamePixel(rendered, point);
                    if (Math.Abs(want.R - actual.R) > 1.1f / 255 || Math.Abs(want.G - actual.G) > 1.1f / 255 || Math.Abs(want.B - actual.B) > 1.1f / 255)
                        throw new InvalidOperationException($"Orbit render {name} ({x},{y}): expected {want}, got {actual}");
                }
            foreach (var layer in foreground) layer.Image.Dispose();
            GD.Print($"NATIVE ORBIT PIXELS: {name} {expected.GetWidth() * expected.GetHeight()} passed");
            await CaptureDisplayEvidence("orbit-" + name);
        }

    }
}
