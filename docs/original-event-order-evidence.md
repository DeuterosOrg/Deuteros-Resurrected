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
| `$23CE2` | `$79E7A` | MTX transfer after the active Disk 2 overlay replaces this initial `RTS` stub; see the overlay trace below. |
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

These paths have distinct stock thresholds/caps. The [refining follow-up](original-fuel-refining-evidence.md) now maps the mineral names, research/resource-count gate and all five batch rules, and reproduces starvation caused by the remake's shared per-factory toggle. The isolated refining correction at `f913679` now preserves saved phase and allocation, including construction/capture relocation, and passes all 450 Mac regressions. Windows execution and the remaining scheduler/clock work are outstanding.

## Remake comparison and implementation boundaries

At `55316b6`, `GameCore` registers Unlocker → planets → production → ships → research → attrition → enemy drones → MTX, then notifies display observers. `Earth.DayTick` mines before training. `Production.UpdateProduction` also refines fuel inside each factory iteration and toggles the shared item's `AutoProduceFlip` only when that factory has ingredients.

Differences requiring targeted work:

1. Training precedes mining in the original. The simulation-order branch now runs active-world training once, then Earth extraction, then local extraction; the Earth override no longer repeats training.
2. Research and attrition precede ship processing in the original. The simulation-order branch now preserves that sequence. Cases 451–452 reproduce the old order through actual completion/arrival behavior and cover graduates, extraction, factory completion, research promotion, crew countdowns, frozen cryopods, ACC arrival/departure and ordered promotion News.
3. Refining follows ships in the original. Corrected at `f913679`: a saved global phase and station allocations replace the per-factory toggle. Case 449 uses the actual simulation update to prove that ACC cannot consume newly refined fuel until the following update.
4. Original pending discovery flags form a priority chain. Immediate remake callbacks could overwrite a bulletin or replace a locked screen. The delivery correction below preserves notices; original priority remains separate work.
5. SDM simulation expiry follows ships and enemy actions, while its separate real-time consumer `$23E4E` uses `$20290`. A single day counter cannot stand in for both paths.

This is evidence for the integration work, not a reason to reorder isolated handlers while retaining incompatible timing. The subsequent fractional-clock integration consumes one update per centiday or manual whole-day increment; independent interstellar clocks remain pending.

## Acceptance still required

Instrument one original update with training graduation, extraction, production, research completion and ship arrival due together. Record stores, crew/rank and pending news before/after each mapped call. Repeat an ACC arrival with insufficient fuel before refining, simultaneous discoveries, and an armed SDM on an arrival update. Compare ordinary and accelerated time, including early-return paths. Add deterministic remake scenarios for those outcomes and then verify them in source and exported Windows builds. No new task is counted complete by this research alone.


## Validated simultaneous-event correction

The isolated `codex/simulation-order` branch reproduces research completion observing an already-departed shuttle (451) and arrival seeing its pilot before attrition (452). Moving research and attrition ahead of ships corrects both. A strengthened 451 also reproduces Mercury mining before graduation when only Earth's method is reordered; resolving active-world training before all mining corrects that path. Cases 42–44 guard world replacement and duplicate updates. All these focused checks, 449–450, 350/357–358, and native 451–452 pass. Full Mac validation at `08eda4d` passes 452/452, nine Python checks, strict import, startup and audited Windows cross-export; integrated into the local contribution branch. Windows execution remains outstanding.

Evidence: `artifacts/validation/evidence/simulation-order/{red,training-red,green}/`. The initial 451 fixture had an invalid ACC cursor and was stopped; its log is retained under `setup/invalid-cursor-451.log`, separate from the valid red ordering reproductions. This change preserves the current update unit. The later fractional-clock batch preserves this order. Independent star clocks, original discovery priority, all early-return paths and original-runtime/Windows comparison remain open.

## Bulletin delivery correction

