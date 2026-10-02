# AMA mining and cargo compatibility

Partial findings for **1215685674676221 — AMA: investigate source code**, decoded 2026-10-02. The mixed-cargo crash is corrected; full original mining fidelity remains under investigation.

## Original evidence

Uses the same Disk 1 image and address conversion as [the asteroid ACC trace](original-asteroid-acc-evidence.md): SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`, disk offset `$6E000 + RAM address - $13000`.

The ship update at `$23936` requires hull byte `+3 == 2`; state `$0D` invokes mining `$23C14` through the branch at `$23952`. State `$0C` invokes the scanner instead. Mining entry `$31834` requires a valid scan and zero-based class at least 5, then sets state 7, action `$0D` and countdown 2. Do not equate that countdown with remake days without tracing its scheduler.

`$23C30–$23C7A` scans three module words at `+$16/+$18/+$1A`. It skips non-supply modules, full pods and partial pods containing another mineral. The first zero-count pod or compatible partial pod receives ore. Empty pods are identified by quantity, regardless of the old mineral bits.

If none can accept the ore, `$23CA2` jumps to `$30B48`. Its state-`$0D` branch reaches `$30C08`, setting launch state 1, return-to-scan action `$0C` and countdown 2. No incompatible cargo is overwritten. In automatic scan handling, the selected-mineral path `$23BD4–$23C0E` likewise searches compatible pods; exhaustion jumps to departure `$30EA8` instead of starting another mining approach.

The mining amount block `$23C7C–$23C86` calls the random routine, masks with `$1F`, then adds `$0C`: numeric range **12–43**. It updates only the selected pod and clamps at 250. This establishes arithmetic bounds, not uniformity or ore per remake day.

## Generated classes and minerals

The scanner at `$23AF8` uses the same slot/clock gate with mask 7 rather than mining's mask 3. When eligible, `$23B14–$23B3A` takes the random result's low three bits as zero-based class and bits 3–6 as a sixteen-entry mineral-table index. Table `$364BE` contains `01 02 03 04 09 0A 0B 0D 01 02 09 0B 03 0A 04 0D`: each of eight material IDs appears twice. Rendering at `$365B2–$365B8` adds text base `$32`; the corresponding strings are Titanium, Aluminium, Carbon, Copper, Paladium, Platinum, Silver and Silica. Mining separately adds one when storing the material ID in a supply pod.

Class rendering adds one (`$36574–$36578`), establishing visible classes **1–8**. The mass table `$364AE` contains **50, 100, 250, 1,000, 5,000, 10,000, 25,000, 60,000**. The image selector table `$364CE` groups classes 1–3, 4–5 and 6–8. This agrees with the remake's existing mass/artwork mappings.

The remake's `Next(0, 6)` calls excluded classes 7/8 and the last mineral-list entry, Silica; Copper was absent from the list altogether. Generation now includes the eight established minerals and all eight classes. This restores the candidate set and equal table weights; it does not reproduce the original RNG state or replace provisional scan cadence. A caller-supplied standard `Random` permits deterministic regression sampling; normal play uses `Random.Shared`.

Case **443** first reproduced only 36 of the 64 possible class/mineral combinations, then only 20 of the original 32 mining amounts after correcting generation. The amount range now includes **12–43**. Seeded samples check all combinations, class masses/artwork groups, fresh scan state and amount bounds. The case also mines generated class-7 Copper and class-8 Silica through the actual ship updater. Headless and native Mac checks pass; compatibility cases 305–313 pass. Full 443-case Mac validation and audited Windows cross-export pass at `e68c6bb`; Windows execution remains pending. Initial test setup/registration failures are retained separately and are not counted as gameplay reproductions.

Raw scan/yield/display instructions: `artifacts/research/ama-generation/scan-yield-display.txt`, SHA-256 `b166ea7ce8c98d8dc59ad93c6bc2b56780a51e6aedfe273e82b51d2cbf6c29a5`.

## Clock and short-cycle findings

The disk starts `$1378E` at **310,000,000**, corresponding to the displayed year 3100. Date rendering at `$22BF2–$22C56` derives the year by dividing by 100,000, the three-digit day from the next three decimal digits, and the fraction from the remainder modulo 100. Thus a 100-unit increment advances one displayed day.

`$20536` selects increment 100 for time advancement; `$20510` restores increment 1. The interrupt at `$20414` raises pending flag `$20291`, then `$20422–$2043A` applies the increment. While the flag is set, `$203DE–$203E4` prevents another increment. The simulation consumer `$23CA8` requires that flag, calls the ship updater `$2384E` at `$23CFE`, then reaches `$22BB4` to clear the flag. This establishes one ship update per consumed clock increment, rather than repeated mining on every rendered frame.

Within that update, the mining gate compares `(ship[+4] - 1) & 3` with `(clock >> 7) & 3`. For phase 0, stepping from the initial clock by whole days permits mining on days **2, 7, 12, 17, 22, 23, 27, 28, 32, 33, 38…**. The resulting 1-, 4- and 5-day gaps follow the clock bits; there is no random instant-cycle roll. Other phases shift the schedule. This is a calculation of the traced rule, not an emulator gameplay capture. Fractional advancement can produce different intervals, so the sequence assumes uninterrupted 100-unit steps.

The amount calculation reads neither pilot rank nor asteroid size. Class is an entry gate (zero-based class at least 5); it does not scale the mining amount once mining is active. The manual entry checks nonzero fuel and scan state, then schedules two consumed updates before `$31608` enters mining state `$0D`. Wider scanner/crew eligibility still requires tracing. Byte `+4` is now mapped to the allocated ship slot below; the remake still needs a stable saved equivalent before changing scheduling.

## Remake correction

Previously, `ShipInterior.UpdateShips` checked for any partial supply pod, then used `First` to find an empty/matching one. If only incompatible cargo had space, the second search threw `Sequence contains no matching element`. Zero-count pods retaining an old mineral could also trigger that exception. ACC could initiate docking with the same unusable cargo layout.

The correction selects a compatible pod explicitly, caps its quantity, and launches when none exists on a mining attempt. ACC returns home when a selected mineral cannot fit. Existing cargo remains intact. Cases **310–313** reproduce these failures and cover empty, partial, incompatible and full pods. The prior successful-mining fixture now supplies an empty pod; its old incompatible cargo setup had encoded the erroneous docking behavior.

## Remaining investigation

The amount range and generated mineral/class set are corrected. The remake still uses its provisional five-day/first-day chance. Original mining is gated by `(ship[+4] - 1) & 3` matching `(clock[$1378E] >> 7) & 3`; clock units and caller cadence are now traced above. A stable remake mapping for the original ship-slot phase and fractional clock still needs a compatibility decision before replacing timing. Pilot effects, SCG availability, Complete Cycle, scan generation and normal campaign behavior also remain open. These cargo fixes do not complete the AMA research task.

Raw traces are under ignored `artifacts/research/supply-pod/ama-{dispatch,mining,clock}-followup.txt`. See [validation](validation-results.md#ama-compatible-cargo--2026-10-02) and the [Windows acceptance brief](windows-agent-brief.md).


## Ship-slot identity and reuse

IOS construction `$2FBB0–$2FC56` reads selected local slot `$3237E`, adds the current star index shifted left four, and uses that index for the 28-byte ship record at `$1ACA6` and ten-byte name entry at `$13128`. `$2FC2A–$2FC2C` increments it and stores the one-based value in byte `+4`. SCG construction `$2FC58–$2FD1A` similarly indexes 34-byte records at `$1BC66`, twelve-byte names at `$136C8`, and the private clock at `$137B6` using its own global slot.

The selectors `$3223E–$32252` (SCG) and `$322DE–$322F2` (IOS) scan their allocation bitmap from the low bit, choosing the first free index 0–15; a full bitmap rejects construction. IOS destruction `$362EA–$36302` clears the corresponding local bit in the star bitmap. SCG destruction `$36394–$363A4` clears its bit in `$13050`. Freed slots can be reused; surviving ships retain their slot identities.

Thus the IOS phase is `(allocatedSlot - 1) & 3` for mining and `& 7` for scanning. The star offset of sixteen cancels under both masks. A mutable ship name, pilot identity or current collection position is not that phase. Integrating it into the remake needs a saved stable allocation and deterministic legacy-save mapping; deriving it again from ship order after a removal would incorrectly change surviving ships' schedules. This resolves the original field meaning, not the outstanding fractional-clock/migration decision.

Raw traces: `artifacts/research/ama-generation/ship-allocation.txt` and `slot-release-and-manual-entry.txt`, with hashes and reproduction notes alongside them.

## Manual entry fuel boundary

`$31810–$3182E` first rejects zero fuel (ship byte `+6`), then requires scanning state `$0C`, before calling `$31834`. That shared entry requires the valid-scan bit and zero-based class at least five. The automatic scanner at `$23C0E` jumps directly to `$31834`; this manual fuel check alone is not evidence for changing every automatic/docking caller.

Case **444** reproduced the remake entering `Docking` with zero fuel. The manual control and callback now share an eligibility check for undocked asteroid scans, class at least six and positive fuel. Rejected commands leave the mining timestamp unchanged; refuelling and an eligible scan permit retry. Focused headless and native Mac checks pass, alongside 307/310–313/443 compatibility checks. Automatic mining and generic docking are unchanged. This follow-up is later than the fully validated 443-case checkpoint; no full 444-case or Windows acceptance claim is made.
