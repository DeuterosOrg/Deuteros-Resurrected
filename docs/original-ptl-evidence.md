# Original PTL discovery and installation

## Source and status

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. Addresses below are original RAM addresses; raw disk offset is RAM plus `$5B000`. Capstone 5.0.7 M68000 extracts are retained under ignored `artifacts/research/ptl/`: `install-discovery-and-enable.txt` and `cockpit-control-and-stock-event.txt` (latter SHA-256 `43662279c269b4fab407eb01fe6610b05cd23d7fcfa3b2526fc543936ecd1089`). These are static source findings; normal PTL progression is not yet implemented or accepted.

## Permanent cockpit fitting

Cockpit rendering `$31CBC–$31DC6` chooses menu `$21744` when the hull code is at least 3, its bit 2 is set, and research byte `$1A266` is at least 100. Fitting DFCC (original item index 20) sets that conversion bit at `$33052` and copies it to the cached bay hull at `$33068`. The PTL menu uses the same control rectangle as ordinary ACC fitting. Its text `$18A` resolves to “Fit Torpedo Launcher”; action 112 at `$2175C` dispatches through `$212B0` to `$33756`.

The handler checks ship byte `+0F`, bit 7. An existing installation reports a duplicate without spending stock. Otherwise it requires item index 26, sets the permanent flag and debits one unit through `$2415C`. It does not replace a tool pod. The normal hull tool masks correctly exclude this item.

In the remake, the recipe is `prejudice_torpedo_launcher` (enum 37, Research.Index 27), not the unused `p__t__l` alias. `InterStellarShip.PTL` already stores the flag, but ordinary gameplay never sets it. The existing `AddACC` control does have a live handler: do not treat it as unused or add PTL to the tool list. Any fitting correction must preserve ACC behavior, validate the current bay/ship and stock, reject duplicate or retained invalid commands, and preserve the flag through save/load.

## Discovery shares a captive-colony event

Dispatcher `$37808–$37896` consumes its active delay before observing a changed hostile-system count. Count 6 selects delay 40 and handler `$37AD8`. That handler decrements a separate nonzero cooldown; when zero, it resets it to 79 and draws an ordinal from 1–32. It scans 98 station records for type `+EE = 8` and local state `+F0 = 5`: completed human stations with a captive colony. The latter state is identified by the [station-text and recovery trace](original-self-destruct-evidence.md#ground-services-and-rebuilding).

If that ordinal is not found, `$37B24` discovers PTL only while its research is still zero. Otherwise the event draws mineral index 0–3 (iron, titanium, aluminium, carbon). A missing bit in the colony's availability mask `+90` takes the same discovery branch. A present bit adds 10,000 to that mineral's **ground stock**, capped at 50,000, then publishes bulletin 17. The destination `+AE` is confirmed by the ordinary extraction path `$23204–$2323E`; it is not a new ore vein or survey completion.

Discovery `$37772–$377DE` uses descriptor `(26, 0, 21)`, exposes research at progress 1 and preserves existing progress. Consequently, unlocking PTL immediately after Sol capture or merely observing six hostile systems would skip original conditions.

## Implementation boundaries

The remake has no separate captive-base state and its existing Hyperlight dispatcher covers only count 7. Preserve the shared countdown ordering, bulletin priority, stock-event branch and save compatibility when restoring count 6. Resolve the original fitting artwork and coordinate changes to Windows-owned bay work before editing those controls. Existing installed PTLs must not disappear on load. Combat boundaries and their completed validation are documented in [battle lifecycle](battle-lifecycle.md#empty-fleets-and-original-ptl-boundaries--2026-10-04); they do not establish this progression path.
