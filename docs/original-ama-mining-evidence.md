# AMA mining and cargo compatibility

Partial findings for **1215685674676221 — AMA: investigate source code**, decoded 2026-10-02. The mixed-cargo crash is corrected; full original mining fidelity remains under investigation.

## Original evidence

Uses the same Disk 1 image and address conversion as [the asteroid ACC trace](original-asteroid-acc-evidence.md): SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`, disk offset `$6E000 + RAM address - $13000`.

The ship update at `$23936` requires hull byte `+3 == 2`; state `$0D` invokes mining `$23C14` through the branch at `$23952`. State `$0C` invokes the scanner instead. Mining entry `$31834` requires a valid scan and zero-based class at least 5, then sets state 7, action `$0D` and countdown 2. Do not equate that countdown with remake days without tracing its scheduler.

`$23C30–$23C7A` scans three module words at `+$16/+$18/+$1A`. It skips non-supply modules, full pods and partial pods containing another mineral. The first zero-count pod or compatible partial pod receives ore. Empty pods are identified by quantity, regardless of the old mineral bits.

If none can accept the ore, `$23CA2` jumps to `$30B48`. Its state-`$0D` branch reaches `$30C08`, setting launch state 1, return-to-scan action `$0C` and countdown 2. No incompatible cargo is overwritten. In automatic scan handling, the selected-mineral path `$23BD4–$23C0E` likewise searches compatible pods; exhaustion jumps to departure `$30EA8` instead of starting another mining approach.

The mining amount block `$23C7C–$23C86` calls the random routine, masks with `$1F`, then adds `$0C`: numeric range **12–43**. It updates only the selected pod and clamps at 250. This establishes arithmetic bounds, not uniformity or ore per remake day.

## Remake correction

Previously, `ShipInterior.UpdateShips` checked for any partial supply pod, then used `First` to find an empty/matching one. If only incompatible cargo had space, the second search threw `Sequence contains no matching element`. Zero-count pods retaining an old mineral could also trigger that exception. ACC could initiate docking with the same unusable cargo layout.

The correction selects a compatible pod explicitly, caps its quantity, and launches when none exists on a mining attempt. ACC returns home when a selected mineral cannot fit. Existing cargo remains intact. Cases **310–313** reproduce these failures and cover empty, partial, incompatible and full pods. The prior successful-mining fixture now supplies an empty pod; its old incompatible cargo setup had encoded the erroneous docking behavior.

## Remaining investigation

The remake still uses its provisional 16–35 yield and five-day/first-day chance. Original mining is gated by `(ship[+4] - 1) & 3` matching `(clock[$1378E] >> 7) & 3`; clock units and caller cadence need mapping before replacing timing. Pilot effects, SCG availability, Complete Cycle, scan generation and normal campaign behavior also remain open. These cargo fixes do not complete the AMA research task.

Raw traces are under ignored `artifacts/research/supply-pod/ama-{dispatch,mining,clock}-followup.txt`. See [validation](validation-results.md#ama-compatible-cargo--2026-10-02) and the [Windows acceptance brief](windows-agent-brief.md).
