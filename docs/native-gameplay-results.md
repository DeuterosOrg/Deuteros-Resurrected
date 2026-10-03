# Native new-game playthrough — 2026-10-03

**PASS for the bounded early-game route below**, using the Mac source build at runtime `89d39ffb164ac1c6cffe21f2cfff18610e193bbe` (documentation checkpoint `982c118`). This was physical mouse/keyboard interaction from New Game, with ordinary fast-forward; no cheats, progression presets, edited saves or injected game state.

## Route exercised

1. Train 70 researchers, 70 production staff and 10 marines in two batches. Confirm the shared recruit population decreases by 150 to 5,850. Later staff losses occur naturally through attrition.
2. Research the shuttle chassis, drive and MeH fuel. Manufacture a Derrick, chassis and drive. Confirm Stores follows the selected manufacturing recipe.
3. Build a shuttle, assign its pilot, fit the manufactured drive and rename it **FIRST LIGHT**. Escape opens Settings and pauses time; closing it resumes time.
4. Save to a previously empty test slot at `3100 926.00`, with fuel research at 45%. Reload and verify the name, engine, crew and research progress. Load opens Master Control; the Earth globe returns to the starting city.
5. Complete fuel research and let normal refining produce stock. Forty fuel-button clicks transfer exactly 40T: Earth stock 336→296, tank 0→40.
6. Take off from Earth and reach orbit with 35T. Land with 33T. Observe the interior status, engine status and available menus change through the flight.
7. Save at `3101 311.01`, reload and verify the grounded ship, name, crew of nine and 33T. Close the game normally: exit 0, strict runtime log clean.

## Evidence and limits

Screenshots, both unmodified normal-play saves, logs and results are preserved locally under `artifacts/validation/evidence/early-game-desktop-89d39ff/`. The final checkpoint is `final-slot-4.json`; its backup preserves the earlier save. These ignored artifacts do not travel with Git. All pre-existing user save files and hashes were restored exactly after testing; the pre-existing Godot editor remained open.

The playthrough exposed overlapping Research mass units, addressed by the later case-591 follow-up. It also prompted a separate low-population training regression; that boundary was staged in case 590, not reached in this campaign.

This establishes early progression and a first shuttle roundtrip on Mac. It does not establish a complete campaign, native Windows behavior, original-runtime timing or subjective audio quality. The continuation below extends the route through the first orbital factory.

## First orbital factory continuation

Runtime `833cb85da2e7bf6d377b0135e2c8f6c88e5f4a24` loaded the unmodified first-flight checkpoint. Through physical controls and ordinary fast-forward:

1. Train another 100 researchers and 100 production staff; the recruit pool falls to 5,650. Research OF frames, tool pods and supply pods. Manufacture eight frames and one tool pod through paid production. Redman earns Engineer, then Expert; Cavell earns Doctor.
2. Fit the tool pod and deliver all eight frames with FIRST LIGHT. Each launch uses 5T and each landing 2T; each deployment consumes one frame. The first section triggers the IOS discovery bulletin. A one-section save/reload preserves incomplete construction and the orbiting shuttle.
3. Complete the eighth section, open Earth Orbital from Master Control, and dock normally: 10T becomes 9T. Blunket reaches Captain through these trips, retaining nine crew.
4. Craig then continues play, including returning to Earth and adding staff/derricks. Preserve that shared checkpoint at `3104 024.06`, reload it and open the completed station. This later state is explicitly shared play, not attributed to the autonomous route.

Evidence: `artifacts/normal-campaign/` contains screenshots, runtime log, partial/final saves and the result ledger. `completed-station-shared-play.json` preserves the later checkpoint. Normal close exited zero; the strict game-log audit passed, with existing focus/screen-lock warnings retained. Original save inventory and hashes were restored.

