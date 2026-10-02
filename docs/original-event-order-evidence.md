# Original simulation order

Asana **1215685674676225**, checked 2026-10-02. This establishes static call order and identifies remake differences. It does not claim an instrumented original-game run or completion of the task's simultaneous-event acceptance.

## Reproduce

Use Disk 1 SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. RAM `$13000` maps to disk `$6E000`; decode big-endian M68000 instructions with Capstone 5.0.7. The contiguous master function is `$23CA8–$23E4C`, ending in `RTS`; `$23E4E` starts a different function. Local extract `artifacts/research/event-order/master-and-subsystems.txt` has SHA-256 `2546151053a945c100ab3e149058810c2080af2503513391a460db9ee8eba3a7`.

Useful separate entry points are `$230CE`, `$231E0`, `$235E2`, `$2364A`, `$2376C`, `$2384E`, `$3684A` and `$22BB4`. Starting at a nearby arbitrary byte can decode an operand as an instruction; do not use such output as evidence.

## Consumed simulation update

`$23CA8` clears `$38CBA`, then returns unless pending-clock flag `$20291` is set. An optional countdown at `$1C397` is handled first (`$23CBA–$23CD0`). The following calls are ordered by their instruction addresses:

| Call site | Target | Established role or limit |
| --- | --- | --- |
| `$23CD6` | `$22E86` | The three training pipelines; see the staff evidence. |
| `$23CDA` | `$230CE` | Earth mineral survey/extraction, including alternating accumulator gates. |
| `$23CDE` | `$231E0` | Local mineral survey/extraction through `$23234`. |
| `$23CE2` | `$79E7A` | In this disk image, jumps to an `RTS` at `$79E1C`. Runtime patching has not been excluded. |
| `$23CF2` | `$235E2` | Ground then local factory production through `$232E0`; this includes the separately traced SDM/MTX completion branches. |
| `$23CF6` | `$2364A` | Research: team/rank checks, progress accumulator, completion and promotion. |
| `$23CFA` | `$2376C` | Staff attrition, when its separate gate is set. |
| `$23CFE` | `$2384E` | Ship updates, starting with shuttles, then IOS records; includes action completion and AMA dispatch. |
| `$23D02` | `$3684A` | Automatic stock conversion/refining; arithmetic and batching below. |
| `$23D08` | `$38A56` | Methanoid scheduling. |
| `$23D0E` | `$38CBC` | Methanoid per-slot actions. |
| `$23D14` | `$21140` | Checks current station/screen and can redirect to station information. |
| `$23D1A` | `$39A0C` | Later special-team/ship processing; full story semantics are not established here. |
| `$23D20` | `$37808` | Conditional story/progression dispatch; not a claim that every story event fires. |
| `$23D28` | `$35E02` | SDM simulation-step expiry. |

There is an early return at `$23CEE–$23CF0` when `$38CBA` is nonzero after the first four calls. These are ordered calls, not a promise that every subsystem always runs.

After SDM, `$23D2E–$23E26` prioritizes further conditional events. It first conditionally calls `$39878`; `$38CBA` or `$1C396` can then skip to the tail. The subsequent priority is `$1C2D6` → `$376AE`, `$1C2D0` → `$37678` (SDM discovery), `$1C2D2` → `$3768A`, `$1C2D4` → `$3769C`, followed by countdown/event branches to `$376E2`, `$376C2`, `$37636`, `$37E70` and `$3771A`. A taken event branch exits to the common tail, so simultaneous pending discoveries do not all produce a bulletin in that pass. Counter branches can fall through while still nonzero.

At `$23E28`, nonzero screen selector `$22D34` invokes its callback through table `$22D36`. `$23E40` calls `$22BB4`, which refreshes the date and clears `$20291`. Thus the clock acknowledgement follows model updates and the selected callback. The callback bodies still need inspection before claiming that all original screens are passive renderers.

## Refining is a separate phase

`$3684A` is not an unidentified combat routine. It reads progression `$1A354`, updates one phase byte `$36848` **once per call**, and processes alternating local-record indices (stride twice `$F6`). The Earth branch runs on one parity. Local processing skips record `+$F1` bit 7 and station types below 8.

The exact stock arithmetic is visible independently of resource-name mapping:

- Earth: one path checks two inputs at least 3, subtracts 2 from each and adds 3 output; a later progression-gated path subtracts 2 each and adds 2.
- Orbital record: inputs at `+$42/+$46` lose 6 each and output `+$54` gains 8; the later path uses `+$44/+$48`, consuming 5 each and adding 5 at `+$56`.
- Ground record: when `+$F0 >= 3`, inputs `+$B8/+$BC` lose 2 each and output `+$CA` gains 3.

These paths have distinct stock thresholds/caps. Do not replace them with one universal recipe multiplier, or infer exact fuel names from offset alone. Complete the name/technology mapping before altering conversion ratios.

## Remake comparison and implementation boundaries

At `55316b6`, `GameCore` registers Unlocker → planets → production → ships → research → attrition → enemy drones → MTX, then notifies display observers. `Earth.DayTick` mines before training. `Production.UpdateProduction` also refines fuel inside each factory iteration and toggles the shared item's `AutoProduceFlip` only when that factory has ingredients.

Differences requiring targeted work:

1. Training precedes mining in the original; the remake's Earth method reverses these operations.
2. Research and attrition precede ship processing in the original. Moving these calls needs same-tick crew, promotion and arrival checks, not only a delegate-order assertion.
3. Refining follows ships in the original. Currently, an ACC fuel wait can see fuel made earlier in the same update. Separating refining also requires preserving its phase in saves and resolving the shared-item/per-factory toggle mismatch.
4. Original pending discovery flags form a priority chain. Immediate event callbacks in the remake can compete for the current screen; a general bulletin queue needs producer and interruption tests.
5. SDM simulation expiry follows ships and enemy actions, while its separate real-time consumer `$23E4E` uses `$20290`. A single day counter cannot stand in for both paths.

This is evidence for the integration work, not a reason to reorder isolated handlers while retaining incompatible timing. The pending fractional/interstellar-clock decision also affects what constitutes a consumed update.

## Acceptance still required

Instrument one original update with training graduation, extraction, production, research completion and ship arrival due together. Record stores, crew/rank and pending news before/after each mapped call. Repeat an ACC arrival with insufficient fuel before refining, simultaneous discoveries, and an armed SDM on an arrival update. Compare ordinary and accelerated time, including early-return paths. Add deterministic remake scenarios for those outcomes and then verify them in source and exported Windows builds. No new task is counted complete by this research alone.
