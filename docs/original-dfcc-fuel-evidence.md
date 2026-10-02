# DFCC fuel units and stock cost

Evidence for [task 1215716464570901](https://app.asana.com/1/507237966097081/project/1214891399253076/task/1215716464570901), checked 2026-10-02. The live task requests increased fuel requirements with DFCC; it has no description, comments or subtasks.

## Original instructions

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. The segment at disk `$6E000–$DA9FF` loads at RAM `$13000`. Addresses below are RAM addresses; convert with `disk_offset = $6E000 + address - $13000`. Capstone 5.0.7 was used in M68000 big-endian mode. The image is not committed.

The [DFCC installation flag](original-engine-damage-evidence.md) sets bit 2 of ship byte `+3`. The low bits identify IOS/SCG, giving values 6/7 when fitted. Fuel routines check the interval `[4,8)`, without reading the drone count.

| Path | Instructions and result |
| --- | --- |
| Manual loading `$367AE–$3680A` | `d2=2` gauge units, `d3=2` stock units; fitted DFCC changes `d3` to 20. `$367F2` subtracts stock and `$367F4` adds gauge units to ship byte `+6`. The gauge capacity check remains 250. |
| Manual unloading `$36730–$36788` | `d3=1` normally, 10 with DFCC. `$3676A` removes one gauge unit and `$36776` credits `d3` to stores. |
| Initial fitting `$3301C–$33058` | Repeatedly invokes unloading until ship `+6` is zero, before setting the DFCC flag at `$33052`. Existing cheap fuel is returned before the conversion. |
| IOS/SCG depletion `$238EE–$23936`, `$2399C–$239E4` | Identified normal depletion blocks subtract one gauge unit under the state/clock mask. They do not apply a second DFCC multiplier. Other effects, including interstellar speed changes, have separate fuel writers. |

For independent byte checks, the loading cost selection at `$367B4` is `7402760210280003b03c0004650ab03c0008640470007614`; stock debit/gauge increment at `$367F2` is `9751d00211400006`. The unload amount selection at `$36736` is `7401760110280003b03c00046508b03c00086402760a`. Both sets were checked against the hashed image.

Store selection uses the current store pointer plus `$1E`, or plus `$20` when `$365D0` is set. Bay setup clears that selector for shuttle/IOS at `$325B8/$32614` and sets it for SCG at `$32666`. The remake uses its existing MeH/HeD `FuelType` selection.

## Remake behavior and limits

One DFCC gauge unit therefore represents **ten store units**. The remake keeps `Fuel` as gauge units and derives `FuelUnitCost` from its existing DFCC flag. Manual loading/unloading, ACC and dismantling share that conversion. Drone count does not change the ratio, and ordinary gauge drain is unchanged. First fitting returns the old tank at its old value; insufficient return capacity rejects the whole fitting.

The original manual loader rejects an exactly sufficient stock amount, and unloading checks capacity before adding up to ten. The remake deliberately permits exact payment and prevents overflow/loss. These are conservation corrections, not reproductions of those boundary quirks.

The original auto-refuel block `$33B38–$33B92` transfers stock 1:1. Its DFCC conversion also changes pod/control state at `$330C0`; this inspection does not prove original ACC availability on converted ships. The remake already permits DFCC with ACC, so automatic refuelling uses the same tenfold cost to prevent a cheap alternate filling route. This consistency choice is not a claim that the original ACC route was identical.

Already-loaded tanks in existing saves retain their gauge amounts and range. Future transfers use the corrected ratio, including refunds; a pre-fix tank can consequently refund more stock than was originally paid. No historical debit is reconstructed. Preserve save provenance when comparing inventories. New fitting/refuelling cycles cannot create this conversion gain.

Raw extracts and byte checks are in ignored `artifacts/research/supply-pod/dfcc-fuel-*.txt` and `dfcc-fuel-audit.json`. See [regressions and Windows limits](validation-results.md#dfcc-fuel-cost-and-conservation--2026-10-02). Full original DFCC conversion, automatic-control fidelity and clock cadence remain separate research topics.
