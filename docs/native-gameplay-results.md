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

## Supply-pod removal and refitting

Runtime `3ebeca0` loads the unmodified balanced Moon checkpoint. Physical clicks remove WAYFARER's second supply pod downward, then refit the returned spare upward. Saved station stock follows 0→1→0, and the mount follows Supply→None→Supply. The other modules, seven crew, 242T fuel and both orbital stations' other stores remain unchanged. Two centidays elapse naturally, allowing ground mining/refining to continue.

Saving and reloading retains the fitted pod; Service and mount selection still work. Normal close exits zero, the strict runtime log passes, and the original save inventory is restored exactly. Screenshots, saves and runnable conservation checks are in `artifacts/pod-motion/physical/`. This normal-input observation covers supply pods; tool/cryo fitting and replacement have separate staged native regression evidence, not normal-campaign acceptance.

## First contact and CommsPod research

Runtime `3ebeca0` continues from the unmodified pod-refitting save. WAYFARER returns from the Moon to Earth Orbital, replaces its empty cryopod with a tool pod, and fits one grapple. The saved stocks confirm one cryopod returned, one tool pod consumed and grapple stock 3→2. The other two supply mounts stay empty; crew remains seven and fuel falls 242→238 through the return.

Ordinary flight then reaches Jupiter, where the Methanoid menu icon appears. Docking and selecting the tool module plays the first-contact message, supplies a communications object to the empty grapple and launches the ship. The object survives save/reload and the return to Earth. This roundtrip consumes 28T, leaving 210T. Unloading shows the research-analysis message, clears the held object and unlocks CommsPod research while manufacturing remains locked.

Professor Cavell's existing 249-person team completes the research through normal time advancement. The interface displays the 5T orbital-only recipe: two aluminium and one each carbon, copper and gold. Saving and reloading retains the completed research. Normal close exits zero, the strict runtime log passes and original user saves are restored exactly.

Evidence and runnable stock/research checks: `artifacts/first-contact-campaign/`. Continuation `researched-slot-4.json` has SHA-256 `ffc3c2299fd2d65f18889b9f679e13efc9e7b41d3f0a33f02f8e63cf9cefa75d`. Two initial manual date advances occurred while the location view was displayed, before the actual departure; they are not counted as flight updates. Manufacture, fitting and trading continue below; Windows acceptance remains pending.

## CommsPod supply chain and fitting

Normal play at `3ebeca0` continues from that researched save. WAYFARER reaches Moon Orbital with 206T and seven crew. Redman's 198-person factory builds a Supplypod; the Moon shuttle launches, replaces its empty tool pod, lands and loads 91 mined gold. It returns to orbit and unloads all 91. The shuttle consumes 15T across these movements and receives one unit of orbital MeH, ending docked with 4T and its original 40 crew.

The factory manufactures one CommsPod. Dedicated before/after saves prove the exact charge: two aluminium and one each carbon, copper and gold. Fitting returns WAYFARER's grapple to stock and consumes the single Comms unit. Both supply mounts remain empty; fuel and crew are unchanged. Save/reload preserves the fitted CommsPod in the cockpit.

Normal close exits zero, strict log validation passes and original saves are restored exactly. Evidence and runnable audit: `artifacts/comms-production-campaign/`. Continuation `fitted-slot-4.json` has SHA-256 `838295c737c55f0ee504d94540aca24efb6c976ce7c81432cf2deae82f923960`. Initial mistaken production selections left paid, inactive Tool/Cryo jobs; these are excluded from the dedicated Comms recipe comparison. A duplicate startup without the .NET environment was closed separately, as recorded in `startup-note.txt`; the intended game completed cleanly. The next session below verifies trade acceptance/refusal and the fuel gift; Windows acceptance remains open.

## Normal Methanoid trading

The next normal session at `3ebeca0` loads 180 iron and 116 titanium from Moon Orbital into WAYFARER's two supply pods. Ordinary launch, twelve travel updates and docking at Jupiter consume 14T, leaving 192T and seven crew. The first offer previews iron→silica and titanium→copper with unchanged quantities. Declining preserves both cargoes, fuel and trade count zero; departure begins normally.

After departure and redocking, accepting the same offer exchanges exactly 180 iron→180 silica and 116 titanium→116 copper, fills fuel from 190T to 250T and advances trade count 0→1. Save/reload preserves that committed result. Another encounter is refused: the exchanged cargo remains and count returns 1→0. A natural .01 clock update completes departure, so the subsequent save has 247T after three movements since acceptance. The saved clock records two manual days plus that centiday; this fuel change is movement, not a refusal charge.

Redocking consumes one further unit. Escape cancels the next offer without exchange, fuel gift, count change or departure. Saving and reloading leaves WAYFARER docked at Jupiter with the same cargo, 246T and seven crew. The native session exits zero, its strict log passes, and original saves are restored exactly.

Screenshots, six normal saves and runnable cargo/fuel/counter checks are in `artifacts/comms-trading-campaign/`. Final continuation `cancelled-slot-4.json` has SHA-256 `9dd75b0346129f327b7e28dd5e0aaab3bf51649ffc76f7f3accf99bd63c8abec`. This verifies normal IOS decisions and persistence. The next session verifies the war threshold; Windows acceptance, original decision timeout and SCG cargo-position fidelity remain separate open requirements.

## Normal war declaration and Fusion Laser research

Continuing that unmodified save at `3ebeca0`, WAYFARER completes 16 ordinary trade encounters. Saved checkpoints at counts 3, 6, 10, 15 and 16 retain peace, seven crew and both cargo quantities. Exchanges alternate iron/silica and titanium/copper, with each acceptance refilling fuel to 250T. The count-15 save follows a natural departure update and therefore contains 249T. Reloads after trades six and sixteen preserve progression.

The next encounter declares war, replaces Comms with a grapple holding the unknown Fusion Laser prototype, and starts departure at 248T. The gift survives reload. Cavell's defence bulletin unlocks DFCC/drone research; the first alien transmission is subsequently acknowledged during the return. Its saved stage advances from 0/countdown 10 to 1/countdown 249 and survives reload. A natural clock update during the message accounts for one travel unit and the countdown decrement.

WAYFARER docks at Earth with 234T, seven crew, 180 silica and 116 copper. Unloading analyses the prototype and unlocks Fusion Laser research; Cavell's existing 249-person team completes it through ordinary time advancement. Saving and reloading retains the completed 25T orbital-only recipe. No weapon manufacture or combat is claimed.

Evidence: `artifacts/normal-war-campaign/`, including ten saves, screenshots and runnable cargo, fuel, research and transmission checks. Native session 70996 closes with exit zero; the final strict log and saved-state audit pass. Original user saves are restored exactly. Continuation `researched-slot-4.json` has SHA-256 `5923f4246c75d7ccb9eda9c922ef186f092e6b6821d4942851f1c203963a2b7e`. Clicks while the defence bulletin held input were ignored and are not counted as travel updates. Windows acceptance and the original trade timeout/cargo-layout questions remain open.

## Normal DFCC manufacture, fitting and fuel

The unmodified war-campaign continuation at `3ebeca0` completes DFCC and IOS Battle Drone research with Cavell's existing team. WAYFARER unloads its traded silica at Earth, loads 45 platinum and delivers it with the retained 116 copper to Moon Orbital. The trip consumes 4T, leaving 230T and seven crew. The Moon shuttle receives 12 MeH, collects 250 mined titanium and delivers it to orbit, ending with 7T and its original 40 crew.

Redman's staffed orbital factory produces one DFCC. Dedicated before/after saves prove the recipe charge: two titanium, two platinum, and one each aluminium, carbon, copper and gold. Fitting replaces WAYFARER's empty grapple, returning that grapple to stock and consuming the DFCC. The old 230T tank contents return to stores, changing Moon MeH from 52 to 282 and the gauge to zero. One fuel click costs ten MeH; a minus click refunds ten. Refuelling to 23 leaves 52 MeH, conserving the pre-installation stock value. Both supply pods remain empty.

Save/reload retains the DFCC, fuel, crew and stock in both cockpit and Service views. Evidence: `artifacts/defence-production-campaign/`, with four saves, screenshots and runnable research, freight, recipe and fuel-conservation checks. Native session 63976 closes with exit zero; strict log and saved-state audits pass, and original saves are restored exactly. Continuation `fitted-slot-4.json` has SHA-256 `cefcc0c051b539dce19a836ccbfe38bf7db62d88547670781bab1e0c6f23c6a8`.

