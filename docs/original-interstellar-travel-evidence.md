# Original interstellar travel and Hyperlight

Research follow-up for **SCGs (1215685674676241)** and **Warlord promotion (1215716464570913)**, 2026-10-02. This establishes mechanics missing from the remake; it does not certify an implemented travel system.

## Source and route distance

Uses the disk/hash/address mapping in [the original ACC evidence](original-asteroid-acc-evidence.md). RAM `$3518E` calculates travel distance. For two different star codes (`$A0` and above), `$351CC–$351F4` indexes a 9×9 unsigned-word table at `$34FE8`, with index `destination + origin * 9`. Selected exact entries are:

| Star indices | Distance/countdown units |
| --- | ---: |
| 0 → 1 | 4,300 |
| 1 → 2 | 200 |
| 3 → 7 | 23,000 |
| 7 → 8 | 650 |

The table is symmetric with zero diagonal. Planet-to-star and local-body routes have separate branches. The remake's subtraction of local orbital indices is not an interstellar distance rule. Mapping original star codes and star-arrival states to the remake's direct body-to-body route interface is an explicit integration decision.

## Acceleration and fuel

Engine engagement `$30F5C–$30FB4` selects accelerated SCG state `$0A` and action `$17` when the route countdown is at least 100, starting travel phase 1. `$30E4E` loads the phase's two parameters. The speed words at `$34FB8` are:

```text
256, 257, 268, 295, 320, 358, 426, 587, 821, 1816, 5689, 10000
```

Each consumed simulation update adds the speed's low byte to the saved fractional accumulator (`ship +$15`), carries into the speed word, then subtracts `(speed >> 8) + 1` from remaining distance, capped at the actual remainder. This is fixed-point acceleration, not a fixed journey-length multiplier.

With at least 47 units left, phases advance toward 11. Below 47 they step down toward 1. Phase changes charge the newly selected phase's fuel cost from `$36838`:

```text
phase: 0  1  2  3  4  5  6   7   8   9  10  11
cost:  1  1  2  3  4  6  8  12  15  18  20  20
```

The generic fuel mask for state `$0A` is `$FF`; the ship-update counter cycles through 1–255 and skips zero, so that state does not also pay one fuel every ordinary update. Fuel exhaustion and damaged-drive transitions need to remain part of any integrated port.

## Hyperlight and Warlord

At phase 11 with at least 47 units remaining, completed Hyperlight research (`$1A1C6 == 100`) selects state `$15`, action `$16`, and a one-update countdown. The next update invokes `$314E6`, synchronizes the ship clock to the destination star and reaches the [Warlord promotion handler](original-warlord-evidence.md). This route promotes an Admiral and emits the rank news; ordinary action `$17` bypasses it.

A calculation using the decoded tables/rules, starting at phase 1 with 250 fuel and no damage, gives 12 consumed updates and 141 fuel remaining for Hyperlight distances 200, 4,300 and 23,000. Without Hyperlight, countdown exhaustion takes 26, 208 and 1,005 updates respectively. These are **calculated traces, not emulator observations or proof of successful ordinary arrival**. Raw calculations and disassembly are retained under ignored `artifacts/research/supply-pod/`.

## Independent star and ship clocks

The nine initial star clocks at `$13792` are:

```text
310000000, 310430000, 310440000, 310600000, 310820000,
310870000, 311100000, 311130000, 311180000
```

A new SCG copies its current star's clock into its slot in `$137B6` at `$2FCEE–$2FD0E`. The clock interrupt advances all nine star clocks and sixteen ship clocks by the same ordinary increment. Accelerated travel additionally adds `distance advanced * 100` to that ship clock (`$23A30–$23A48`).

Ordinary arrival `$314B2–$314E0` requires exact equality between the ship and destination-star clocks. A mismatch jumps to `$36362`, which clears the SCG record and releases its crew/modules before emitting loss news. Hyperlight arrival instead explicitly copies the destination clock. Therefore a simple distance countdown that always arrives safely omits a material original rule. Emulator comparison should confirm the visible warning/loss behavior before reproducing it in the remake.

