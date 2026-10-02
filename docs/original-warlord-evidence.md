# Original Warlord promotion: trigger investigation

Partial evidence for **1215716464570913 — warlord status for pilots**, decoded on 2026-10-02. The live Asana task has no description or comments. The original contains a specific promotion path; a generic battle-win or interstellar-arrival threshold would not reproduce it.

## Evidence identity

Use the disk identity and address conversion in [the original ACC trace](original-asteroid-acc-evidence.md): SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`; disk offset `$6E000 + RAM address - $13000`, big-endian Motorola 68000. Local raw traces are `artifacts/research/supply-pod/warlord-{promotion-lead,flight-trigger}.txt` and are not distributed with Git.

## Rank and notification

The signed-offset text table at `$1C482` maps `$A1/$A2/$A3/$A4` to Pilot, Captain, Admiral and Warlord. Warlord's string starts at `$1D02F`.

The special arrival handler starts at `$314E6`. At `$31516` it reads the ship's team reference and looks up its 8-byte record in `$197C2`. `$3152A–$31534` requires the team's low two rank bits to equal 3. `$31536: 5211` increments the rank byte. For an Admiral record this changes stage 3 to 4. The handler then emits news event 1 with text `$A0 + (rank & 7)`, selecting Warlord, and the team's name index. Ordinary promotion `$22E2C–$22E34` stops at stage 3, so accumulating ordinary actions cannot invoke this fourth rank.

## How that handler is reached

The action dispatch table at `$237D8` maps action `$16` (22) to `$314E6`; action `$17` instead goes to `$314B2`, bypassing the Warlord block.

The SCG walker at `$23990` traverses 16 records at `$1BC66`, stride `$22`. Within its accelerated travel path, `$23A4A–$23A64` requires remaining travel at least `$2F`, travel-phase byte `+$14` equal `$0B`, and research byte `$1A1C6` equal 100. It then sets ship state `$15`, action `$16`, clears the travel phase and schedules one remaining update. Completion dispatches that action through `$2383A`.

`$1A1C6` is byte 2 of recipe/research record `$1A1C4`, index 22 (record ID `$17`). The item-name mapping `$101 + index` gives text `$117`, **Hyperlight**. This ties the observed promotion path to completed Hyperlight research and its special travel transition. It is stronger evidence than the walkthrough description “fly an Admiral to Proxima and back”; the precise meaning of the travel-phase/countdown transition still needs comparison with gameplay.

## Implementation boundary

The remake has only Pilot/Captain/Admiral and a single generic transit-arrival path. Its travel calculator currently has no Hyperlight branch. Do not promote every Admiral on an ordinary cross-star arrival or invent a battle-win threshold. First establish the Hyperlight transition and its eligibility/timing, then persist the rank, emit one promotion report, and test save/load and combat/display use.

The ordinary rank-display inconsistency is now corrected: marine text derives from `GetLevel`, so Admiral is shown at 40 actions, matching promotion news. Cases 330–332 cover boundaries and save/load. This does not implement Warlord. The [travel follow-up](original-interstellar-travel-evidence.md) now traces acceleration, Hyperlight and per-star/ship clocks; integration scope remains to be settled.
