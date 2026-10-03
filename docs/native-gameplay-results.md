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
