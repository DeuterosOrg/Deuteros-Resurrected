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
            CheckUi("Ground survey RNG boundaries and exact-depletion cadence match original arithmetic", GroundSurveyRandomBoundaries);
            CheckUi("Ground mining saves distinguish known zero from surveys and repair legacy overdraw", GroundMiningSavedStates);
            await CheckAsync("Ground materials display distinguishes known zero and zero-delay survey", GroundMiningSurveyDisplay);
            CheckUi("Seven hostile systems discover Hyperlight after original progression delay", HyperlightDiscoveryProgression);
            CheckUi("Hyperlight discovery delay survives save and load", HyperlightDiscoverySavedDelay);
            CheckUi("Hyperlight discovery waits for enemy scheduler count sampling", HyperlightDiscoverySampledCount);
            await CheckAsync("Pending Hyperlight discovery survives competing transmission and reaches normal research", HyperlightPendingResearchFlow);
            CheckUi("Hyperlight discovery state rejects malformed saves and accepts missing legacy fields", HyperlightDiscoverySaveValidation);
            CheckUi("Hyperlight discovery requires seven hostile systems and resets on recapture", HyperlightDiscoveryCountBoundaries);
            CheckUi("Enemy production follows remaining hostile systems and preserves its saved deadline", EnemyProductionRemainingSystems);
            CheckUi("Enemy production resumes after zero systems and crossed deadlines", EnemyProductionOverdueDeadline);
            CheckUi("Hyperlight countdown finishes before detecting changed hostile ownership", HyperlightRecaptureCountdownOrder);
            await CheckAsync("Training light switch dims safely and restores controls", TrainingLighting);
            await CheckAsync("Training door audio follows transitions once and preserves button feedback", TrainingDoorAudio);
            await CheckAsync("Training doors preserve independent animation and lock ownership", TrainingDoorLifecycle);
            CheckUi("Ship loss reports pilots and cryopod crews once with saved history", CrewShipLossNews);
            CheckUi("Station destruction and capture report lost crews without killing survivors", CrewStationLossNews);
            await CheckAsync("Simultaneous production and research bulletins survive delivery and save reload", SimultaneousBulletins);
            await CheckAsync("Blocked discovery bulletins preserve modal ownership and validate saved pending notices", BlockedBulletins);
            CheckUi("Natural fractional time advances the visible date and consumes a simulation update", NaturalFractionalClock);
            CheckUi("Manual advancement and save reload retain the partial natural interval", MixedClockSave);
            CheckUi("Stalled natural time produces one update without catch-up bursts", StalledNaturalClock);
            CheckUi("A blocked natural update survives saving and consumes once after release", PendingNaturalClock);
            await CheckAsync("News and save slots display saved fractional dates independently of update count", FractionalNewsAndSlot);
            await CheckAsync("Arrival date projects the active clock mode across year boundaries", FractionalArrivalDisplay);
            CheckUi("Legacy saves retain dates and in-progress training and flight", LegacyClockSave);
            CheckUi("Malformed saved clocks cannot replace the active world", RejectMalformedClock);
            CheckUi("Paused and invalid frame intervals cannot advance or poison the clock", PausedClock);
            CheckUi("A queued natural increment cannot discard a later manual day request", ManualAfterPendingClock);
            CheckUi("Staff age on displayed calendar crossings independently of simulation count", FractionalAttritionGate);
            CheckUi("Enemy production preserves exact fractional deadlines across saving", FractionalEnemyDeadline);
            CheckUi("AMA mines once per consumed natural update only in its original slot phase", NaturalAmaPhase);
            CheckUi("Manual AMA mining follows the original irregular clock-bit schedule", ManualAmaPhases);
            CheckUi("Asteroid scanning uses the eight-phase ship clock gate", AsteroidScanClockPhase);
            CheckUi("Ship automation slots survive removal reload and interstellar movement", StableAutomationSlots);
            CheckUi("Ship automation slots migrate legacy saves and reject malformed allocation", AutomationSlotSaveValidation);
            CheckUi("Asteroid approach and departure each consume two saved updates", AsteroidApproachCountdown);
            await CheckAsync("Arrival date follows time controls and external stops without waiting for a tick", ArrivalModeControls);
            CheckUi("SCG cross-star travel cannot arrive from matching local orbit indices", InterstellarNotInstant);
            CheckUi("SCG acceleration charges original phase fuel without generic double burn", InterstellarPhaseFuel);
            CheckUi("SCG uses original star distances clocks and local entry mapping", InterstellarTables);
            CheckUi("SCG reload preserves every flight phase and mixed-mode clock", InterstellarSaveJourney);
            CheckUi("Ordinary SCG arrival checks exact star clock and reports loss once", InterstellarOrdinaryClocks);
            CheckUi("SCG exact phase fuel and final ordinary unit follow original boundaries", InterstellarFuelBoundaries);
            CheckUi("Malformed SCG flight saves cannot replace the active world", InterstellarMalformedSave);
            CheckUi("Empty-fuel SCG star arrival strands before body approach", InterstellarEmptyStarArrival);
            CheckUi("SCG disengagement respects accelerated and local flight boundaries", InterstellarDisengagement);
            CheckUi("Hyperlight arrival promotes and persists one Warlord milestone", HyperlightWarlord);
            CheckUi("Hyperlight eligibility precedes ordinary arrival experience", HyperlightRankBoundary);
            await CheckAsync("SCG screen follows private star clocks and Hyperlight rank", InterstellarClockDisplay);
            await CheckAsync("Active SCG flight rejects retained course selection", InterstellarCourseLock);
            await CheckAsync("SCG phase-fuel loss exits the active interior once", InterstellarLossScreen);
            CheckUi("Warlord saves validate rank and preserve frozen transferred and combat crews", WarlordSaveAndConsumers);
            CheckUi("SCG damaged routes and research completed mid-flight retain original transitions", InterstellarDamageAndDiscovery);
            CheckUi("SCG private clock preserves pending blocked updates across reload", InterstellarPendingClock);
            CheckUi("SCG ACC completes only at final body arrival without duplicate delivery", InterstellarAccFinish);
            CheckUi("SCG ACC roundtrip saves correct legs cargo and finishing mode", InterstellarAccRoundTrip);
            CheckUi("Fuelled SCG flight clears old fall debt before later exhaustion", InterstellarRescuedFallCounter);
            await CheckAsync("Legacy SCG saves preserve cargo and gain a functional sixth mount", ScgLegacySixthMount);
            foreach (var hull in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
                await CheckAsync($"{hull} manual cargo service preserves capacity and blocked transfers", () => BayCargoCapacity(hull));
            await CheckAsync("Equipment replacement checks all returns before changing fuel or fittings", BayEquipmentCapacity);
            await CheckAsync("Empty pod removal and replacement respect spare stock capacity", BayPodCapacity);
            foreach (var hull in new[] { Ship_Types.IOS, Ship_Types.SCG })
                await CheckAsync($"{hull} DFCC removal returns converted fuel and drones and restores ordinary controls", () => DfccRemoval(hull));
            await CheckAsync("DFCC removal rejects every insufficient return capacity before mutation", DfccRemovalCapacity);
            await CheckAsync("Remaining controllers and legacy converted hulls retain DFCC state", DfccRemainingControllers);
            await CheckAsync("Engine readout distinguishes drifting from powered transit on every hull", EngineDriftReadout);
            CheckUi("Qualifying Hyperlight Warlord mutinies once without premature cargo loss", RogueSelection);
            CheckUi("Rogue selection follows original eligibility rather than unrelated hull flags", RogueSelectionGates);
            CheckUi("Older saves preserve Warlords without inventing a rogue event", RogueSaveLegacy);
            CheckUi("Rogue save identity rejects malformed and duplicated crew state", RogueSaveValidation);
            CheckUi("Rogue crew identity follows roster prison and loss without a second mutiny", RogueCrewLocations);
            CheckUi("Rogue selection follows allocated slots and disconnects active ACC", RogueStableSelection);
            CheckUi("Rogue controller waits and advances once at each original wait stage", RogueStageWaits);
            CheckUi("Rogue completes real local travel hostile refit and human raid", RogueLocalRaidJourney);
            CheckUi("Rogue raid overwrites supply cargo preserves tools and refuels exactly", RogueMixedRaid);
            CheckUi("Rogue MTX redirection preserves original source and allocation gates", RogueMtxRedirect);
            CheckUi("Occupied rogue docking persists single sabotage roll and crew restoration", RogueOccupiedSabotage);
            CheckUi("Station self destruct frees rogue before ordinary casualty enumeration", RogueSdmEscape);
            CheckUi("Rogue ownership bypasses hostile orbit damage and danger accumulation", RogueHostileProtection);
            CheckUi("Rogue composed cross star relocation saves and consumes real fuel", RogueCrossStarRoute);
            CheckUi("Shared ship loss clears rogue pilot and prison identities exactly once", RogueCrewLossPaths);
            CheckUi("Every ordinary ACC mutation rejects rogue owned ships", RogueAccIsolation);
            CheckUi("Rogue controller rejects travelling raids and obsolete world commands", RogueBusySafety);
            CheckUi("Hostile rogue refit never discards a stored crew", RogueRefitProtectsCrew);
            CheckUi("Rogue emergency launch bypasses manual fuel gate without creating fuel", RogueZeroFuelEscape);
            CheckUi("Interrupted rogue sabotage restores surviving crew including zero strength", RogueInterruptedSabotage);
            CheckUi("Rogue routing prefers mixed strong systems and stable station allocation", RogueRoutingPriority);
            CheckUi("Rogue MTX assignment enforces first eight Solar slots and stock boundary", RogueMtxAllocationBoundary);
            await CheckAsync("Rogue cockpit recovery preserves identity and rejects non SCG reassignment", RogueCockpitRecovery);
            await CheckAsync("Actual bay crew controls reject rogue loading into ordinary cryopods", RogueCryoRejected);
            CheckUi("Fitted prison capture release and full roster preserve saved rogue identity", RoguePrisonModel);
            CheckUi("Free rogue hijacks a docked SCG and swaps its displaced pilot", RogueRosterHijack);
            await CheckAsync("Prison roster gesture retains on right click and releases on timeout", RoguePrisonGesture);
            await CheckAsync("Prison capture callbacks cannot mutate exited or replaced worlds", RoguePrisonInterruptedUi);
            await CheckAsync("Prison capture timeout preserves prisoner when roster becomes full", RoguePrisonFullRosterUi);
            await CheckAsync("Occupied prison rejects equipment replacement pod removal and dismantling", RoguePrisonEquipmentSafety);
            await CheckAsync("Empty prison equipment remains reachable and refunds once", RogueEmptyPrisonReturn);
            await CheckAsync("Rogue bay commands dispatch escape without fitting refuelling or scrapping", RogueBayCommandRejection);
            await CheckAsync("Rogue interior buttons and retained signals cannot command the ship", RogueInteriorCommandRejection);
            await CheckAsync("Rename confirmation retained before mutiny cannot change rogue ship name", RogueRetainedRename);
            CheckUi("Retained mining controls reject takeover and replacement world", () => RogueRetainedModules("mining"));
            CheckUi("Retained grapple controls reject takeover and replacement world", () => RogueRetainedModules("grapple"));
            CheckUi("Retained acc controls reject takeover and replacement world", () => RogueRetainedModules("acc"));
            CheckUi("Retained fleet controls reject takeover and replacement world", () => RogueRetainedModules("fleet"));
            CheckUi("Retained battle controls reject takeover and replacement world", RogueRetainedBattle);
            CheckUi("Obsolete ACC model cannot debit replacement world stores", RogueObsoleteAcc);
            await CheckAsync("Retained cargo and course dialogs preserve rogue and obsolete ships", RogueRetainedCargoCourse);
            await CheckAsync("Retained bay fitting and grapple completion cannot mutate rogue or obsolete ships", RogueRetainedBayFitting);
            CheckUi("Prison discovery follows saved fourth-visit countdown and unlocks research once", RoguePrisonDiscoveryClock);
            await CheckAsync("Normal simulation and story phases preserve mutiny and prison bulletin priority", RogueStoryIntegration);
            await CheckAsync("Prison research paid manufacture fitting and capture use normal controls", RoguePrisonPaidProgression);
            await CheckAsync("Prison capture instructions fit the native panel width", RoguePrisonHelpBounds);
            CheckUi("Sabotage crew restoration preserves attrition across reload and recovery", RogueSabotageAttrition);
            CheckUi("Destroyed sabotage targets release surviving rogue crews and resume routing", RogueSabotageDestroyedTarget);
            await CheckAsync("Ending renders original indexed artwork labels fade and black", EndingPixels);
            await CheckAsync("Ending pauses campaign suppresses Escape and rejects duplicate activation", EndingPauseAndInput);
            await CheckAsync("Ending completes to black and waits for mouse release before replay", EndingHeldReplay);
            await CheckAsync("Ending releases an obsolete world without retaining its pause", EndingWorldReplacement);
            await CheckAsync("Closing the window during ending playback drains native audio", EndingWindowClose);
            await CheckAsync("Ending follows audio clock after a long frame stall", EndingClockCatchup);
            await CheckAsync("Ending rejects malformed timeline and releases its pause", EndingRejectsMalformedTimeline);
            await CheckAsync("Ending scene exit preserves a pre-existing pause", EndingSceneExitAndPriorPause);
            await CheckAsync("Fitted transmitter activates with an undocked Warlord and survives reload", () => TransmitterActivation(false));
            await CheckAsync("Fitted transmitter takes precedence over DFCC generic module interception", () => TransmitterActivation(true));
            await CheckAsync("Transmitter explains its Warlord requirement and preserves original rank gate", TransmitterRankGate);
            await CheckAsync("Transmitter rejects travel and rogue commands while preserving docked bay access", TransmitterStateAndRogueGates);
            await CheckAsync("Ending rejects retained ship controls while owned rename and cargo overlays work", TransmitterRetainedControls);
            await CheckAsync("Eight captured recovered segments manufacture fit and activate the original ending", TransmitterRecoveredCampaign);
            await CheckAsync("Overview pages all stations and survives hostile tails and recapture", OverviewStationCapacity);
            await CheckAsync("Overview pages IOS fleets without losing selection hover or drone counts", () => OverviewFleetCapacity(false));
            await CheckAsync("Overview pages SCG fleets without losing selection hover or drone counts", () => OverviewFleetCapacity(true));
            await CheckAsync("Training allocations share the remaining recruit population", TrainingRecruitCapacity);
            await CheckAsync("Research mass units do not overlap one to four digit values", ResearchMassLayout);
            await CheckAsync("Interior service artwork opens the correct docked ship bay by pointer", InteriorServiceNavigation);
            await CheckAsync("Interior service rejects unavailable bays and retained locked commands", InteriorServiceGates);
            await CheckAsync("Takeoff awards experience and clears bay state only for an actual departure", InteriorTakeoffGates);
            CheckUi("AMA departure remains possible after mining exhausts the last fuel", AmaEmptyFuelDeparture);
            CheckUi("MTX delivers freshly extracted materials before factory work", () => MtxProductionOrder(false));
            CheckUi("MTX sends newly completed factory output on the following update", () => MtxProductionOrder(true));
            CheckUi("ACC Clear immediately refreshes both endpoint selections and cycle markers", AccClearRefresh);
            await CheckAsync("Ground construction and repair require a nonempty pilot crew", GroundWorkRequiresCrew);
            await CheckAsync("Supply tool and cryo pods animate fitting and removal with one stock transaction", PodFittingMotion);
            await CheckAsync("Pod replacement animates old then new and releases only its own lock on exit", PodReplacementAndExit);
            await CheckAsync("Empty combat fleets settle through the actual battle completion path", BattleEmptyFleets);
            CheckUi("PTL equality boundaries preserve original survivors and refresh combat power", BattlePtlBoundaries);
            await CheckAsync("SCG trading uses the original three pod positions and preserves later cargo through saves", TradeScgPositions);
            CheckUi("Six hostile systems discover PTL after the saved original delay even after Hyperlight research", PtlDiscoveryProgression);
            await CheckAsync("Captive colony events preserve stock caps deposits cooldown and saved bulletin replay", PtlColonyStockEvent);
            CheckUi("Capture repair and station loss retain the original captive colony event eligibility", PtlCaptiveLifecycle);
            CheckUi("PTL discovery saves reject malformed state and retain old cargo and installed launchers", PtlDiscoverySaveValidation);
            await CheckAsync("PTL cockpit fitting consumes one local launcher and reaches saved combat on IOS and SCG", PtlCockpitFitting);
            await CheckAsync("PTL fitting gates research stock locks and retained ships while preserving ordinary ACC", PtlFittingGates);
            CheckUi("Asteroid fuel uses the saved shared 255-update phase across scanning and mining hulls", AsteroidFuelCadence);
            CheckUi("Asteroid exhaustion disengages ACC without destroying existing scanners or miners", AsteroidFuelExhaustion);
            CheckUi("Asteroid approach charges alternating updates and preserves exhausted countdown across reload", AsteroidEmptyApproach);
            CheckUi("Asteroid departure doubles remaining time on exhaustion then strands for six updates", AsteroidEmptyLaunch);
            await CheckAsync("Asteroid interior distinguishes continuing empty-tank scanning from stranded arrival", AsteroidFuelStatus);
            CheckUi("Asteroid fuel saves validate countdowns migrate old approaches and preserve empty arrivals", AsteroidFuelSavesAndArrival);
            CheckUi("ACC commands preserve asteroid scans approaches mining and departures across reload", AccAsteroidActivation);
            CheckUi("Paid manual production resumes without a second recipe after switching and reloading", ResumePaidProduction);
            CheckUi("Removing production staff preserves paid jobs and progress through reload", () => ResumePaidProduction(true));
            CheckUi("AOC conversion retains paid paused jobs through selection cancellation and legacy reload", AocPaidManualQueue);
            CheckUi("AOC installation waits for staff capacity and commits once without transferable stock", AocStaffCapacity);
            await CheckAsync("MTX preset checks indirect crew allocation without blocking an existing station", SettingsMtxFullCrew);
            await CheckAsync("Unavailable bay pods explain rejection without changing stock or fittings", BayUnavailablePods);
            await CheckAsync("Module service background returns to the cockpit without intercepting controls", BayModuleBackground);
            await CheckAsync("Launch view uses full-size storm doors and preserves the small preview", InteriorStormDoorArtwork);
            await CheckAsync("Deployment completion text retains visible colours and fits the module window", DeploymentCompletionText);
            await CheckAsync("Orbital departure refreshes both planet views while the large screen is open", InteriorOrbitViews);
            CheckUi("Hostile docking checks defenders before the first danger tick and preserves cleared access", HostileDockingGates);
            await CheckAsync("Real docking control rejects defenders and accepts victory despite a stale danger counter", HostileDockingPointer);
            await CheckAsync("Combat owns ship commands and freezes normal updates until its window closes", BattleOwnsCommands);
            CheckUi("Cleared and SDM stations preserve orbiting IOS and SCG after an old danger tick", () => ClearedStationDanger(false));
            CheckUi("Cleared and SDM stations never roll engine damage on IOS and SCG escape", () => ClearedStationDanger(true));
            await CheckAsync("Fitted ACC lamp pulses only original palette pixels in every hull and mode", AccLampPulse);
            await CheckAsync("Optional bulletin skip preserves pause and input lock ownership", BulletinSkip);
            await CheckAsync("SCG chassis research discovers Star Drones once and retains the bulletin across save load", StarDroneDiscovery);
            Check("Completed-chassis saves recover missing Star Drone discovery without changing campaign assets", StarDroneDiscoveryLegacy);
            CheckUi("Star Drone manual and AOC production require the original 300 titanium", StarDroneRecipe);
            CheckUi("Star Drone recipe migration preserves paid work and charges future orders at the corrected cost", StarDroneRecipeLegacy);
            await CheckAsync("IOS and SCG can select Oberon through its moon button and retain the ACC course across saves", OberonCourse);
            CheckUi("Original moon chart positions reach all 160 bodies and display their deposits", OriginalMoonRoutes);
            Check("Old default moon charts migrate without changing campaign state or custom charts", LegacyMoonRoutes);
            await CheckAsync("Reopened planet and moon courses show their destination star immediately", ReopenedCourseStarHeader);
            await CheckAsync("DFCC ships operate selected tools in friendly orbit without losing fleet controls", DfccFriendlyModuleRouting);
            CheckUi("Enemy fleets require a player station before scheduling an attack", EnemyAttackRequiresPlayerStation);
            CheckUi("Enemy arrivals and captures recheck saved station ownership", EnemyAttackRechecksOwnership);
            await CheckAsync("MTX star icon centres select the matching system and preserve the chosen route", MtxStarIconTargets);
            await CheckAsync("Research and Production shortcuts follow current menu eligibility and input ownership", SettingsNavigationShortcuts);
            await CheckAsync("Speed shortcuts select existing fast and normal time without toggling or bypassing locks", SettingsSpeedShortcuts);
            await CheckAsync("Pause binding opens and resumes Settings without discarding changes or another pause owner", SettingsPauseShortcut);
            await CheckAsync("Next Location cycles accessible stations in overview order while respecting input ownership", SettingsNextLocationShortcut);
            Check("Quick Save has an independent atomic slot and retains all five manual saves", QuickSaveStorage);
            await CheckAsync("Quick Save shortcut persists without navigation and its visible slot confirms loading", QuickSaveShortcut);
            await CheckAsync("Pixel scaling previews persists and discards while keeping pointer and overlay transforms aligned", SettingsPixelScaling);
            await CheckAsync("Scanline intensity changes rendered pixels and preserves settings and pointer behavior", SettingsScanlines);
            await CheckAsync("Tooltips preference hides shared hover labels without hiding save feedback or changing navigation", SettingsTooltips);
            await CheckAsync("Auto Pause freezes background simulation and resumes only its own pause", SettingsAutoPause);
            await CheckAsync("Autosave times active play, preserves other slots and confirms recovery", SettingsAutosave);
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