The final-frame interior briefly displayed Empty despite the bay and saved module containing the frame; visiting through Save refreshed it. This remains an unconfirmed display/timing discrepancy, not a claimed fix. Craig also identified the dead Service icon; its separate repair and verification are recorded with the subsequent validation checkpoint. Later colonies, interplanetary/interstellar progression and Windows acceptance remain open.

## Reported Service control

At runtime `50ea4af`, click the highlighted upper-left Service artwork (185,98 in the 960-pixel game window) from the shared checkpoint. It now opens the Earth ground bay at the crew section. Returning to the interior and repeating the click also succeeds; fuel remains 6T and Captain Blunket retains nine crew. Native close exits zero and the strict game-log audit passes. Original save files and hashes were restored exactly. Evidence is under `artifacts/validation/evidence/interior-service/physical/`.

## Orbital staffing continuation

Runtime `50ea4af` continued from the unmodified shared checkpoint. Cryopod research and paid manufacture completed normally, alongside IOS chassis research. IOS drive research was selected and the final save confirms it also completed. Redman's 200-person Expert team left the Earth factory, boarded FIRST LIGHT's cryopod, launched and docked at Earth Orbital. Refueling transferred exactly 14T (Earth stock 3,448 → 3,434; tank 6 → 20); launch used 5T and docking 1T.

The repaired Service control opened the orbital bay with the correct empty orbital fuel store and retained 14T tank. Unloading and assigning the team preserved Expert rank and all 200 staff. Save/reload at `3104 414.08` preserved the staffed factory, docked shuttle, empty cryopod and fuel. Blunket's saved crew count was already eight before reload. Native close exited zero, the strict log audit passed and original saves were restored exactly. Evidence and the next unmodified checkpoint are in `artifacts/orbital-campaign/`.

Inspection of the working orbital crew-assignment action also exposed a missing hover explanation; its follow-up is recorded separately in validation results.

## First IOS and interplanetary roundtrip

Runtime `8fec147419d2a91d69e78cd50aeb7a9e1160def6` continued the unmodified orbital checkpoint through physical controls and ordinary fast-forward:

1. Train 50 production recruits, manufacture a supply pod on Earth, and transport Raphael's 41-person pilot team to orbit in the existing cryopod. Unloading preserves all 41 people; Redman's 200-person Expert team remains assigned to the orbital factory.
2. Replace the empty cryopod with the manufactured supply pod. Deliver six full 250-unit loads: titanium twice, then iron, aluminium, carbon and copper. Every load unloads completely into orbital stores. A later refuel transfers exactly 36T: Earth 4,814→4,778; shuttle 14→50.
3. Manufacture one IOS chassis and drive. Combined stock changes match both recipes exactly: iron 250→120, titanium 500→200, aluminium 250→75, carbon 250→200, copper 250→160. The first chassis triggers the IOS tools bulletin.
4. Assemble the IOS, fit its drive, assign Raphael, and deliver 250 MeH as a seventh cargo shipment. Refuel the IOS to 50T, leaving 200 in orbital stores; rename it **WAYFARER**.
5. Undock (50→49T), set an Earth–Moon course, engage, arrive (47T), return to Earth (45T), and dock (44T). All 41 crew remain aboard. Service opens the correct IOS bay.
6. Save/reload at `3106 049.12`: name, engine, three empty mounts, crew, docked state and 44T survive. FIRST LIGHT remains separately docked with 17T and its empty supply pod. Ordinary attrition leaves Redman's factory team at 199 and Cavell's researchers at 249 in this final save. Close normally: exit zero, strict log clean, original save inventory and hashes restored exactly.

Evidence is in `artifacts/ios-campaign/`, including 38 screenshots, unmodified `final-slot-4.json`, provenance, results and runtime log. These ignored files do not travel with Git. This extends normal Mac acceptance through a first IOS roundtrip; later colonies, mining, interstellar progression and Windows acceptance remain open.

The fuel delivery reproduced a display defect: loading/unloading fuel changes cargo-service stock correctly while the lower bay readout remains stale until refresh. Source review also found invalid takeoff commands granting experience without a departure. Both follow-ups are recorded separately in validation results; they do not increase the Asana completion count.