Cases 473–474 reproduce actual production and research completing in the same update and the second bulletin replacing the first, plus a discovery replacing a screen owned by another input lock. The existing News model now saves pending bulletin IDs. The shared display guard preserves active bulletins and modal owners; deferred requests are delivered one at a time, in request order, on an available update. Repeated pending requests coalesce. News replay explicitly replays its selected report without consuming another discovery. Follow-up failures reproduced new requests overtaking saved notices and replay consuming the wrong notice before those paths were corrected.

Focused 473/474/126/128/199/427/432/462 and native 473/474/128/462 pass. Checks include saved delivery, old saves with no pending field, rejected null/unknown/duplicate IDs, input ownership, real producers and replay controls. The native screenshot was inspected. Evidence: `artifacts/validation/evidence/bulletin-delivery/`; `setup/invalid-headless-screenshot.log` is a harness configuration mistake, not a gameplay failure. Full aggregate and Windows results are recorded separately. New saves include the pending list and require this or a newer build; preserve backups for older builds.

This correction preserves the remake's current producer order. It does not claim the original flag-priority scheduler: initialized text-table decoding maps `$1C2D6/$1C2D0/$1C2D2/$1C2D4` to **Blaser → SDM → MTX → Hyperlight**, with descriptor words `[9,0,9]`, `[18,0,4]`, `[23,0,5]`, `[22,0,8]`. Exact decoded descriptors are in `artifacts/research/event-order/discovery-priority-descriptors.json`. Implementing that priority alongside the original deferred unlock semantics remains open; fractional-clock integration alone does not implement that priority.


## Named subsystem coverage — 2026-10-03

A fresh read-only task lookup confirms seven explicitly named areas: current screen, AMA, ACC, ships, resource extraction, MTX and menus. It asks for source investigation; the broader runtime comparisons above remain verification work, and unresolved source mappings below still prevent accepting this research task.

| Requested area | Established order and remaining limit |
| --- | --- |
| Current screen | The selector at `$22D34` indexes the callback table at `$22D36`; the master calls the selected entry at `$23E3E`, after its model/story branches and before date acknowledgement at `$23E40`. Selector zero skips the callback. Early returns can skip this tail. |
| AMA | Inside the IOS portion of `$2384E`, scanning state `$0C` dispatches to `$23AF8`; mining state `$0D` dispatches to `$23C14` at `$23952`. These are ship-update branches, not a separate screen-driven mining tick. [Phase and yield details](original-ama-mining-evidence.md) remain separate. |
| ACC | Automatic completion is nested in ship action dispatch, including arrival/unload/disengage and next-departure handling. The [Complete Cycle trace](original-acc-cycle-evidence.md) identifies `$31492 → $33AE2` then `$31498 → $33CAE`. There is no separately established global ACC pass between mining and ships. |
| Ships | `$23CFE → $2384E` follows research and attrition and precedes refining. The updater processes shuttle records, then IOS records; independent interstellar time remains separately documented. |
| Planet extraction | Earth `$230CE`, then local `$231E0`, precede production. Training `$22E86` runs before both. |
| MTX | The active Disk 2 overlay resolves `$23CE2 → $79E7A → $7CE0A` to transfer/balance processing: after extraction, before production/research/ships. Screen callback 8 separately resolves to `$7CDBC`. Initial Disk 1 stubs are inactive implementations, not the full running program. |
| Menus | The master can redirect station/screen state through `$21140` before later story events, then calls the selected screen callback and date refresh. This establishes those boundaries, not a single universal menu-only pass or the identity of every screen helper. |

The callback table has nineteen nonzero selector entries in the region ending before `$22D86`. Selectors 9/14/17/19 point to the bare `RTS` at `$23838`; selector 7 points to `$30490`, which reads the selected ship pointer `$19D30`, detects state changes and calls shared UI helpers. The entries and direct immediate selector writes are retained in `artifacts/research/event-order/screen-callbacks.json`. Linear entry excerpts and the specific branch below are in `screen-callbacks.txt`, SHA-256 `9f3409d75d9ab105033b333bb290cebc21d289a54cbb13ff1b4debc2ffcb3550`. They are entry excerpts, not complete control-flow graphs. The original disk hash and address mapping remain those above.

