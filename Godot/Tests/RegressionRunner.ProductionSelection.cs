using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using StoreScreen = Deuteros.Code.Platform.Screens.Store;
using MenuControl = Deuteros.Code.Platform.MenuButton;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void ResumePaidProduction() => ResumePaidProduction(false);

        private void ResumePaidProduction(bool removeTeam)
        {
            GameCore.SingletonInstance.SetProcess(false);
            foreach (var ground in new[] { true, false })
            {
                PrepareRecipeStocks();
                GameCore.Earth.GroundSelected = ground;
                var factory = ground ? GameCore.Earth.Factory : GameCore.Earth.Station.Factory;
                factory.AOC = false;
                factory.Ground = ground;
                factory.ProductionQueue.Clear();
                factory.Builder = new Staff { Leader = "Builder", Type = StaffType.Production, Count = 250 };
                factory.Builder.AddAction(12);
                var stores = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
                stores.Items.Clear();
                var types = new[] { ItemTypes.derrick, ItemTypes.supply_pod, ItemTypes.tool_pod };
                foreach (var type in types)
                {
                    var item = GameCore.SingletonInstance.GameData.GetItem(type);
                    item.Locked = item.Research.Locked = false;
                    item.Research.Researched = true;
                    item.Research.ResearchOrder = Save.BaseGameData.ItemList.Max(i => i.Research?.ResearchOrder ?? 0) + 1;
                    if (type != ItemTypes.tool_pod)
                        foreach (var cost in item.BuildRequirements) stores[cost.ItemType] += cost.ItemCount;
                }
                void Select(Production panel, ItemTypes type) => panel.Buttons.Single(b => b.ObjectData?.ItemType == type)
                    .EmitSignal(BaseButton.SignalName.Pressed);
                var screen = OpenMtxProduction(ground);
                try
                {
                    Select(screen, ItemTypes.derrick);
                    Select(screen, ItemTypes.supply_pod);
                    Equal(2, factory.ProductionQueue.Count, "both jobs reserved and paid through real controls");
                    Equal(true, stores.Items.Values.All(n => n == 0), "exact two recipes were charged once");
                    if (removeTeam)
                    {
                        var resources = ground ? (Deuteros.Code.Platform.Resource)GameCore.Earth.PlanetResources : GameCore.Earth.Station.Resources;
                        var builder = factory.Builder;
                        for (var tick = 0; tick < 100 && factory.CurrentProductionItem().Production_Complete == 1; tick++)
                            Production.UpdateProduction((uint)tick, (uint)tick + 1);
                        Equal(2, factory.CurrentProductionItem().Production_Complete, "paid active job has real accumulated progress");
                        for (var slot = 0; slot < resources.Staff.Length; slot++)
                            resources.Staff[slot] = new Staff { Leader = "Occupied", Type = StaffType.Production, Count = 1 };
                        screen.GetNode<TextureButton>("RemoveStaff").EmitSignal(BaseButton.SignalName.Pressed);
                        Equal(builder, factory.Builder, "full staff store prevents removal");
                        Equal(2, factory.ProductionQueue.Count, "blocked removal preserves both jobs");
                        resources.Staff[0] = null;
                        var oldSelection = screen.SelectedButton;
                        screen.GetNode<TextureButton>("RemoveStaff").EmitSignal(BaseButton.SignalName.Pressed);
                        Equal<Staff>(null, factory.Builder, "removed team leaves factory");
                        Equal(builder, resources.Staff[0], "same team returns to local staff store");
                        Equal(2, factory.ProductionQueue.Count, "removing team preserves both paid jobs");
                        Equal<ProductionItem>(null, factory.CurrentProductionItem(), "removing team pauses active work");
                        Equal(true, stores.Items.Values.All(n => n == 0), "paid materials remain reserved");
                        Production.UpdateProduction(100, 101);
                        Equal(2, factory.ProductionQueue.Single(j => j.Product.ItemType == ItemTypes.supply_pod).Production_Complete,
                            "team removal preserves completed production stages");
                        oldSelection.Free(); // Complete the removal already queued by rebuilding the buttons.
                        Select(screen, ItemTypes.derrick);
                        Equal<ProductionItem>(null, factory.CurrentProductionItem(), "new controls remain usable without resuming unstaffed work");
                    }
                }
                finally { screen.Free(); }
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                factory = ground ? GameCore.Earth.Factory : GameCore.Earth.Station.Factory;
                stores = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
                if (removeTeam)
                {
                    var resources = ground ? (Deuteros.Code.Platform.Resource)GameCore.Earth.PlanetResources : GameCore.Earth.Station.Resources;
                    Equal(2, factory.ProductionQueue.Single(j => j.Product.ItemType == ItemTypes.supply_pod).Production_Complete,
                        "paused progress survives reload");
                    factory.Builder = resources.Staff[0];
                    resources.Staff[0] = null;
                }
                screen = OpenMtxProduction(ground);
                try
                {
                    Select(screen, ItemTypes.derrick);
                    Equal(ItemTypes.derrick, factory.CurrentProductionItem().Product.ItemType, "paid job resumes with empty stores after reload");
                    Select(screen, ItemTypes.tool_pod);
                    Equal(ItemTypes.derrick, factory.CurrentProductionItem().Product.ItemType, "unpaid new recipe remains blocked");
                    Equal(2, factory.ProductionQueue.Count, "rejected recipe cannot enter the paid queue");
                    foreach (var type in types.Take(2))
                    {
                        Select(screen, type);
                        for (var tick = 0; tick < 100 && factory.CurrentProductionItem() != null; tick++)
                            Production.UpdateProduction((uint)tick, (uint)tick + 1);
                        Equal(1, stores[type], "one paid output completes");
                    }
                    Equal(0, factory.ProductionQueue.Count, "both paid jobs complete exactly once");
                    Equal(0, stores[ItemTypes.tool_pod], "unpaid product never produced");
                    foreach (var cost in types.Take(2).SelectMany(t => GameCore.SingletonInstance.GameData.GetItem(t).BuildRequirements))
                        Equal(0, stores[cost.ItemType], "resumption never charges materials again");
                }
                finally { screen.Free(); }
            }
        }

        private void AocPaidManualQueue()
        {
            GameCore.SingletonInstance.SetProcess(false);
            foreach (var spareRecipes in new[] { 0, 2 })
            {
                PrepareRecipeStocks();
                GameCore.Earth.GroundSelected = false;
                var factory = GameCore.Earth.Station.Factory;
                factory.AOC = false;
                factory.ProductionQueue.Clear();
                factory.Builder = new Staff { Leader = "Expert", Type = StaffType.Production, Count = 250 };
                factory.Builder.AddAction(12);
                var stores = GameCore.Earth.Station.Resources.Stores;
                stores.Items.Clear();
                System.Array.Clear(GameCore.Earth.Station.Resources.Staff);
                var product = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick);
                var aoc = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.a__o__c);
                foreach (var item in new[] { product, aoc })
                {
                    item.Locked = item.Research.Locked = false;
                    item.Research.Researched = true;
                    foreach (var cost in item.BuildRequirements) stores[cost.ItemType] += cost.ItemCount;
                }
                void Select(Production panel) => panel.Buttons.Single(b => b.ObjectData?.ItemType == product.ItemType)
                    .EmitSignal(BaseButton.SignalName.Pressed);
                var screen = OpenMtxProduction(false);
                try
                {
                    Select(screen);
                    if (spareRecipes > 0)
                        for (var day = 0; day < 100 && factory.CurrentProductionItem().Production_Complete == 1; day++)
                            Production.UpdateProduction((uint)day, (uint)day + 1);
                    var paidStage = factory.CurrentProductionItem().Production_Complete;
                    Equal(spareRecipes > 0 ? 2 : 1, paidStage, "manual job covers initial and accumulated progress");
                    screen.Buttons.Single(b => b.ObjectData?.ItemType == aoc.ItemType).EmitSignal(BaseButton.SignalName.Pressed);
                    for (var day = 0; day < 100 && !factory.AOC; day++) Production.UpdateProduction((uint)day, (uint)day + 1);
                    Equal(true, factory.AOC, "paid AOC completes normally");
                    Equal(1, factory.ProductionQueue.Count, "manual paid job remains after conversion");
                }
                finally { screen.Free(); }
                foreach (var cost in product.BuildRequirements) stores[cost.ItemType] += spareRecipes * cost.ItemCount;
                ProductionDays(2);
                Equal<ProductionItem>(null, factory.CurrentProductionItem(), "conversion does not silently select paused manual work");
                // Old saves have no payment marker. Preserve the identifiable manual reservation.
                var json = Newtonsoft.Json.Linq.JObject.Parse(SaveStorage.Serialize(Save));
                foreach (var field in json.Descendants().OfType<Newtonsoft.Json.Linq.JProperty>()
                    .Where(p => p.Name == "MaterialsPaid").ToList()) field.Remove();
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(json.ToString());
                factory = GameCore.Earth.Station.Factory;
                stores = GameCore.Earth.Station.Resources.Stores;
                Equal(spareRecipes > 0 ? 2 : 1, factory.ProductionQueue.Single().Production_Complete, "legacy reload preserves paid stages");
                screen = OpenMtxProduction(false);
                try
                {
                    Select(screen);
                    Equal(true, factory.ProductionQueue.Single().AOCOneTime, "paused manual job can be selected once");
                    Select(screen);
                    Select(screen);
                    Equal(1, factory.ProductionQueue.Count, "deselecting paid pending work retains its materials and progress");
                    ProductionDays(2);
                    Equal<ProductionItem>(null, factory.CurrentProductionItem(), "deselected paid job remains paused");
                    Select(screen);
                }
                finally { screen.Free(); }
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                factory = GameCore.Earth.Station.Factory;
                stores = GameCore.Earth.Station.Resources.Stores;
                ProductionDays(20);
                Equal(1, stores[product.ItemType], "selected paid work completes exactly once");
                Equal(0, factory.ProductionQueue.Count, "one-time work leaves the queue");
                foreach (var cost in product.BuildRequirements)
                    Equal(spareRecipes * cost.ItemCount, stores[cost.ItemType], "reserved recipe is never charged again");
                screen = OpenMtxProduction(false);
                try { Select(screen); Select(screen); }
                finally { screen.Free(); }
                // Legacy unstarted AOC repeat selections must still pay, unlike manual reservations.
                json = Newtonsoft.Json.Linq.JObject.Parse(SaveStorage.Serialize(Save));
                foreach (var field in json.Descendants().OfType<Newtonsoft.Json.Linq.JProperty>()
                    .Where(p => p.Name == "MaterialsPaid").ToList()) field.Remove();
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(json.ToString());
                stores = GameCore.Earth.Station.Resources.Stores;
                ProductionDays(40);
                Equal(1 + spareRecipes, stores[product.ItemType], "fresh repeated output consumes each new recipe");
                foreach (var cost in product.BuildRequirements) Equal(0, stores[cost.ItemType], "repeat consumes available recipes exactly");
            }
        }

        private void AocStaffCapacity()
        {
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            PrepareRecipeStocks();
            GameCore.Earth.GroundSelected = false;
            var factory = GameCore.Earth.Station.Factory;
            factory.AOC = false;
            factory.ProductionQueue.Clear();
            factory.Builder = new Staff { Leader = "Expert", Type = StaffType.Production, Count = 250 };
            factory.Builder.AddAction(12);
            var resources = GameCore.Earth.Station.Resources;
            for (var slot = 0; slot < resources.Staff.Length; slot++)
                resources.Staff[slot] = new Staff { Leader = "Occupied" + slot, Type = StaffType.Marines, Count = 1 };
            var item = core.GameData.GetItem(ItemTypes.a__o__c);
            item.Locked = item.Research.Locked = false;
            item.Research.Researched = true;
            resources.Stores.Items.Clear();
            foreach (var cost in item.BuildRequirements) resources.Stores[cost.ItemType] = cost.ItemCount;
            var notices = 0;
            var observedCommittedState = true;
            void Completed(Factory completed)
            {
                if (completed != factory) return;
                notices++;
                observedCommittedState &= completed.AOC && completed.Builder == null
                    && resources.Staff[2]?.Leader == "Expert";
            }
            core.ProductionFinished += Completed;
            var screen = OpenMtxProduction(false);
            try
            {
                screen.Buttons.Single(b => b.ObjectData?.ItemType == item.ItemType).EmitSignal(BaseButton.SignalName.Pressed);
                ProductionDays(100);
                Equal(false, factory.AOC, "full quarters defer installation");
                Equal("Expert", factory.Builder.Leader, "waiting factory retains the builder");
                Equal(3, factory.CurrentProductionItem().Production_Complete, "original completion waits at stage three");
                Equal(0, notices, "blocked completion emits no notice");
                Equal(0, resources.Stores[item.ItemType], "blocked completion creates no stock");
                Equal(12, (int)typeof(Staff).GetProperty("ActionsTaken", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance).GetValue(factory.Builder), "waiting does not award experience repeatedly");
            }
            catch { core.ProductionFinished -= Completed; throw; }
            finally { screen.Free(); }
            try
            {
                core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                factory = GameCore.Earth.Station.Factory;
                resources = GameCore.Earth.Station.Resources;
                var builder = factory.Builder;
                Equal(3, factory.CurrentProductionItem().Production_Complete, "waiting stage survives reload");
                resources.Staff[2] = null;
                screen = OpenMtxProduction(false);
                try
                {
                    ProductionDays(100);
                    Equal(true, factory.AOC, "freeing one slot permits installation");
                    Equal(builder, resources.Staff[2], "the same builder occupies the free slot");
                    Equal(1, notices, "installation notifies exactly once");
                    Equal(true, observedCommittedState, "observers see committed installation and staff transfer");
                    Equal(0, factory.ProductionQueue.Count, "completed AOC leaves the queue");
                    Equal(0, resources.Stores[item.ItemType], "installed AOC is not transferable equipment");
                    screen.DrawData();
                    Equal(true, screen.GetNode<TextureRect>("Sprites/AocPanel").Visible, "completed installation displays the AOC panel");
                    Equal(false, screen.GetNode<TextureButton>("RemoveStaff").Visible, "completed installation hides manual staff controls");
                    Equal(13, (int)typeof(Staff).GetProperty("ActionsTaken", System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance).GetValue(builder), "one successful completion awards one action");
                    var button = screen.Buttons.Single(b => b.ObjectData?.ItemType == item.ItemType);
                    button.EmitSignal(BaseButton.SignalName.Pressed);
                    Equal(0, factory.ProductionQueue.Count, "installed AOC cannot be ordered again");
                    Equal(item.FullName + " - Installed", button.HoverText, "installed AOC is visibly unavailable");
                    factory.ProductionQueue.Add(new ProductionItem(core.GameData.GetItem(item.ItemType)) { AOCRepeat = true });
                    ProductionDays(20);
                    Equal(0, factory.ProductionQueue.Count, "obsolete saved AOC repeat is discarded");
                    Equal(1, notices, "obsolete repeat produces no completion");
                    foreach (var cost in item.BuildRequirements) Equal(0, resources.Stores[cost.ItemType], "one recipe charged");
                    for (var slot = 0; slot < resources.Staff.Length; slot++)
                        if (slot != 2) Equal("Occupied" + slot, resources.Staff[slot].Leader, "other teams are preserved");
                }
                finally { screen.Free(); }
            }
            finally { core.ProductionFinished -= Completed; }
            core.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(true, GameCore.Earth.Station.Factory.AOC, "completed installation survives reload");
            Equal("Expert", GameCore.Earth.Station.Resources.Staff[2].Leader, "returned builder survives reload");
            Equal(0, GameCore.Earth.Station.Factory.ProductionQueue.Count, "completed queue remains empty after reload");
        }

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
            if (mtx)
            {
                Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver);
                GameCore.Earth.Station.MtxInstalled = true;
            }
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
