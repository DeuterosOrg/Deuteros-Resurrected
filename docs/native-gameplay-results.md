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
