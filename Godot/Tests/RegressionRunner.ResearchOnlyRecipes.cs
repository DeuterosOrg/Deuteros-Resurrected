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
                Equal(new Vector2(48, 48), image.Texture.GetSize(), "recovered/canonical illustration canvas");
                await InputFrames();
                if (type == ItemTypes.pulse_blaster_laser) await CaptureDisplayEvidence("research-recovered-" + restored);
            }
            research.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.hyperlight).EmitSignal(BaseButton.SignalName.Pressed);
            research.DrawData(false);
            Equal("", research.GetNode<Label>("Labels/Researched/ProductionAmountListLabel").Text, "switching back clears prior recipe");
            Equal("Research complete", research.GetNode<Label>("Labels/Researched/ItemNotesLabel").Text, "repeat selection remains safe");
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

        private void AutomaticFuelRecipes()
        {
            foreach (var type in new[] { ItemTypes.meh_fuel, ItemTypes.hed_fuel })
            {
                DisableFuelRefining();
                var fuel = GameCore.SingletonInstance.GameData.GetItem(type);
                fuel.Locked = fuel.Research.Locked = false;
                fuel.Research.Researched = true;
                fuel.AutoProduce = true;
                fuel.AutoProduceFlip = false;
                var ground = type == ItemTypes.meh_fuel;
                var stores = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
                foreach (var material in fuel.BuildRequirements) stores[material.ItemType] = material.ItemCount;
                stores[type] = 0;
                Production.UpdateProduction(0, 1);
                Equal(3, stores[type], "automatic fuel output despite no manual manufacturing flag");
                foreach (var material in fuel.BuildRequirements)
                    Equal(0, stores[material.ItemType], "fuel recipe charged exactly once");
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
                Equal(true, store.BuildAmountLabel.Text.StartsWith("Enough supplies for "), "each offered recipe has a capacity readout");
            }
        }
    }
}
