using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;
using StoreScreen = Deuteros.Code.Platform.Screens.Store;
using MenuControl = Deuteros.Code.Platform.MenuButton;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public void RunProductionSelectionRegressions()
        {
            CheckUi("Ground production selection opens matching stores recipe without changing orbit", () => ProductionSelectionToStores(true, false));
            CheckUi("Orbital production selection opens matching stores recipe without changing ground", () => ProductionSelectionToStores(false, false));
            CheckUi("AOC production click retains the recipe selection for stores", () => ProductionSelectionToStores(false, true));
            CheckUi("Stores category round trip restores the selected recipe", () => StoreRecipeCategoryRoundTrip(false));
            CheckUi("Stores MTX round trip restores the selected recipe", () => StoreRecipeCategoryRoundTrip(true));
            CheckUi("Store recipe shows stock capacity above 200 and updates when materials run out", StoreRecipeCapacity);
        }

        private T ActiveRecipeScreen<T>() where T : Node
        {
            return GameCore.SingletonInstance.GetNode<Node>("MainScene").GetChildren().OfType<T>()
                .Single(node => !node.IsQueuedForDeletion());
        }

        private void PrepareRecipeStocks()
        {
            GameCore.Earth.Factory.Builder = null;
            GameCore.Earth.Station.Factory.Builder = null;
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            var ground = GameCore.Earth.PlanetResources.Stores;
            ground[ItemTypes.iron] = 9;
            ground[ItemTypes.titanium] = 12;
            ground[ItemTypes.carbon] = 7;
            var orbit = GameCore.Earth.Station.Resources.Stores;
            orbit[ItemTypes.iron] = 100;
            orbit[ItemTypes.titanium] = 8;
            orbit[ItemTypes.carbon] = 20;
        }

        private StoreScreen SelectProductionAndOpenStores(bool ground)
        {
            // Exercise real production controls and the stores navigation button without entering
            // the independently reproduced Godot 4.2.1 background-audio shutdown failure.
            var production = GD.Load<PackedScene>("res://Screens/Production.tscn").Instantiate<Production>();
            production.SceneVariables = new List<SceneVariables> { ground ? SceneVariables.Ground : SceneVariables.Orbit };
            production.GetNode("SoundController").Free();
            GameCore.SingletonInstance.UpdateMenuButtons(ground, !ground);
            AddChild(production);
            try
            {
                production.Buttons.Single(button => button.ObjectData?.ItemType == ItemTypes.derrick)
                    .EmitSignal(BaseButton.SignalName.Pressed);
                var menu = ActiveRecipeScreen<MainMenu>();
                menu.GetNode<Node>("MainButtons").GetChildren().OfType<MenuControl>()
                    .Single(button => button.TargetScene == Scenes.Store).EmitSignal(BaseButton.SignalName.Pressed);
                return ActiveRecipeScreen<StoreScreen>();
            }
            finally { production.Free(); }
        }

        private void ProductionSelectionToStores(bool ground, bool aoc)
        {
            PrepareRecipeStocks();
            var inventory = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
            inventory.AlternativeView = true;
            (ground ? GameCore.Earth.Factory : GameCore.Earth.Station.Factory).AOC = aoc;
            var store = SelectProductionAndOpenStores(ground);
            Equal(false, inventory.AlternativeView, "production selection opens the resource category");
            Equal(ItemTypes.derrick, store.SelectedButton?.ObjectData.ItemType ?? ItemTypes.none, "same product selected");
            Equal(true, store.Buttons.Single(button => button.ObjectData?.ItemType == ItemTypes.derrick).Selected, "visible recipe button selected");
            Equal(ground ? "Enough supplies for 3 Resource Mining Rigs" : "Enough supplies for 2 Resource Mining Rigs",
                store.BuildAmountLabel.Text, "local limiting material determines capacity");
            GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables>
                { ground ? SceneVariables.Orbit : SceneVariables.Ground });
            var other = ActiveRecipeScreen<StoreScreen>();
            Equal(true, other.SelectedButton == null, "other inventory recipe selection unchanged");
            Equal("", other.BuildAmountLabel.Text, "unselected inventory has no stale recipe");
        }

        private void StoreRecipeCategoryRoundTrip(bool mtx)
        {
            PrepareRecipeStocks();
            if (mtx) Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver);
            var store = SelectProductionAndOpenStores(false);
            Equal(ItemTypes.derrick, store.SelectedButton?.ObjectData.ItemType ?? ItemTypes.none, "recipe selected initially");
            store.SwitchStoreType.EmitSignal(BaseButton.SignalName.ButtonUp);
            if (mtx)
            {
                Equal(true, store.MTX.Visible, "MTX category is shown");
                store.MTX.GetNode<Button>("Config/Buttons/SwitchStore").EmitSignal(BaseButton.SignalName.Pressed);
            }
            else
            {
                Equal("", store.BuildAmountLabel.Text, "equipment category hides mineral recipe");
                Equal(true, store.SelectedButton == null, "hidden product button is not retained");
                store.SwitchStoreType.EmitSignal(BaseButton.SignalName.ButtonUp);
            }
            Equal(true, store.TradStore.Visible, "resources shown again");
            Equal(ItemTypes.derrick, store.SelectedButton?.ObjectData.ItemType ?? ItemTypes.none, "recipe restored after category switch");
            Equal(true, store.SelectedButton.Selected, "restored button highlights selection");
            Equal("Enough supplies for 2 Resource Mining Rigs", store.BuildAmountLabel.Text, "recipe restored with local stock");
        }

        private void StoreRecipeCapacity()
        {
            PrepareRecipeStocks();
            var inventory = GameCore.Earth.PlanetResources.Stores;
            inventory[ItemTypes.iron] = 1503;
            inventory[ItemTypes.titanium] = 2004;
            inventory[ItemTypes.carbon] = 501;
            GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Ground });
            var store = ActiveRecipeScreen<StoreScreen>();
            store.Buttons.Single(button => button.ObjectData?.ItemType == ItemTypes.derrick)
                .EmitSignal(BaseButton.SignalName.Pressed);
            Equal("Enough supplies for 501 Resource Mining Rigs", store.BuildAmountLabel.Text, "recipe capacity is not arbitrarily capped");
            inventory[ItemTypes.titanium] = 3;
            store.DrawData();
            Equal("Enough supplies for 0 Resource Mining Rigs", store.BuildAmountLabel.Text, "insufficient limiting material updates count");
            Equal(true, store.ResourceListLabel.Text.Contains("[color=#ff0000]"), "insufficient recipe material highlighted");
        }
    }
}
