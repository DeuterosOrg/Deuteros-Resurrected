# Enemy attacks require a player station

## Reproduced campaign failure — 2026-10-07

The day26727 normal save already has both Proxima stations under Methanoid ownership, yet its enemy fleet targets Atlantic with26 updates remaining. On unchanged runtime9247615,26 manual updates produce another `Atlantic UNDER ATTACK` report, set its capture countdown to5 and stop fast time. Five more updates reset the already-hostile station's garrison64→50 and randomize its resources. Earlier duplicate attack reports remain in News. The retained failure is `artifacts/enemy-target-native-before-20261007/`: unchanged start, day26753 `invalid-arrival-slot-2.json`, day26758 `invalid-capture-slot-3.json`, native log and exit0. Original saves were restored.

`FindAttackStation` excludes hostile stations, but `ChooseAttackTarget` keeps the previous destination when no candidate exists. A repeat can also reuse the previous target. `ProcessFleet` previously entered attack without checking the destination's current ownership or station existence; `CapturePlanet` then reset it unconditionally. Thus the missing original gates affect scheduling, arrival and capture.

## Original evidence

Pinned Disk1 SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`; RAM→disk mapping `0x6e000 + address - 0x13000`. `artifacts/enemy-target-ownership-20261007/verify-original.py` asserts the relevant bytes and emits aligned disassembly in `original-target-checks.txt`. Run with `uv run --with capstone==5.0.7 python artifacts/enemy-target-ownership-20261007/verify-original.py`.

- `$35C00–$35C10`: player capture increments the per-system count at `$196FA+9 = $19703`, while decrementing the hostile count at `$196FA`.
- `$38BC8–$38BD2`: scheduling returns when that player-station count is zero, before processing the repeat counter.
- `$38C28–$38C56`: candidate search skips absent stations and hostile ownership (bit test at `$3642E`); an absent candidate returns without scheduling.
- `$38D16–$38D4A`: arrival clears the expired countdown, requires a station-map entry, and requires station type8 before starting the five-update capture window.
- `$38E4A–$38E7E`: capture expiry clears its countdown and checks the same station presence/type8 gates before applying capture. Enemy capture writes type9 at `$38EF4`.

This establishes source-level ownership gates. It does not claim a new original-emulator observation or resolve every enemy scheduling difference.

## Correction and verification

The remake reuses one player-station predicate for candidate filtering, arrival and capture, and checks that a player station exists before scheduling. Pending invalid countdowns expire without attacking, interrupting time or resetting resources. Current station ownership supplies this decision for existing saves; no campaign migration or history deletion is required. The existing repeat-selection and random travel behavior is preserved.

New cases647–648 reproduce the old scheduling and arrival failures. They cover no-player systems with/without repeat state, resumed attacks after player recapture, ownership/station loss across saving at arrival/capture, preserved stock/News/time state, and a valid five-update capture. Both pass after the correction. The first green attempt exposed a test-fixture mistake: deserialization intentionally stops fast time, so the test must enable it after loading. That log remains retained; the corrected fixture passes. Build retains14 inherited warnings, zero errors; strict import passes with the established editor teardown allowance.

Normal corrected play continues from the retained day26758 failure, without replay or save edits. After65 updates, Proxima has no attack or countdown, News is unchanged and Atlantic's garrison grows50→70 normally. Sol subsequently selects Neptune, and WAYFARER arrives normally to defend it. The final day27109 checkpoint retains57 defence losses,71 Earth Star reserves, exact reload, both strict native exits0 and original-save restoration. The first full run retained the case281 unbuilt-station fixture failure; correcting that fixture for both hulls passes280–283,440,471,647–648. The fresh full648/648 aggregate, all19 tooling checks, build/import/startup and source/log audit pass with exit0. A matching local Windows export passes all1,259 payload hashes; it has not run onWindows. See the current validation/gameplay reports. Matching Windows acceptance remains open.
