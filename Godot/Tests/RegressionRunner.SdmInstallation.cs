using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunSdmInstallationRegressions()
        {
            foreach (var automated in new[] { false, true })
                CheckUi($"Paid SDM production installs locally and stops repeat automated={automated}", () => SdmProductionInstalls(automated));
            foreach (var automated in new[] { false, true })
                CheckUi($"Installed SDM rejects duplicate orders automated={automated}", () => SdmDuplicateOrder(automated));
            Check("Installed SDM clears legacy repeat orders without charging", SdmInstalledQueue);
            Check("Part-built SDM saves and installs once without transferable output", SdmProductionSave);
            foreach (var automated in new[] { false, true })
                CheckUi($"Ground factory cannot queue SDM installation automated={automated}", () => SdmGroundOrder(automated));
            foreach (var captured in new[] { false, true })
                Check("SDM installation suspends while station is " + (captured ? "captured" : "absent"), () => SdmStationUnavailable(captured));
        }

        private Item PrepareSdmProduction(bool automated, bool ground = false)
        {
            PrepareMtxProduction(automated, ground);
            GameCore.Earth.Station.MtxInstalled = true; // Independent installed facilities must coexist.
            GameCore.Earth.Station.SdmInstalled = false;
            var item = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.s__d__m);
            item.Locked = item.Research.Locked = false;
            item.Research.Researched = true;
            item.Research.ResearchOrder = 19;
            var stores = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
            foreach (var recipe in item.BuildRequirements) stores[recipe.ItemType] = recipe.ItemCount * 2;
            return item;
        }

        private void OrderSdm(Production screen) => screen.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.s__d__m)
            .EmitSignal(BaseButton.SignalName.Pressed);

        private void SdmProductionInstalls(bool automated)
        {
            var item = PrepareSdmProduction(automated);
            var station = GameCore.Earth.Station;
            station.Resources.Stores[ItemTypes.s__d__m] = 3; // Preserve pre-fix save inventories.
            var screen = OpenMtxProduction();
            try
            {
                OrderSdm(screen);
                if (automated) OrderSdm(screen);
                ProductionDays(30);
                Equal(true, station.SdmInstalled, "producing station gains installed SDM");
                Equal(true, station.MtxInstalled, "existing MTX preserved");
                Equal(false, Save.BaseGameData.Planets[StellarBodies.the_moon].Station.SdmInstalled, "another station gains nothing");
                Equal(3, station.Resources.Stores[ItemTypes.s__d__m], "no new stock or retroactive inventory deletion");
                Equal(0, station.Factory.ProductionQueue.Count, "repeat terminates after installation");
                foreach (var recipe in item.BuildRequirements)
                    Equal(recipe.ItemCount, station.Resources.Stores[recipe.ItemType], "one recipe charged");
                Equal("Static_Locked", screen.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.s__d__m)
                    .GetNode<AnimatedSprite2D>("ButtonSpriteAnimation").Animation.ToString(), "completion locks the recipe control");
            }
            finally { screen.Free(); }
        }

        private void SdmDuplicateOrder(bool automated)
        {
            var item = PrepareSdmProduction(automated);
            var station = GameCore.Earth.Station;
            station.SdmInstalled = true;
            var screen = OpenMtxProduction();
            try
            {
                OrderSdm(screen);
                Equal(0, station.Factory.ProductionQueue.Count, "duplicate request rejected");
                foreach (var recipe in item.BuildRequirements)
                    Equal(recipe.ItemCount * 2, station.Resources.Stores[recipe.ItemType], "no duplicate material debit");
                Equal(item.FullName + " - Installed", screen.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.s__d__m).HoverText,
                    "installed hover explains unavailable recipe");
            }
            finally { screen.Free(); }
        }

        private void SdmInstalledQueue()
        {
            var item = PrepareSdmProduction(true);
            var station = GameCore.Earth.Station;
            station.SdmInstalled = true;
            station.Factory.ProductionQueue.Add(new ProductionItem(item) { AOCRepeat = true });
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ProductionDays(20);
            station = GameCore.Earth.Station;
            Equal(0, station.Factory.ProductionQueue.Count, "loaded obsolete repeat removed");
            foreach (var recipe in item.BuildRequirements)
                Equal(recipe.ItemCount * 2, station.Resources.Stores[recipe.ItemType], "no additional charge");
        }

        private void SdmProductionSave()
        {
            var item = PrepareSdmProduction(true);
            GameCore.Earth.Station.Factory.ProductionQueue.Add(new ProductionItem(item) { AOCRepeat = true });
            ProductionDays(1);
            Equal(false, GameCore.Earth.Station.SdmInstalled, "first tick is still constructing");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ProductionDays(20);
            Equal(true, GameCore.Earth.Station.SdmInstalled, "loaded paid order installs");
            foreach (var recipe in item.BuildRequirements)
                Equal(recipe.ItemCount, GameCore.Earth.Station.Resources.Stores[recipe.ItemType], "loaded order charged once");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save)).BaseGameData.Planets[StellarBodies.earth].Station;
            Equal(true, restored.SdmInstalled, "installation persists");
            Equal(0, restored.Resources.Stores[ItemTypes.s__d__m], "no transferable output");
        }

        private void SdmGroundOrder(bool automated)
        {
            var item = PrepareSdmProduction(automated, true);
            var screen = OpenMtxProduction(true);
            try
            {
                OrderSdm(screen);
                Equal(0, GameCore.Earth.Factory.ProductionQueue.Count, "ground installation cannot be queued");
                foreach (var recipe in item.BuildRequirements)
                    Equal(recipe.ItemCount * 2, GameCore.Earth.PlanetResources.Stores[recipe.ItemType], "ground stock unchanged");
            }
            finally { screen.Free(); }
        }

        private void SdmStationUnavailable(bool captured)
        {
            var item = PrepareSdmProduction(true);
            var station = GameCore.Earth.Station;
            station.Factory.ProductionQueue.Add(new ProductionItem(item) { AOCRepeat = true });
            ProductionDays(1);
            var progress = station.Factory.CurrentProductionItem().Production_Complete;
            if (captured) GameCore.Earth.ActiveMethanoid = true;
            else station.Built = false;
            ProductionDays(20);
            Equal(false, station.SdmInstalled, "unavailable station cannot complete installation");
            Equal(progress, station.Factory.CurrentProductionItem()?.Production_Complete ?? -1, "paid order retained and suspended");
            GameCore.Earth.ActiveMethanoid = false;
            station.Built = true;
            ProductionDays(20);
            Equal(true, station.SdmInstalled, "restored station completes paid order");
            foreach (var recipe in item.BuildRequirements)
                Equal(recipe.ItemCount, station.Resources.Stores[recipe.ItemType], "resumption does not repay recipe");
        }
    }
}
