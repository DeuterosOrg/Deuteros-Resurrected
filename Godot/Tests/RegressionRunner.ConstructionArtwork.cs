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

        private async Task RunRecoveredConstructionRegressions()
        {
            var items = new[] { ItemTypes.pulse_blaster_laser, ItemTypes.g_chassis, ItemTypes.star_drive,
                ItemTypes.prejudice_torpedo_launcher, ItemTypes.star_drone, ItemTypes.prison_pod, ItemTypes.sonic_blaster,
                ItemTypes.s__d__m, ItemTypes.m__t__x };
            for (var row = 0; row < items.Length; row++)
            {
                var selected = row;
                foreach (var automated in new[] { false, true })
                    await CheckAsync($"{items[row]} recovered original stages follow paid production automated={automated}",
                        () => ConstructionArtwork(items[selected], "RecoveredConstruction", automated, selected));
            }
        }

        private async Task ConstructionArtwork(ItemTypes type, string sheet, bool automated, int recoveredRow = -1)
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
            var installation = type == ItemTypes.s__d__m || type == ItemTypes.m__t__x;
            Func<bool> completed = () => type == ItemTypes.s__d__m ? station.SdmInstalled
                : type == ItemTypes.m__t__x ? station.MtxInstalled : station.Resources.Stores[type] > before;
            GameCore.SingletonInstance.ChangeScene(Scenes.Production, new List<SceneVariables> { SceneVariables.Orbit });
            var screen = ActiveScreen<Production>();
            screen.Buttons.Single(b => b.ObjectData?.ItemType == type).EmitSignal(BaseButton.SignalName.Pressed);
            var image = screen.GetNode<TextureRect>("Sprites/ItemProgressImage");
            var seen = new HashSet<int>();
            for (var day = 0; day < 100 && !completed(); day++)
            {
                var order = factory.CurrentProductionItem();
                if (order != null && seen.Add(order.Production_Complete))
                {
                    screen.DrawData();
                    Equal(true, image.Texture is AtlasTexture, "construction uses its stage rather than the static illustration");
                    var atlas = (AtlasTexture)image.Texture;
                    Equal("res://Sprites/Items/Sheets/" + sheet + ".png", atlas.Atlas.ResourcePath, "correct unmodified source sheet");
                    Equal(recoveredRow < 0 ? new Rect2(8 + (order.Production_Complete - 1) * 72, 8, 64, 56)
                        : new Rect2((order.Production_Complete - 1) * 64, recoveredRow * 48, 64, 48), atlas.Region, "frame order and crop");
                    Equal(new Vector2(64, 56), image.Size, "construction remains within its display bounds");
                    Equal(true, image.Material is ShaderMaterial, "source-sheet background is masked at display time");
                    await CheckConstructionPixels(image, atlas);
                    if (recoveredRow >= 0)
                    {
                        var small = screen.GetNode<TextureRect>("Sprites/SmallItemImage");
                        Equal(true, small.Texture != null, "recovered item has a research illustration");
                        if (new[] { ItemTypes.pulse_blaster_laser, ItemTypes.prejudice_torpedo_launcher, ItemTypes.prison_pod, ItemTypes.sonic_blaster }.Contains(type))
                            await CheckResearchPixels(small);
                    }
                    if (!automated) await CaptureDisplayEvidence(type + "-stage-" + order.Production_Complete);
                }
                Production.UpdateProduction(Save.CurrentDay, ++Save.CurrentDay);
                screen.DrawData();
            }
            Equal("1,2,3", string.Join(',', seen.OrderBy(stage => stage)), "normal production displays all stages");
            Equal(true, completed(), "paid production completes");
            Equal(before + (installation ? 0 : 1), station.Resources.Stores[type], "one item delivered or installed locally without extra stock");
            foreach (var recipe in item.BuildRequirements) Equal(0, station.Resources.Stores[recipe.ItemType], "artwork does not alter recipe charges");
            Equal(true, image.Texture.ResourcePath.EndsWith("/idle.png"), "completed production restores idle artwork");
            Equal(true, image.Material == null, "idle artwork does not retain source-sheet material");

            foreach (var fallback in new[] { ItemTypes.derrick, ItemTypes.m__f__l })
            {
                factory.ProductionQueue.Clear();
                factory.ProductionQueue.Add(new ProductionItem(GameCore.SingletonInstance.GameData.GetItem(fallback)) { Active = true, Production_Complete = 2 });
                screen.DrawData();
                var expected = fallback == ItemTypes.derrick ? "/Production/derrick_2.png" : "/Production/Illustrations/m__f__l.png";
                Equal(true, image.Texture.ResourcePath.EndsWith(expected), "existing frame/static fallback still renders");
                Equal(true, image.Material == null, "PNG artwork has no source-sheet material");
                if (recoveredRow >= 0 && fallback == ItemTypes.m__f__l)
                    await CheckResearchPixels(screen.GetNode<TextureRect>("Sprites/SmallItemImage"));
            }
        }

        private async Task CheckResearchPixels(TextureRect control, bool hasOpaqueBlack = true, bool transparentBackground = true)
        {
            Equal(true, control.Texture.GetSize().X <= 48, "illustration retains original draw width");
            if (DisplayServer.GetName() == "headless") return;
            Cursor.Hide();
            control.Visible = false;
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var background = GetViewport().GetTexture().GetImage();
            control.Visible = true;
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var rendered = GetViewport().GetTexture().GetImage();
            using var source = (Image)control.Texture.GetImage().Duplicate();
            var scale = rendered.GetSize() / GetViewport().GetVisibleRect().Size;
            var rect = control.GetGlobalRect();
            var transparent = 0;
            var opaqueBlack = 0;
            for (var y = 0; y < source.GetHeight(); y++)
                for (var x = 0; x < source.GetWidth(); x++)
                {
                    var sample = source.GetPixel(x, y);
                    var px = (int)((rect.Position.X + x + 0.5f) * scale.X);
                    var py = (int)((rect.Position.Y + y + 0.5f) * scale.Y);
                    var expected = sample.A == 0 ? background.GetPixel(px, py) : sample;
                    Equal(expected.ToHtml(false), rendered.GetPixel(px, py).ToHtml(false), $"research pixel ({x},{y})");
                    if (sample.A == 0) transparent++;
                    else if (sample.R == 0 && sample.G == 0 && sample.B == 0) opaqueBlack++;
                }
            Equal(true, (transparentBackground ? transparent > 0 : transparent == 0) && (!hasOpaqueBlack || opaqueBlack > 0), "index zero is transparent but index five remains opaque black");
            GD.Print($"NATIVE PIXELS: recovered research illustration: {source.GetWidth() * source.GetHeight()} passed");
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
            var height = (int)atlas.Region.Size.Y;
            for (var y = 0; y < height; y++)
                for (var x = 0; x < 64; x++)
                {
                    var sample = source.GetPixel((int)atlas.Region.Position.X + x, (int)atlas.Region.Position.Y + y);
                    var px = (int)((rect.Position.X + x + 0.5f) * scale.X);
                    var py = (int)((rect.Position.Y + (56 - height) / 2 + y + 0.5f) * scale.Y);
                    var key = sample.R == 1 && sample.G == 0 && sample.B == 1;
                    var expected = key ? background.GetPixel(px, py) : sample;
                    var actual = rendered.GetPixel(px, py);
                    if (Math.Abs(actual.R - expected.R) > 1.1f / 255 || Math.Abs(actual.G - expected.G) > 1.1f / 255 || Math.Abs(actual.B - expected.B) > 1.1f / 255)
                        throw new InvalidOperationException($"Construction pixel ({x},{y}), background={key}: expected {expected}, got {actual}");
                    if (key) masked++; else artwork++;
                }
            Equal(true, artwork > 0 && (height == 48 || masked > 0), "render check covers all pixels and source-sheet transparency where present");
            GD.Print($"NATIVE PIXELS: {artwork} artwork and {masked} transparent-background samples passed");
        }
    }
}
