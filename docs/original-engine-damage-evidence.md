# Original engine damage and recovery

Evidence for [Asana task 1216073204565506](https://app.asana.com/1/507237966097081/project/1214891399253076/task/1216073204565506), checked 2026-10-02. The live task identifies wartime arrival at an occupied planet and being in orbit when a planet is attacked; it has no recovery details, comments or subtasks.

## Reproducible source

The original Disk 1 image has SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. The segment at disk offsets `$6E000–$DA9FF` loads at RAM `$13000`; addresses below are RAM addresses. Convert with `disk_offset = $6E000 + address - $13000`. Disassembly uses Capstone 5.0.7, M68000 big-endian mode. The image is not committed.

Raw extracts are retained locally in `artifacts/research/supply-pod/engine-damage-followup.txt` and `engine-hostile-state.txt`. Read instructions from the stated entry points: decoding an arbitrary earlier word can start in an operand and produce misleading instructions.

## Damage and escape

The engine byte at ship offset `+7` holds type in bits 0–5, engagement in bit 6 and damage in bit 7. At `$306F6–$30718`, the display checks absence, then damage, then engagement. Text ID `$30` resolves to “Damaged !”; IDs `$CD/$CE/$CF` resolve to the other three statuses. Damage is distinct from an absent drive.

The identified damage write is inside the departure routine `$30EA8`, reached from the engine-engage action. Normal states `$03/$0E/$0C` bypass damage. State `$14` enters the danger branch at `$30ECC`. Ship byte `+3 >= 4` bypasses its roll; otherwise the code calls `$1F42C`, masks the result with one, and sets engine bit 7 only when the result is zero. The instruction sequence at `$30EDC` is:

```text
10280003 b03c0004 6412 4eb90001f42c 02000001 6606 08e800070007
```

This is a binary random-result gate on escape, not an unconditional damage write on arrival. The actual random distribution has not been measured. DFCC installation explains the protection: original item `$14` maps to text `$115` (“D.F.C.C.”), and its installation path sets bit 2 of ship byte `+3` at `$33052` (`002800040003`). The hull's low two bits remain intact.

Both reported danger contexts have corresponding state-entry code:

- Arrival at `$311EE–$312B8` checks war state, excludes shuttle/enemy records, checks hostile destination data, then writes state `$14`, countdown 3 and expiry selector `$11` at `$3124E–$3125E`.
- The attack path at `$38D7E–$38DF2` scans IOS and SCG records in state `$03` at the affected body and writes the same state/countdown/selector.

Neither of these identified entry blocks writes the damage bit. Selector `$11` at table `$237D8` resolves through `$2383A` to `$3145A`, which dispatches hull removal. The IOS tick at `$2396A` decrements the stored countdown and calls that selector at zero. Do not equate this original count with three remake days without verifying clock cadence. These paths do not prove that no other damage writer exists elsewhere in the image.

## Travel and replacement

Engagement at `$30E2C–$30E48` rejects only an absent engine byte; it preserves the damage bit. After computing the journey duration, `$30F32–$30F3A` doubles it when damaged and stores the result at ship `+8`. ETA rendering repeats that multiplication at `$35268–$35270`. Both checks have bytes `0828000700076702e348`.

The bay fitting routine `$3290C–$329D8` rejects replacing a healthy installed drive, but allows replacing a damaged one. The table at `$32904` is `00000003000b000d`: hull drive items 3, 11 and 13, whose names resolve to S Drive, I Drive and Star Drive. The routine checks availability, overwrites the engine byte with the new type at `$329A8`, and consumes a store part at `$329CA`. Overwriting clears damage and engagement; no usable old-drive credit appears on this replacement path.

Removal at `$3331E–$33356` clears the engine byte, tests the old damage bit, and branches past the inventory-credit code when damaged. Healthy removal masks the type, looks up the same table and credits stores through `$240E4`. A damaged drive must therefore not become a usable spare through dismantling.

## Implementation boundary

The implementation now follows the traced escape rule, with separate persistent damage, a warning, doubled travel, replacement costing one matching local drive, and no damaged-drive salvage. Cases 277–293 cover both task contexts, protected/enemy/peaceful ships, both binary outcomes, save/load, ETA/arrival, unavailable spares and dismantling capacity. See [validation and limitations](validation-results.md#engine-damage-and-recovery--2026-10-02). The remake's random generator does not reproduce the original random sequence, and existing attack/destruction timing is unchanged. Task-owner review of the task's shorthand, normal campaign progression and native Windows acceptance remain required.


## Hostile docking gate — 2026-10-04

A staged arrival with 92 station defenders reproduced immediate docking while `AttackedCount` was still zero. ACC also calls `Ship.Dock()` directly, bypassing the former screen-only counter check. The shared docking entry now checks current defenders and attacking fleets. A player hull also needs DFCC conversion to enter a hostile wartime station; peaceful trading, enemy and rogue access remain available. An active station SDM suppresses its defence gate. Cleared stations accept docking even before an old danger counter refreshes.

The same pinned Disk 1 provides the source: arrival `$31564–$315B2` branches immediately to `$311EE`; `$311EE–$312BE` checks station defenders at record `+$F2`, bypasses danger for a nonzero SDM byte, and otherwise assigns state `$14`. Manual docking `$30C7C–$30CEA` accepts only states 3/`$10`. The docking lookup `$310EC–$31144` rejects an unconverted player hull at a wartime hostile station. Extracts are retained in `artifacts/research/docking/`; `manual-dock-and-danger.txt` starts at the aligned `$30C36` entry. The friendly attacking-fleet gate preserves existing remake intent; its docking interaction is not independently established by these extracts.

Cases 628–629 and 75 related cases pass strict headless checks. They cover IOS/SCG, manual pointer input, ACC, cleared defenders, active SDM, conversion, peaceful/enemy access, friendly fleet attacks and preserved fuel/timestamps. The existing SDM capture fixture now supplies the required DFCC hull. Both the initial bypass failure and an over-restrictive SDM candidate failure are retained under `artifacts/validation/evidence/hostile-docking/`. This is staged verification, not normal station-capture or Windows acceptance. Combat-window command ownership is a separate follow-up.
