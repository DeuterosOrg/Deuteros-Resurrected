using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.GameData;
using Godot;
using static Deuteros.Code.Enums;
using ProductionScreen = Deuteros.Code.Platform.Screens.Production;
using MtxScreen = Deuteros.Code.Platform.Screens.MTX;
using InteriorScreen = Deuteros.Code.Platform.Screens.ShipInterior;

namespace Deuteros.Tests
{
    // Runs the real C# simulation inside Godot, with the real Master scene initialized.
    public partial class RegressionRunner : Node
    {
        private int passed;
        private int failed;
        private int declaredCases;
        private int selectedCase;
        private bool listCases;
        private bool isolatedCase;
        private SaveFile Save => GameCore.SingletonInstance.GameData.ActiveSaveFile;

        public override void _Ready() => CallDeferred(nameof(Run));

        private bool StartCase(string name)
        {
            declaredCases++;
            if (listCases)
            {
                GD.Print($"TEST CASE: {declaredCases}: {name}");
                return false;
            }
            if (isolatedCase && selectedCase != declaredCases) return false;

            // An isolated process already has a fresh world from Master._Ready.
            // Batch mode remains available for diagnostics, but does not isolate static state.
            if (!isolatedCase)
            {
                CoreData.CreateBaseGameData();
                GameCore.SingletonInstance.GameData.ActiveSaveFile = CoreData.CreateNewSaveFile();
            }
            GameCore.SingletonInstance.InfiniteResources = false;
            return true;
        }

        private void Check(string name, Action test)
        {
            try
            {
                if (!StartCase(name)) return;
                test();
                passed++;
                GD.Print("PASS: " + name);
            }
            catch (Exception error)
            {
                failed++;
                GD.Print("FAIL: " + name + " -- " + error);
            }
        }

        private async Task CheckAsync(string name, Func<Task> test)
        {
            try
            {
                if (!StartCase(name)) return;
                await test();
                passed++;
                GD.Print("PASS: " + name);
            }
            catch (Exception error)
            {
                failed++;
                GD.Print("FAIL: " + name + " -- " + error);
            }
        }

        private static void Equal<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new InvalidOperationException($"{message}: expected {expected}, got {actual}");
        }

        public async void Run()
        {
            var selection = System.Environment.GetEnvironmentVariable("DEUTEROS_TEST_CASE");
            listCases = selection == "list";
            isolatedCase = selection != null && !listCases;
            if (isolatedCase && (!int.TryParse(selection, out selectedCase) || selectedCase < 1))
            {
                GD.Print("FAIL: DEUTEROS_TEST_CASE must be 'list' or a positive case number");
                GetTree().Quit(1);
                return;
            }
            foreach (var c in new[] { (100, 1000, 550, 550), (1, 0, 1, 0), (0, 0, 0, 0),
                                      (50000, 49999, 50000, 49999), (0, 1000, 500, 500) })
                Check($"MTX conserves stocks {c.Item1}/{c.Item2}", () => Balance(c.Item1, c.Item2, c.Item3, c.Item4));
            Check("AOC repeat stops when recipe materials run out and resumes after replenishment", AocConsumesEveryCycle);
            Check("Ground output stays on ground while orbit is selected", () => FactoryDestination(true, false));
            Check("Orbital output stays in orbit while ground is selected", () => FactoryDestination(false, true));
            Check("Multiple falling ships are removed without aborting the update", RemoveCasualties);
            RunUiRegressions();
            await RunTimedUiRegressions();
            RunShipBayBacklogRegressions();
            RunOverviewBacklogRegressions();
            RunPaletteRegressions();
            RunDayTickRegressions();
            RunSaveRegressions();
            RunProductionSelectionRegressions();
            await RunNavigationRegressions();
            await RunSaveUiRegressions();
            RunAccFuelRegressions();
            await RunSettingsRegressions();
            RunAccBacklogRegressions();
            await RunInteriorBacklogRegressions();
            await RunTimeAnimationRegressions();
            await RunSettingsAdditionalRegressions();
            await RunSettingsPresetRegressions();
            await RunNewsRegressions();
            await RunPilotWarningRegressions();
            await RunUnknownObjectRegressions();
            await RunStationStatusRegressions();
            RunMtxRouteRegressions();
            await RunMtxInstallationRegressions();
            RunScriptLifetimeRegressions();
            await RunMenuArtworkRegressions();
            await RunAmbienceRegressions();
            await RunConstructionArtworkRegressions();
            await RunMenuSoundRegressions();
            await RunTradeDecisionRegressions();
            await RunHullTravelRegressions();
            await RunShipAssemblyRegressions();
            await CheckAsync("Queued navigation input survives physics picking and collection", QueuedNavigationInputLifetime);
            await RunSupplyPodRegressions();
            Check("Window close drains resources whose finalizers are still pending", PendingFinalizersAtShutdown);
            await RunEngineDamageRegressions();
            await RunDfccFuelRegressions();
            RunAsteroidAccRegressions();
            RunAmaCargoRegressions();
            RunAccCycleRegressions();
            RunStaffRankRegressions();
            await RunResearchOnlyRecipeRegressions();
            RunSdmInstallationRegressions();
            foreach (var dispose in new[] { false, true })
                Check($"Window close releases parsed C# input events disposed={dispose}", () => ParsedInputAtShutdown(dispose));
            RunStaffAttritionRegressions();
            await RunSelfDestructRegressions();
            await RunSdmAlarmRegressions();
            if (listCases)
            {
                GD.Print($"TEST CASE COUNT: {declaredCases}");
                GetTree().Quit(0);
                return;
            }
            if (isolatedCase && passed + failed == 0)
            {
                failed++;
                GD.Print($"FAIL: Unknown regression case {selectedCase}");
            }
            // Let detached QueueFree nodes finish deletion before shutting down the process.
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            GD.Print($"REGRESSION RESULT: {passed} passed, {failed} failed");
            if (closeWindowAfterTests && failed == 0)
                GameCore.SingletonInstance.Notification((int)Node.NotificationWMCloseRequest);
            else
                GameCore.SingletonInstance.RequestQuit(failed == 0 ? 0 : 1);
        }

