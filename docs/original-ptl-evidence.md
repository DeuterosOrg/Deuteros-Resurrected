# Original PTL discovery and installation

## Source and status

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. Addresses below are original RAM addresses; raw disk offset is RAM plus `$5B000`. Capstone 5.0.7 M68000 extracts are retained under ignored `artifacts/research/ptl/`: `install-discovery-and-enable.txt` and `cockpit-control-and-stock-event.txt` (latter SHA-256 `43662279c269b4fab407eb01fe6610b05cd23d7fcfa3b2526fc543936ecd1089`). These are static source findings. Discovery, the captive-colony stock event and cockpit fitting are implemented below; normal campaign and Windows acceptance remain open.

## Permanent cockpit fitting

Cockpit rendering `$31CBC–$31DC6` chooses menu `$21744` when the hull code is at least 3, its bit 2 is set, and research byte `$1A266` is at least 100. Fitting DFCC (original item index 20) sets that conversion bit at `$33052` and copies it to the cached bay hull at `$33068`. The PTL menu uses the same control rectangle as ordinary ACC fitting. Its text `$18A` resolves to “Fit Torpedo Launcher”; action 112 at `$2175C` dispatches through `$212B0` to `$33756`.

The handler checks ship byte `+0F`, bit 7. An existing installation reports a duplicate without spending stock. Otherwise it requires item index 26, sets the permanent flag and debits one unit through `$2415C`. It does not replace a tool pod. The normal hull tool masks correctly exclude this item.

In the remake, the recipe is `prejudice_torpedo_launcher` (enum 37, Research.Index 27), not the unused `p__t__l` alias. The existing `AddACC` control now switches to permanent PTL fitting on DFCC-converted IOS/SCG hulls after completed research. It consumes one local launcher and sets `InterStellarShip.PTL`, preserving pods, cargo, drones, fuel and ACC settings. Duplicate/no-stock attempts use existing error feedback. Input locks, stale/departed hulls, hostile ownership and invalid stations reject retained callbacks. Ordinary ACC fitting remains available on unconverted hulls.

## Original fitting control artwork

The cockpit loads icon 19 for PTL (`$31D84`) and icon 27 for ACC (`$31DA2`) through `$41ABE`. That routine selects bitmap bank 53 at `$41FAA + $D4`; its source begins at `$621D6`. Each control is **24×16**, using a 64-byte row stride, twelve bytes per icon column and `$400` bytes per icon row. This bank is read directly by the blitter; it must not be fed through the compressed illustration decoder.

The extracted ACC entry matches **all 384 pixels** of the supplied `Sprites/Buttons/Shipbay/ACC.png`, with a unique mapping for each of its five palette indices. Applying that same supplied palette to PTL preserves the existing bay colours and opaque black background. PTL indexed-pixel SHA-256 is `816e35b5c694a0f799025f794d5512cf431f51a0c2c90770884c0e6e5d0c359b`; decoded PNG SHA-256 is `1051d3d6763e9ff1bed0d47a895898454803c1db3e88b36a539a269341809ba9`.

Reproduce with `python3 artifacts/research/ptl/extract-cockpit-icon.py` (Pillow and the pinned local Disk 1 required). The script, `cockpit-sprite-dispatch.txt`, `cockpit-control-art.json` and `fit-torpedo-launcher.png` remain local research evidence. The icon is wired into gameplay as `Sprites/Buttons/Shipbay/PTL.png`; original display colour calibration remains separate.

## Discovery shares a captive-colony event

Dispatcher `$37808–$37896` consumes its active delay before observing a changed hostile-system count. Count 6 selects delay 40 and handler `$37AD8`. That handler decrements a separate nonzero cooldown; when zero, it resets it to 79 and draws an ordinal from 1–32. It scans 98 station records for type `+EE = 8` and local state `+F0 = 5`: completed human stations with a captive colony. The latter state is identified by the [station-text and recovery trace](original-self-destruct-evidence.md#ground-services-and-rebuilding).

If that ordinal is not found, `$37B24` discovers PTL only while its research is still zero. Otherwise the event draws mineral index 0–3 (iron, titanium, aluminium, carbon). A missing bit in the colony's availability mask `+90` takes the same discovery branch. A present bit adds 10,000 to that mineral's **ground stock**, capped at 50,000, then publishes bulletin 17. The destination `+AE` is confirmed by the ordinary extraction path `$23204–$2323E`; it is not a new ore vein or survey completion.

Discovery `$37772–$377DE` uses descriptor `(26, 0, 21)`, exposes research at progress 1 and preserves existing progress. Consequently, unlocking PTL immediately after Sol capture or merely observing six hostile systems would skip original conditions.

## Implementation boundaries

The shared research dispatcher now covers count 6 as well as count 7, including the saved 40-pass delay and 79-pass colony cooldown. SDM defusal records captive colonies; completed ground repair or station destruction clears that state. Eligible colonies retain the existing original station-slot order. Successful mineral events credit ground stores and queue a saved Mining Dump notice; unsuccessful selection unlocks PTL and queues Eureka without granting completed research. Events continue after Hyperlight and PTL research.

Older saves lack reliable captive-colony history: their new flag defaults false, without fabricating previous captures, stock gifts or notices. Future captures establish eligibility normally. Existing PTL installations and Hyperlight delays survive loading; malformed or explicit-null new state is rejected. New-format saves should not be loaded into older builds.

Cases 605–608 cover actual day progression, stock/cooldown boundaries, bulletin replay, capture/repair/loss and save compatibility. See [discovery validation](validation-results.md#ptl-discovery-and-captive-colony-stock-events). Cases 609–610 cover pointer fitting on both hulls, duplicate/ownership/input gates, unchanged inventories, reload and subsequent battle use with staged prerequisites. See [fitting validation](validation-results.md#permanent-ptl-cockpit-fitting). Windows-owned Service, popup, Torso scene and Stocktaker patches remain separate. Combat boundaries are documented in [battle lifecycle](battle-lifecycle.md#empty-fleets-and-original-ptl-boundaries--2026-10-04). These checks do not establish the complete normal discovery → research → paid manufacture → fitting → battle campaign.
