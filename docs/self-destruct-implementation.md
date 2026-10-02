# Self-destruct gameplay implementation

Task **1215685674676229**, 2026-10-02. The control, capture and expiry paths now run in the remake. The task remains partially accepted: the original alarm/audio, timing observations and ground-team lifecycle still need work. [Original instruction evidence](original-self-destruct-evidence.md) supplies the rules; this page records implementation choices and verification limits.

## Player flow

An installed friendly orbital station exposes **Self-destruct** in the side menu. A hostile station exposes it after a human IOS/SCG occupies its berth. Docking at war starts count 16 on docking completion and retains hostile ownership. Inspecting an undiscovered mechanism opens the existing SDM bulletin and unlocks its research.

Two switches reproduce the traced combinations: switch 1 on/switch 2 off arms at 12; switch 1 off/switch 2 on disarms. Intermediate combinations do neither. Reopening derives the switches from the armed state. At a hostile station, discovered Hyperlight locks switch 1 while switch 2 is on; completed Hyperlight research is not required. Successful defusal captures the station, assigns its ordinal, restores the traced mineral/derrick ranges and leaves a non-Earth captive colony needing repair.

The panel uses ordinary controls and existing fonts. It is a functional remake screen, not recovered original SDM artwork. It currently displays its alarm visually; an audible original alarm is not implemented.

## Simulation and persistence

`SdmSystem` owns the mutations. `SpaceStation.SdmCountdown` stores the raw byte range, including the pirate skip bit. The separate saved `SdmTimerRemainder` keeps subsecond progress. Version-1 saves missing these fields start at zero; explicit null, invalid countdowns and invalid timer remainders are rejected. Keep backups: older builds reject the new fields.

The real-time path currently uses one count per second, based on 50 original producer calls and an assumed PAL rate. Godot tree pause also pauses this processing; elapsed frame time is accumulated. Original wall-clock rate, pause behavior and dropped/coalesced producer ticks are not yet measured, so this is a documented provisional mapping.

The simulation path runs after ships, enemy processing and MTX, before display notification. Friendly armed stations expire on that pass. Hostile counts 16/15 survive it; counts below 15 expire. A set high bit is cleared and skips that pass. No pirate event producer was added.

## Loss and recovery

Expiry removes the station, its stocks, installed facilities, production queue and local staff. It removes berthed human hulls with their pilot/cargo/cryo references, while preserving nearby undocked, approaching and in-transit hulls. Non-Earth local shuttles are lost; Earth loses only its orbital docking/docked shuttle. Earth ground stores and services survive.

A non-Earth colony becomes damaged; local ground stocks and derricks are discarded with the lost record. Ground roster entries are presently retained because the original ground-team record lifecycle is still unresolved. This boundary needs reconciliation before claiming original casualty fidelity. Pending enemy attack targets are cleared so a stale attack cannot repopulate the removed station.

Loss news is emitted once, fast-forward stops, and an affected screen exits before the next display event. A surviving ship can retain its interior. Callbacks holding a planet from an obsolete loaded world cannot arm or defuse it.

## Verification

Cases **360–374** cover docking/defusal, manual switches, both expiry paths, grace/skip boundaries, berth versus nearby ships, Earth/non-Earth shuttles, save phase and legacy fields, malformed saves, Hyperlight interlock, discovery, stale callbacks, screen cleanup and one-time capture resources. Fourteen cases reproduced missing behavior before implementation; case 373 verified the cleanup already implemented for the earlier expiry cases. All fifteen pass focused headless checks. Native Mac case 361 passes and its screenshot was reviewed; the clipped warning was shortened and recaptured.

The first full validation attempt stopped after `IMPORT OK` with an editor timer error. Its log is preserved under ignored `artifacts/validation/evidence/self-destruct/import-failure/`. An unchanged strict import probe passed; no filter or code workaround was added. The next full run passed import and stopped at existing case 119; Windows independently reached the same failure. Structured save comparison found only the new real-time timer phase changed across the preset test's rendered frames. That test now freezes GameCore frame processing around the repeated preset, retains its entire-save equality check and restores processing afterwards. All four affected preset cases (119–122) pass focused checks. Fresh full validation is required for the correction. Latest completed cross-platform baseline remains 359 cases at `55316b6`; these new cases have not yet completed Windows validation. Existing desktop testing at `8cdd458` remains a separate revision.
