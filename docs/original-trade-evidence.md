# Original Methanoid trade rules

Verified **2026-10-02** for Asana **1215691951441128**, against the original Disk 1 image, independently of the parallel port's implementation. This establishes rules for the accept/decline flow. The decision, counter and exchange paths now have local regression evidence; original timeout behavior and native Windows acceptance remain incomplete.

## Source and reproduction

Disk SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. The image at disk offsets `0x6e000..0xdaa00` loads at RAM `0x13000`; a RAM address maps to disk offset `0x6e000 + address - 0x13000`. Local source: ignored `artifacts/research/mtx/disk1.adf`. No original binary is committed. Capstone 5.0.7 M68000 disassembly is retained under ignored `artifacts/research/trade/encounter-disassembly.txt`.

The [archived encounter decode](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/7BE24_encounter_handler.md) supplied addresses, which were checked against the bytes. Several prose conclusions in that note and the later port notes need the corrections below.

To check the central instructions using Python alone, supply the same local disk image:

```python
from pathlib import Path
import hashlib
disk = Path("artifacts/research/mtx/disk1.adf").read_bytes()
assert hashlib.sha256(disk).hexdigest() == "6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38"
checks = {
    0x7BE24: "0c79ffff0001bf34",  # compare counter with war sentinel
    0x7BE64: "b07c0010640001ba",  # compare 16; unsigned >= branches to war
    0x7BF72: "30390001bf34670653790001bf34",  # decline: skip zero, subtract one
    0x7C240: "52790001bf34",      # acceptance: add one
    0x7BE02: "33fc00120001bf34",  # expired decision timer: assign 18
    0x7C034: "33fcffff0001bf34",  # war branch: assign sentinel
    0x7C11C: "000e05040302080906070d0c0b0a010f10",  # exchange table, IDs 0–16
}
for address, expected in checks.items():
    start = 0x6E000 + address - 0x13000
    assert disk[start:start + len(expected) // 2].hex() == expected, hex(address)
print("Original trade instruction/table checks passed")
```

## Counter, decision and cargo behavior

| Path | Original instructions | Required behavior |
| --- | --- | --- |
| Already at war | `$7BE24` compares `$1BF34` with `0xFFFF`, then exits | No further peaceful exchange or repeated war gift. |
| Threshold | `$7BE64..68` compares with 16 and uses unsigned `BCC` | Counts **16 or higher** enter the war path; equality alone is insufficient. |
| Accept | `$7C240` adds one after cargo exchange | Advance the counter once per accepted encounter. |
| Explicit decline | `$7BF72..7A` tests zero, otherwise subtracts one | Preserve cargo; decrement the counter with a floor of zero. |
| No eligible cargo | `$7BE90..9A` branches to the common ending | No exchange and no counter change on this path. |
| Decision timeout | `$7BDCC..7BE0A` decrements a loop counter, displays response 14 at expiry, sets it to `-1` and assigns trade count **18** | This differs from explicit decline. The loop resumes; war is tested on the next encounter entry. Real-time duration remains unverified. |

The input loop has **two actionable selections**: selector 1 branches to accept at `$7C136`; selector 2 branches to decline at `$7BF58`; selector 0 keeps waiting. The older note's “three menu options” wording mistakes an inactive region for a third action. Mapping modern Escape/right-click to refusal would be an explicit UI choice, not a recovered third original action.

The handler examines three packed cargo words starting at ship offset `+0x16`; eligible words have `(word & 0xC000) == 0x4000` and a nonzero low byte. The old note calls these “armed weapons”, but the code alone establishes the encoding test, not that interpretation. `$7C24A` replaces only bits 8–13 through the table, preserving `word & 0xC0FF`, including the quantity byte.

Using this repository's mineral IDs, the table matches the existing seven pairs: iron/silica, titanium/copper, aluminium/carbon, hydrogen/methane, deuterium/helium, palladium/gold, platinum/silver. IDs 15 and 16 map to themselves. Do not turn unchanged fuel IDs into a different exchange, or infer support for equipment outside this table. Acceptance at `$7C150` writes `250` to ship byte `+6` (bytes `117c00fa0006`). The independently traced [manual fuel routines](original-dfcc-fuel-evidence.md) identify that field as gauge units: `$367F4` adds loaded fuel there and `$3676C` writes the decremented gauge during unloading. This establishes a full-tank gift on acceptance, without consuming player stock.

## Correction to the later research summary

[M2 Addendum 2](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/docs/m2-findings.md#L81-L113) observes an all-accept campaign reaching war. This disproves “only refusals advance the counter”, but does **not** prove refusals increment it. Its port change to count both outcomes conflicts with the verified decline instruction. Use the original increment/decrement paths above for this remake.

## Implementation and acceptance still needed

`ShipInterior.cs` now offers Accept/Decline through a modal cargo preview. Acceptance exchanges supported supplies, fills the 250-unit fuel gauge and increments once; refusal leaves cargo unchanged and decrements with a zero floor. The war gate uses >=16 and `AtWar` prevents another peaceful encounter. Empty/unsupported cargo cannot advance the count. Original `0xFFFF` state is represented by the remake's existing `AtWar` flag rather than changing the save format.

Cases 220–235 cover decisions, all table entries 1–16, quantities, repeat input, stale cargo, pointer/keyboard input, interrupted typing, scene exit and the war boundary. Case 150 now explicitly accepts through the control after normal comms research/manufacture/fitting. Cargo and counter settle together before response playback. Escape or external overlay dismissal abandons the choice without a decision or launch; this is a modern UI behavior, distinct from explicit Decline. The choice pauses the simulation; no original-timeout parity is claimed.

The fuel gift was added after the field was identified; strengthened cases 220 and 233 reproduced the missing refill before correction. All 16 trade cases pass afterwards, including no gift for unaccepted, refused, ineligible, stale or interrupted pre-decision offers. An accepted response interruption preserves the committed gift.

Before claiming full original parity, establish decision timing and how the original three cargo words correspond to this remake's IOS/SCG layouts. Keep those evidence gaps separate from implementation and native Windows acceptance; do not invent an arbitrary timeout or silently spread the exchange to unsupported modules.

## Decision-loop follow-up — 2026-10-03

The expiry counter is initialized to **200,000** (`$30D40`) at `$7BEE2`, then decremented once per input-loop iteration by `$7BDCC`; expiry occurs on the following call with zero. This is **not a count of display frames or game-clock units**. The companion idle-animation function `$7BD76` usually decrements its own counter and returns. Only its occasional redraw path calls `$1F452` with 4; that helper waits until the separate counter `$20284` advances by more than 4. Rendering also waits on the blitter. Converting 200,000 directly to seconds at 50 Hz would therefore be wrong; CPU speed, input and redraw work affect elapsed time.

Expiry writes trade count 18 and returns to the same input loop: the existing Accept/Decline branches remain reachable. It does not immediately launch war or automatically close the choice. The next encounter's threshold check starts the war branch. Reproduce that behavior in an original runtime before choosing a modern timer; an arbitrary countdown that disables both choices would introduce another divergence.

The selected-module address calculation at `$340A2–$340BA` is `ship + $16 + 2 * moduleIndex`. This identifies the trade handler's three words as the first three module positions. The six-mount SCG has additional positions, so the remake's all-eligible-module exchange is broader than the literal original loop. Whether to preserve that original restriction remains a compatibility decision; this trace does not justify silently changing existing six-mount save behavior. Exact bytes/disassembly are retained in `artifacts/research/trade/timer-and-module-followup.txt`. No runtime behavior was changed by this follow-up.
