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

## Implemented arrival path and remaining acceptance

The `e1754eb` runtime reaches this promotion through saved Hyperlight travel. It persists `Staff.Warlord` independently of ordinary actions, exposes rank 4 through the existing rank APIs and emits one report. Promotion happens at star arrival before `$31564`-style ordinary experience: a Captain at 39 actions becomes Admiral afterwards, without chaining Warlord on that arrival. Ordinary arrival or additional actions never grant the milestone. Missing legacy flags default to false; invalid crew milestones are rejected before save activation.

Cases 503–505/508 cover the real route, pre-arrival reload, repeated travel/news, Captain/Admiral boundary, ordinary arrival, rendered crew rank, combat strength, frozen/returned teams and malformed saves. Full 513 Mac validation and audited cross-export pass; native route/UI/transfer checks pass and screenshots were inspected. See [validation](validation-results.md#saved-interstellar-travel-and-warlord-513-case-checkpoint) and [save compatibility](save-files.md#interstellar-flights-and-warlord).

This adds implementation evidence for the Warlord task. Native Windows, original-runtime comparison and unstaged normal-campaign acceptance remain pending. Rogue-crew progression, transmitter activation and ending are separate unfinished mechanics.
