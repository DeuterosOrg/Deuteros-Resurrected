# Battle completion and cancellation

Additional review fixes found while connecting News battle-loss reports. These do not add an extra task to the 48-task count or establish original combat fidelity.

## Reproduced failures

- Completing a battle detached its window without freeing it. Case 438 failed the lifetime assertion and logged leaked texture, text and font resources at shutdown.
- Leaving battle or loading another world lost the station drones reserved for that encounter. Cases 439/440 reproduced a 40-drone reservation returning zero.
- Leaving after the result became defeat but before the presentation delay ended could retain the defeated ship. Case 441 reproduced that outcome after the initial cleanup correction.

## Corrected behavior

The window is freed on normal and interrupted exit. Its pending waits cancel when the scene leaves. Remaining player/enemy drone counts settle on exit; a station reservation returns its surviving drones once to the original, still-matching hostile station. A replaced world is not credited by an old encounter.

A completed result commits once before the presentation delay, including when exit occurs at the completion boundary. Defeat removes the ship and records its News report. Active defeated-ship controls close immediately; an older result cannot redirect a newer screen. An incomplete cancelled encounter retains casualties already calculated but does not invent a defeat or retreat. The existing retreat behavior remains, with its threshold updated once.

## Verification

Cases **438–442** pass headless and native Mac checks. They open the actual interior battle control, then stop random combat and stage outcomes to test lifecycle boundaries deterministically. They cover defeat, partial cancellation (27 surviving reserved drones), world replacement, completed-result interruption, and enemy retreat (threshold 20→40 exactly once, both fleets' survivors retained).

Compatibility cases 23/127/128/276/348/349/436/437 also pass. The full 442-case Mac aggregate, nine Python tests, strict import, startup and audited Windows cross-export subsequently pass at `625cae9`; Windows execution and desktop checks remain pending. Raw failed and passing logs are under ignored `artifacts/validation/evidence/battle-cleanup/`.

Desktop acceptance should include finishing and fleeing real battles, leaving at different stages, saving after cancellation, loading another save, reopening controls, and repeated application closing. Check source and export separately. Staged results do not verify combat balance, animation timing, or normal campaign acceptance.

## Normal campaign follow-up — 2026-10-04

At runtime `3ebeca0`, a normally researched/manufactured DFCC ship and drone now reach hostile Jupiter. Real defeat removes the ship, records crew then vessel loss, preserves the station's surviving defenders and survives reload. A separate replay of the unedited encounter completes Flee and preserves both fleets and return transit through reload. Both native Mac sessions close cleanly with strict logs and original saves restored. See [normal campaign evidence](native-gameplay-results.md#normal-battle-defeat-and-retreat). Victory, PTL, interrupted presentation, Windows and original-runtime acceptance remain separate.

## Empty fleets and original PTL boundaries — 2026-10-04

The baseline review's latent PTL crash is reproduced: exact damage leaves zero drones, then attrition decrements to −1 and indexes the explosion coordinates. An ordinary encounter starting with zero drones also fails to complete. The shared round now settles an empty fleet before attrition, retaining the existing result/cleanup path.

Direct Disk 2 inspection additionally corrects three PTL comparisons. Raw disk offsets `$23780–$23788` and `$237B0–$237B8` branch on unsigned carry: subtraction occurs only when damage is **strictly below** the fleet size; equality and excess select two survivors. `$237A8–$237AE` halves splash while it is greater than **or equal to** the enemy roll. The remake had excluded equality in all three places. The round preparation at `$22D1A–$22D44` recomputes both powers from current counts; the remake now refreshes them before deciding casualties instead of retaining pre-impact power.

These are raw file offsets, not RAM addresses. Disk 2 SHA-256 is `99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a`. Capstone 5.0.7 M68000 big-endian extracts are retained in `artifacts/research/ptl/damage-and-rounds.txt`; the disk is not distributed. Existing damage ranges, fuel charge and beam timing are unchanged. This does not establish normal PTL discovery/fitting reachability or original runtime presentation.

Cases 602–603 reproduce the empty encounter and original equality mismatch before correction. After correction, both pass headless and natively, along with 13 related checks. They cover empty player/enemy/both, actual completion/freeing, damage below/equal/above fleet size, equal/repeatedly-halved/zero splash and current fleet power. All 19 Python checks pass with the original ending disk supplied. An unused-label leak in the first test fixture was removed; its failed shutdown log is retained. The earlier guard-only full run was deliberately interrupted for the source-backed follow-up; it is not a passing aggregate. Full **603/603** Mac validation subsequently passes at `bd566582a6906dbcff7714ef0aacc42bb4974313`: build, strict import, fresh process per case, source startup and Windows cross-export. Independent audit verifies all fresh logs, 1,230 pack entries, 64 illustration imports, no test resources and ending payload hashes. EXE SHA-256: `a8f779001eaee385b107db4f77e12cfa79edabe18b1ed0dca82476ff1e3db101` (163,898,048 bytes). The known editor-only teardown diagnostic remains recorded under the existing exact allowlist; gameplay logs pass strict checks. This is not a Windows execution result. The audit and raw logs are archived in `full-run-bd56658/`. Evidence: `artifacts/validation/evidence/empty-battle/`.

The separate [PTL progression investigation](original-ptl-evidence.md) traces the cockpit control, DFCC/research gates and captive-colony discovery branch. Those reachability changes remain unimplemented.
