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
            await RunPlanetViewRegressions();
            await RunProductionRodRegressions();
            await RunRecoveredConstructionRegressions();
            await RunResearchDetailRegressions();
            await CheckAsync("Recovered research diagrams preserve original pixels and screen placement", RecoveredResearchDiagrams);
            foreach (var legacy in new[] { false, true })
                await CheckAsync($"SCG item names reach all stock and production screens legacy={legacy}", () => ScgItemNames(legacy));
            await CheckAsync("Module dialogue preserves readable palette colours and formatted text", ModuleDialogueColours);
            CheckUi("Eight artifact deliveries complete the device without scientist work", ArtifactDeliveryCompletion);
            Check("Legacy artifact delivery credit migrates once without losing cargo", ArtifactDeliveryLegacySaves);
            foreach (var automated in new[] { false, true })
                foreach (var ground in new[] { false, true })
                    await CheckAsync($"Recovered device manufactures only in orbit automated={automated} ground={ground}", () => ArtifactManufacture(automated, ground));
            Check("Legacy completed devices acquire their original recipe without losing progress", ArtifactManufactureLegacySave);
            foreach (var hull in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
                await CheckAsync($"Tool selector follows original hull eligibility and exposes its last row hull={hull}", () => ToolFittingHull(hull));
            await CheckAsync("Legacy incompatible equipment is returned once when replaced", ToolFittingLegacyEquipment);
            await CheckAsync("Recovered manufactured device fits one SCG tool slot and survives a save", ArtifactManufactureToFitting);
            Check("New campaigns do not expose artifacts before system capture", ArtifactsStartHidden);
            CheckUi("Only the final hostile station capture reveals one persistent artifact", ArtifactCaptureAssignment);
            CheckUi("Legacy artifact locations and held cargo survive capture-state migration", ArtifactCaptureLegacy);
            CheckUi("Malformed artifact assignment state cannot replace the active save", ArtifactCaptureInvalidSave);
            CheckUi("Eight captured systems reveal eight segments while destruction grants none", ArtifactCaptureAllSystems);
            await CheckAsync("Clearing Sol discovers SCG research after earlier bulletins without granting production", CaptureDiscoversScg);
            await CheckAsync("Trade war starts the first saved transmission after ten eligible updates", TradeWarTransmission);
            await CheckAsync("Six-station war starts the first saved transmission after ten eligible updates", StationWarTransmission);
            await CheckAsync("Transmission stages use original delays and save replay context", TransmissionStageSequence);
            CheckUi("Invalid saved transmission stages and locations cannot replace the world", TransmissionInvalidState);
            await CheckAsync("Interrupted transmission notices retain their stage and release only their own locks", TransmissionNoticeInterruption);
            await CheckAsync("Eight capture reports lead through real grapple recovery to final instructions", TransmissionCaptureToFinal);
            CheckUi("Final recovery supersedes queued segment notices exactly once", TransmissionFinalOverridesLocations);
            await CheckAsync("News replay rotates the original decoding mask without advancing transmission progress", TransmissionReplayMask);
            await CheckAsync("Ship attack and loss transitions publish persistent News once", NewsShipEvents);
            await CheckAsync("Fleet station attack and capture publish ordered News once", NewsStationEvents);
            await CheckAsync("Battle defeat frees its window and closes the destroyed ship controls", BattleLossCleanup);
            await CheckAsync("Leaving battle returns reserved drones and cancels its pending timer", () => BattleInterrupted(false));
            await CheckAsync("Replacing the world during battle cannot mutate the new save", () => BattleInterrupted(true));
            await CheckAsync("Leaving just-completed battle cannot rescue a defeated ship", BattleCompletedThenLeave);
            await CheckAsync("Completed enemy retreat settles survivors and attack threshold once", BattleEnemyFleesOnce);
            CheckUi("Original asteroid classes minerals and mining amount bounds are reachable", AsteroidOriginalRanges);
            await CheckAsync("Manual AMA approach rejects zero fuel and permits eligible refuelled mining", AmaManualFuelGate);
            CheckUi("Refining serves every eligible station over two original phases", RefiningDoesNotStarveOtherFactories);
            CheckUi("Original refining batches enforce all input output and ownership boundaries", RefiningBatchBoundaries);
            CheckUi("Refining phase and allocations survive saves and reject malformed state", RefiningSavedPhase);
            CheckUi("Station construction capture and loss preserve original refining slot lifecycle", RefiningSlotLifecycle);
            CheckUi("ACC consumes post-ship refining output only on the following update", RefiningAfterAccFuelWait);
            CheckUi("Initial station slots match original tables while existing larger worlds remain loadable", RefiningInitialSlotsAndOverflow);
            CheckUi("Simultaneous training production research and ACC arrival preserve original event order", SimultaneousProductionResearchAndArrival);
            CheckUi("Original attrition phase precedes actual shuttle arrival while cryopods remain frozen", AttritionBeforeShipArrival);
            CheckUi("Earth and local mining reject insufficient batches and cap ground and MTX stores", GroundMiningBoundaries);
            CheckUi("Original ground surveys run without derricks and resolve zero or one countdown", GroundSurveyWithoutDerricks);
            CheckUi("Local mining rejects hostile damaged unfinished and nonadvancing updates", GroundMiningEligibility);
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
