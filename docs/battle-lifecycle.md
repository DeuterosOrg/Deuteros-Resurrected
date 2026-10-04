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
