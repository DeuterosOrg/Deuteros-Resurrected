using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code.Utility;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;
using StoreScreen = Deuteros.Code.Platform.Screens.Store;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunResearchOnlyRecipeRegressions()
        {
            foreach (var ground in new[] { true, false })
                CheckUi($"Stores reject saved research-only recipe selection ground={ground}", () => ResearchOnlyStores(ground));
            CheckUi("Production excludes research-only technology and rejects its resource check", ResearchOnlyProduction);
            await CheckAsync("Stores recipe buttons remain usable after every item has been researched", AllStoreRecipes);
            Check("Automatic fuel refining still consumes real recipes outside manual production", AutomaticFuelRecipes);
        }

        private async Task RunResearchDetailRegressions()
        {
            foreach (var restored in new[] { false, true })
                await CheckAsync($"Research-only completion displays safely and physical recipes recover restored={restored}",
                    () => ResearchOnlyDetails(restored));
        }

        private async Task ResearchOnlyDetails(bool restored)
        {
            InitializeUi();
            var hyperlight = CompleteHyperlightForRecipeTest();
            if (restored)
            {
                GameCore.Earth.CurrentResearchItem = hyperlight.Research;
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            }
            else
            {
                hyperlight.Research.Researched = false;
                hyperlight.Research.ResearchPercentageComplete = 99;
                hyperlight.Research.ResearchValue = 255;
                GameCore.Earth.ResearchStaff = new Staff { Type = StaffType.Research, Count = 200, Leader = "Research" };
                GameCore.Earth.ResearchStaff.AddAction(9);
            }
            GameCore.SingletonInstance.ChangeScene(Scenes.Earth_Research, new List<SceneVariables> { SceneVariables.Ground });
            var research = ActiveScreen<Research>();
            if (!restored)
            {
                research.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.hyperlight).EmitSignal(BaseButton.SignalName.Pressed);
                Research.UpdateResearch(0, 1);
                Equal(true, hyperlight.Research.Researched, "normal research simulation completes the technology");
                research.UpdateResearchButton(true);
            }
            research.DrawData(false);
            Equal("Research complete", research.GetNode<Label>("Labels/Researched/ItemNotesLabel").Text, "technology is not offered for manufacture");
            foreach (var label in new[] { "MassLabel", "MassDataLabel", "ProductionAmountListLabel", "ProductionMaterialListLabel", "ItemNotesDataLabel" })
                Equal("", research.GetNode<Label>("Labels/Researched/" + label).Text, "no fictional technology recipe or mass: " + label);
            Equal("Hyperlight Travel", research.GetNode<Label>("Labels/ItemNameLabel").Text, "technology details remain visible");
            await InputFrames();
            await CaptureDisplayEvidence("research-hyperlight-" + restored);

            foreach (var type in new[] { ItemTypes.derrick, ItemTypes.bandaid, ItemTypes.pulse_blaster_laser,
                ItemTypes.m__f__l, ItemTypes.prejudice_torpedo_launcher, ItemTypes.prison_pod, ItemTypes.sonic_blaster })
            {
                var item = GameCore.SingletonInstance.GameData.GetItem(type);
                item.Locked = item.Research.Locked = false;
                item.Research.Researched = true;
                research.Buttons.Single(b => b.ObjectData?.ItemType == type).EmitSignal(BaseButton.SignalName.Pressed);
                research.DrawData(false);
                Equal(string.Join('\n', item.BuildRequirements.Select(r => r.ItemCount)),
                    research.GetNode<Label>("Labels/Researched/ProductionAmountListLabel").Text, "physical recipe returns after technology");
                Equal(item.Mass.ToString(), research.GetNode<Label>("Labels/Researched/MassDataLabel").Text, "physical mass returns");
                var image = research.GetNode<TextureRect>("Sprites/ResearchImage");
                Equal(true, image.Texture != null, "completed item illustration loads");
                Equal(48f, image.Texture.GetSize().X, "recovered/canonical illustration width");
                await InputFrames();
                if (type == ItemTypes.pulse_blaster_laser) await CaptureDisplayEvidence("research-recovered-" + restored);
            }
            research.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.hyperlight).EmitSignal(BaseButton.SignalName.Pressed);
            research.DrawData(false);
            Equal("", research.GetNode<Label>("Labels/Researched/ProductionAmountListLabel").Text, "switching back clears prior recipe");
            Equal("Research complete", research.GetNode<Label>("Labels/Researched/ItemNotesLabel").Text, "repeat selection remains safe");
        }

        private async Task RecoveredResearchDiagrams()
        {
            InitializeUi();
            var rows = new[] {
                (ItemTypes.alien_artifact, 32, 3, "2ea9ab9198d1638007400cd2c3bef1cc745b864b76011a0e1bc52180ac6452d4"),
                (ItemTypes.derrick, 48, 43, "b73c1c9db478c46a0f916fa3d71c93ff58d34dca0a8ee4c29deba1c845f4fb41"),
                (ItemTypes.s_chassis, 48, 32, "4e914d1a952c7f79f7396353e856e9c555e7c41b5ccd0b6e213153843d42d8fc"),
                (ItemTypes.s_drive, 48, 37, "8fb12b8e944222606f320ba87832efb0cc6a07d236a14ea1ca3a6340e2b67316"),
                (ItemTypes.meh_fuel, 48, 31, "f4666348c826e2e5d878cb900361ddef18e288bc26c7434caa45519c3c4e5d93"),
                (ItemTypes.of_frame, 48, 27, "8d5f585eb2a987c4dbf58cb421655dc885996f94618d7452056f4d6d79e038b9"),
                (ItemTypes.supply_pod, 48, 36, "9ea1201e72123a32f292b8d68d8e752c9e833d9701c5c914153fbb80d24ff14f"),
                (ItemTypes.tool_pod, 48, 27, "0dbe748c9f3c3fbc6f01db530479c9d3b64eeb06b618b1bc3c456c152c0c50f4"),
                (ItemTypes.cryo_pod, 48, 35, "37f902cc1847afc10609e5f522036265573816f95966846c9a148e3b4a1c0b77"),
                (ItemTypes.pulse_blaster_laser, 48, 37, "0c156d79e59da78dfac5d223ef23672218ae584206cbbe7868237e2924c6a6d0"),
                (ItemTypes.i_chassis, 48, 37, "9ecb34f0534b7c973149279a845638c57005ef0abe60f0f0797ff0936a75d145"),
                (ItemTypes.i_drive, 48, 37, "8fb12b8e944222606f320ba87832efb0cc6a07d236a14ea1ca3a6340e2b67316"),
                (ItemTypes.g_chassis, 48, 45, "ae52819493391ec6b5888b61f2d5fd6ced2d86be211e76e4cf900c92b8b32475"),
                (ItemTypes.star_drive, 48, 36, "3980bbcff8863b5b226ee02a74c8345254717159b9c7c6488668bcba28b4dab4"),
                (ItemTypes.hed_fuel, 48, 32, "dc20ab2a42eea3c21a70aae7cc7b95b2c23fec023cec98814bfcce3fc59882bf"),
                (ItemTypes.a__c__c, 48, 42, "102e919670ba8ed37dab96b616a96058998c9d53f77968a23197e111aae4e13f"),
                (ItemTypes.a__o__c, 48, 33, "62b0c54ff3b6f75fd0745f7c3b00227e84d7b22fe5c720a64740c299e4e072af"),
                (ItemTypes.bandaid, 48, 32, "f8eae054b82aebb03df65da8d4fed3fce27e3042b81354cc94e548edff2adcfb"),
                (ItemTypes.s__d__m, 48, 35, "2616f691c6753bfde7dd1b58b007f09c23e66c3d6cfec6248c11a1fa25736294"),
                (ItemTypes.grapple, 48, 37, "ed9c8ebaa60ef361d8deaabe9f7123bc741bd5ea6dadba76cc1dc38dcb0dca8e"),
                (ItemTypes.d__f__c__c, 48, 37, "f454c87ae41f0d9e20f4ba462efb3d0c37d0fb544a846a7100f35e9f06a507d6"),
                (ItemTypes.a__m__a, 48, 36, "8f44a33d0a0a2a85e27036f0c63f88b081bb78cbbc6121643d54a9d0fa0361ac"),
                (ItemTypes.hyperlight, 48, 32, "bb47669e84fd50ba8b0be392a7bb0104ea0e53eefc38c8577a732008d878ebf4"),
                (ItemTypes.m__t__x, 48, 41, "ca441fe2018b5c829eb4c5b91010332ac702b3814e10b433a976ee70f44214b8"),
                (ItemTypes.m__f__l, 48, 35, "cedba9c65f70d60d4cab167245956fc9ca081210c460cf49dad093abe6e95d86"),
                (ItemTypes.r_frame, 48, 33, "205533d6af9fc2f20156505ba9955c17050ace1fa06fb286a22d15c0094622ca"),
                (ItemTypes.prejudice_torpedo_launcher, 48, 36, "88be76a775f056d928fb0743369893ed1a550b7f639bae4283c29c33682de13c"),
                (ItemTypes.commspod, 48, 30, "dcff93432cc454d3790ae3e6d373fa377d36b086e0bf4734b426aa1922e2d516"),
                (ItemTypes.ios_drone, 48, 33, "81963ac5779f991c7df9565b59f2d982168621ee1f3105729f810d12a7873d94"),
                (ItemTypes.star_drone, 48, 33, "02dd536a74db810a8934ac20864ad638e02c4665f6e7f2b8fca33b6084132436"),
                (ItemTypes.prison_pod, 48, 30, "a80efa4d39f9694eec21411a6fd84e8bacadf3d1ed53f9287822205c01b7ed2d"),
                (ItemTypes.sonic_blaster, 48, 37, "f894d503a27b75cf6cb409b5dc62f47202b929aac056afb724fb1bff7aab3201") };
            var palette = new[] { "00000000", "a0a060ff", "808040ff", "606020ff", "404000ff", "000000ff",
                "002040ff", "004060ff", "002080ff", "00c0f0ff", "008000ff", "806000ff", "f0f000ff", "f00000ff", "800000ff", "f0f0f0ff" };
            var nextOrder = Save.BaseGameData.ItemList.Where(i => i.Research != null).Max(i => i.Research.ResearchOrder) + 1;
            foreach (var (type, width, height, hash) in rows)
            {
                var item = GameCore.SingletonInstance.GameData.GetItem(type);
                item.Locked = item.Research.Locked = false;
                item.Research.Researched = true;
                item.Research.ResearchOrder = nextOrder++;
                GameCore.Earth.CurrentResearchItem = item.Research;
                GameCore.SingletonInstance.ChangeScene(Scenes.Earth_Research, new List<SceneVariables> { SceneVariables.Ground });
                var research = ActiveScreen<Research>();
                var image = research.GetNode<TextureRect>("Sprites/ResearchImage");
                Equal(new Vector2(width, height), image.Texture.GetSize(), "Research uses the complete original bitmap bounds");
                Equal(new Vector2(208, 68), image.Position, "original Research draw origin");
                Equal(TextureRect.StretchModeEnum.Keep, image.StretchMode, "original pixels are not enlarged or resampled");
                using var source = (Image)image.Texture.GetImage().Duplicate();
                var productionTexture = GD.Load<Texture2D>("res://Sprites/Items/Production/Illustrations/" + type + ".png");
                using var masked = (Image)productionTexture.GetImage().Duplicate();
                Equal(new Vector2I(48, 46), masked.GetSize(), "original production scratch canvas");
                var indices = new byte[width * height];
                for (var y = 0; y < 46; y++)
                    for (var x = 0; x < 48; x++)
                    {
                        var pixel = masked.GetPixel(x, y);
                        if (x >= width || y >= height) Equal(0f, pixel.A, "unused production canvas is transparent");
                        else
                        {
                            indices[y * width + x] = pixel.A == 0 ? (byte)0 : checked((byte)Array.IndexOf(palette, pixel.ToHtml()));
                            var expected = pixel.A == 0 ? Colors.Black : pixel;
                            Equal(expected.ToHtml(), source.GetPixel(x, y).ToHtml(), "Research draws opaque black where Production masks index zero");
                        }
                    }
                Equal(hash, Convert.ToHexString(SHA256.HashData(indices)).ToLowerInvariant(), "complete original bitmap including its last row");
                await InputFrames();
                await CheckResearchPixels(image, false, false);
                if (type == ItemTypes.g_chassis) await CaptureDisplayEvidence("research-scg-original");
                if (item.Production && item.BuildRequirements != null)
                {
                    var factory = GameCore.Earth.Station.Factory;
                    factory.ProductionQueue.Clear();
                    factory.ProductionQueue.Add(new ProductionItem(item) { Active = true, Production_Complete = 1 });
                    GameCore.SingletonInstance.ChangeScene(Scenes.Production, new List<SceneVariables> { SceneVariables.Orbit });
                    var small = ActiveScreen<Production>().GetNode<TextureRect>("Sprites/SmallItemImage");
                    Equal(new Vector2(136, 54), small.Position, "original production content origin");
                    Equal(TextureRect.StretchModeEnum.Keep, small.StretchMode, "production illustration is not resampled");
                    await InputFrames();
                    await CheckResearchPixels(small, type != ItemTypes.alien_artifact && type != ItemTypes.meh_fuel && type != ItemTypes.hed_fuel && type != ItemTypes.hyperlight);
                    if (type == ItemTypes.g_chassis) await CaptureDisplayEvidence("production-scg-original");
                }
            }
        }

        private Item CompleteHyperlightForRecipeTest()
        {
            PrepareRecipeStocks();
            var item = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.hyperlight);
            item.Locked = item.Research.Locked = false;
            item.Research.Researched = true;
            item.Research.ResearchPercentageComplete = 100;
            item.Research.ResearchOrder = 31;
            // Old saves can retain the previous incorrect manufacturing flag.
            item.Production = true;
            return item;
        }

        private void ResearchOnlyStores(bool ground)
        {
            CompleteHyperlightForRecipeTest();
            var inventory = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
            inventory.SelectedRecipe = ItemTypes.hyperlight;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            inventory = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
            GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { ground ? SceneVariables.Ground : SceneVariables.Orbit });
            var store = ActiveRecipeScreen<StoreScreen>();
            Equal(false, store.Buttons.Any(b => b.ObjectData?.ItemType == ItemTypes.hyperlight), "research-only technology has no recipe control");
            Equal(true, store.SelectedButton == null, "stale research-only selection is not activated");
            Equal("", store.BuildAmountLabel.Text, "no fictional build capacity");
            store.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.derrick).EmitSignal(BaseButton.SignalName.Pressed);
            Equal(ItemTypes.derrick, inventory.SelectedRecipe, "valid selection still works");
            Equal(true, store.BuildAmountLabel.Text.Contains(ground ? "for 3 " : "for 2 "), "local stock determines real recipe capacity");
        }

        private void ResearchOnlyProduction()
        {
            var hyperlight = CompleteHyperlightForRecipeTest();
            GameCore.SingletonInstance.ChangeScene(Scenes.Production, new List<SceneVariables> { SceneVariables.Orbit });
            var production = ActiveRecipeScreen<Production>();
            Equal(false, production.Buttons.Any(b => b.ObjectData?.ItemType == ItemTypes.hyperlight), "technology cannot be selected for manufacture");
            Equal(false, Production.CheckResourceAvailable(GameCore.Earth, hyperlight, false), "no recipe means not manufacturable");
            Equal(0, GameCore.Earth.Station.Factory.ProductionQueue.Count, "no technology queued");
        }

        private void RefiningDoesNotStarveOtherFactories()
        {
            DisableFuelRefining();
            var fuel = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.meh_fuel);
            fuel.Locked = fuel.Research.Locked = false;
            fuel.Research.Researched = true;
            fuel.AutoProduce = true;
            fuel.AutoProduceFlip = false;
            foreach (var planet in Save.BaseGameData.Planets.Values)
            {
                planet.Station.Resources.Stores[ItemTypes.hydrogen] = 0;
                planet.Station.Resources.Stores[ItemTypes.methane] = 0;
            }
            GameCore.Earth.PlanetResources.Stores[ItemTypes.hydrogen] = 0;
            GameCore.Earth.PlanetResources.Stores[ItemTypes.methane] = 0;
            var earth = GameCore.Earth.Station.Resources.Stores;
            var moon = Save.BaseGameData.Planets[StellarBodies.the_moon].Station.Resources.Stores;
            foreach (var location in new[] { StellarBodies.earth, StellarBodies.the_moon })
            {
                var planet = Save.BaseGameData.Planets[location];
                planet.Station.Built = true;
                planet.Station.BuildParts = 8;
                planet.Station.Type = 8;
                planet.ActiveMethanoid = false;
                var stores = planet.Station.Resources.Stores;
                stores[ItemTypes.hydrogen] = stores[ItemTypes.methane] = 100;
                stores[ItemTypes.meh_fuel] = 0;
            }
            FuelRefining.DayTick(0, 1);
            FuelRefining.DayTick(1, 2);
            Equal(true, moon[ItemTypes.meh_fuel] > 0, "second eligible factory cannot be starved by a shared per-factory toggle");
            Equal(8, earth[ItemTypes.meh_fuel], "orbital Earth batch follows original output");
            Equal(8, moon[ItemTypes.meh_fuel], "orbital Moon batch follows original output");
            Equal(94, earth[ItemTypes.hydrogen], "orbital batch consumes six hydrogen");
            Equal(94, moon[ItemTypes.methane], "orbital batch consumes six methane");
        }

        private void AutomaticFuelRecipes()
        {
            foreach (var type in new[] { ItemTypes.meh_fuel, ItemTypes.hed_fuel })
            {
                DisableFuelRefining();
                var fuel = GameCore.SingletonInstance.GameData.GetItem(type);
                fuel.Locked = fuel.Research.Locked = false;
                fuel.Research.Researched = true;
                fuel.AutoProduce = true;
                var stores = GameCore.Earth.PlanetResources.Stores;
                foreach (var material in fuel.BuildRequirements) stores[material.ItemType] = 3;
                stores[type] = 0;
                Save.RefiningPhase = 0;
                FuelRefining.DayTick(0, 1);
                Equal(0, stores[type], "Earth waits for even phase");
                FuelRefining.DayTick(1, 2);
                Equal(type == ItemTypes.meh_fuel ? 3 : 2, stores[type], "original Earth fuel output");
                foreach (var material in fuel.BuildRequirements)
                    Equal(1, stores[material.ItemType], "Earth fuel consumes two with minimum three");
            }
        }

        private async Task AllStoreRecipes()
        {
            InitializeUi();
            PrepareRecipeStocks();
            var items = Save.BaseGameData.ItemList.Where(i => i.Research != null).ToList();
            for (var i = 0; i < items.Count; i++)
            {
                items[i].Locked = items[i].Research.Locked = false;
                items[i].Research.Researched = true;
                items[i].Research.ResearchOrder = i + 1;
            }
            GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Ground });
            var store = ActiveRecipeScreen<StoreScreen>();
            var offered = store.Buttons.Where(b => b.ObjectData != null).Select(b => b.ObjectData.ItemType).ToList();
            Equal(false, offered.Contains(ItemTypes.hyperlight), "completed technology is omitted");
            Equal(true, offered.Contains(ItemTypes.s_chassis) && offered.Contains(ItemTypes.i_chassis), "physical recipes remain available");
            foreach (var type in offered)
            {
                store.Buttons.Single(b => b.ObjectData?.ItemType == type).EmitSignal(BaseButton.SignalName.Pressed);
                await InputFrames();
                Equal(type, store.CurrentStore.SelectedRecipe, "each physical recipe can be inspected");
                if (type == ItemTypes.alien_artifact)
                    Equal("No materials required", store.BuildAmountLabel.Text, "original device recipe has no material cost");
                else
                    Equal(true, store.BuildAmountLabel.Text.StartsWith("Enough supplies for "), "material recipes retain their capacity readout");
            }
        }
    }
}
