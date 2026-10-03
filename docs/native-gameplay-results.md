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