## Cargo/departure physical follow-up

Runtime `20e74aa` loads the unmodified IOS checkpoint above. With the cargo selector open, loading/unloading all 200 orbital MeH immediately changes both stock displays 200→0→200; FIRST LIGHT's tank stays at 17T. WAYFARER undocks once, followed by ten repeated Take Off clicks. The saved pilot action count is exactly 4, up from 3; the completed departure consumes one tank unit (44→43T), retaining all 41 crew. This verifies the additional shared fixes without cheats or edited saves. Close exits zero, the strict log is clean and original saves/hashes are restored. Evidence: `artifacts/validation/evidence/cargo-fuel-readout/physical/`.

## Grapple preparation and Service recheck

Runtime `b2c435c` continues the unmodified IOS checkpoint through normal research, paid orbital manufacture of one grapple and tool pod, and fitting both to WAYFARER. The upper-left Service control again opens FIRST LIGHT's orbital crew bay. Transfer Captain Blunket's eight-person crew to WAYFARER through the orbital roster and assign Raphael to FIRST LIGHT. Ordinary time advancement before transfer reduced Raphael's crew from 41 to 40.

Refuel WAYFARER 44→50T, with orbital MeH 200→194. Save/reload at `3106 681.14` preserves the grapple, crews, docked ships and fuel. Native close exits zero; the strict log audit passes and original saves/hashes are restored exactly. Evidence is under `artifacts/grapple-campaign/`; `preflight-slot-4.json` is the next unmodified checkpoint. The subsequent salvage route is recorded below.

## First normal asteroid recovery

Runtime `b2c435c` continues the unmodified grapple checkpoint. WAYFARER undocks with 49T, reaches the asteroid belt with 41T, and captures a naturally generated 250T palladium asteroid. Save/reload preserves its mass, mineral and held-object state. The return flight leaves 33T; docking at Earth Orbital leaves 32T, with all eight crew retained. Service opens the IOS bay; the five-second breakup clears the held object and credits exactly 250 palladium to orbital stores.

Native close exits zero, the strict log audit passes and original save inventory/hashes are restored. Evidence and the next normal checkpoint are in `artifacts/salvage-campaign/`, including captured and delivered saves. No save editing or forced random results were used.

This route exposed hardcoded breakup quantities (`100 … 50000`) despite the correct stock transfer. The shared display correction at `0a2f417` shows actual held mass and projected capped stock. Existing case 26 now checks both ordinary and near-capacity unloading of consecutive pods; it fails on the placeholder and passes headlessly and natively after correction. Full validation passes 595/595 with audited startup and cross-export. A corrected breakup screenshot remains pending: the short native test passes, but attempted UI inspection selected a separately launched project manager. The original counter animation in [the source trace](original-asteroid-acc-evidence.md) remains distinct from this static summary correction.


A separate physical reload at `0a2f417` confirms the delivered 250 palladium, empty grapple, 32T tank and Captain Blunket's eight crew. Service again opens the correct Earth orbital IOS crew bay. Native close exits zero, the strict log audit passes and exact original saves are restored. Evidence: `artifacts/salvage-reload/`. The next normal campaign checkpoint remains the unmodified `artifacts/salvage-campaign/final-slot-4.json`.


## Three-grapple salvage and sequential unloading

Normal Mac play at `bcaeb1c` manufactured and fitted two additional tool pods/grapples, refuelled WAYFARER, completed AMA research and captured naturally scanned 250T silver, 100T silica and 50T platinum. All three distinct held objects survived save/reload. The return to Earth consumed eight fuel in flight and one docking, leaving 32T; Blunket remained Admiral with eight crew. No save values or scan outcomes were edited.