No battle drone was manufactured and no combat occurred. The [earlier Windows DFCC checks](windows-validation-results.md#desktop-agent-reports-retrieved-2026-10-03) remain credited separately; SCG/HeD, ACC, legacy-save and matching-export acceptance remain open.

## Normal drone production and fleet transfer

Continuing the unmodified DFCC save at `3ebeca0`, WAYFARER flies Moon–Earth–Moon and delivers 179 iron and 250 palladium. The roundtrip consumes eight gauge units, leaving 15, with seven crew retained. The Moon shuttle receives ten MeH and brings 250 mined aluminium to orbit, ending with eight fuel units and its original 40 crew.

Redman's factory manufactures one IOS drone. Dedicated saves prove a single charge of 120 each iron/titanium/aluminium, 15 carbon, 55 copper and 30 each platinum/palladium. A subsequent Fusion Laser costs five copper and ten each platinum/palladium. Both finished items appear in orbital stock.

Undocking WAYFARER and selecting its DFCC opens Fleet Transfers. Loading changes orbital drones 1→0, fleet drones 0→1 and fleet power 0→7. Another load from the empty pool leaves all three unchanged. Returning the drone reverses the transfer; reloading it restores the original totals. The overview displays one drone. Docking, saving, reloading and undocking preserve the drone; reopening Fleet Transfers still shows zero orbital drones, one fleet drone and power seven.

Evidence: `artifacts/drone-production-campaign/`, including six saves, screenshots and runnable freight, recipe, fleet and fuel checks. Native session 25625 closes with exit zero; strict log and saved-state audits pass, and original saves are restored exactly. Continuation `reloaded-slot-4.json` has SHA-256 `c89bb9d6a59772b264649caa3fb48881e34102a6f89c693fca59f255b0ec8371`; WAYFARER is in Moon orbit with 12 fuel units. No combat/capture or battle-power effect from manufactured Fusion Laser stock is claimed. Windows and original-runtime acceptance remain separate.

## Normal battle defeat and retreat

The unmodified drone-campaign save at `3ebeca0` now reaches a real station battle. WAYFARER docks at the Moon and pays 40 MeH for four DFCC fuel units. Undocking and twelve travel updates reach hostile Jupiter with two fuel units, seven crew and one drone. Jupiter has naturally increased its defenders to 36. The actual DFCC control opens combat with displayed powers 7 and 144; no battle outcome or fleet is staged.

Allowing combat to finish destroys WAYFARER and returns to Master Control with its entry removed. News appends Admiral Blunket's death followed by the ship's destruction; the screen displays newest first. Save/reload preserves both reports exactly once and does not resurrect the ship. All other ships and every planet's saved state match the prebattle checkpoint, including Jupiter's 36 surviving defenders.

A separate session reloads the unedited prebattle save and clicks Flee. The ship enters return transit to the Moon, retaining one drone, seven crew, two fuel units and its modules; Jupiter retains 36 defenders and no loss report is added. Reload preserves the transit state. The return journey is not completed with this insufficient tank.

Evidence: `artifacts/normal-battle-campaign/`, including six checkpoints, screenshots and `audit.py`. Native sessions 47380 and 53610 exit zero, both strict logs pass, and original saves are restored exactly. An accidental time-run toggle during post-defeat navigation was stopped and its unsaved progression discarded by reloading the defeat checkpoint before the final audit. These checks establish normal defeat/retreat persistence; victory, station capture, PTL, Windows and original-runtime acceptance remain open.

## Normal automated freight and crew transport

At `239beba`, the unmodified drone-campaign continuation resumes with one drone and insufficient orbital supplies for fleet expansion. WAYFARER returns from the Moon to Earth, consuming three DFCC gauge units. Admiral Blunket's seven-person crew temporarily transfers to FIRST LIGHT through the orbital roster.

The shuttle lands, selects MeH as ground-source cargo and starts ACC. Four 250-unit deliveries establish 1,000 MeH in orbit. Complete Cycle stops after unloading at the next arrival. Selecting iron, titanium, aluminium and copper then produces six further deliveries while 41 marines train normally. The trained Kingston crew travels to orbit in an existing cryopod; Kingston takes the shuttle and Blunket returns to WAYFARER. The shuttle returns its empty cryopod to Earth stores, refits the conserved supply pod and completes a seventh metal delivery under Kingston.

Saved-state audit confirms 500 each iron/titanium/copper and 250 aluminium delivered. Ground stock plus remaining ore accounts for every unit. MeH totals include 177 refined units, 57 spent on ground shuttle refuelling, 51 on orbital shuttle refuelling and 200 buying 20 DFCC gauge units. Total travel consumption is 138 stock-equivalent fuel units. Earth orbit ends with 749 MeH; WAYFARER has 29 gauge units, seven crew and its original drone/modules. FIRST LIGHT has 46 fuel, 41 crew and an empty supply pod. Training deducted exactly 41 available recruits; both cryopod stocks are restored.

Reload preserves both crews, ships, inventories and selected ACC rotation. No additional simulation update occurs during the reload inspection; normal wall-time accumulation continues. Native session 10429 exits zero and its strict log passes. Original user saves are restored exactly. Evidence and runnable audit: `artifacts/fleet-supply-campaign/`. Continuation `reloaded-slot-4.json` SHA-256: `8bdfa99dea7ccac53f2a13f6a7fffa6eb00dc4ce35a1598259df6a0f42edf6ae`.

This prepares normal fleet expansion; it does not establish victory, capture, PTL progression or Windows acceptance. Rare-metal supply and further drone manufacture remain next.

## Rare-metal route and endpoint failure

The fleet-supply continuation replaces WAYFARER's last DFCC with the stocked AMA through the normal equipment panel. It returns 290 MeH, one drone and the DFCC, consumes one AMA and preserves the other pods. FIRST LIGHT resumes six-product ground-to-orbit ACC. The retrofit reloads, and WAYFARER's ACC refills its empty tank with 250 orbital MeH.

Changing the return-leg course makes Asteroids the left/source endpoint. Palladium and platinum selected there never trigger mining because the model reads only destination selections. The untouched failure save records source Asteroids, destination Earth, source items 10/11, an empty destination list and empty cargo. The second alien transmission also occurs naturally and renders its partially translated text.

With the endpoint fix, the same save and selections visibly produce 250 plus 104 platinum aboard. This replay is **not a successful delivery**: News records the Moon captured at 8755.62, Earth captured at 8822.62 and WAYFARER destroyed by Earth's station self-destruct at 8904.63. The outcome is retained rather than presented as a UI disappearance or successful return. Repeat delivery from the earlier `retrofit-slot-4.json` checkpoint, before the stalled run wastes campaign time; do not edit enemy state or stock.

Evidence: `artifacts/rare-metal-campaign/`, including retrofit, stalled and final-loss saves, transmission/selection screenshots and native logs. Sessions 90096, 26927 and 51832 exit zero with clean strict logs; original user saves are restored. Victory, capture, rare-metal delivery and Windows acceptance remain open.

## Normal asteroid-source ACC delivery

The untouched day-8715 retrofit checkpoint now completes a rare-metal expedition on gameplay `c8ac3d7` (tested candidate `e6e5ccb`). Ordinary chart navigation sets Asteroids as the ACC source and Earth as destination; source palladium/platinum selections are retained. ACC buys 250 fuel units, launches, scans, approaches and fills both supply pods with 250 palladium each. The second alien transmission appears naturally during the voyage.

Complete Cycle is selected on the loaded return leg. Six controlled updates finish travel/docking; both pods empty and ACC stops. Saved Earth orbital palladium increases **0→500**, with the AMA still fitted and WAYFARER at **228 fuel**. Reloaded ship and Store screens retain the empty pods, stopped ACC, source selections and all 500 palladium; a second save confirms the orbital inventory is unchanged. Earth remains human. The Moon is captured during this replay, so it is not a combat or defence success.

Native session **59694** exits zero with a clean strict log. Original user saves are restored exactly. Evidence: `artifacts/rare-metal-campaign/early-replay/`, including pre/post-reload saves, stock/ship screenshots, results and save audit. Continuation `reloaded-slot-4.json` SHA-256: `432d79149de2a73385c6975e16f6d0c89f59d7c08f47d5d32c1c6a3e1c02dd55`. This closes the normal Mac delivery/reload gap for the endpoint fix; Windows execution, fleet expansion, victory/capture and PTL campaign acceptance remain open.

## Normal asteroid fuel and reloaded departure

Candidate `d7c2ef3` was played from the unmodified normal retrofit checkpoint (update 8715, date `3108 653.62`), with original user saves protected. WAYFARER's existing AMA and empty supply pods were retained. ACC purchased 250 fuel, launched from Earth and reached the asteroid field; the second transmission appeared naturally and was acknowledged.

After searching for rare metals, the saved scanner at update 8813 had **240 fuel**: nine outbound movement units plus the newly implemented scan charge at update 8798. Expanding the normal ACC mineral choices found a suitable titanium asteroid. Its two-update approach cost one unit, leaving **239 fuel**, and ordinary mining produced **165 titanium**. Complete Cycle was selected and the physical TakeOff control initiated an early return with that partial cargo.

The first departure update was saved at update 8838 with **238 fuel and one action update remaining**. Loading that save and advancing once completed departure into the normal Earth return flight, with 237 fuel and the same cargo. At update 8848 (`3108 785.63`), WAYFARER docked, unloaded once and disengaged ACC with **228 fuel**, the AMA retained and two empty supply pods. A second reload preserved its stopped state and every orbital stock quantity.

Earth orbital titanium reached **1,415**: the previous 1,000, WAYFARER's 165, and a separate 250-unit FIRST LIGHT freight delivery. The audit includes Earth's ground stock, remaining deposit and both ships' cargo, proving 54,295 total titanium is conserved across the return instead of attributing the whole 415-unit orbital gain to mining. Earth remained human; the Moon was lost during this campaign continuation.

The native process exited zero with a strict clean log. Original user saves were restored and their hashes verified. Raw saves, inspected screenshots and the runnable `audit.py` are under `artifacts/rare-metal-campaign/fuel-replay/`. This is normal Mac scanning/mining/departure/delivery/reload evidence. Empty-tank boundaries use the separate staged regressions; Windows and full campaign completion remain pending.


## Natural transmission and loss of the undefended station — 2026-10-04

At runtime `cef95c0`, the unmodified palladium-delivery checkpoint (day 8834) begins a platinum-only expedition. During scanning, the normal enemy campaign captures Earth and destroys FIRST LIGHT. The next alien bulletin also arrives naturally; its partially translated message is displayed and acknowledged. UI save/load retains transmission Stage 3, LastStage 2, its countdown and the exact News history, including the ship loss and Earth capture. No platinum delivery is claimed for this run.

Evidence: `artifacts/platinum-campaign/`, with screenshots, before/after reload saves and `audit.py`. Native session 60212 closes zero, its strict log passes and user saves are restored exactly. This identifies a preparation gap in that campaign: the remaining station was undefended while rare metals were still being collected. It does not establish a simulation defect or complete alien-message acceptance.

## Pre-war platinum delivery and fleet preparation — 2026-10-04

The separate continuation starts from the unmodified day 8409 CommsPod-fitting save, before war. WAYFARER returns from the Moon to Earth through ordinary controls. Service exchanges the fitted CommsPod for the stocked AMA: one CommsPod returns, one AMA is consumed, and both empty supply pods remain. The saved day 8667 preflight has 202 fuel, Earth→Asteroids routing, no Earth pickup selection and only platinum selected at the asteroid endpoint.

Normal scanning finds a class-6, 10,000-unit platinum asteroid. At 64 cargo and 191 fuel, Complete Cycle preserves mining; UI save/load retains that cargo and finishing mode. Subsequent normal updates fill the two pods and return to Earth. Orbital platinum rises **45→545**, all other orbital stocks match preflight, both supply pods empty, ACC stops and fuel is 179. Reload displays 545 platinum in Stores and preserves the exact delivered inventory. Earth and Moon remain friendly and war remains undeclared.

Evidence: `artifacts/fleet-preparation-campaign/`, including four checkpoints, screenshots and `audit.py`. Native session 99389 closes zero, strict logs and saved-state audits pass, and user saves are restored byte-for-byte. The continuation `reloaded-slot-4.json` (day 9125) has SHA-256 `fb049a4812815bc31feb39685a53996bb8341c7f3c28194257f0588ac9cbe3a9`. It contains 250 palladium and 545 platinum in Earth orbit; common metals and fuel remain on Earth, and 90 gold is available in Moon orbit. Next: transfer fleet materials and crew before triggering war, then research/manufacture defence equipment and test victory/capture. This is normal Mac supply evidence, not Windows, fleet-construction or SCG acceptance.

## Fleet materials and crew delivery — 2026-10-04

Normal play continues from the unmodified day9125 checkpoint on runtime `01c81f9`. FIRST LIGHT initially waits for orbital fuel under ACC; manual undocking/landing returns it to Earth without changing settings or saves. ACC then refuels and ferries the selected common metals and fuel. Complete Cycle unloads its final cargo in orbit and stops normally.

Earth orbital stores gain **1,500 iron and 1,250 each of titanium, aluminium, carbon and copper**. Final stocks include 1,233 MeH, 250 palladium and 545 platinum. The latter two rare stocks are unchanged. Ongoing ground mining/refining is retained, so this does not claim simple ground-plus-orbit conservation from the two endpoint saves.

Training recruits 41 marines and 100 production staff through normal controls; after intervening days, Kingston has 40 marines and the existing Morse team has 148 workers. Morse leaves the idle ground factory, travels in the stocked cryopod and is assigned to Earth orbital production. All 148 workers and their attrition countdown7 survive the seven-day transfer; Kingston retains40, and shuttle fuel falls79→73. Empty supply-pod replacement returns one spare and consumes the ground cryopod. The integrated module-background return works in this ordinary servicing flow.

UI save/load preserves the exact Earth state and shuttle, with 148 workers visible at the orbital factory and delivered stocks visible in Stocktaker. Day9499 continuation: `artifacts/fleet-materials-campaign/reloaded-slot-4.json`, SHA-256 `7d4d6e251c5b8062d250d5705775240086017cbbe29a44d1d274efad36f3ab33`. Native session71272 exits0 with a clean strict log; `audit.py` checks the checkpoints and original-save restoration.

Next: advance Morse from Apprentice through paid production (currently one completed action; Expert requires12), collect Moon gold, prepare defence equipment and test combat/capture. War remains undeclared and Earth/Moon friendly. This is normal Mac supply/staffing evidence, not fleet manufacture, Windows or SCG acceptance; backlog totals remain unchanged.

## Paid production and Expert promotion — 2026-10-04

The unmodified day9499 checkpoint continues on runtime `01c81f9`. Ordinary factory controls manufacture five Derricks, five Tool Pods and one Orbital Factory Frame. Morse advances from one to twelve completed actions, displaying Engineer and then Expert, with 148 workers retained. No save values or production costs are edited.

The day10912 save accounts for every recipe exactly: orbital iron decreases70, titanium110, aluminium55, carbon30 and copper45; the eleven finished items appear once. Fuel and rare-metal stocks are unchanged. The manual production queue is empty and AOC remains off. Earth and Moon remain friendly, with war undeclared.

UI reload retains the Expert display, exact Earth state and all ships. Native session73699 exits0, the strict log passes, and original user saves are restored byte-for-byte. Evidence and runnable audit: `artifacts/fleet-manufacture-campaign/`. Continuation `reloaded-slot-4.json` SHA-256: `8dd7e0b59aad213a1863b166de1e083ef9f7a9c62ff2fba4206ca0aa500ba997`.

Next: collect the Moon's90 gold and prepare defence research/manufacture. FIRST LIGHT now has39 marines after ordinary elapsed-time attrition; WAYFARER retains seven. This verifies normal paid production, promotion and reload, without adding Windows, drone-combat or SCG acceptance. Backlog counts remain35/48 implementation evidence and four locally accepted requirements.

## Moon gold delivery — 2026-10-04

Normal navigation changes WAYFARER's route from Earth–Asteroids to Earth–Moon. Clearing ACC orders and selecting gold only at the Moon endpoint delivers all90 gold to Earth. ACC continues some empty trips before Complete Cycle is selected; it stops at the Moon, then a separate finishing cycle returns the ship to Earth. This is not a minimum-fuel voyage.

The day10945 checkpoint has90 gold at Earth, zero at the Moon, empty supply pods, the AMA retained,218 ship fuel and stopped ACC. Earth spends71 MeH on refuelling; ship-plus-station fuel accounting shows32 units consumed by travel. Every other orbital stock at both endpoints matches the starting day10912 checkpoint. UI reload preserves exact Earth/Moon and ship state, including Morse's148 Expert workers. War remains undeclared.

Evidence: `artifacts/moon-gold-campaign/`, including the ACC order, returned ship and reloaded Stores captures, four saves and runnable `audit.py`. Native session53506 exits0 with a strict clean log; original user saves are restored exactly. Continuation `reloaded-slot-4.json` SHA-256: `e0734f822e58c513c6208533e2c5e577eaf0e09c3345adc0c177ace07638992f`.

Combat preparation remains incomplete:250 palladium supports only eight drones at30 per recipe. The next supply target is material for60 drones:7,200 each iron/titanium/aluminium,900 carbon,3,300 copper and1,800 each palladium/platinum, plus the DFCC recipe and fuel. This is a preparation target against the initial40-drone attack trigger, not proof of a winning fleet. Stronger hostile stations and later progression need further production. Collect supplies before triggering war; no additional task acceptance is claimed.

## Concurrent bulk freight and rare-metal supply — 2026-10-04

Normal play continues from day10945 on `01c81f9`. FIRST LIGHT lands and exchanges its empty cryopod for the stocked supply pod, returning one cryopod. Its ACC ferries selected common metals and fuel while WAYFARER mines palladium/platinum through the ordinary asteroid route. Later orders narrow to titanium/aluminium for the shuttle and platinum for the miner; no stock or crew values are edited.

By day12210, Earth orbit has gained6,000 iron,7,750 each titanium/aluminium,6,000 copper,2,000 palladium,1,500 platinum and4,628 net MeH. Final stocks include2,250 palladium,2,045 platinum,90 gold and5,790 MeH, satisfying60 drone recipes plus one DFCC. Ground mining/refining and storage caps continue, so this is an exact orbital-stock audit, not a global conservation claim.

Both ACCs stop normally. FIRST LIGHT rests on Earth with an empty supply pod,52 fuel and Kingston's39 marines, now Admiral. WAYFARER returns to Earth orbit with its AMA, two empty supply pods,232 fuel and five crew. Ordinary attrition leaves Morse147 Expert workers; Cavell remains Professor with248 researchers. War is undeclared, Earth/Moon remain friendly and the trade count is zero.

UI reload preserves exact Earth and ship state. Native session62209 exits0 with a strict clean log and byte-for-byte original-save restoration. Evidence: `artifacts/fleet-bulk-campaign/`, with intermediate saves, captures and runnable `audit.py`. Continuation `reloaded-slot-4.json` SHA-256: `8ea88ac2c93aa6e6a9a50a237d6e1618987b40539f44b045d6f2b26ddf9c2b80`.

Next: refit the stocked CommsPod, progress normal Methanoid trading to the war/prototype event, then research and manufacture defence equipment promptly. The supplies are ready; drones, fleet fitting, combat/capture and later campaign acceptance remain unproved. Totals remain35/48 implementation evidence and four locally accepted requirements.

## Prepared-fleet campaign reaches war and defence research — 2026-10-04

The unmodified day12210 checkpoint continues on runtime `01c81f9`. Service exchanges the AMA for the stocked CommsPod and loads two250-unit copper pods. Sixteen ordinary accepted trades at Jupiter preserve both quantities. Saved milestones confirm counts3,8,12 and16 with war undeclared; UI reload at count16 preserves the exact ships and peaceful boundary.

The next encounter gives the war warning, replaces the CommsPod with a grapple holding the Fusion Laser prototype, and declares war at day12309 without incrementing the trade count. The next-day defence bulletin unlocks DFCC/drone research. Jupiter damages WAYFARER's engine during departure; the slowed ship nevertheless reaches Earth, docks with five crew and222 fuel, and retains both cargo pods. The first alien transmission appears during the return.

Normal grapple unloading consumes the prototype for research. Cavell's248 Professor researchers complete DFCC, Methanoid Fusion Laser and IOS Battle Drone research. The day12396 UI reload preserves exact ships, research, News and transmission state. Earth differs only in the deliberately changed ground/orbit screen selection. Orbital stocks match the initial checkpoint except500 copper loaded, one AMA returned and one CommsPod consumed; the500 copper remains aboard.

Native session76875 exits0; strict logs and `artifacts/fleet-war-campaign/audit.py` pass, and original user saves are restored byte-for-byte. Continuation `reloaded-slot-4.json` SHA-256: `a8d679bbe8b948caca786b71778b31f6e472b811f335bd7ff7a17436f0dad970`.

Next: manufacture a replacement IOS drive, DFCC and drones, then fit/refuel the fleet and defend Earth. The local enemy fleet already has11 drones against its40-drone attack trigger; Earth/Moon remain friendly. Morse retains147 Expert workers. This verifies normal Mac trading, escape, prototype research and reload; engine replacement, victory/capture, later progression and matching Windows export acceptance remain open. Backlog totals do not change.

## Paid fleet manufacture, failed defence and separate retreat replay — 2026-10-04

Normal play from day12396 manufactures one IOS drive, one DFCC and28 drones. An initially misselected grapple job is paused, resumed and completed; the audit includes its recipe exactly once. All jobs finish with an empty manual queue. Replacing the damaged engine consumes the new drive without returning damaged salvage. Service unloads the500 copper and exchanges the grapple for DFCC. Conversion returns222 MeH and clears the gauge; filling250 fleet units consumes2,500 MeH.

Five drones transfer first, then23 more. WAYFARER reaches the Moon with28 drones and fleet power196. The next natural transmission interrupts production; the saved partial job is completed rather than counting ignored clicks. At day12654, the enemy arrives with43 drones and power301. Fast time stops with the full five-update response window.

**The first defence fails:** all28 player drones are destroyed, the enemy retains35, and News records WAYFARER's destruction and Admiral Blunket's death once. This outcome is retained in `lost-fleet-slot-4.json`; no victory is claimed.

A **separate replay** loads the unchanged pre-battle save through test slot3 and selects Flee immediately. All28 drones and five crew escape and return to Earth with245 fuel. The day12657 reload preserves exact Earth, ships and News state. The Moon remains under attack with two updates left; retreat does not cancel that attack.

Evidence: `artifacts/fleet-defence-campaign/`, including both outcomes and runnable `audit.py`. Native session4058 exits0, strict logs pass and original saves are restored exactly. Retreat checkpoint SHA-256: `4991af5ce89585ac0327a1f097dcfaa63eec7cbf2a6c253dc0efa81590c46594`.

The next preparation branch should use the preserved peaceful16-trade checkpoint, research/install the already-available AOC before war, and start defence research immediately after declaration. The previous manual production schedule produced an understrength fleet; this is a preparation finding, not proof of a combat defect. Normal AOC installation/repeat production, successful defence/capture and Windows acceptance remain open. Counts remain35/48 implementation evidence and four locally accepted requirements.


## Prewar AOC installation and paid repeat production — 2026-10-04

A separate normal replay starts from the unchanged peaceful 16-trade checkpoint at day 12306 on runtime `01c81f9`. Cavell's team researches AOC, and Morse's orbital factory manufactures it. Installation consumes exactly four titanium, one aluminium, two carbon and one silver, creates no stock item, clears its queue and returns all 147 workers to quarters. Morse gains one completed-job action.

AOC repeat mode manufactures one IOS drive and starts a second paid drive. Selecting the active recipe again disables repeat while retaining the reservation. Saving/reloading mid-build then finishes the second drive and leaves the queue empty. The two drives cost exactly 60 iron, 100 titanium and 30 copper; reload neither charges again nor duplicates output. Workers remain in quarters throughout automation.

Final day 12342 remains peaceful, with two spare drives and AOC installed. UI reload preserves exact Earth, ships and News. `artifacts/aoc-prewar-campaign/audit.py` passes; native session 51315 exits zero with strict logs passing, and original saves are restored exactly. Continuation SHA-256: `3fbb36a77abb6d1afee8d6446002e4636e44982fcf171794e417d2edc249c12c`.

Next, use this checkpoint for the war encounter, prompt defence research and automated drone manufacture. Successful defence/capture and matching Windows export acceptance remain open; this result does not change accepted-task totals.


## Automated fleet manufacture and successful Moon defence — 2026-10-04

Normal progression from the unmodified day-12342 AOC checkpoint reaches war at day 12343. The first attempt omitted the engine command after launch and lost WAYFARER in hostile orbit while Research was open. That failed save is retained; source tracing confirms launch returns to orbit and requires a separate travel command. A separate replay loads the byte-identical start through test slot 3, then commands departure before advancing research.

The replay survives a naturally damaged-engine escape. Drone research completes first; AOC manufacture runs alongside DFCC and recovered Fusion Laser research. WAYFARER returns to Earth, unloads the prototype and 500 copper, consumes one previously manufactured replacement drive and fits the paid DFCC. Conversion returns 222 MeH; 250 fleet fuel units cost 2,500 MeH. Two naturally scheduled transmissions interrupt progression and are acknowledged normally.

AOC completes **58 drones and one DFCC** with exact recipe debits and an empty final queue. All 58 drones transfer to WAYFARER, which reaches Moon orbit at day 12658 with 247 fuel and five crew. At day 12714, the enemy arrives with 46 drones; fast time stops with five response updates remaining.

**Successful defence:** ordinary combat makes the enemy flee with 23 drones. WAYFARER retains 46 drones and all five crew; the Moon stays friendly. The attack clears and its threshold doubles from 40 to 80. Save/reload preserves exact ships, News and all planets, including a single Moon attack report and no player casualty report.

Evidence: `artifacts/aoc-fleet-campaign/`, including both attempts, before/after battle saves, screenshots and runnable `audit.py`. Native session 20844 exits zero, strict logs pass and original saves are restored exactly. Continuation SHA-256: `2b78ca5299163f2fd598f6ec33c0834a13b0315a4c317c4d7a655e0c6df25a99`. Runtime remains `01c81f9`.

This verifies normal defensive victory, not station capture. Hostile Sol stations now hold 92 defenders each, while the player has 46; capture preparation and original docking/combat gates need review before the next expedition. Windows matching-export acceptance remains open. Task totals do not change.


## Fleet reinforcement and pre-war stock preparation — 2026-10-04

Runtime `d0ace3d`, owned native session 89668, continued the unmodified day-12714 Moon-defence save. Ordinary shuttle freight and AOC production made **ten additional drones**, charging 150 carbon, 550 copper and 300 each palladium/platinum, alongside the freight-supplied base metals. Returning to mining configuration preserved all **46 surviving drones** in stores, giving 56 total; replacing DFCC with the existing AMA returned the converted tank value and charged the normal refill, an exact orbital fuel increase of 2,190. A normal **500-palladium delivery** and two naturally acknowledged transmissions reached saved stage 4/day 12935. This wartime fork is retained separately under `artifacts/station-capture-campaign/`; its mining-save SHA-256 is `16df1fe011fea61c8a0dcf37abe5ed08db2a06021cdb8e8b3a94d5bca1b232f6`.

A stronger capture attempt needs more material than that branch held. The separate, unchanged pre-war day 12342 checkpoint was therefore loaded and played normally to **day 14508**, without resource edits or cheats. WAYFARER returned the earlier 500 copper, replaced Comms with the existing AMA and ran repeated palladium/platinum expeditions. FIRST LIGHT supplied surface metals, then completed its final cycle. Actual orbital stock increases are 12,000 iron, 11,750 titanium, 11,750 aluminium, 1,500 carbon, 2,250 copper (including the original cargo), **3,000 palladium and 3,000 platinum**.

The resulting stocks can pay for **160 IOS drones and one DFCC**: iron 19,549, titanium 20,536, aluminium 20,694, carbon 3,186, copper 8,925, palladium 5,250, platinum 5,045 and gold 90. These are materials, not an already manufactured fleet. Both ships are docked at Earth with empty supply pods and ACC stopped. WAYFARER retains five crew, a healthy engine and 232 fuel; two spare IOS drives, Comms and an installed AOC remain available. War is still false and the trade count remains 16.

Exact ships, every planet and News survive reload at both day 13009 and the final checkpoint. The native process closed through its window with **exit 0**, strict logs pass, and original saves are restored byte-for-byte. Runnable audit and intermediate saves: `artifacts/prewar-reinforcement-campaign/`. Resume from `reloaded-slot-3.json`, SHA-256 **`054195c8ed899c7fe977faaa57c8792df289eff5e84154e2c926176e8cf6ba49`**. Next: restore Comms, trigger the normal war/prototype encounter, research and manufacture the fleet, then attempt capture/SDM and SCG progression. This is separate from the successful Moon-defence fork and does not establish station-capture or Windows acceptance.

## Funded fleet, two defences and first station capture — 2026-10-04

Normal play from the unchanged day 14508 checkpoint on `d0ace3d` restored Comms, triggered the Jupiter war/prototype encounter, survived a damaged-engine return, replaced one stored drive and delivered the Blazer. DFCC, fusion-laser and IOS-drone research completed normally. AOC manufactured **162 drones, one DFCC and one extra fusion laser**; the latter was an operator recipe-selection mistake, retained with its cost. Exact combined debits: 19,440 iron, 19,442 titanium, 19,441 aluminium, 2,431 carbon, 8,916 copper, 4,870 palladium, 4,872 platinum and one gold. The final queue is empty. DFCC conversion/refill leaves exactly 1,803 orbital MeH.

Two natural attacks interrupted production. At day 14902, **48 player drones versus 47 attackers** defended Moon; the player retained 34 and the enemy retreated 23. At day 15386, **97 versus 92** defended Earth; the player retained 67 and the enemy retreated 46. Both attacks cleared without advancing simulation time during combat. Both stations, all three human ships and their crews survived.

After manufacture, **118 surviving drones** reached Jupiter and destroyed its 200-drone garrison, leaving 24 aboard WAYFARER at day 15602. The first docking expired before the controls were reached. Immediate SDM inspection in a separate replay then exposed a real defect: mandatory discovery text locked input while the timer continued, destroying the station before defusal was possible. Both failure saves are retained.

Correction `929b4a5` suspends countdown consumption during locked bulletin playback; [original trace and reproduced regression](original-self-destruct-evidence.md#mandatory-discovery-playback--2026-10-04). The unchanged post-battle checkpoint was reloaded in the corrected build. Three timed operator retries missed the remaining window; the successful replay used the observed controls in one sequence to avoid tool round-trip delays. Switch 1 off/switch 2 on **captured Jupiter**, preserving WAYFARER, five crew, 24 drones and 232 fuel. The SDM is disarmed, ownership is friendly/type 8, and the captive colony correctly still needs repair. No resource or prerequisite edits were used.

Day 15603 save/reload preserves exact ships, every planet and News, including one capture entry. Runnable audit, before/after battles, expiry saves, screenshot and logs: `artifacts/funded-capture-campaign/`. Both native sessions 24347/6578 closed with exit 0, strict logs pass and original saves were restored exactly. Resume from `jupiter-reloaded-slot-4.json`, SHA-256 **`de673bd73d905d1dc871bed0c5625401ed7f788aa101da5150bf3715b5345528`**. This is normal station-capture evidence after a separately preserved defect/replay, not SCG progression or matching Windows acceptance. Counts remain 35/48 implementation evidence and four locally accepted requirements.

## Normal MTX discovery, supply and Jupiter repair — 2026-10-05

The unchanged captured-Jupiter checkpoint continued on runtime `929b4a5`, without resource or prerequisite edits. Inspecting the captured station's alternate Stores view and advancing time triggered the MTX discovery bulletin. Earth research completed normally. FIRST LIGHT delivered **250 copper** from surface stores, and orbital AOC built one MTX for exactly **500 titanium, 82 copper, 100 palladium and 40 gold**. The final queue was empty.

Normal MTX controls sent all 109 orbital iron to Jupiter and balanced titanium, aluminium, carbon, copper and MeH fuel. The six combined endpoint totals are conserved, with odd remainders retained at Earth. Transfer selections were then cleared. Jupiter's AOC produced one shuttle chassis, drive, tool pod and Bandaid; exact combined costs were **56 iron, 92 titanium, 70 aluminium, 40 carbon and 46 copper**.

WAYFARER returned to Jupiter and temporarily transferred Blunket's five-person crew to the newly assembled shuttle. The equipped shuttle landed, activated its Bandaid and completed the two-update repair at **day 15778**. The Bandaid was consumed, `BaseDamaged` and `CaptiveBase` cleared, and the crossed-out ground controls returned to their normal graphics. Subsequent mining added six hydrogen and six helium directly to orbital stocks.

The shuttle returned to orbit, refuelled to 10 and handed the same crew back to WAYFARER. At **day 15785**, four human ships survive; WAYFARER retains 24 drones, five crew, a healthy engine and 204 fuel. The new uncrewed shuttle is docked at Jupiter with an empty tool pod. Exact ships, every planet, News and unlocks survive save/reload.

Evidence and runnable conservation/reload audit: `artifacts/jupiter-repair-campaign/`, including intermediate saves, screenshots and `native.log`. Native session 51423 closed through its window with **exit 0** and a passing strict log. Original save inventory was restored exactly. Continue from `reloaded-slot-3.json`, SHA-256 **`0dbe072a65d33d6d5f3949cf44ff7d8b671dfe254c2016fa2625acfbc1ac4e87`**. This adds normal campaign evidence for MTX, paid colony repair and crew transfer; it does not establish SCG discovery, Windows acceptance or resolution of the earlier shutdown failures. Counts remain 35/48 implementation evidence and four locally accepted requirements.

## Resupply failure and separate pre-war funding — 2026-10-07

Resumed clean source `a6c1d01` (unchanged runtime `929b4a5`) from the hash-verified repaired-Jupiter day-15785 checkpoint. A fresh build with the pinned .NET 6.0.428 SDK passes with zero warnings/errors; all 19 tooling checks pass with the original ending disk supplied. No new full game-regression aggregate or Windows acceptance is claimed.

The first branch returns WAYFARER safely to Earth at day 15806 with 24 drones, five crew, a healthy engine and 190 fuel. Normal DFCC-to-AMA replacement returns the controller, all 24 drones and 1,900 store-fuel units; automated mining then starts while FIRST LIGHT freights the five base metals. An accidentally selected DFCC production queue was unpaid and removed before advancing time; IOS drones were selected afterward. The saved day-15906 state already has Earth under attack by 174 drones, five updates remaining and time advancement stopped. The operator resumed time without checking News, then repeated that mistake at Jupiter's attack pause. Earth and Jupiter were captured; FIRST LIGHT, the Jupiter shuttle and WAYFARER were lost. The retained day-15980 state has the 192-drone enemy fleet attacking the Moon. This is a failed strategy/operator sequence, not successful reinforcement or proof of a new game defect. Future unexpected stops require checking News before advancing time.

Original Disk 1 independently confirms the five-update attack countdown: `$38D54` writes `$8005`; `$38E18–$38E4A` decrements its low byte and branches to capture at zero. `$38DFE` separately sets the seven-update feedback countdown at `$1C397`; `$36446` sets suppression and a location bitmap, not a direct screen transition. This bounded source check does not establish complete attack presentation or original-runtime timing. The byte assertions and aligned extracts are retained with the campaign audit.

A **separate pre-war continuation** loaded the unchanged day-14508 funding checkpoint, without resource/prerequisite edits. AMA/ACC mining delivered **1,000 additional palladium** to Earth orbit. The day-14960 checkpoint holds 6,250 palladium and 5,045 platinum; every other orbital stock except fuel is unchanged. WAYFARER returned docked, healthy, with five crew, 231 fuel and empty supply pods; its ACC is stopped. FIRST LIGHT and the Moon shuttle remain intact, war remains false and News is unchanged. Exact ships, world data, News, unlocks and story state survive normal UI load and resave. This increases available supplies but does not yet fund the larger fleet needed for another war/capture attempt.

Evidence: `artifacts/sol-reinforcement-campaign-20261007/`, including both branches, `audit.py`, `original-attack-countdown.txt`, tooling results and strict native log. Owned native session 18586 exits **0**; the strict log and runnable audit pass, and original saves are restored byte-for-byte. A subsequent CUA state read reopened a separate bare Godot project manager without the pinned SDK environment; that manager was closed separately and is not a game-runtime failure. The older pre-existing Godot 4.2.1 manager and unrelated `weppy-project-sync/` directory were left untouched. Resume from `prewar-reloaded-slot-1.json`, SHA-256 **`ac7727abf5d1edc007057f1e0a67b897d5f710f1b633dd6890f86eda80f10226`**. Totals remain **35/48 implementation evidence; four locally accepted requirements**. Earlier Mac/Windows shutdown failures remain unresolved.


## Full-fleet funding and concurrent freight — 2026-10-07

On source `154545f` (unchanged runtime `929b4a5`), the unedited day-14960 pre-war checkpoint continues through ordinary ACC mining and shuttle freight. An initial mistaken bay click was discarded by reloading that untouched checkpoint before route advancement. No resources, prerequisites or scan outcomes were edited. Platinum-only mining delivered two 500-unit loads; both rare-metal selections were restored during the second expedition. FIRST LIGHT hauled the five base metals and refined fuel, then a final iron-only run. Complete Cycle was selected after the miner had already begun another scan, so normal engine engagement returned it empty to Earth.

Final **day 15901** orbital gains are **5,000 iron; 4,250 titanium; 4,000 aluminium; 4,000 carbon; 4,250 copper; 1,000 platinum; and 1,822 net MeH**. All other orbital stocks are unchanged. Current stocks pay for **200 IOS drones plus one DFCC**; they do not represent a built fleet or substantial replacement reserves. The 200-drone per-fleet loading limit means additional production must remain in stores or use another hull. War remains false and News is unchanged. WAYFARER is safely docked with five crew, a healthy engine and 232 fuel. FIRST LIGHT is docked with 55 fuel and 37 crew, down from 38 through ordinary attrition. Both have empty supply cargo and stopped ACCs; the shuttle's retained freight selection is iron only.

Normal UI load/resave preserves exact ships, planets, News, unlocks, day, war and story state. Owned native session 30000 closes through its window with **exit 0** and a passing strict log. Original saves are restored byte-for-byte. Evidence and runnable stock/funding/reload audit: `artifacts/fleet-reserve-campaign-20261007/`. Resume save `reloaded-slot-1.json` SHA-256 **`faee387508e4cd2350b485463af98da01f25758564f98b84138d9b82564f52e2`**. Replacement reserves, later normal campaign progression, original-runtime comparisons and current Windows acceptance remain open. Counts stay **35/48 implementation evidence and four locally accepted requirements**; no new full regression aggregate is claimed.

A read-only Windows Downloads check succeeds using the existing Keychain-backed SSH identity after a default client authentication failure. Its 20 newest files include no newer handoff than the already delivered `929b4a5` package. This is a handoff-folder observation, not proof about the desktop agent's current work; no Windows process or profile was changed.


## Second IOS and first mining delivery — 2026-10-07

On source `20292fd` (runtime matching the verified upstream integration), the unchanged day-15901 checkpoint continues through normal native controls. The orbital AOC manufactures one IOS chassis, two supply pods, one cryo pod, an ACC and an AMA. The chassis receives a spare drive and existing tool pod. A mistaken cryo-pod selection during fitting is corrected normally; its returned pod remains in stores. No save fields, resources, prerequisites or scan outcomes are edited.

A 41-person marine class trains on Earth; Floyd's team has 40 members when FIRST LIGHT transports it in a cryo pod to orbit. The new ship, renamed **PROSPECTOR**, receives the team, AMA, two supply pods and ACC. Engaging its Earth/asteroid route automatically purchases a full 250-unit MeH tank. Its first trip returns **500 platinum**; WAYFARER delivers **1,000 palladium** over two trips. Both use Complete Cycle to finish. Earth's single IOS berth requires moving the stopped WAYFARER into orbit before PROSPECTOR can dock. The blocked return continues consuming fuel; subsequent logistics must keep the berth available.

The final **day-16570** save remains pre-war with unchanged News and unlocks. Orbital stock deltas exactly match those deliveries and recipes: iron −106, titanium −328, aluminium −189, carbon −81, copper −81, silver −1, platinum +495, palladium +1,000, MeH −250, spare drive −1, spare tool pod −1 and spare cryo pod +1. All other orbital stock counts are unchanged. The existing 200-drone/DFCC funding remains covered, but substantial replacement reserves and manufacture are still pending.

PROSPECTOR is docked at Earth with 39 crew and 144 fuel; WAYFARER is undocked in Earth orbit with five crew and 187 fuel. Both have healthy engines, AMA, empty supply pods and stopped ACCs. FIRST LIGHT is docked in orbit with 37 crew, 46 fuel and an empty cryo pod. Its old supply pod is in ground stores, so re-fit a supply pod before resuming freight.

Native load/resave preserves exact ships, planets, News, unlocks, day, war and story state. Owned session 99542 closes through its window with **exit 0**, the strict log passes, original saves are restored byte-for-byte and no settings file is introduced. Evidence and runnable recipe/stock/reload audit: `artifacts/fleet-expansion-campaign-20261007/audit.py`. Resume `reloaded-slot-4.json` SHA-256 **`bf7ddf576bba8821c03649bc967b10b23d67280c6fcb363bcec695ed0e4e4472`**. This is normal Mac gameplay evidence, not a new full-suite run or Windows acceptance. Counts remain **35/48 implementation evidence and four locally accepted requirements**. Earlier checkpoints and failed branches remain intact.

## Replacement reserves and continuous routes — 2026-10-07

The integrated runtime (`365b0a6`, main checkout `5fd3c8a`) continues the unchanged day-16570 checkpoint through normal freight and two concurrent mining routes. AOC manufactures one Supply Pod for two titanium, one aluminium and one copper; FIRST LIGHT's empty cryo pod returns to orbital stock. The shuttle then delivers base metals, copper and fuel. WAYFARER and PROSPECTOR deliver **5,000 palladium and 6,500 platinum**. Normal mining promotes Floyd to Captain and then Admiral, with both News entries retained. No resources, crew, scan results or prerequisites were edited.

At simulation day **19868**, Earth orbit holds **48,693 iron, 48,456 titanium, 48,504 aluminium, 7,105 carbon, 22,343 copper, 12,250 palladium, 13,040 platinum, 90 gold and 9,489 MeH**. This funds **400 drones plus one DFCC** at current recipes, before further trade/hardware costs; none of those drones was manufactured in this run. Gross freight delivers 24,250 iron, 24,000 titanium, 24,000 aluminium and 9,250 copper, with the supply-pod recipe accounting for the exact net differences. MeH rises by a net 3,867 after normal refuelling and consumption. Unlocks and peace state remain unchanged.

Both mining ACCs are stopped with empty supply pods and healthy drives: WAYFARER is docked at Earth with Blunket's four crew and 231 fuel; PROSPECTOR is safely undocked in Earth orbit with Admiral Floyd's 38 crew and 228 fuel. FIRST LIGHT finishes in Earth's ground bay with Kingston's 35 crew, an empty supply pod and 70 fuel; its stopped ACC retains iron/titanium/aluminium freight. The Moon shuttle remains docked at the Moon with 35 crew and four fuel. Replenish WAYFARER's crew before relying on it for further long routes. Earth's single IOS berth remains a logistics constraint: PROSPECTOR was undocked after unloading so WAYFARER could return. Engage on an already undocked ship and Complete Cycle while scanning asteroids both required manual engine engagement to start the intended flight; these are operational observations, not new fixes.

The first reload comparison crossed a natural 0.01-day clock boundary and is retained separately. A subsequent checkpoint/reload preserves the displayed date, simulation day, ships, world state, News and unlocks exactly; only elapsed clock/SDM timers advance during UI interaction. The owned game closes with **exit 0** and a strict clean log. Temporary saves are retained, and the original save directory is restored byte-for-byte with no settings file introduced. The audit initially assumed no new News and only four saved ship records; its retained corrections account for Floyd's promotions and nine existing enemy fleets. Neither was a gameplay failure.

Evidence: `artifacts/replacement-reserves-campaign-20261007/`; run `python3 artifacts/replacement-reserves-campaign-20261007/audit.py`. Resume from `reloaded-slot-4.json`, SHA-256 **`5523a3741bf0e961b613878c6f9025f82da1db1713647e5f2ad94f0479d49107`**. `funded-slot-3.json` SHA-256: `335266809b3579674a671abdef11501a8ca7be9983da8500aa1a801d37dfafd7`. Start/intermediate saves, natural-tick comparison, audit revisions, exit result and file hashes remain together. The separate [635-case complete run](upstream-integration-results.md#complete-final-source-run--2026-10-07) passes on identical runtime/tests. Windows execution, actual replacement-fleet manufacture, later normal campaign and prior shutdown failures remain open; task counts are unchanged.

## Replacement fleet war and Moon defence — 2026-10-07

The integrated runtime at `365b0a6`, unchanged in contribution checkpoint `0066ce0`, continues the unedited day-19868 reserve save through normal war, prototype research, paid manufacture and defence. No cheats, resource edits or staged prerequisites were used.

PROSPECTOR exchanged its AMA for the stocked CommsPod and visited Jupiter. The retained campaign already had 16 trades; the next contact delivered the normal war/prototype event at day 19991. A fast-forward departure attempt lost PROSPECTOR before its engine was engaged; that save remains separate. Replaying the unchanged war-boundary save with one date-button update followed by engine engagement returned the prototype to Earth at day 20005, with a healthy drive, 199 fuel and all 38 crew.

Normal prototype analysis unlocked the fusion laser, DFCC and IOS drone research sequence. The first production attempt reached the Moon attack with only 35 drones against 43 and lost PROSPECTOR. That outcome is retained. A separate replay from the unchanged returned-prototype save prioritised drone production while researching the DFCC and stopped time during menu/outfitting work. It reached the next Moon battle with **46 drones against 43**, repelled the attack and retained **34**, while the enemy fled with 21. No combat rules were changed to obtain this result.

PROSPECTOR returned to Earth and received replacements. The final day-20583 checkpoint has **89 drones manufactured: 12 lost and 77 aboard**, plus one paid drone in progress. Exact stock deductions reconcile 90 drone recipes and one DFCC; remaining stores fund another 310 drone recipes beyond the paid job. All four named ships survive, Earth/Moon remain friendly, and PROSPECTOR is orbiting Earth with 45 fuel, a healthy drive, 38 crew and inactive ACC. WAYFARER is also undocked at Earth with four crew and 230 fuel; replenish its crew before a long route. The Earth berth is free. FIRST LIGHT remains grounded and stopped. The Sol enemy fleet has 52 drones, attack threshold 80 and no current attack. The orbital factory retains its active repeat drone order.

Save/reload preserves all world, ship, research, production, News and transmission fields. Only normal elapsed time, the SDM timer remainder and the explicitly reset fast-forward timestamp differ; date `2051766` and simulation day remain exact. The native process exits **0**, the strict runtime log passes, and the original user save is restored byte-for-byte. The unrelated Godot 4.2.1 process remains open.

Evidence: `artifacts/replacement-fleet-war-campaign-20261007/`, including both failed gameplay saves, the two attack checkpoints, successful defence, replenished/reloaded saves, `native.log`, `native-exit.json`, `result.json` and runnable `audit.py`/`audit.log`. The first audit incorrectly required the IOS engine flag to clear on arrival; its failure is retained, and the check now uses the actual orbit/transit state. No runtime or saved state was altered for that correction.

Resume from **`reloaded-slot-5.json`**, SHA-256 **`a5a2d2a87a925bb93e34c24abbe65c306b0161a9f67aa8a9e4f98a879ba526ec`**. Continue replacements, defend subsequent attacks and build the capture/reserve fleet. This extends normal gameplay evidence for the integrated runtime; it does not complete Sol/SCG progression, Windows acceptance or the retained shutdown investigations. Counts remain **35/48 implementation evidence and four locally accepted requirements**.

## Crew reinforcement and Earth defence — 2026-10-07

The unchanged integrated runtime continues the day-20583 checkpoint through normal training and transport of 41 marines. Thackray's team replaces WAYFARER's three remaining crew; Blunket's team returns to orbital quarters. FIRST LIGHT carries an empty cryo pod and is docked at Earth with 55 fuel. PROSPECTOR refuels its DFCC and receives replacements, reaching 110 drones and 99 fuel. At day **20809**, it defeats the attacking 82-drone fleet with **80 survivors**; the enemy retreats with 41 and its next attack threshold rises to 160. All four named ships survive and Earth/Moon remain friendly. Exact stock deductions reconcile 37 additional drone recipes, 560 store-fuel units and the existing cryo/supply pod exchange. The orbital pool holds four drones.

A later manufacturing continuation leaves fast-forward active too long. The final day-21385 save has both colonies hostile and no surviving named ships. It is retained as a failed operator attempt, not a successful capture checkpoint or an established game defect. The transfer overlay's displayed inventory was stale while production continued. Resume from the earlier **`earth-defended-slot-4.json`**, SHA-256 **`112ac404a707f8f965e57040174c8deb0fc8fa1495f6ad6fdda5fcd341b6438b`**, and monitor attack stops before advancing time. This run does not add a reload comparison.

Evidence: `artifacts/replacement-capture-campaign-20261007/`, including the reinforced and attack saves, successful defence, `failed-unattended-slot-5.json`, exact resource `audit.py`, native log, exit result and original-save hashes. The audit passes; owned session 91532 closes with exit **0**, the strict log passes, and original saves are restored byte-for-byte. The unrelated pre-existing Godot process remains untouched. A read-only Windows refresh fails SSH authentication; no newer Windows result is claimed. Counts remain **35/48 implementation evidence and four locally accepted requirements**.


## Live fleet inventory and sustained defence — 2026-10-07

The day-20809 Earth-defence save continues unedited on the fleet-display follow-up to `576e45d`. Six manual date updates produce a drone while the transfer panel remains open: its pool changes from four to five without a transfer click. Continued production and loading reconcile 75 paid drone recipes at day 21255, with 154 aboard and five in orbit. This physically verifies the [live-count fix](validation-results.md#live-fleet-transfer-inventory--2026-10-07), independently of the automated case.

The next Moon attack pauses time at day 21313. The first defence departs with 166 drones against 167 and wins with only 32 remaining. Two replacements had remained in Earth orbit; that result is retained. A separate replay loads those two through the normal controls before departure, fights with 168, and retains 117. Both saves show the enemy retreating with 83, its attack threshold capped at 200, and all four named ships and both human colonies surviving. Battle outcomes include randomness; the two extra drones do not establish a guaranteed casualty difference.

After returning to Earth, continued paid production supplies a **full 200-drone fleet**, with two in stores at day 21821. Another Moon attack is already active with four updates left. An attempted navigation batch remains on News and consumes two date updates; the full-fleet checkpoint is reloaded before departure. Right-click background navigation succeeds, and observed ship selection/engine engagement precede two travel updates. The 200-versus-200 defence wins with **43 survivors**; the enemy retreats with 100. The ineffective Master Control click from News remains an observation requiring a focused reproduction, not a diagnosed defect or part of the fleet-label fix.

Final day **21824**, displayed date `2175569`, retains friendly Earth/Moon and all four named ships. PROSPECTOR is undocked at the Moon with 43 drones, 93 fuel, Admiral Floyd's 37 crew and inactive ACC. WAYFARER remains at Earth with Thackray's 41 crew and 228 fuel; FIRST LIGHT is docked there with its empty cryo pod and 55 fuel. Earth has two drones in stock and a paid repeat order. From the starting 84 drones aboard/in stores, **169 additional drones are manufactured, 51 are lost in the selected first defence and 157 in the final defence: 45 remain**. All seven material debits reconcile exactly; remaining materials fund 107 further recipes beyond the paid job. This establishes full-fleet manufacture and defence, not a sustainable station-capture reserve. One factory's replacement rate and these casualty outcomes warrant assessing a second orbital production site before repeating the same strategy.

Native load/resave preserves every game field except normal elapsed time, the SDM timer remainder and the reset fast-forward timestamp. Owned session 91801 exits **0** with a strict clean log, and original saves are restored byte-for-byte. Evidence: `artifacts/fleet-refresh-native-20261007/`, including both defence variants, full-fleet boundary, final/reloaded saves, `audit.py`, `audit.log` and exit record. Resume `reloaded-slot-5.json`, SHA-256 **`5281725de798f82e2b4e826e663fa46821136504806453e7d69dcad46764f8b9`**; preserve the earlier pre-war day-19868 reserve save for a separately labelled production-expansion route. The full 635-case follow-up passes; Windows, later campaign progression and prior shutdown investigations remain open. Counts stay **35/48 implementation evidence; four locally accepted requirements**.

## Moon AOC and parallel drone production — 2026-10-07

Runtime **`77986bc`** continues the unchanged day-21824 defence checkpoint, without replaying the pre-war branch. All construction, freight, production and transfers use ordinary native controls. No resources, crews, research prerequisites, scans or random outcomes are edited.

WAYFARER docks at Earth and delivers 247 silver and 250 aluminium to the Moon. Redman's existing 188-person Expert team completes the Moon AOC in six manual updates, reaching day **21836**. The station deducts exactly four titanium, one aluminium, two carbon and one silver, installs the AOC locally and returns the intact team to a free staff slot. The two previously paid Tool Pod/Cryo Pod jobs retain their materials and progress through conversion and reload.

Two further ordinary round trips deliver 250 each of titanium, copper, palladium and platinum. With both factories supplied, six updates from day **21853 to 21859** increase Earth's drone pool **7 → 8** and the Moon's **0 → 1**. The Moon deducts exactly one seven-mineral drone recipe. Its drone transfers to PROSPECTOR, changing the local pool **1 → 0** and fleet **43 → 44**, with power **301 → 308**. This verifies one parallel production cycle; sustained two-factory throughput is not yet established.

The complete ledger from day 21824 accounts for **six Earth drones plus one Moon drone**, the AOC recipe and every transferred mineral. WAYFARER consumes exactly **21 fuel** across five two-update flights, six dockings and five undockings, ending at the Moon with 207 fuel, 40 crew and Thackray newly rated Captain. PROSPECTOR remains there with 93 fuel and Floyd's 37 crew. FIRST LIGHT and the Moon shuttle survive unchanged. Both colonies remain friendly; the enemy has 108 drones, threshold 200 and no active attack.

Resume **`artifacts/moon-aoc-campaign-20261007/reloaded-slot-3.json`**, SHA-256 **`76de745c8cd7f0499795dc677a596c944e343496927c740915cce2dfcbb28433`**, simulation day **21859**, date `3121 788.71`, time advancement off. Earth has eight reserve drones and a paid repeat job; the Moon AOC is idle with 60 iron, 242 titanium, 211 aluminium, 191 carbon, 222 copper and 220 each palladium/platinum. Iron is the immediate shortage. WAYFARER is docked with empty supply pods and an Earth return course; both IOS ACCs remain off. Establish a funded freight cycle and further production before another large offensive. This checkpoint supersedes day 21824 for continuation; all earlier defence, failed capture and pre-war branches remain retained.

Save/reload preserves all game data apart from elapsed-time bookkeeping. The owned game closes with exit 0 and a strict log; original user saves are restored byte-for-byte, with no settings file introduced or pre-existing process closed. Evidence: `artifacts/moon-aoc-campaign-20261007/`, including staged checkpoints, `audit.py`, native log and hash manifest. No runtime changes or new Windows/full-suite result are claimed. Wider station capture, SCG and ending acceptance remain open; task counts are unchanged.

## Moon paid-order resumption and automated freight — 2026-10-07

Source **`60d09c6`**, with unchanged runtime `77986bc`, continues the exact day-21859 checkpoint. WAYFARER's ACC is cleared through its native controls, then configured for Earth → Moon iron/titanium/aluminium only, with no return selections. Before engaging it, the Moon AOC resumes its two preserved paid orders: twelve ordinary updates produce exactly one Tool Pod and one Cryo Pod, emptying the queue with **no additional mineral charge**. This completes normal campaign verification of those reservations through AOC conversion, unrelated drone production and eventual resumption.

With Moon drone Repeat selected, the ACC runs **three eight-update freight circuits**, delivering **500 each of iron, titanium and aluminium**. After the third outbound load, “finish cycle” completes the trip, unloads both pods and stops docked at the Moon without starting another leg. It retains the configured route with Active/CycleMode off and empty cargo. Automatic refuelling transfers exactly 51 MeH from Moon stores; six flights, six dockings and six undockings consume 24 fuel, so WAYFARER finishes at **234 fuel** and Moon stock at **13**. Earth fuel stock is unchanged.

The entire continuation manufactures **eight Earth drones and four Moon drones**, with exact seven-mineral payments and conserved freight. The Moon then waits on an unpaid repeat order because iron/copper are insufficient; it does not deduct another recipe. Its four drones transfer to PROSPECTOR (**44 → 48**, power **308 → 336**); Earth holds **16** in reserve. All four ships and both colonies survive. The enemy reaches **120 drones**, threshold 200, with no active attack. Player and enemy drone totals each grew by 12 during this continuation: this verifies functioning freight, not a production advantage sufficient for the remaining campaign.

Resume `artifacts/moon-freight-campaign-20261007/reloaded-slot-4.json`, SHA-256 **`d1767892dc9c133572049914a4ed16ac96190bcd92f35d457665154ced4965cc`**, simulation day **21907**, date `3121 834.73`, time off. Both IOS ships remain at the Moon: WAYFARER docked, PROSPECTOR undocked with 93 fuel/37 crew. Moon stores hold 80 iron, 262 titanium, 231 aluminium, 131 carbon, two copper, 100 each palladium/platinum and 13 MeH. Replenish the full recipe mix and fuel, with iron/copper the immediate shortages, before extending production. The iron/titanium/aluminium ACC selection remains ready but is insufficient by itself.

Reload preserves all game data except elapsed-time bookkeeping. The native game exits 0 with a strict log; original user saves are restored exactly and the existing Godot process is untouched. Evidence, intermediate saves, runnable conservation/reload audit and hash manifest: `artifacts/moon-freight-campaign-20261007/`. No runtime edits, new full-suite result or Windows acceptance are claimed. Counts remain **35/48 implementation evidence and four locally accepted requirements**.

## Larger freight capacity and renewed Jupiter capture — 2026-10-07

Source **`c824d9e`**, unchanged runtime `77986bc`, continues the exact day-21907 save. At Earth, WAYFARER returns its AMA and Tool Pod to stores and fits the existing spare Supply Pod. All three cargo slots now hold supplies, raising capacity from 500 to **750** without manufacturing or losing equipment. Ordinary ACC selection changes deliver **22 full cargoes** to the Moon: 4,500 iron, 3,750 each titanium/aluminium, 500 carbon, 1,750 copper, 1,000 each palladium/platinum and 250 MeH. The route finishes docked with empty pods and ACC off. Refuelling withdraws 104 MeH at Earth and 52 at the Moon; 44 flight legs plus dockings/launches consume 176 fuel, leaving **214**. Thackray advances from Captain to Admiral through those flights; all 40 crew survive.

The retained day-21965 → 22013 interval produces **eight drones at each factory in 48 updates**, compared with twelve enemy drones. The full continuation completes **37 Earth and 29 Moon drones**, with exact seven-mineral accounting including the Moon's final paid work in progress. This establishes a sustained interval with buffered secondary minerals, not an indefinitely sufficient mineral mix. PROSPECTOR loads 26 Moon drones and 51 Earth reserves, reaching **125 drones / 875 power**, then flies to Jupiter with 79 fuel and all 37 Floyd crew.

The **first battle**, retained at day **22127**, defeats the 200-drone / 800-power Jupiter garrison but costs **116 drones**, leaving **nine**. No combat replay or RNG alteration was used. The first docking/SDM attempt expires during navigation back from mandatory discovery playback, destroying PROSPECTOR and the station; `sdm-expired-slot-5.json` preserves that failure. Reloading the unchanged **post-battle** save repeats only docking and defusal. Watching the bulletin finish, then promptly opening the observed SDM controls and setting switch 1 off / switch 2 on captures Jupiter. The nine-drone combat result is unchanged. This is a successful defusal retry after an operator timing failure, not an uninterrupted capture or a new runtime correction.

Resume **`artifacts/moon-supply-capacity-20261007/reloaded-slot-2.json`**, SHA-256 **`aa91aa3725c346c10dc714a6ab5807be6c5d0a5d0d079194fee1b7bab472c5c5`**, simulation day **22128**, date `3122 052.76`, time off. PROSPECTOR is docked at Jupiter with nine drones, 78 fuel, 37 crew and a healthy drive. Earth has two reserve drones, the Moon three; all four ships and all three friendly stations survive. Jupiter has its AOC/MTX and a disarmed SDM, but its captive ground colony still needs repair. Its only retained minerals are 918 hydrogen and 668 helium. The enemy fleet has **174 drones**, threshold 200, no active attack and no countdown yet. The player has only **14 total drones** after combat; this is a vulnerable recovery checkpoint.

Prioritize normal MTX discovery/research and supply recovery before further offensives. Earth currently holds 2,053 iron, 2,314 titanium, 2,363 aluminium, 1,399 carbon, 1,257 copper, 590 palladium, 1,378 platinum, 89 gold and 8,274 MeH. Reserve the MTX recipe (500 titanium, 82 copper, 100 palladium, 40 gold) before drone production consumes it. The Moon holds 980 iron, 412 titanium, 381 aluminium, 181 carbon, 102 copper, 200 each palladium/platinum and 211 MeH; both factories have a paid drone in progress. WAYFARER remains at the Moon with three empty Supply Pods and a configured but stopped Earth → Moon base-metal route. FIRST LIGHT still has an empty Cryo Pod; Earth ground has a spare Supply Pod and 50,000 of each base metal. Restore surface freight and investigate the defence shortfall rather than assuming the larger freighter makes another attack affordable. Check News immediately on unexpected time stops.

Capture/reload preserves all game data except the three elapsed-time fields. The owned native session exits **0**, strict log and runnable cargo/recipe/fuel/loss/capture/reload audits pass, and original user saves are restored byte-for-byte with no settings file introduced. All earlier checkpoints, the failed SDM attempt and the pre-war reserve remain retained. Evidence: `artifacts/moon-supply-capacity-20261007/`. Runtime is unchanged; no new full-suite or Windows acceptance is claimed. Counts remain **35/48 implementation evidence and four locally accepted requirements**; colony repair, SCG, interstellar progression and ending acceptance remain open.

## MTX research, surface freight and Jupiter supplies — 2026-10-07

Source `2c9b000`, unchanged runtime `77986bc`, continues the unedited day-22128 capture checkpoint. Normal discovery and Cavell's research unlock MTX; the Earth AOC installs it for exactly 500 titanium, 82 copper, 100 palladium and 40 gold. Its earlier paid drone finishes once before construction, leaving three reserves and an empty queue. Moon production completes its paid drone and one additional recipe, reaching five reserves before copper limits further work.

FIRST LIGHT returns to Earth ground, exchanges its empty Cryo Pod for the existing Supply Pod and delivers 250 each iron, titanium and aluminium through ACC. Finish Cycle stops it docked in Earth orbit with empty cargo. The post-refit cycle consumes 24 fuel and refills 54 from orbital stores, leaving 82. WAYFARER remains docked at the Moon with three empty supplies and 214 fuel; PROSPECTOR remains at Jupiter with nine drones and 78 fuel.

Eight selected MTX transfers balance iron, titanium, aluminium, carbon, copper, palladium, platinum and MeH between Earth and Jupiter, retaining odd remainders at Earth. Jupiter receives respectively **1,151 / 1,032 / 1,306 / 699 / 587 / 245 / 689 / 4,110**. Its original 918 hydrogen and 668 helium remain. Selections are cleared after transfer; no continuing freight is active. Jupiter's ground colony remains captive/damaged. The enemy has 184 drones, threshold 200, with no active attack; the player has only 17 across fleet and stores. Colony repair and defence funding remain next, before further offensives.

Resume `artifacts/jupiter-mtx-recovery-20261007/reloaded-slot-3.json`, SHA-256 **`60c6741b87076b38117991a8e779ca3094cb0c294ac339f2031e86431b1ea264`**, simulation day **22171**, displayed date `3122 091.80`, time off. Save/reload preserves every game field except the three elapsed-time fields. The runnable artifact audit passes exact construction/freight/fuel/balance checks, strict native logging and byte-for-byte original-save restoration. Owned session 93933 exits 0; no settings file is introduced. Intermediate saves and hashes remain local under the same artifact directory.

The Production screen appeared to retain an earlier date while manufacturing progressed; navigating to Save refreshed it. This is an observation awaiting focused reproduction, not a diagnosed or corrected runtime defect. No new full-suite or Windows result is claimed. Counts remain **35/48 implementation evidence and four locally accepted requirements**; SCG, interstellar progression and ending acceptance remain open.

## Renewed Jupiter repair and crew return — 2026-10-07

Source `d354bf7`, unchanged runtime `77986bc`, continues the exact day-22171 MTX checkpoint. Jupiter's AOC completes one shuttle chassis, drive, Tool Pod and Bandaid in 24 manual updates. The five-mineral debit is exactly **56 iron, 92 titanium, 70 aluminium, 40 carbon and 46 copper**; the queue ends empty. Normal assembly consumes those components, and 30 MeH moves from station stores into the new Jupiter Shuttle. Floyd's 37-person crew temporarily leaves PROSPECTOR to fly it.

After undocking and landing, the first Tool mount activates the Bandaid. Two further updates consume it and clear both `BaseDamaged` and `CaptiveBase` at day **22200**. The restored ground services are visible. The shuttle returns to orbit and docks; all 37 crew return to PROSPECTOR, with its nine drones and 78 fuel unchanged. The shuttle retains its empty Tool Pod, has no pilot and finishes with **21 fuel**: exactly nine spent across undocking, descent, ascent and docking. Five post-survey mining updates from seven derricks add **35 each hydrogen and helium** to Jupiter's orbital stores.

Resume `artifacts/jupiter-repair-campaign-20261007/reloaded-slot-5.json`, SHA-256 **`1dcc13cfde194c4ddfa42c307108ab88727c817953c9797088437a576049cf23`**, simulation day **22206**, displayed date `3122 126.80`, time off. All five named ships and three friendly stations survive. Earth and Moon stocks are unchanged from the MTX checkpoint. Jupiter holds 1,095 iron, 940 titanium, 1,236 aluminium, 659 carbon, 541 copper, 245 palladium, 689 platinum, 953 hydrogen, 703 helium and 4,080 MeH. No production or freight is active, apart from the Moon's unfunded repeat selection. The enemy has **192 drones**, threshold 200, with no active attack. Player reserves remain only 17. The repair succeeds, but this does not establish a viable defence or further capture reserve.

Exact recipe/fuel/crew/mining/reload checks pass in the artifact `audit.py`. Owned session 26582 exits 0 with a strict log; original saves are restored byte-for-byte and no settings file is introduced. The first isolated date observation in the same process loaded day 22171, advanced seven times on Production and displayed `3122 098.80`, matching the saved clock. That diagnostic branch is retained separately under `artifacts/production-date-observation-20261007/`; campaign repair reloads the unchanged source afterward. The earlier stale-date observation did not reproduce in that fresh session, so no runtime fix is claimed.

Before extending the campaign, establish a defensible production/reserve strategy. The retained pre-war day-19868 branch is confirmed `AtWar = false`, with 16 trades and resources for roughly 400 drone recipes, but drones/DFCC remain locked until war. Preparing additional factories and distributing their full mineral mix before the war trigger is a concrete alternative to the current vulnerable branch. Preserve current capture/repair evidence and combat losses; do not alter resources, prerequisites or RNG. No new full-suite or Windows result is claimed; counts remain **35/48 implementation evidence and four locally accepted requirements**.

## Separate pre-war three-factory expansion — 2026-10-07

Source `3b6a53f`, unchanged runtime `77986bc`, uses the unchanged **day-19868 pre-war reserve** as a separately labelled strategy branch. The repaired-Jupiter day-22206 checkpoint and all earlier battle/SDM losses remain retained. No resources, crews, prerequisites or RNG are edited. War stays false, trade count stays 16 and the war-declaration day stays zero throughout this continuation.

WAYFARER delivers 247 silver and 250 titanium to the Moon. Redman's 191-person Expert team builds the Moon AOC for four titanium, one aluminium, two carbon and one silver, then returns intact to quarters. The Moon's earlier paid Cryo Pod completes without another material charge. WAYFARER returns its AMA and Tool Pod locally, fits that Cryo Pod and carries Redman, the remaining 246 silver and 206 carbon to Mars.

Earth manufactures exactly **seven OF Frames** to supplement its existing one. PROSPECTOR temporarily carries three Tool Pods and deploys all eight sections in three trips, completing the third friendly orbital station at Mars without triggering the six-station war condition. The last paid frame finishes after Repeat is turned off; Earth's queue ends empty. Subsequent cargo conversion uses two returned supplies and one newly manufactured Supply Pod, giving PROSPECTOR **750 capacity**. It delivers 250 each titanium, iron, aluminium and copper to Mars. Redman transfers normally from Cryo to quarters and then Production, completes the Mars AOC and returns intact to quarters with 51 actions.

Across Earth, Moon and Mars, the exact total debits are **385 iron, 570 titanium, 353 aluminium, 179 carbon, 281 copper and two silver**: seven frame recipes, two AOCs and one Supply Pod. Palladium, platinum, gold and stored MeH totals are unchanged. PROSPECTOR consumes exactly **39 fuel**, ending docked at Mars with 189 fuel, 38 crew and three empty supplies. WAYFARER consumes **11 fuel**, ending undocked at Mars with 220 fuel, four crew, an empty Cryo Pod and two empty supplies. All four named ships and all crew survive. FIRST LIGHT and the Moon shuttle retain 70/4 fuel and 35 crew each.

Resume `artifacts/prewar-factory-expansion-20261007/reloaded-slot-5.json`, SHA-256 **`77159323f3a640ea722da5a5d1425d5c9b2189fc3430800ec6d325c3ebe3ce1f`**, simulation day **19947**, date `3119 881.66`, time off. Earth, Moon and Mars now have AOCs, but the secondary factories still lack the full buffered drone recipe. Mars has 250 iron, 246 titanium, 249 aluminium, 204 carbon, 250 copper and 245 silver; the Moon has 180 iron, 362 titanium, 81 aluminium, zero carbon, 27 copper and 64 MeH. Neither has palladium/platinum yet. Earth retains 48,058 iron, 47,394 titanium, 47,903 aluminium, 6,930 carbon, 21,812 copper, 12,250 palladium, 13,040 platinum and 9,489 MeH. Distribute the full mineral mix and fuel before triggering war; strengthen WAYFARER's four-person crew before prolonged freight. This is production capacity and normal station-construction evidence, not a demonstrated three-factory wartime throughput or successful Sol campaign.

The runnable construction/freight/fuel/crew/peace/reload audit passes. UI reload preserves every field except the three elapsed-time fields. Owned session 88788 exits 0 with a strict log; original saves are restored byte-for-byte and no settings file is introduced. Evidence and staged checkpoints remain under the artifact directory above. No runtime edits, new full-suite result or Windows acceptance are claimed. Counts remain **35/48 implementation evidence and four locally accepted requirements**.

## Pre-war freight crew preparation — 2026-10-07

Continue the day-19947 expansion using unchanged runtime `77986bc`. Mars manufactures one Supply Pod for two titanium, one aluminium and one copper. WAYFARER replaces its empty Cryo Pod with this supply, returns the Cryo to Mars stock, then flies to the Moon. Normal cockpit transfers exchange Raphael's intact 35-person team into WAYFARER and Blunket's intact four-person team into the idle Moon shuttle. WAYFARER ends docked at the Moon with three empty supplies and 213 fuel.

PROSPECTOR starts Earth-to-Mars ACC with iron, titanium and aluminium selected. Its first 750-capacity delivery contains **250 of each mineral**: selection advances per pod, rather than filling all pods with the same selected mineral. Earth supplies 67 MeH in an automatic refill; PROSPECTOR ends returning from Mars with 242 fuel and 38 crew. Mars holds 500 iron, 494 titanium, 498 aluminium, 204 carbon, 249 copper and 245 silver. Full recipe and fuel buffering remains pending; Mars still has no MeH, palladium or platinum. Verify the saved refuel threshold before planning long freight runs.

Resume `artifacts/prewar-buffered-freight-20261007/reloaded-slot-3.json`, SHA-256 **`c844d984abb36cf08e6a7ba59d3acf6a5536a57e8f5e48751be7be7fa7d9259b`**, day **19962**, date `3119 894.68`, fast time off. PROSPECTOR ACC remains active; WAYFARER ACC is off with its old Mars/Moon route, which must be reconfigured for Earth/Moon. All four ships, all crews and all three AOCs survive; war remains false with 16 trades. The saved day-19961 clock had nearly reached its natural timer interval, so the reloaded save includes one ordinary .01 tick and its mining/refining/travel effects; byte-equivalent reload is not claimed.

The artifact's runnable audit passes exact pod recipe, cargo, crew, fuel and peace checks. Owned session 73448 exits 0 with a strict log, and original saves are restored byte-for-byte. This checkpoint prepares further gameplay; no runtime change, new full-suite result, Windows acceptance or additional completed requirement is claimed. Counts remain **35/48 implementation evidence and four locally accepted requirements**.

## Pre-war reserves at all three factories — 2026-10-07

Source `b251f13`, unchanged runtime `77986bc`, continues the day-19962 crew checkpoint through normal ACC freight and native time controls. No campaign resources, prerequisites, crews or RNG are edited. Both secondary stations first receive **750 MeH**; the saved IOS refuel threshold is **200**, rather than the previously assumed 50. WAYFARER then delivers 48 mixed base-metal loads, 15 copper/palladium/platinum loads and five carbon/copper loads to the Moon. PROSPECTOR delivers 47 mixed base-metal loads, 12 copper/palladium/platinum loads, two copper loads and four carbon/copper loads to Mars. Together with the two fuel cargoes, this is **135 full 750-unit deliveries**. Selected minerals rotate per pod, allowing three different 250-unit minerals in one load.

| Station | Iron | Titanium | Aluminium | Carbon | Copper | Palladium | Platinum | MeH | Complete drone recipes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Earth | 24,058 | 23,394 | 23,903 | 3,680 | 10,062 | 5,500 | 6,290 | 7,284 | 182 |
| Moon | 12,180 | 12,362 | 12,081 | 1,750 | 5,777 | 3,750 | 3,750 | 501 | 100 |
| Mars | 12,250 | 12,244 | 12,248 | 1,704 | 6,249 | 3,000 | 3,000 | 371 | 100 |

All orbital minerals/items are conserved exactly; no new manufacturing occurs. Automatic refills consume **1,330 stored MeH**, while the ships consume **1,346 fuel** including their starting onboard reserve: WAYFARER 555, PROSPECTOR 791. WAYFARER stops undocked at Earth with 231 fuel and Raphael/34 crew. PROSPECTOR finishes its last cycle docked at Mars with 208 fuel and Floyd/37 crew. Both ACCs and fast time are off, all cargo pods are empty, and all four named ships survive. FIRST LIGHT and the Moon shuttle retain 70/4 fuel and 35/4 crew. Normal attrition also leaves Morse/143 at Earth and Redman/189 at Mars. All three AOCs remain installed; the old paid Moon Tool Pod remains paused. War is false, trade count is 16 and the war-declaration day is zero.

Resume `artifacts/prewar-freight-reserves-20261007/reloaded-slot-4.json`, SHA-256 **`33773d19978f3dacc892b7644b4374c6357e28ccd38b6b674bb32fee65628d6f`**, simulation day **20753**, date `3120 684.69`. The day-20537 intermediate Moon-ready save is retained. Final native reload preserves every field except the three elapsed-time fields. The runnable mineral/cargo/fuel/crew/peace/factory/reload audit passes, owned session 15007 exits 0 with a strict log, and original saves are restored byte-for-byte without introducing settings.

The **382 distributed recipes** establish a buffer, not manufactured drones or proven wartime replacement capacity. Budget additional DFCC/research equipment before treating all recipes as fleet capacity. Next: equip the trade ship normally, trigger the seventeenth trade, return the prototype, research defence and validate paid concurrent production at all three AOCs while responding to enemy movement. Preserve this pre-war save, all prior losses and the separate repaired-Jupiter branch. Later Sol/SCG/interstellar/ending progression and matching Windows acceptance remain open. No runtime change or new full-suite result is claimed; counts remain **35/48 implementation evidence and four locally accepted requirements**.

## Three-factory war production and first defence — 2026-10-07

Source `7238d66`, unchanged runtime `77986bc`, continues the day-20753 buffered pre-war checkpoint without editing resources, prerequisites or RNG. WAYFARER fits a stocked Tool Pod and CommsPod, visits Jupiter and receives the normal war/prototype encounter at **day 20769**, with trade count remaining 16. One launch update followed by engine engagement escapes normally. The ship returns to Earth with a healthy drive, 202 fuel and all 34 crew; unloading the grapple analyses the prototype. PROSPECTOR returns from Mars and clears the Earth berth for the carrier. Both naturally scheduled alien transmissions are acknowledged.

Cavell's normal research completes the recovered Methanoid Fusion Laser, then IOS Drone and DFCC. In this trade-triggered route the recovered laser research unlocks drones/DFCC; the separate next-day war unlock applies to trade counts below 16. Drone repeat production starts at **all three AOCs** while DFCC research proceeds. Earth manufactures exactly two control computers; both IOS ships convert normally and receive 100 fleet-fuel units each. The 202/200 old fuel is returned to Earth, and 2,000 MeH pays for the new fuel. No replacement drive or additional laser is manufactured.

An exact saved **48-update interval**, day 20853 to 20901, produces **eight drones at each station**, charging eight complete recipes per factory. The local enemy fleet grows from 10 to 16 during that interval. Both carriers gather drones through ordinary orbital-pool transfers: WAYFARER collects Mars/Moon output, and PROSPECTOR brings Earth's output to the Moon before returning to Earth. No stored fleet is credited as an automatic defence.

The first enemy attack is retained without replay: **113 WAYFARER drones versus 48 attackers** at the Moon. Ten player drones are lost; WAYFARER retains **103**, the enemy withdraws with **24**, the attack ends and its threshold doubles to **80**. All four named ships and all three friendly stations survive. The factories have manufactured **169 drones** in total: 55 Earth, 57 Moon, 57 Mars. With ten combat losses, 159 remain: 103 aboard WAYFARER and 26/3/27 in Earth/Moon/Mars pools. Each AOC also has one paid active repeat order, so exact resource charges are **56/58/58 drone recipes plus two Earth DFCC recipes**. The old paid Moon Tool Pod remains inactive. No crew or ship loss is recorded.

Resume `artifacts/three-factory-war-20261007/reloaded-slot-1.json`, SHA-256 **`b978fc559d204df43b2f0a88571d442675caee3d13fa635ea03e75b8b16cc34a`**, day **21157**, date `3121 080.77`, fast time off. WAYFARER is undocked at the Moon, 103 drones, 91 fuel, Raphael/34; PROSPECTOR is undocked at Earth, zero drones, 95 fuel, Floyd/37. Both drives are healthy, both DFCCs installed and both ACCs off. Orbital MeH is Earth **5,686**, Moon **501**, Mars **371**. The six hostile Sol garrisons currently hold **96 drones each**, not the 200 of the older campaign. Gather the 56 stored drones and current production promptly for the next Jupiter assault; preserve the first outcome, then progress SDM capture/repair and the remaining Sol stations while handling defence. The remaining uncharged stocks fund 126/42/42 further drone recipes, plus the three already paid jobs. This is a stronger replacement position, not completion of capture, SCG or the later campaign.

## Three-station capture and overview-loss follow-up — 2026-10-07

Source `6b0bbf8`, runtime `77986bc`, continues the unchanged day-21157 checkpoint. WAYFARER gathers three Moon drones, 26 Earth drones and 28 Mars drones, including one newly manufactured drone, reaching **160**. The first approach mistakenly advances past Jupiter arrival and the hostile warning, destroying WAYFARER without a battle. That failure is retained. Repeating only the route from the unchanged starting save stops at the actual **eight-update Mars–Jupiter journey**; no campaign resources, prerequisites, crews or RNG are edited.

All three first battle outcomes are retained without combat replay:

| Station | Attacking drones | Garrison | Survivors | Lost |
| --- | ---: | ---: | ---: | ---: |
| Jupiter | 160 | 100 | 144 | 16 |
| Uranus | 144 | 102 | 123 | 21 |
| Titania | 123 | 104 | 84 | 39 |

Jupiter's first four docking/defusal attempts miss the real-time window. Each failure save is retained, and each retry starts from the unchanged post-battle save with 144 drones. The successful retry watches discovery playback, returns through Master Control to WAYFARER, opens the PANIC control and switches **1 off, 2 on** in one continuous action sequence. Escape did not pause this flow. Uranus and Titania are subsequently captured and disarmed on their first docking attempts; no discovery replay is needed. All three stations retain their AOC/MTX, but their captive ground colonies still need repair. Captured derricks are Jupiter seven, Uranus two and Titania two.

The original failed native run also logs two stale-overview ship-selection exceptions after SDM destruction. These are not treated as a clean-log result or hidden as operator error. The root-cause follow-up refreshes the global overview immediately on station loss; both fleet regressions reproduce it before the correction. [Focused validation and separate destructive native verification](validation-results.md#overview-refresh-after-real-time-station-loss--2026-10-07). The later successful campaign run has a strict clean log. Both campaign processes exit **0**.

Across the successful **33 simulation updates**, the three factories complete five drones each, charging exactly five further recipes apiece: each loses 600 iron/titanium/aluminium, 75 carbon, 275 copper and 150 palladium/platinum. Total surviving drones are **159 + 15 - 76 = 98**. WAYFARER spends **31 fuel**; all other ship fuel and orbital MeH remain unchanged. Two natural 0.01-day ticks account for the difference between simulation updates and displayed whole-day advancement. Normal attrition leaves Raphael/33 and Kingston/34. No ship or friendly station is lost in the successful branch.

Resume **`artifacts/three-factory-jupiter-20261007/reloaded-slot-1.json`**, SHA-256 **`4c39fec329c78bdd462cb99d644b957e5abb481072ada230809f43aa7f67bc25`**, simulation day **21190**, date **`3121 111.79`**, fast time off. WAYFARER is docked at Titania with **84 drones, 60 fuel**, Raphael/33 and a healthy drive; PROSPECTOR is undocked at Earth with zero drones, 95 fuel and Floyd/37. FIRST LIGHT remains docked at Earth with 70 fuel; the Moon shuttle remains docked with four fuel. Both IOS ACCs are off. Earth/Moon/Mars drone pools hold **5/5/4** and uncharged materials fund **121/37/37** further recipes. The Sol enemy fleet has **28 drones**, threshold 80, no active attack and countdown zero. **Neptune, Triton and Pluto remain hostile with 104 drones each.** Reinforce before those three captures, then verify the Sol-clear/SCG unlock and continue colony repair and later progression. Do not substitute the destructive overview-verification save for this continuation.

Canonical native reload preserves every game field except `Clock.NormalElapsed`, `SdmTimerRemainder` and `TimeSkipStart`. Original saves are restored exactly, no settings file is introduced, and owned native processes are closed. Runnable audits, first battle states, five failed branches, final/reloaded saves, hashes and logs are retained in `artifacts/three-factory-jupiter-20261007/`. The post-fix destructive replay is separately retained in `artifacts/overview-sdm-loss-20261007/`. Counts remain **35/48 implementation evidence and four locally accepted requirements**; full Sol/SCG/interstellar/ending, colony repair on this branch, matching Windows acceptance and earlier shutdown investigations remain open.

The runnable audit passes normal war/research, two DFCCs, the exact production interval, 169 manufactured drones, first-outcome battle losses, all recipe/fuel charges, crew/ship survival and canonical reload (only the three elapsed-time fields excluded). Owned session 69032 exits 0 with a strict log; original saves are restored byte-for-byte and no settings file is introduced. Staged checkpoints, log, hashes and audit remain in the artifact directory. An initial audit date literal incorrectly encoded `3121 080.77`; correcting the expected centiday value to `2108077` changes no runtime/save data. No new full-suite or Windows result is claimed. Counts remain **35/48 implementation evidence and four locally accepted requirements**.


## Six-factory reinforcement — 2026-10-07

Runtime `8166c0e` continues the unchanged day-21190 three-capture save through normal UI controls. Research MTX, install it at Earth for **500 titanium, 82 copper, 100 palladium and 40 gold**, and balance drone ingredients/fuel to Titania, Uranus and Jupiter. Their captured AOCs then receive paid repeat drone orders. The first eight-update Titania pass moved the seven minerals but left fuel unchanged; a separate fuel-only balance succeeded. Both before/after saves remain, without claiming a runtime defect from that observation.

The measured interval **21246→21294** produces **eight drones at each of six factories**, with exactly eight complete recipe debits per station. A scheduled alien transmission interrupts date clicking; the checkpoint records 35 actual updates, then its normal View Transmission flow and the remaining 13 updates complete the interval. No resources, crews, prerequisites or battle outcomes were edited.

Across the whole **21190→21294** continuation, **77 drones** finish and three additional paid jobs begin. Total mineral debits equal **80 drone recipes plus the single MTX installation**, with no unexplained transfer loss. The resulting **175 drones** comprise WAYFARER's unchanged 84 and station pools **22/23/22/8/8/8** (Earth/Moon/Mars/Jupiter/Uranus/Titania). All six paid repeat orders remain active; uncharged materials fund another **5/19/19/6/23/51** recipes respectively. MTX routes are cleared. Earth/Jupiter will need more of Titania's surplus before a long production interval.

WAYFARER stays docked at Titania with 60 fuel and Raphael/33; PROSPECTOR stays undocked at Earth with 95 fuel and Floyd/37. Other ships' positions, fuel and crew counts are preserved; normal crew attrition countdowns each decrease by one. Neptune/Triton/Pluto remain hostile with **130 drones each**. Sol's enemy fleet has **41**, attack threshold **80**, no attack/countdown. Transmission stage 2 has been viewed, stage 3 countdown 68. No new battle or colony repair is claimed.

Resume: `artifacts/sol-clear-campaign-20261007/reloaded-slot-3.json`, day **21294**, date **3121 212.82**, fast time off; SHA-256 **`1b053716c6dbbedc73e5a653c8b5cff7f461f00bb7109bffa6ec5fa38c33d55f`**. `audit.py`/`audit.log` retain runnable assertions for exact production, global costs, ship state, enemies, transmission, reload and original profile restoration. Native log passes strict error inspection and the owned process exits **0**. Original saves are restored exactly. Local ignored saves/logs do not travel with Git.

Implementation/acceptance totals stay **35/48** and **four locally accepted requirements**. Remaining captures, Sol-clear/SCG/interstellar progression, repairs and relevant Windows acceptance remain open. The tested overview fix `8166c0e` is verified on the main repository contribution branch; this campaign introduces documentation/evidence only.


## Sol-clear and SCG discovery — 2026-10-07

Continue the unchanged day-21294 reinforcement save at `e9ff6dc`, adding the MTX pointer correction described in [validation](validation-results.md#mtx-menu-pointer-obstruction--2026-10-07). Titania balances surplus ingredients to Earth/Jupiter. Earth, Jupiter and Uranus send drones to Titania; PROSPECTOR collects **26 Moon + 25 Mars drones**, returns them to Earth, then the transmitter forwards them. WAYFARER assembles **200 drones**. The pre-fix blocked navigation also left a fuel-send selection at Titania: its 2,963 MeH moved to Jupiter, where it remains available; the route was cleared. This normal UI side effect and its save are retained, not edited away.

| First battle | Player / defenders | Surviving player drones | Losses |
| --- | --- | --- | --- |
| Neptune | 200 / 138 | 163 | 37 |
| Triton | 172 / 142 | 131 | 41 |
| Pluto | 139 / 142 | 77 | 62 |

Nine reinforcements reached WAYFARER at Neptune and eight at Triton through the new forward routes. Each station's first battle result was saved before docking. All three self-destructs were disarmed on the first attempt. No battle, defusal or RNG state was replayed. Pluto's docking tick crossed the enemy build deadline before defusal; its two newly built defenders became captured stock. The retained post-battle and captured saves establish that timing.

Across **21294→21356**, the six active factories consume exactly **60 drone recipes** and finish 60 drones. Global player accounting is **175 + 60 manufactured + 2 captured − 140 losses = 97 drones**. WAYFARER has 77; pools are Moon 7, Mars 7, Triton 4 and Pluto 2, with other friendly Sol pools empty. All six factories retain paid repeat orders, with remaining uncharged recipes **18/9/9/8/13/8** at Earth/Moon/Mars/Jupiter/Uranus/Titania. Drone-only routes remain active: Earth/Jupiter/Uranus→Titania→Neptune→Triton.

All six hostile Sol stations are now friendly, built and disarmed, with captive ground repairs pending. The normal **Sol Cleared** bulletin unlocks research for **SCG chassis, star drive and HeD fuel**, plus the interstellar-travel unlock. Those three technologies are not yet researched. The enemy Sol fleet remains at **48**, threshold 80, no active attack/countdown; Sol-clear does not establish removal of residual warships. Transmission stage 3 has six updates left.

Resume **`artifacts/sol-capture-final-20261007/reloaded-slot-4.json`**, day **21356**, date **3121 269.87**, fast time off. SHA-256: **`03175227f3c3a581bbe832d7257be9d1f575493487ed7f8d40f03c85df352ee1`**. WAYFARER is docked at Pluto with **77 drones, 38 fuel, Raphael/33**; PROSPECTOR is undocked at Earth with **0 drones, 85 fuel, Floyd/37**. Other ships' fuel and all named crews' counts are unchanged. WAYFARER used 22 fuel; PROSPECTOR used 10. Both drives remain healthy and ACCs off.

`audit.py`/`audit.log` validate all first outcomes, exact recipe and fleet accounting, research unlocks, reload, both strict runtime logs/exit **0**, and exact restoration of original saves. Both owned game processes are closed; ignored evidence remains local. Research, SCG construction, first interstellar journey, colony repair, later progression and matching Windows acceptance remain open. Totals stay **35/48 implementation evidence and four locally accepted requirements**.
