using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Godot;
using Newtonsoft.Json;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunProductionRodRegressions()
        {
            foreach (var ground in new[] { true, false })
                foreach (var automated in new[] { false, true })
                    await CheckAsync($"Production rod preserves original frames, cadence and active gate ground={ground} automated={automated}",
                        () => ProductionRod(ground, automated));
        }

        private async Task ProductionRod(bool ground, bool automated)
        {
            InitializeUi();
            DisableFuelRefining();
            Save.CurrentPlanet = StellarBodies.earth;
            var earth = GameCore.Earth;
            earth.Station.Built = true;
            var factory = ground ? earth.Factory : earth.Station.Factory;
            factory.AOC = automated;
            // The original gate follows the current product even without staff/materials.
            factory.Builder = null;
            var order = new ProductionItem(GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick))
                { Active = true, AOCRepeat = automated, Production_Value = 100 };
            factory.ProductionQueue.Add(order);
            var state = JsonConvert.SerializeObject(factory);
            var variables = new List<SceneVariables> { ground ? SceneVariables.Ground : SceneVariables.Orbit };
            GameCore.SingletonInstance.ChangeScene(Scenes.Production, variables);
            var screen = ActiveScreen<Production>();
            screen.SetProcess(false);
            var rod = screen.GetNodeOrNull<AnimatedSprite2D>("Sprites/ProductionRod");
            Equal(true, rod != null, "production rod exists separately from construction artwork");
            Equal(new Vector2(240, 96), rod.GlobalPosition, "original draw record position");
            Equal(false, rod.Centered, "position is the upper left pixel");
            Equal(2, rod.Frame, "static background starts at resource 128");
            var hashes = new[] {
                "100979511b9dd7d25586aeb9bf9a87c04fd2f0ead2ef1c1a164dc82223c4d406",
                "ba6322943df714d6cac9f1430d2075163c285b8d897888dc2c71ddd57674273a",
                "ca90fe82203780910d2d8af0116a2d6e8957589e23ecbedca593afba2980fa90" };
            var palette = new[] { "000000", "a0a060", "808040", "606020", "404000" };
            for (var frame = 0; frame < 3; frame++)
            {
                var atlas = (AtlasTexture)rod.SpriteFrames.GetFrameTexture("default", frame);
                using var source = (Image)atlas.Atlas.GetImage().Duplicate();
                var pixels = new byte[288];
                for (var y = 0; y < 9; y++)
                    for (var x = 0; x < 32; x++)
                        pixels[y * 32 + x] = checked((byte)Array.IndexOf(palette,
                            source.GetPixel((int)atlas.Region.Position.X + x, (int)atlas.Region.Position.Y + y).ToHtml(false)));
                Equal(hashes[frame], Convert.ToHexString(SHA256.HashData(pixels)).ToLowerInvariant(), "decoded original frame pixels");
            }
            // Original VBlank startup differs from a uniform looping sprite animation.
            var expected = new[] { 2, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0 };
            for (var tick = 0; tick < expected.Length; tick++)
            {
                Save.TimeSkip = tick % 2 == 0;
                screen._Process(0.01);
                screen._Process(0.01);
                Save.TimeSkip = false;
                Equal(expected[tick], rod.Frame, "original VBlank " + (tick + 1));
                screen.DrawData();
                Equal(expected[tick], rod.Frame, "redraw preserves phase");
                if (tick == 1 || tick == 3 || tick == 7) await CheckRodPixels(rod, ground, automated);
            }
            screen._Process(0.04); // Counter 2, resource 130.
            order.Active = false;
            screen._Process(1);
            Equal(0, rod.Frame, "idle queue does not animate");
            order.Active = true;
            screen._Process(0.04);
            Equal(1, rod.Frame, "resuming retains counter");
            GetTree().Paused = true;
            try
            {
                screen._Process(0.08);
                Equal(1, rod.Frame, "tree pause stops animation");
            }
            finally { GetTree().Paused = false; }
            screen._Process(0.08);
            Equal(2, rod.Frame, "resume advances by active elapsed time only");
            Equal(state, JsonConvert.SerializeObject(factory), "animation never mutates production/save state");

            GameCore.SingletonInstance.ChangeScene(Scenes.Store, variables);
            GameCore.SingletonInstance.ChangeScene(Scenes.Production, variables);
            screen = ActiveScreen<Production>();
            screen.SetProcess(false);
            rod = screen.GetNode<AnimatedSprite2D>("Sprites/ProductionRod");
            screen._Process(0.04);
            Equal(2, rod.Frame, "navigation retains counter 10 rather than restarting");
            screen._Process(0.04);
            Equal(0, rod.Frame, "navigation resumes original cycle");
            factory.ProductionQueue.Clear();
            screen._Process(1);
            Equal(0, rod.Frame, "completion/cancellation with no current product stops animation");
            var otherFactory = ground ? earth.Station.Factory : earth.Factory;
            otherFactory.ProductionQueue.Add(new ProductionItem(order.Product) { Active = true });
            screen._Process(1);
            Equal(0, rod.Frame, "another factory cannot animate the selected factory's rod");
            factory.ProductionQueue.Add(order);
            screen.SetProcess(true);
            var observed = new HashSet<int>();
            for (var sample = 0; sample < 8; sample++)
            {
                await ToSignal(GetTree().CreateTimer(0.05), SceneTreeTimer.SignalName.Timeout);
                observed.Add(rod.Frame);
            }
            screen.SetProcess(false);
            Equal(true, observed.Count > 1, "normal scene processing visibly advances the rod");
            factory.ProductionQueue.Clear();
            otherFactory.ProductionQueue.Clear();
        }

        private async Task CheckRodPixels(AnimatedSprite2D rod, bool ground, bool automated)
        {
            if (DisplayServer.GetName() == "headless") return;
            Cursor.Hide();
            await InputFrames();
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var rendered = GetViewport().GetTexture().GetImage();
            var atlas = (AtlasTexture)rod.SpriteFrames.GetFrameTexture("default", rod.Frame);
            using var source = (Image)atlas.Atlas.GetImage().Duplicate();
            var scale = rendered.GetSize() / GetViewport().GetVisibleRect().Size;
            for (var y = 0; y < 9; y++)
                for (var x = 0; x < 32; x++)
                {
                    var expected = source.GetPixel((int)atlas.Region.Position.X + x, (int)atlas.Region.Position.Y + y);
                    var actual = rendered.GetPixel((int)((240 + x + 0.5f) * scale.X), (int)((96 + y + 0.5f) * scale.Y));
                    Equal(expected.ToHtml(false), actual.ToHtml(false), $"native rod pixel ({x},{y})");
                }
            GD.Print($"NATIVE PIXELS: production rod frame {rod.Frame}: 288 passed");
            await CaptureDisplayEvidence($"production-rod-{ground}-{automated}-{rod.Frame}");
        }
    }
}