This exposed a remaining defect in [grapple unloading](https://app.asana.com/0/1214891399253076/1215716464570921): after the timed dialog closed, its invisible enclosing control still intercepted pod clicks. Leaving and re-entering the bay worked around it. The existing regression emitted button signals and missed this pointer-only failure. Case 26 now uses real GUI hit-testing through a SubViewport, covers all three mounts and retains stock-cap, lock and day-refresh checks. It fails on the second unload before the one-line enclosing-panel cleanup and passes afterward.

A physical replay of the unedited captured-object checkpoint then unloaded all three consecutively in one bay visit. Screenshots show `250 Silver 250`, `100 Silica 100` and `50 Platinum 50`; the correct orbital stocks survive save/reload. Both native sessions exited zero, strict log checks passed, and the exact original save inventory was restored. Runtime `06d09c3` passes all 595 fresh-process regressions, 19 Python checks, strict import/startup and the audited Windows cross-export. Windows execution of this cleanup remains pending.

Evidence: `artifacts/ama-salvage-campaign/`; corrected final slot SHA-256 `8fc343d0d9d5d71776d8813ad978e93db1403951b445936fefe0d20d282c916f`. That run also exposed hardcoded mount numbers in the grapple and AMA panels; the later correction is recorded below.

## Normal AMA mining and ACC return

The next unedited campaign checkpoint completed ACC research, paid manufacture of AMA, ACC and one supply pod, and normal fitting. The saved orbital stock debits match the combined recipes exactly: iron 6, titanium 74, aluminium 12, carbon 31, copper 4, platinum 5 and silver 1. ACC installs through the cockpit control; carrying it in a tool pod does not install the controller. Removing all three grapples and exchanging one tool pod returns that equipment to stores. Manual refuelling transfers 18 MeH, taking WAYFARER from 32 to 50T; initial ACC departure transfers the remaining 158 orbital MeH before launch.

At runtime `551f31a`, the Earth–asteroid route rejects naturally scanned small asteroids and automatically approaches a class-8, 60,000T copper asteroid. Mining produces cargo, and save/reload preserves the scan, route, fuel and 28 copper. The third-pod Ditch control removes only those 28 units; a saved-state comparison preserves stores, other pods, pilot, fuel and automation. Reloading resumes mining normally. Ordinary attrition reduces Blunket's crew from eight to seven.

Filling the pod to 250 triggers automatic launch. Selecting **Complete Cycle** during launch exposed a stall: the scanner branch intercepted launch completion, leaving the ship orbiting the asteroids. Runtime `a06bb0f` lets the existing launch transition start the return flight. Existing case 325 fails before the correction and passes afterward, including a launch-time save/reload, delivery and stopped automation; 25 related headless checks and native case 325 also pass.

A physical replay from the unedited mining save fills the pod, selects Complete Cycle from the launch screen, returns to Earth, docks and unloads exactly 250 copper. Orbital copper rises 150→400; the empty pod, 186T tank, seven crew and disengaged ACC survive reload at `3107 918.24`. Service opens the correct Earth orbital IOS bay. Both campaign sessions exit zero with clean strict logs; original saves are restored exactly. Evidence: `artifacts/ama-acc-campaign/`; final slot SHA-256 `67971615e03e1f66304d4f1fa0c0dc06698612ac3c3e6c832e54e3984f731133`.

This covers one normal mining delivery and cycle stop. Repeated continuous trips, a two-hull expedition, original-runtime comparisons and native Windows acceptance remain open; the 48-task acceptance count is unchanged.


## AMA removal, refitting and module labels

Runtime `56884880684c054d64446c72ce88fcd6e4c27262` continues from the unedited normal mining-return save. Three physical clicks selecting unavailable ACC equipment leave the installed AMA and zero AMA stock unchanged. Selecting the installed AMA removes it, clears its mount and raises orbital AMA stock exactly 0→1; other orbital stocks and the 186T tank remain unchanged. The removal survives save/reload. This exercises removal and an unavailable-replacement edge case for task 1216065854613348.

Normal refitting puts an existing grapple into mount two, exchanges the empty third supply pod for the returned tool pod, and installs the single AMA in mount three. Saved stock and modules confirm exactly one fitted AMA, two remaining stored grapples, one returned supply pod and no remaining stored tool pod. Undocking consumes one fuel, leaving 185T and seven crew. The grapple window displays **2**, the AMA window **3**, and the AMA number/equipment survive another reload. Both panels previously hardcoded 1; their existing `Load` methods now derive the number from the selected module. Case 65 reproduced each wrong label separately and passes with its modal-close checks retained.

The native session exits zero with a clean strict log, and exact original saves are restored. Evidence is in `artifacts/module-panel-number/physical/`; the refitted orbital checkpoint SHA-256 is `2f6ecc1529cd7d9ac137e1d1ee063b33f7444adc72201bd82b0f12e06cfaa68c`. The main colony continuation may still use the earlier docked mining-return save. An old mining scan was visible on the first undocked Earth update and cleared on the following scan update; no capture exploit or original-fidelity conclusion was established. Windows acceptance remains open.


## Departure scans and ACC cursor lock

The unedited mining checkpoint is replayed at `eda1edd`: 28 copper grows to a full 250T pod, the ship departs automatically, and Complete Cycle is selected during the return flight. A transit save contains no asteroid scan and all 250 copper. Reloading that save reaches Earth, credits copper 150→400, empties the pod and stops automation with 186T fuel and seven crew. The arrival save also has no scan. This corrects the retained mining scan observed in the preceding refitting run: manual departure cleared it in the screen handler, while ACC called the shared model directly. Successful model departures now clear it; rejected departure and same-location mining launch preserve it. Evidence: `artifacts/scan-lifetime/physical/`; arrival SHA-256 `e8fe3345500f5f94f311c9f37af666c7411674550ca5130e70a3e030b967e971`.

That replay also reproduces a remaining Service failure after ACC use. Service ignores clicks even after leaving/re-entering the ship; a single right-click releases the stale cursor lock and restores Service. ACC command buttons removed their panel without unlocking the cursor. Runtime `2151aa3` adds the missing unlock to `CloseACC`. From the unedited arrival save, physically opening ACC, choosing Off and clicking Service immediately opens the correct Earth orbital IOS crew bay without the workaround. Regression 592 now performs the ACC command before its Service pointer check; regression 325 and other ACC command checks assert lock release before test cleanup. Evidence: `artifacts/acc-cursor-lock/physical/`.

Both normal-play sessions exit zero with clean strict logs; original save inventories are restored exactly. These additional fixes do not close a Windows acceptance task. The physical corrected Service check uses Off; Engage/Complete Cycle lock release also passes automated checks.


## Shuttle supply runs and Moon station preparation

Normal Mac play at `2151aa3` continues from the unedited mining-arrival save. FIRST LIGHT descends to Earth, opens Service in both the ground and orbital bays, refuels normally and delivers 250 MeH to orbit. Redman's orbital factory manufactures one paid ACC, which is installed in the shuttle cockpit. Repeated automatic trips deliver 750 iron, 750 titanium, 500 aluminium and 750 carbon. Automatic refuelling works; Raphael reaches Captain with 40 crew. Complete Cycle during the last loaded ascent delivers the titanium, empties the pod and stops with 46T fuel. Service opens immediately afterward.

Eight orbital-factory sections are then manufactured individually. Saved-state comparison verifies exactly eight outputs and recipe debits of 440 iron, 640 titanium, 400 aluminium, 200 carbon and 320 copper. No resources, crew, unlocks or random results were edited. Evidence: `artifacts/moon-campaign/`. Moon deployment is covered by the continuation below; ground-base repair remains pending.

Loading the sections reveals a shared input defect: the drawn cursor stays inside the equipment picker, but real clicks can switch the background mount. The stale picker then fits an OF frame into a supply pod. The reproduction save is retained separately and is not a campaign continuation. Runtime `00949a5` blocks mouse-button presses outside the active cursor rectangle while preserving releases and right-click dismissal. Existing regression 64 reproduces the failure and now checks cargo, tool and crew pickers.

A physical replay from the clean eight-frame checkpoint confirms that an outside mount click is rejected, a frame selection still works, right-click closes the picker, and subsequent mount navigation works. WAYFARER then legitimately exchanges its empty supply pod for the spare tool pod and loads three sections. The saved state contains three Tool mounts with one frame each, five stored frames, one returned AMA and one returned supply pod; fuel stays 186T and Blunket retains seven crew. Evidence: `artifacts/bay-modal/physical/`; preflight SHA-256 `8d38e2db994046ea57e367e9c76f756b71d3261389be4649a8ac67bd80b18e86`. Both native sessions exit zero with strict logs passing and exact original saves restored. Windows acceptance remains open.


## Moon orbital station construction

Runtime `00949a5` continues from the unedited three-frame preflight checkpoint. Select Bandaid research, set WAYFARER's course to the Moon, and deliver eight paid frames in three normal shipments of three, three and two. Each deployment consumes its own frame. A three-section save/reload preserves the incomplete station and empty tool pods. Both return flights allow immediate Service access at Earth Orbital.

The eighth section enables the Moon station menus and docking. WAYFARER docks with 170T fuel, down from 186T across the complete route, and retains Blunket's seven crew. Service opens the new Moon orbital bay before and after save/reload. Saved-state comparison confirms eight built sections, station ordinal 2/refining slot 1, zero remaining OF frames in Earth stores or the three tool pods, and unchanged other Earth orbital stocks. Bandaid research reaches 100% through normal elapsed game time.

Evidence: `artifacts/moon-station-campaign/`, including screenshots, partial/completed/reloaded saves and `results.json`. The completed checkpoint at `3108 136.31` has SHA-256 `d424506796bc787a2d2b9a9add8909639781a2cccc89791f98dbcdba69184c41`. Native close exits zero, the strict log audit passes, and original user saves are restored exactly. The Moon ground base remains damaged; staffing, local manufacturing, repair and Windows acceptance are still pending.


## Moon staffing, manufacture and ground-base repair

Normal play at `00949a5` continues from the completed station checkpoint. WAYFARER receives one newly manufactured Cryo pod and two Supply pods, returns its three empty Tools, and ferries Redman (198) and Raphael (40) to the Moon. Redman staffs the local factory; Raphael leaves FIRST LIGHT to pilot the new Moon shuttle. Three freight trips deliver iron 250, titanium 218, aluminium 157, carbon 250, copper 77 and MeH 199. The two new freight pods cost titanium 4, aluminium 2 and copper 2.

Local manufacture produces one shuttle chassis, drive, Tool pod and Bandaid, consuming exactly iron 56, titanium 92, aluminium 70, carbon 40 and copper 46. Assembly/fitting consumes those outputs. The shuttle takes 30T fuel, undocks and lands with 27T, then activates its Bandaid through the first Tool mount. Two updates consume the kit and clear the damaged-base flag. Service, Resources and Mining Stores work afterward and after save/reload; crew and fuel remain unchanged. No progression data was edited.

The continuation is `artifacts/moon-repair-campaign/repaired-slot-4.json`, date `3108 255.36`, SHA-256 `01128776734389466c1f34f4b11a556a2c60e3a6fa7c4dfc3aca8c24334e4049`. WAYFARER is Moon-docked with 146T fuel; FIRST LIGHT remains Earth-docked without its transferred pilot. Moon production has Redman; Earth orbital production is vacant. The repaired Moon shuttle is ground-docked with Raphael, an empty Tool and 27T fuel.

**Runtime limitation:** the long session exits zero but fails strict log validation with `InvalidOperationException: Handle is not initialized` in `ScriptManagerBridge.SwapGCHandleForType`. Its exact trigger is unknown. The original log and bounded gameplay assertions are retained separately in `artifacts/moon-repair-campaign/results.json`. A short replay, including save/reload during active repair, exits zero with a clean strict log; this does not resolve the long-session failure.

The repair also exposed status text overlapping cargo. Runtime `149ec85` shortens the first lines to “Repairing On” and “Launching”, retaining the location below. Existing case 593 reproduces both width failures and now checks the 104-pixel status area. Focused cases 103/104/195/196/325/592/593 pass. A physical repair replay confirms clear text, kit consumption and restored controls, with a clean strict log and normal close. Evidence: `artifacts/repair-status/`. Original user saves were restored exactly after every session. Windows acceptance remains open.


## Moon derrick delivery, mining and reload

Normal play at `149ec85` continues from the repaired Moon save. Redman starts one paid Derrick (iron 3, titanium 4, carbon 1). The first save captures work in progress after two updates; manufacture finishes while the shuttle ascends. Raphael docks, loads that rig into the empty Tool pod, returns to the surface and unloads it. Resources consumes the stored rig and changes the deployed count from zero to one. The route uses exactly nine fuel units (27→18); crew remains 40. The shortened “Launching” status is visibly separate from Derrick cargo during undocking, and Service works after docking and landing.

Two subsequent updates reduce each seam by exactly its credited output: iron/titanium/aluminium/carbon/silica each gain four units; deuterium/gold each gain two. Orbital stocks stay unchanged. Save/reload preserves the rig, deposits and stock; Resources, Mining Stores and the station's deployed-rig readout agree. A naturally elapsed 0.01 date increment after reload produces one further mining batch, giving six units of each basic mineral/silica and three deuterium/gold. This additional batch is accounted for in the saved-state audit rather than mistaken for a reload mutation.

Evidence: `artifacts/moon-derrick-campaign/`, including 50 phase snapshots, saves, `audit.py` and `results.json`. The continuation is `reloaded-mined-slot-4.json`, date `3108 268.38`, SHA-256 `74cf5d7cfd06c7463db2c9285e7bb61452f753f37939045f75956f39cf29db58`. Native close exits zero; every phase and the final strict log pass. Original user saves are restored exactly. The prior longer-session handle error is not reproduced or resolved by this run; Windows acceptance remains open.

## ACC stock balancing and Clear refresh

Normal play at `149ec85` continues from the unedited Moon-mining checkpoint. Selecting iron at both ACC endpoints and completing a Moon-to-Earth cycle loads 11 iron, then unloads it at Earth: orbital stocks change from Earth 168 / Moon 191 to 179 / 180, conserving all 359 units. A second cycle returns to the Moon with empty cargo and leaves those balanced stocks unchanged. Both arrivals survive save/reload; Service opens immediately. Automatic refuelling consumes 104 Moon MeH, and eight travel updates consume eight tank units, leaving 242T and Blunket's seven crew.

Evidence: `artifacts/acc-balance-campaign/`, with saved-state `audit.py` and `results.json`. Continuation `balanced-return-slot-4.json`, date `3108 276.38`, has SHA-256 `fcc54aa4395ccc97bbdf0f5147d788f7496f5da4dcc1dc8ea863d888452605da`. An accidental pre-route dismantle was discarded by reloading the original checkpoint before the measured route. Initial screenshot-helper phase entries referenced an older log; `capture-note.txt` identifies them. The actual complete session log passes strict validation, normal close exits zero, and original saves are restored exactly.

Clear exposed a separate display bug: selections were removed from the model but their diamonds and cycle markers remained until reopening ACC. Calling the existing `UpdateState()` after Clear fixes the cause. New regression 598 fails before the change and passes afterward; related cases 87/88/99/100/561 and native case 598 pass. A physical replay confirms immediate refresh without reopening, followed by a clean strict log and exact save restoration. Evidence: `artifacts/acc-clear-refresh/`. The subsequent [599-case aggregate](validation-results.md#acc-clear-and-ground-crew-guards--599-case-checkpoint) includes this change. Windows validation of these particular scenarios remains pending, separately from the Windows passes already credited elsewhere.
