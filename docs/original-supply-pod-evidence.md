# Original supply-pod discard behavior

Evidence for Asana **1215685674676219**, decoded on 2026-10-02 from the original Disk 1 image. This establishes the selected-pod operation; it is not a recorded emulator playthrough.

## Reproduce the lookup

Disk SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`.
The main image occupies disk bytes `0x6E000:0xDAA00`, loaded at RAM `0x13000`. Convert a RAM address with `disk_offset = 0x6E000 + address - 0x13000`. Decode Motorola 68000 big-endian instructions at the stated entry boundaries; nearby data is not executable code. The local disk/probes are ignored artifacts and do not travel with the repository.

## Control and dispatch

- `$340A2–$340BA` derives the selected module from the input selection, adds ship offset `$16` plus twice the module index, and stores its pointer at `$33D1C`.
- The supply display branch `$33E32–$33E48` draws its control and installs input table `$2205E` in `$222B6`.
- That table contains one rectangle and its hover/action IDs: `0001 00F0 0120 0060 0070 00BF 006D`. The rectangle parser `$225EC–$22654` uses 12-byte entries; `$2263A` follows the secondary table. Action `$6D` indexes dispatch base `$212B0`; entry `$21464` contains `$00033F40`. `$2237C–$2238E` performs that dispatch.
- `$403E6` initializes the text-table pointer to `$1C482`. The signed word offset for text ID `$BF` resolves to `$1D307`: **“Ditch Contents !”**. The text lookup is at `$1FB9A`.

## Exact mutation

```text
33F40 207900033D1C  movea.l $33D1C,a0
33F46 3010          move.w  (a0),d0
33F48 0240C000      andi.w  #$C000,d0
33F4C B07CC000      cmp.w   #$C000,d0
33F50 6602          bne     $33F54
33F52 4E75          rts
33F54 3080          move.w  d0,(a0)
33F56 6000FE22      bra     $33D7A
```

The packed word retains only the pod-kind bits. For a supply pod (`$4000`), both resource type (bits 8–13) and quantity (low byte) become zero. Only the selected word is written; neighboring pods, stores, fuel and mining timestamps are untouched. Cryogenic pods (`$C000`) return unchanged. There is no confirmation, timed wait or stock transfer before the write; the final branch redraws the module screen.

By contrast, `$33AE2–$33B36` iterates supply pods, credits their resource quantities to a local store with a 50,000 cap, and resets each word to `$4000`. That separate bulk-unloading routine must not be substituted for this action.

## Availability and remake acceptance

The mutation itself has no docking, ACC or mining check. However, screen entry `$33D7A` special-cases ship state `$08`, then uses `$30B08`; states `$00/$05/$12/$13` route to `$30D12` instead of the pod display. This prevents claiming that the original button was reachable in every state. Full state-name mapping and emulator verification remain open.

The remake follows the task's requested availability through an explicit interior cargo dialog, including docked ships. This is a modern access decision, not proof of identical original navigation. Ditch must affect only the chosen supply cargo, leave the fitted pod and all stores intact, clear the empty item type, preserve journeys/ACC/mining, and survive save/load. Tool/cryo equipment is outside this task. Native Windows acceptance must include mining, transit, docking, empty pods, cancellation and multiple unlike cargo pods.