**Screen callbacks are not proven read-only.** Selector 3 enters `$24868`, reads the current record pointer from `$19D16`, and can call `$248A6`. When `$24700` is nonzero and record byte `+1` is zero, `$248BE–$248C2` writes the default value `2` into that byte before calculating its item-table address. This narrow conditional write disproves treating every original callback as a passive renderer; it does not establish an additional extraction, production or ship simulation pass.

At remake runtime `06d09c3`, `GameCore` explicitly runs unlocks → active-world training/extraction → production → research → attrition → ships → refining → enemy production → MTX → rogue crews → SDM; public display observers follow, then pending-story delivery and menu refresh. The MTX position conflicts with the newly resolved original overlay order below. Correcting that position requires a failing simultaneous transfer/production regression; it is not included in the frozen `06d09c3` grapple candidate. Original discovery priority and broader screen-specific side effects remain separate boundaries.


## MTX overlay resolves the missing transfer phase

The initial main image is insufficient for this subsystem. Selector `$385DC` compares gate `$1A176` with the installed-overlay marker `$38092` and requests mode 0 or 1 through `$38840` when they differ. The loader first checks Disk 2 identifier `$8B632804` using `$3880A`, disables the timing callback, then sets destination `$79E1E`, length `$5800` and source offset `$1B800` (mode 0) or `$21000` (mode 1). `$208C0` copies that disk region; `$38880` records the mode and `$38886` reinstalls timing. Loader register roles follow the reader: `d0` is length, `d1` destination and incoming `d7` becomes source offset `d2`.

Disk 2 is the existing pinned image SHA-256 `99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a`. Mode 0 overlay SHA-256 is `6d7dbce7e3ec2eb920ba1440b4700e262081ac3cf6d3ca5a558514e20f26b91c`; mode 1 is `df04433b4dc420b8f1c500981fd8d4873bc8ef42134effadfb2aa2095b4217c2`. In mode 1, `$79E7A` contains `4ef90007ce0a`, while screen callback `$79E74` contains `4ef90007cdbc`. Both mode 0 entries still point to the `RTS`. These are the loaded addresses, not addresses obtained by applying the main-image mapping to overlay bytes.

`$7CE0A` checks the MTX research byte `$1A1EE` (item 24 record `+2`), refreshes eligible item mappings through `$7C6CC`, then walks local records at `$13810` with stride `$F6`. It skips hostile records (`+$F1` bit 7), requires station type 8 and the installed MTX bit (record `+$36` bit 6), resolves the saved target, and selects an enabled item from its rotating cursor/bitsets. `$7CEC0` calls send routine `$7CEE8`; `$7CEC8` calls balance routine `$7CFF6`. The mineral send path caps destination stock at 50,000 and keeps overflow at the source; the balance path splits total stock with an odd remainder retained by the source. These independent stock writes and the installed flag establish MTX transfer identity.

Consequently the active original order is **training → Earth extraction → local extraction → MTX → production → research → attrition → ships → refining**, subject to the already recorded gates/early return. A destination factory can use MTX-delivered materials in that same consumed update; an item newly completed by production cannot be sent by the earlier MTX phase until a later update. The current remake reverses both dependencies. This is instruction-derived behavior, not a measured original-runtime observation.

Reproduce with `artifacts/research/event-order/trace-mtx-overlay.py` using the two pinned local disks and Capstone 5.0.7. It checks disk hashes and dispatch bytes and writes the aligned loader, dispatcher, send and balance traces. Trace SHA-256: `bbaca3f7e5adce2c16d5b72e442d65020e806d6d9069aa1c77aecab4b1f19cc6`; machine-readable facts are in `mtx-overlay-facts.json`. The next implementation check must exercise both material-arrival and completed-output dependencies through the actual master update.