## Integration work remaining

The remake currently stores one global integer day, generic transit state and a start day. It lacks per-star/ship clocks, fractional travel progress and Hyperlight travel. SCG and delayed Hyperlight discovery are now connected; their original clock/campaign acceptance remains pending. A faithful implementation needs a save migration and complete travel/fuel/arrival handling; merely assigning Warlord on any cross-star arrival would be incorrect. Craig has been asked whether the intended port preserves original star arrivals/clocks or adapts them to direct planet-to-planet routes. Keep that scope decision separate from proven instruction arithmetic.


## Hyperlight discovery producer (additional static trace)

`$38A86–$38AA4` counts nonzero bytes in the nine-entry Methanoid station-count table at `$196FA` and writes the count to `$1C370`. Capture decrements the appropriate system count at `$35C10`; enemy capture increments it at `$38F08`. This is the number of systems with surviving hostile stations, not the number of artifacts recovered or staff actions.

The story dispatcher `$37820–$37868` detects changes in that count. Count **7** selects delay **8** from `$37800` and handler `$37898` from `$377E0`. After the change-detection pass, eight eligible passes only decrement the delay; the following pass invokes the handler. `$37898–$378AA` sets pending flag `$1C2D4` only while Hyperlight's research byte `$1A1C6` is zero. Earlier active bulletins can defer these passes.

The master tail consumes that flag through `$3769C`, after the higher-priority `$1C2D6/$1C2D0/$1C2D2` discoveries. Its list at `$3761A` contains research index **22**, a zero terminator and bulletin ID **8**. Shared helper `$377B0` changes only a zero research progress byte to **1**, making the subject available without completing it. Index 22 addresses `$1A1C4`, the already-mapped Hyperlight record. A pending flag is cleared by the consumer, rather than waiting for travel or promotion.

Raw traces and exact tables are under `artifacts/research/supply-pod/hyperlight-*`; `hyperlight-discovery-facts.json` records file hashes. The bulletin renderer indexes a runtime table at `$29540`; that table is unpopulated at the static disk mapping, so the rendered bulletin text is not established by this extract. The remake now connects the corresponding research and bulletin definitions through the saved discovery producer described below.

This narrows the missing progression path. The count is sampled by the original enemy scheduler, so an immediate capture callback alone would not reproduce its timing. Saved delay/pending state, competing bulletin delivery, save/load and research completion now have integrated tests. Actual Hyperlight travel still needs implementation. No new task is declared complete.


## Hyperlight discovery correction

The locally integrated correction now samples hostile-system counts at the existing enemy build boundary and persists the sample, observed count, eight-pass countdown and pending notice. When seven systems remain, research becomes available through the existing Hyperlight bulletin after the delay. Competing messages retain priority; interrupted, unacknowledged alien transmissions retry before the queued discovery. Recapture resets an ineligible countdown. Discovery never grants completed research and never resets existing research progress.

Cases 459–461 reproduced the missing discovery through real simulation updates, including a save mid-delay and ownership changes between enemy samples. Cases 462–464 cover competing notices and saved pending state, Research-screen selection and completion, legacy/malformed saves, all system-count boundaries and recapture. Focused checks pass, including existing transmission cases 427–432/434–435. Full 464-case Mac validation and native cases 459/460/461/462/464 pass at `fbffac5`; Windows execution remains pending. An initial 462 assertion incorrectly expected Hyperlight to bypass an unacknowledged transmission; the fixture now acknowledges that message and proves the pending discovery survives it.

This restores discovery within the remake's current scheduler. The [enemy build-frequency correction](original-enemy-production-evidence.md) separately restores the remaining-system lookup; the whole-day clock still does not reproduce original enemy cadence. Hyperlight acceleration/arrival, per-star clocks, Warlord and transmitter activation remain outstanding; this branch is not full acceptance of the travel or campaign tasks. Evidence is retained under `artifacts/validation/evidence/hyperlight-discovery/`.
