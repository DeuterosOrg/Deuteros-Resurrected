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
