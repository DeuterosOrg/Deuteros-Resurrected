# AMA mining and cargo compatibility

Partial findings for **1215685674676221 — AMA: investigate source code**, decoded 2026-10-02. The mixed-cargo crash is corrected; full original mining fidelity remains under investigation.

## Original evidence

Uses the same Disk 1 image and address conversion as [the asteroid ACC trace](original-asteroid-acc-evidence.md): SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`, disk offset `$6E000 + RAM address - $13000`.

The ship update at `$23936` requires hull byte `+3 == 2`; state `$0D` invokes mining `$23C14` through the branch at `$23952`. State `$0C` invokes the scanner instead. Mining entry `$31834` requires a valid scan and zero-based class at least 5, then sets state 7, action `$0D` and countdown 2. Do not equate that countdown with remake days without tracing its scheduler.

`$23C30–$23C7A` scans three module words at `+$16/+$18/+$1A`. It skips non-supply modules, full pods and partial pods containing another mineral. The first zero-count pod or compatible partial pod receives ore. Empty pods are identified by quantity, regardless of the old mineral bits.

If none can accept the ore, `$23CA2` jumps to `$30B48`. Its state-`$0D` branch reaches `$30C08`, setting launch state 1, return-to-scan action `$0C` and countdown 2. No incompatible cargo is overwritten. In automatic scan handling, the selected-mineral path `$23BD4–$23C0E` likewise searches compatible pods; exhaustion jumps to departure `$30EA8` instead of starting another mining approach.

The mining amount block `$23C7C–$23C86` calls the random routine, masks with `$1F`, then adds `$0C`: numeric range **12–43**. It updates only the selected pod and clamps at 250. This establishes arithmetic bounds, not uniformity or ore per remake day.

## Clock and short-cycle findings

The disk starts `$1378E` at **310,000,000**, corresponding to the displayed year 3100. Date rendering at `$22BF2–$22C56` derives the year by dividing by 100,000, the three-digit day from the next three decimal digits, and the fraction from the remainder modulo 100. Thus a 100-unit increment advances one displayed day.

`$20536` selects increment 100 for time advancement; `$20510` restores increment 1. The interrupt at `$20414` raises pending flag `$20291`, then `$20422–$2043A` applies the increment. While the flag is set, `$203DE–$203E4` prevents another increment. The simulation consumer `$23CA8` requires that flag, calls the ship updater `$2384E` at `$23CFE`, then reaches `$22BB4` to clear the flag. This establishes one ship update per consumed clock increment, rather than repeated mining on every rendered frame.

Within that update, the mining gate compares `(ship[+4] - 1) & 3` with `(clock >> 7) & 3`. For phase 0, stepping from the initial clock by whole days permits mining on days **2, 7, 12, 17, 22, 23, 27, 28, 32, 33, 38…**. The resulting 1-, 4- and 5-day gaps follow the clock bits; there is no random instant-cycle roll. Other phases shift the schedule. This is a calculation of the traced rule, not an emulator gameplay capture. Fractional advancement can produce different intervals, so the sequence assumes uninterrupted 100-unit steps.

The amount calculation reads neither pilot rank nor asteroid size. Class is an entry gate (zero-based class at least 5); it does not scale the mining amount once mining is active. The manual entry checks nonzero fuel and scan state, then schedules two consumed updates before `$31608` enters mining state `$0D`. Wider scanner/crew eligibility and the semantic allocation of byte `+4` still require tracing before changing remake scheduling.

## Remake correction

Previously, `ShipInterior.UpdateShips` checked for any partial supply pod, then used `First` to find an empty/matching one. If only incompatible cargo had space, the second search threw `Sequence contains no matching element`. Zero-count pods retaining an old mineral could also trigger that exception. ACC could initiate docking with the same unusable cargo layout.

The correction selects a compatible pod explicitly, caps its quantity, and launches when none exists on a mining attempt. ACC returns home when a selected mineral cannot fit. Existing cargo remains intact. Cases **310–313** reproduce these failures and cover empty, partial, incompatible and full pods. The prior successful-mining fixture now supplies an empty pod; its old incompatible cargo setup had encoded the erroneous docking behavior.

## Remaining investigation

The remake still uses its provisional 16–35 yield and five-day/first-day chance. Original mining is gated by `(ship[+4] - 1) & 3` matching `(clock[$1378E] >> 7) & 3`; clock units and caller cadence are now traced above. A stable remake mapping for the original ship-slot phase and fractional clock still needs a compatibility decision before replacing timing. Pilot effects, SCG availability, Complete Cycle, scan generation and normal campaign behavior also remain open. These cargo fixes do not complete the AMA research task.

Raw traces are under ignored `artifacts/research/supply-pod/ama-{dispatch,mining,clock}-followup.txt`. See [validation](validation-results.md#ama-compatible-cargo--2026-10-02) and the [Windows acceptance brief](windows-agent-brief.md).
