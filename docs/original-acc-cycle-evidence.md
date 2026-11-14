# Original ACC Complete Cycle behavior

**Complete Cycle finishes the current leg, unloads at the next ground/orbital arrival, and disengages.** It does not promise a round trip. This follow-up supports ACC investigation task **1215683087492480** without counting another completed Asana task.

## Reproducible original evidence

Decoded on 2026-10-02 from the same disk and address mapping as [the grapple-only ACC trace](original-asteroid-acc-evidence.md): disk SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`, disk offset `$6E000 + RAM address - $13000`, big-endian Motorola 68000. Local raw dumps are retained under ignored `artifacts/research/supply-pod/acc-cycle-*.txt`; the disk and dumps are not distributed with Git.

The verified control table maps Complete Cycle to `$33A30`. That handler sets bit 2 and clears bit 1 of ship byte `+$0F`, producing mode `4` under mask `6`. Engage uses `6`; Disengage uses `2`. With no pending countdown and docked state `0` or `5`, Complete Cycle calls `$33B38` to refuel/load and then `$30B48` to depart on success. Otherwise it changes the requested mode without replacing the current operation.

Ground arrival `$311A8–$311C0` and orbital arrival `$313FA–$31402` both mask the mode and branch to `$31486`. Its dispatch is:

| Mode | Original arrival behavior |
| --- | --- |
| `2` (disengaged) | Return without automatic handling. |
| `4` (finishing) | `$31492: 4eb900033ae2` calls unload; `$31498: 4ef900033cae` jumps to Disengage. |
| `6` (engaged) | Call unload at `$3149E`, refuel/load at `$314A4`, and depart if successful. |

The unload routine `$33AE2–$33B36` credits supplies to local stores. It caps stores at 50,000 and clears the pod; the remake deliberately retains overflow aboard under its existing inventory-conservation rule.

The asteroid scan automation at `$23B3E–$23B4C` requires mode `6`. Finish mode must not initiate another automatic mining approach. This establishes the scan gate, not the complete lifecycle of finishing a mining expedition.

## Remake correction and acceptance

Previously the Cycle control set both flags, the interior always displayed Engaged, arrival refuelled/reloaded and departed again, and Engage could leave a finish request set. The corrected command selects one mode, preserves an existing journey/fuel wait, and unloads without refuelling or departing at the next ground/orbital arrival. Older saves containing both flags finish on arrival; explicitly choosing Engage cancels that request. Refuelling text follows the actual wait state and configured threshold.

Cases **314–329** cover controls, shuttle/IOS fuel waits, arrival with full stores, save round trips, legacy flags, command changes, the asteroid scan gate, and real shuttle flights in both directions. Focused regressions and the full 329-case headless Mac suite pass. A separate native Mac check failed during teardown after passing its assertions; see [validation results](validation-results.md#acc-complete-cycle--2026-10-02). Native Windows automation also passes all 329 cases at `d057b9d`; physical desktop acceptance remains pending. Preserve the distinction between automatic regression fixtures and physical controls or normal campaign progression.

## Changing ACC commands during asteroid activity — 2026-10-04

Engage at `$339E4–$33A18` and Complete Cycle at `$33A30–$33A68` change the mode flags, then call resupply/departure only when the action countdown is zero and the original hull state is **0 or 5**. Original mining is state **13**, dispatched at `$2394C–$23952`; its handler reads the retained asteroid byte at `$23C38`. The command handlers do not clear that byte. The same identified Disk 1 bytes and aligned instructions are checked by `artifacts/research/acc-asteroid-activation/verify.py`, with the trace in `handlers.txt` (ignored research artifacts).

The remake uses `Docked` for both mining and station berths. Starting either ACC mode from Off therefore incorrectly cleared the asteroid and entered the station refuel/launch path. With sufficient fuel it departed immediately; otherwise it could wait for nonexistent asteroid fuel with its mining resource gone. The shared activation now preserves the current scan/action and restricts immediate resupply to non-asteroid berths. Actual travel already clears scans in `Ship.EngageEngine`.

New case 617 reproduces the missing asteroid through the real ACC controls before the correction. It verifies both commands from Off while scanning, approaching, mining and launching, with zero and 100 fuel; save/reload retains the asteroid and pending countdown. The mining scenarios consume another eligible update and require additional matching ore without departure. This is regression evidence; normal desktop and Windows results must be recorded separately.
