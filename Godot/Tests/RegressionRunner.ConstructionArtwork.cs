using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunConstructionArtworkRegressions()
        {
            foreach (var (item, sheet) in new[] {
                (ItemTypes.i_chassis, "item_interplanetary_chassis"),
                (ItemTypes.i_drive, "item_interplanetary_drive"),
                (ItemTypes.r_frame, "item_resource_frame") })
                foreach (var automated in new[] { false, true })
                    await CheckAsync($"{item} construction follows all supplied frames and returns to idle automated={automated}",
                        () => ConstructionArtwork(item, sheet, automated));
        }

        private async Task ConstructionArtwork(ItemTypes type, string sheet, bool automated)
        {
            InitializeUi();
            DisableFuelRefining();
            Save.CurrentPlanet = StellarBodies.earth;
            var station = GameCore.Earth.Station;
            station.Built = true;
            station.BuildParts = 8;
            var factory = station.Factory;
            factory.AOC = automated;
            factory.Builder = new Staff { Type = StaffType.Production, Count = 200, Leader = "Frame test" };
            factory.Builder.AddAction(20);
            var item = GameCore.SingletonInstance.GameData.GetItem(type);
            item.Locked = item.Research.Locked = false;
            item.Research.Researched = true;
            foreach (var recipe in item.BuildRequirements) station.Resources.Stores[recipe.ItemType] = recipe.ItemCount;
            var before = station.Resources.Stores[type];
            GameCore.SingletonInstance.ChangeScene(Scenes.Production, new List<SceneVariables> { SceneVariables.Orbit });
            var screen = ActiveScreen<Production>();
            screen.Buttons.Single(b => b.ObjectData?.ItemType == type).EmitSignal(BaseButton.SignalName.Pressed);
            var image = screen.GetNode<TextureRect>("Sprites/ItemProgressImage");
            var seen = new HashSet<int>();
            for (var day = 0; day < 100 && station.Resources.Stores[type] == before; day++)
            {
                var order = factory.CurrentProductionItem();
                if (order != null && seen.Add(order.Production_Complete))
                {
                    screen.DrawData();
                    Equal(true, image.Texture is AtlasTexture, "construction uses its stage rather than the static illustration");
                    var atlas = (AtlasTexture)image.Texture;
                    Equal("res://Sprites/Items/Sheets/" + sheet + ".png", atlas.Atlas.ResourcePath, "correct unmodified source sheet");
                    Equal(new Rect2(8 + (order.Production_Complete - 1) * 72, 8, 64, 56), atlas.Region, "frame order and crop");
                    Equal(new Vector2(64, 56), image.Size, "construction remains within its display bounds");
                    Equal(true, image.Material is ShaderMaterial, "source-sheet background is masked at display time");
                    await CheckConstructionPixels(image, atlas);
                    if (!automated) await CaptureDisplayEvidence(type + "-stage-" + order.Production_Complete);
                }
                Production.UpdateProduction(Save.CurrentDay, ++Save.CurrentDay);
                screen.DrawData();
            }
            Equal("1,2,3", string.Join(',', seen.OrderBy(stage => stage)), "normal production displays all stages");
            Equal(before + 1, station.Resources.Stores[type], "one completed item delivered");
            foreach (var recipe in item.BuildRequirements) Equal(0, station.Resources.Stores[recipe.ItemType], "artwork does not alter recipe charges");
            Equal(true, image.Texture.ResourcePath.EndsWith("/idle.png"), "completed production restores idle artwork");
            Equal(true, image.Material == null, "idle artwork does not retain source-sheet material");

            foreach (var fallback in new[] { ItemTypes.derrick, ItemTypes.m__t__x })
            {
                factory.ProductionQueue.Clear();
                factory.ProductionQueue.Add(new ProductionItem(GameCore.SingletonInstance.GameData.GetItem(fallback)) { Active = true, Production_Complete = 2 });
                screen.DrawData();
                var expected = fallback == ItemTypes.derrick ? "/Production/derrick_2.png" : "/Research/m__t__x.png";
                Equal(true, image.Texture.ResourcePath.EndsWith(expected), "existing frame/static fallback still renders");
                Equal(true, image.Material == null, "PNG artwork has no source-sheet material");
            }
        }

        private async Task CheckConstructionPixels(TextureRect control, AtlasTexture atlas)
        {
            if (DisplayServer.GetName() == "headless") return;
            Cursor.Hide();
            control.Visible = false;
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var background = GetViewport().GetTexture().GetImage();
            control.Visible = true;
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var rendered = GetViewport().GetTexture().GetImage();
            using var source = atlas.Atlas.GetImage();
            var scale = rendered.GetSize() / GetViewport().GetVisibleRect().Size;
            var rect = control.GetGlobalRect();
            var masked = 0;
            var artwork = 0;
            for (var y = 0; y < 56; y++)
                for (var x = 0; x < 64; x++)
                {
                    var sample = source.GetPixel((int)atlas.Region.Position.X + x, (int)atlas.Region.Position.Y + y);
                    var px = (int)((rect.Position.X + x + 0.5f) * scale.X);
                    var py = (int)((rect.Position.Y + y + 0.5f) * scale.Y);
                    var key = sample.R == 1 && sample.G == 0 && sample.B == 1;
                    var expected = key ? background.GetPixel(px, py) : sample;
                    var actual = rendered.GetPixel(px, py);
                    if (Math.Abs(actual.R - expected.R) > 1.1f / 255 || Math.Abs(actual.G - expected.G) > 1.1f / 255 || Math.Abs(actual.B - expected.B) > 1.1f / 255)
                        throw new InvalidOperationException($"Construction pixel ({x},{y}), background={key}: expected {expected}, got {actual}");
                    if (key) masked++; else artwork++;
                }
            Equal(true, masked > 0 && artwork > 0, "render check covers background and visible artwork");
            GD.Print($"NATIVE PIXELS: {artwork} artwork and {masked} transparent-background samples passed");
        }
    }
}