        private void Balance(int source, int target, int expectedSource, int expectedTarget)
        {
            Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver);
            var a = Save.BaseGameData.Planets[StellarBodies.earth].Station;
            var b = Save.BaseGameData.Planets[StellarBodies.the_moon].Station;
            a.Built = b.Built = true;
            a.MtxInstalled = b.MtxInstalled = true;
            a.Resources.Stores[ItemTypes.iron] = source;
            b.Resources.Stores[ItemTypes.iron] = target;
            var mtx = a.Resources.Stores.MTX;
            mtx.Target = StellarBodies.the_moon;
            mtx.BalanceItems.Add(ItemTypes.iron);
            MtxScreen.UpdateMTX(0, 1);
            Equal(source + target, a.Resources.Stores[ItemTypes.iron] + b.Resources.Stores[ItemTypes.iron], "total stock");
            Equal(expectedSource, a.Resources.Stores[ItemTypes.iron], "source stock");
            Equal(expectedTarget, b.Resources.Stores[ItemTypes.iron], "destination stock");
        }

        private Item Product()
        {
            return new Item
            {
                ItemType = ItemTypes.derrick,
                Research = new ResearchItem(ItemTypes.derrick, 0, 0),
                BuildRequirements = new List<BuildRequirement> { new BuildRequirement(ItemTypes.iron, 10) }
            };
        }

        private void ProductionDays(int days)
        {
            for (int i = 0; i < days; i++)
                ProductionScreen.UpdateProduction((uint)i, (uint)i + 1);
        }

        private void DisableFuelRefining()
        {
            foreach (var item in Save.BaseGameData.ItemList) item.AutoProduce = false;
        }

        private void AocConsumesEveryCycle()
        {
            DisableFuelRefining();
            var earth = GameCore.Earth;
            earth.GroundSelected = true;
            earth.Factory.Ground = true;
            earth.Factory.AOC = true;
            earth.PlanetResources.Stores[ItemTypes.iron] = 10;
            earth.PlanetResources.Stores[ItemTypes.derrick] = 0;
            earth.Factory.ProductionQueue.Add(new ProductionItem(Product()) { AOCRepeat = true });
            ProductionDays(50);
            Equal(1, earth.PlanetResources.Stores[ItemTypes.derrick], "only paid-for output");
            Equal(0, earth.PlanetResources.Stores[ItemTypes.iron], "materials charged once");
            earth.PlanetResources.Stores[ItemTypes.iron] = 10;
            ProductionDays(50);
            Equal(2, earth.PlanetResources.Stores[ItemTypes.derrick], "production resumes with new materials");
            Equal(0, earth.PlanetResources.Stores[ItemTypes.iron], "second recipe charged");
        }

        private void FactoryDestination(bool groundFactory, bool groundSelected)
        {
            DisableFuelRefining();
            var earth = GameCore.Earth;
            earth.GroundSelected = groundSelected;
            earth.Factory.Ground = true;
            var factory = groundFactory ? earth.Factory : earth.Station.Factory;
            factory.Builder = new Staff { Count = 1, Type = StaffType.Production, Leader = "Tester" };
            factory.ProductionQueue.Add(new ProductionItem(Product()) { Active = true, Production_Complete = 4 });
            earth.PlanetResources.Stores[ItemTypes.derrick] = 0;
            earth.Station.Resources.Stores[ItemTypes.derrick] = 0;
            ProductionScreen.UpdateProduction(0, 1);
            Equal(groundFactory ? 1 : 0, earth.PlanetResources.Stores[ItemTypes.derrick], "ground output");
            Equal(groundFactory ? 0 : 1, earth.Station.Resources.Stores[ItemTypes.derrick], "orbital output");
        }

        private void RemoveCasualties()
        {
            Save.Ships.Clear();
            var survivor = new Shuttle { ShipType = Ship_Types.Shuttle, ShipState = Ship_States.Docked,
                PlanetLocation = StellarBodies.earth, Modules = new List<ShipModule>(), Fuel = 10 };
            Save.Ships.Add(survivor);
            for (int i = 0; i < 2; i++)
                Save.Ships.Add(new Shuttle { ShipType = Ship_Types.Shuttle, ShipState = Ship_States.UnDocked,
                    PlanetLocation = StellarBodies.earth, Modules = new List<ShipModule>(), FallingCount = 4, Fuel = 0 });
            InteriorScreen.UpdateShips(0, 1);
            Equal(1, Save.Ships.Count, "survivor count");
            Equal(survivor, Save.Ships[0], "surviving ship");
        }
    }
}
