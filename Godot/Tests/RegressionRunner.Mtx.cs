using System;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Godot;
using static Deuteros.Code.Enums;
using MtxScreen = Deuteros.Code.Platform.Screens.MTX;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunMtxRouteRegressions()
        {
            foreach (var state in new[] { "captured", "unbuilt", "no module", "missing" })
                Check($"MTX preserves stocks when destination is {state}", () => MtxUnavailableDestination(state));
            Check("MTX sends only available capacity and resumes after destination recapture", MtxSendAndRecapture);
            Check("MTX skips a locked configured item and processes the next eligible item", MtxSkipsLockedItem);
            Check("MTX terminates when all configured items are locked", MtxAllConfiguredItemsLocked);
            Check("MTX tolerates an empty unlocked item list", MtxNoUnlockedItems);
            CheckUi("MTX stale destination button cannot select a different station", () => MtxStaleDestination(false));
            CheckUi("MTX stale last destination button does not index past the station list", () => MtxStaleDestination(true));
            CheckUi("MTX opens a saved route with a missing destination", MtxMissingDestinationView);
        }

        private (SpaceStation source, SpaceStation target) PrepareMtxRoute()
        {
            Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver);
            foreach (var planet in Save.BaseGameData.Planets.Values)
                planet.Station.Built = false;
            var sourcePlanet = GameCore.Earth;
            var targetPlanet = Save.BaseGameData.Planets[StellarBodies.the_moon];
            sourcePlanet.ActiveMethanoid = targetPlanet.ActiveMethanoid = false;
            var source = sourcePlanet.Station;
            var target = targetPlanet.Station;
            source.Built = target.Built = true;
            source.MtxInstalled = target.MtxInstalled = true;
            source.StationOrdinal = 0;
            target.StationOrdinal = 1;
            source.Resources.Stores[ItemTypes.iron] = 100;
            target.Resources.Stores[ItemTypes.iron] = 20;
            source.Resources.Stores.MTX.Target = StellarBodies.the_moon;
            source.Resources.Stores.MTX.CurrentItem = ItemTypes.iron;
            source.Resources.Stores.MTX.SendItems.Add(ItemTypes.iron);
            return (source, target);
        }

        private void MtxUnavailableDestination(string state)
        {
            var (source, target) = PrepareMtxRoute();
            if (state == "captured") Save.BaseGameData.Planets[StellarBodies.the_moon].ActiveMethanoid = true;
            if (state == "unbuilt") target.Built = false;
            if (state == "no module") target.MtxInstalled = false;
            if (state == "missing") Save.BaseGameData.Planets.Remove(StellarBodies.the_moon);
            foreach (var balance in new[] { false, true })
            {
                if (balance) source.Resources.Stores.MTX.BalanceItems.Add(ItemTypes.iron);
                MtxScreen.UpdateMTX(0, 1);
                Equal(100, source.Resources.Stores[ItemTypes.iron], "source unchanged");
                Equal(20, target.Resources.Stores[ItemTypes.iron], "destination unchanged");
            }
        }

        private void MtxSendAndRecapture()
        {
            var (source, target) = PrepareMtxRoute();
            target.Resources.Stores[ItemTypes.iron] = 49970;
            MtxScreen.UpdateMTX(0, 1);
            Equal(70, source.Resources.Stores[ItemTypes.iron], "overflow remains at source");
            Equal(50000, target.Resources.Stores[ItemTypes.iron], "destination capacity respected");
            Save.BaseGameData.Planets[StellarBodies.the_moon].ActiveMethanoid = true;
            target.Resources.Stores[ItemTypes.iron] = 0;
            MtxScreen.UpdateMTX(1, 2);
            Equal(70, source.Resources.Stores[ItemTypes.iron], "capture suspends existing route");
            Save.BaseGameData.Planets[StellarBodies.the_moon].ActiveMethanoid = false;
            MtxScreen.UpdateMTX(2, 3);
            Equal(0, source.Resources.Stores[ItemTypes.iron], "friendly recapture resumes route");
            Equal(70, target.Resources.Stores[ItemTypes.iron], "remaining stock delivered once");
        }

        private void MtxSkipsLockedItem()
        {
            var (source, target) = PrepareMtxRoute();
            var route = source.Resources.Stores.MTX;
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.m__t__x).Locked = true;
            route.SendItems.Add(ItemTypes.m__t__x);
            route.CurrentItem = ItemTypes.m__t__x;
            source.Resources.Stores[ItemTypes.m__t__x] = 2;
            MtxScreen.UpdateMTX(0, 1);
            Equal(2, source.Resources.Stores[ItemTypes.m__t__x], "locked item stays in source store");
            Equal(0, target.Resources.Stores[ItemTypes.m__t__x], "locked item is not delivered");
            Equal(120, target.Resources.Stores[ItemTypes.iron], "next eligible item transfers this tick");
        }

        private void MtxAllConfiguredItemsLocked()
        {
            var (source, target) = PrepareMtxRoute();
            var route = source.Resources.Stores.MTX;
            route.SendItems.Clear();
            route.SendItems.Add(ItemTypes.m__t__x);
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.m__t__x).Locked = true;
            GD.Print("MTX PHASE: advancing with only locked items configured");
            MtxScreen.UpdateMTX(0, 1);
            Equal(100, source.Resources.Stores[ItemTypes.iron], "unconfigured stock unchanged");
            Equal(20, target.Resources.Stores[ItemTypes.iron], "no unintended transfer");
        }

        private void MtxNoUnlockedItems()
        {
            var (source, target) = PrepareMtxRoute();
            foreach (var item in Save.BaseGameData.ItemList) item.Locked = true;
            MtxScreen.UpdateMTX(0, 1);
            Equal(100, source.Resources.Stores[ItemTypes.iron], "no eligible source item");
            Equal(20, target.Resources.Stores[ItemTypes.iron], "no eligible destination item");
        }

        private MtxScreen OpenMtx(SpaceStation station)
        {
            Save.CurrentPlanet = StellarBodies.earth;
            var screen = GD.Load<PackedScene>("res://PreFabs/MTX.tscn").Instantiate<MtxScreen>();
            AddChild(screen);
            screen.LoadScene(station.Resources.Stores, () => { });
            return screen;
        }

        private void MtxStaleDestination(bool last)
        {
            var (source, target) = PrepareMtxRoute();
            if (!last)
            {
                var mars = Save.BaseGameData.Planets[StellarBodies.mars];
                mars.ActiveMethanoid = false;
                mars.Station.Built = mars.Station.MtxInstalled = true;
                mars.Station.StationOrdinal = 2;
            }
            source.Resources.Stores.MTX.Target = StellarBodies.none;
            var screen = OpenMtx(source);
            try
            {
                var button = screen.GetNode<TextureButton>("Destination/StationButtons/01");
                Equal((int)StellarBodies.the_moon, (int)button.GetMeta("StationId"), "displayed destination");
                Save.BaseGameData.Planets[StellarBodies.the_moon].ActiveMethanoid = true;
                button.EmitSignal(BaseButton.SignalName.Pressed);
                Equal(StellarBodies.none, source.Resources.Stores.MTX.Target, "stale click cannot redirect cargo");
            }
            finally { screen.Free(); }
        }

        private void MtxMissingDestinationView()
        {
            var (source, target) = PrepareMtxRoute();
            Save.BaseGameData.Planets.Remove(StellarBodies.the_moon);
            var screen = OpenMtx(source);
            screen.Free();
            Equal(StellarBodies.the_moon, source.Resources.Stores.MTX.Target, "opening does not silently rewrite route");
        }
    }
}
