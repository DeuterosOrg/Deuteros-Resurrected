# AMA mining and cargo compatibility

Findings for **1215685674676221 — AMA: investigate source code**, decoded 2026-10-02–03. The named research questions are answered below; full original mining fidelity remains under investigation.

## Original evidence

Uses the same Disk 1 image and address conversion as [the asteroid ACC trace](original-asteroid-acc-evidence.md): SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`, disk offset `$6E000 + RAM address - $13000`.

The ship update at `$23936` requires hull byte `+3 == 2`; state `$0D` invokes mining `$23C14` through the branch at `$23952`. State `$0C` invokes the scanner instead. Mining entry `$31834` requires a valid scan and zero-based class at least 5, then sets state 7, action `$0D` and countdown 2. The scheduler consumes updates, including fractional increments; it does not wait for two displayed whole days.

`$23C30–$23C7A` scans three module words at `+$16/+$18/+$1A`. It skips non-supply modules, full pods and partial pods containing another mineral. The first zero-count pod or compatible partial pod receives ore. Empty pods are identified by quantity, regardless of the old mineral bits.

If none can accept the ore, `$23CA2` jumps to `$30B48`. Its state-`$0D` branch reaches `$30C08`, setting launch state 1, return-to-scan action `$0C` and countdown 2. No incompatible cargo is overwritten. In automatic scan handling, the selected-mineral path `$23BD4–$23C0E` likewise searches compatible pods; exhaustion jumps to departure `$30EA8` instead of starting another mining approach.

The mining amount block `$23C7C–$23C86` calls the random routine, masks with `$1F`, then adds `$0C`: numeric range **12–43**. It updates only the selected pod and clamps at 250. This establishes arithmetic bounds, not uniformity or ore per remake day.

## Generated classes and minerals

The scanner at `$23AF8` uses the same slot/clock gate with mask 7 rather than mining's mask 3. When eligible, `$23B14–$23B3A` takes the random result's low three bits as zero-based class and bits 3–6 as a sixteen-entry mineral-table index. Table `$364BE` contains `01 02 03 04 09 0A 0B 0D 01 02 09 0B 03 0A 04 0D`: each of eight material IDs appears twice. Rendering at `$365B2–$365B8` adds text base `$32`; the corresponding strings are Titanium, Aluminium, Carbon, Copper, Paladium, Platinum, Silver and Silica. Mining separately adds one when storing the material ID in a supply pod.

Class rendering adds one (`$36574–$36578`), establishing visible classes **1–8**. The mass table `$364AE` contains **50, 100, 250, 1,000, 5,000, 10,000, 25,000, 60,000**. The image selector table `$364CE` groups classes 1–3, 4–5 and 6–8. This agrees with the remake's existing mass/artwork mappings.

The remake's `Next(0, 6)` calls excluded classes 7/8 and the last mineral-list entry, Silica; Copper was absent from the list altogether. Generation now includes the eight established minerals and all eight classes. This restores the candidate set and equal table weights; it does not reproduce the original RNG state sequence. A caller-supplied standard `Random` permits deterministic regression sampling; normal play uses `Random.Shared`.

Case **443** first reproduced only 36 of the 64 possible class/mineral combinations, then only 20 of the original 32 mining amounts after correcting generation. The amount range now includes **12–43**. Seeded samples check all combinations, class masses/artwork groups, fresh scan state and amount bounds. The case also mines generated class-7 Copper and class-8 Silica through the actual ship updater. Headless and native Mac checks pass; compatibility cases 305–313 pass. Full 443-case Mac validation and audited Windows cross-export pass at `e68c6bb`; Windows execution remains pending. Initial test setup/registration failures are retained separately and are not counted as gameplay reproductions.

Raw scan/yield/display instructions: `artifacts/research/ama-generation/scan-yield-display.txt`, SHA-256 `b166ea7ce8c98d8dc59ad93c6bc2b56780a51e6aedfe273e82b51d2cbf6c29a5`.

## Clock and short-cycle findings

The disk starts `$1378E` at **310,000,000**, corresponding to the displayed year 3100. Date rendering at `$22BF2–$22C56` derives the year by dividing by 100,000, the three-digit day from the next three decimal digits, and the fraction from the remainder modulo 100. Thus a 100-unit increment advances one displayed day.

`$20536` selects increment 100 for time advancement; `$20510` restores increment 1. The interrupt at `$20414` raises pending flag `$20291`, then `$20422–$2043A` applies the increment. While the flag is set, `$203DE–$203E4` prevents another increment. The simulation consumer `$23CA8` requires that flag, calls the ship updater `$2384E` at `$23CFE`, then reaches `$22BB4` to clear the flag. This establishes one ship update per consumed clock increment, rather than repeated mining on every rendered frame.

Within that update, the mining gate compares `(ship[+4] - 1) & 3` with `(clock >> 7) & 3`. For phase 0, stepping from the initial clock by whole days permits mining on days **2, 7, 12, 17, 22, 23, 27, 28, 32, 33, 38…**. The resulting 1-, 4- and 5-day gaps follow the clock bits; there is no random instant-cycle roll. Other phases shift the schedule. This is a calculation of the traced rule, not an emulator gameplay capture. Fractional advancement can produce different intervals, so the sequence assumes uninterrupted 100-unit steps.

The amount calculation reads neither pilot rank nor asteroid size. Class is an entry gate (zero-based class at least 5); it does not scale the mining amount once mining is active. The manual entry checks nonzero fuel and scan state, then schedules two consumed updates before `$31608` enters mining state `$0D`. Wider scanner/crew eligibility still requires tracing. Byte `+4` is mapped to the allocated ship slot below; the remake now persists its zero-based equivalent.

## Remake correction

Previously, `ShipInterior.UpdateShips` checked for any partial supply pod, then used `First` to find an empty/matching one. If only incompatible cargo had space, the second search threw `Sequence contains no matching element`. Zero-count pods retaining an old mineral could also trigger that exception. ACC could initiate docking with the same unusable cargo layout.

The correction selects a compatible pod explicitly, caps its quantity, and launches when none exists on a mining attempt. ACC returns home when a selected mineral cannot fit. Existing cargo remains intact. Cases **310–313** reproduce these failures and cover empty, partial, incompatible and full pods. The prior successful-mining fixture now supplies an empty pod; its old incompatible cargo setup had encoded the erroneous docking behavior.

## Remaining investigation

The amount range, generated set and clock-bit cadence are corrected. Cases **487–492** cover natural/manual mining phases, the eight-phase scanner, stable slot reuse and reload, deterministic legacy allocation, malformed slot rejection and two-update approach/departure. Phase evaluation uses the absolute original epoch plus saved centidays. Repeated callbacks without an advancing update cannot mine again. Off-phase scanning preserves the previous result.

IOS slots are allocated per star; SCG slots are global. Existing fleets larger than the original sixteen-slot limit are preserved rather than truncated. Existing crew and SCG mining eligibility also remain: the original SCG dispatch confirms scanning but has no direct mining-state branch in the inspected range. Complete module/crew mapping, private star/SCG clocks, fuel cadence, full ACC expeditions, original-runtime comparison and Windows acceptance remain open. These limits remain open gameplay-fidelity work; the named research questions are assessed separately below.

Raw traces are under ignored `artifacts/research/supply-pod/ama-{dispatch,mining,clock}-followup.txt`. See [validation](validation-results.md#ama-compatible-cargo--2026-10-02) and the [Windows acceptance brief](windows-agent-brief.md).


## Ship-slot identity and reuse

IOS construction `$2FBB0–$2FC56` reads selected local slot `$3237E`, adds the current star index shifted left four, and uses that index for the 28-byte ship record at `$1ACA6` and ten-byte name entry at `$13128`. `$2FC2A–$2FC2C` increments it and stores the one-based value in byte `+4`. SCG construction `$2FC58–$2FD1A` similarly indexes 34-byte records at `$1BC66`, twelve-byte names at `$136C8`, and the private clock at `$137B6` using its own global slot.

The selectors `$3223E–$32252` (SCG) and `$322DE–$322F2` (IOS) scan their allocation bitmap from the low bit, choosing the first free index 0–15; a full bitmap rejects construction. IOS destruction `$362EA–$36302` clears the corresponding local bit in the star bitmap. SCG destruction `$36394–$363A4` clears its bit in `$13050`. Freed slots can be reused; surviving ships retain their slot identities.

Thus the IOS phase is `(allocatedSlot - 1) & 3` for mining and `& 7` for scanning. The star offset of sixteen cancels under both masks. A mutable ship name, pilot identity or current collection position is not that phase. The remake now saves that stable allocation and deterministically assigns missing legacy slots once. Removal leaves surviving identities unchanged; construction reuses a free slot. Save validation rejects duplicate allocated slots within a pool. The original sixteen-slot construction limit remains a separate compatibility decision.

Raw traces: `artifacts/research/ama-generation/ship-allocation.txt` and `slot-release-and-manual-entry.txt`, with hashes and reproduction notes alongside them.

## Manual entry fuel boundary

`$31810–$3182E` first rejects zero fuel (ship byte `+6`), then requires scanning state `$0C`, before calling `$31834`. That shared entry requires the valid-scan bit and zero-based class at least five. The automatic scanner at `$23C0E` jumps directly to `$31834`; this manual fuel check alone is not evidence for changing every automatic/docking caller.

Case **444** reproduced the remake entering `Docking` with zero fuel. The manual control and callback now share an eligibility check for undocked asteroid scans, class at least six and positive fuel. Rejected commands leave the mining timestamp unchanged; refuelling and an eligible scan permit retry. Focused headless and native Mac checks pass, alongside 307/310–313/443 compatibility checks. Automatic mining and generic docking are unchanged. This follow-up is included in later aggregate checkpoints; Windows acceptance remains pending.


## Departure with an exhausted tank

A fresh aligned Disk 1 trace confirms a deliberate exception to the manual fuel check. At `$30B34`, zero fuel follows `$30B3A–$30B42`: mining state `$0D` jumps directly to `$30C08`. That helper selects launch state 1, return-to-scan action `$0C` and countdown 2. The automatic no-compatible-pod path `$23CA2` reaches the same helper through `$30B48`. Ordinary landed/docked states with zero fuel remain rejected by the manual wrapper. The engine-state helper is called separately; this is not permission to create fuel or enable ordinary empty-tank departures.

The remake could consume its last unit during asteroid approach, then remain docked because the shared TakeOff guard required positive fuel. Case **595** reproduces that outcome, then verifies manual and automatic incompatible-cargo departures after a real two-update approach. A shared guard exception for a docked interstellar hull at the asteroid field fixes both callers. Cargo and zero fuel are preserved; ordinary station departure still rejects zero fuel. The earlier docked-state restriction remains. Focused headless/native checks and 13 compatibility cases pass at `b2c435c`; the full 595-case Mac aggregate and audited cross-export also pass. Windows and normal-expedition acceptance remain pending.

Trace: `artifacts/research/ama-generation/empty-fuel-departure.txt`, SHA-256 `dafa8f3e366b5aadb757bdfa210aa16faf7343e41ddfa1d14d1e56c21814195e`. This is static instruction evidence plus staged remake verification, not an original-emulator observation or an unstaged mining expedition.

A separate fuel-cadence lead remains open: original scanning/mining states `$0C/$0D` use mask `$7F` in table `$3680C`. The shared nonzero byte counter `$1BF30` cycles 1–255; only 128 passes that mask. The remake currently does not charge fuel while scanning/mining. This is not simply one unit every 128 displayed days: update consumption, counter wrap, state mapping and zero-fuel consequences need verification before changing the simulation. Raw table bytes and findings are preserved in `artifacts/research/ama-generation/fuel-mask-followup.json`.


## Manual tool rank gates and exhaustion limits

The original AMA window handler `$343B4` selects minimum rank **1** in `$33D22`; it accepts mining state `$0D` directly, or scanning state `$0C` through the shared rank check `$34014`. The grapple handler `$3441E` selects minimum rank **2** and accepts ordinary orbit/scanning states. That shared check resolves the assigned roster reference, masks its rank with 7 and rejects ranks below the requested minimum. A nonzero override byte `$1BF36` bypasses this check; its ownership is not established by this extract. The initialized rank-text table maps 1 to Pilot and 2 to Captain. Thus higher rank gates these manual tools; it does not change the already-traced 12–43 mining yield.

This is a manual-window finding, not proof that the original background scanner requires a pilot or tool: `$23AF8` has no such test, and asteroid arrival `$31590–$315AC` selects scanning after a fuel check. Keep that distinction when comparing the remake's scanner eligibility.

Exhaustion has another boundary: mining-entry completion `$31608` returns without selecting mining state when fuel is zero. This differs from the remake's current approach completion, which can finish with an empty tank; case 595 exercises that reachable remake state, not an assertion that the whole original approach behaves identically. A fuel-cadence port must resolve this transition and stranded-state behavior together. No further simulation change is claimed here.

Aligned trace: `artifacts/research/ama-generation/module-rank-and-zero-fuel.txt`, SHA-256 `ef5d8407c122ea0d79a620f635445ae42844e54adf1b10bf1b83ac5a5c25e8da`. Rank text was decoded from the initialized original table in `artifacts/research/news/crew-rank-texts.json`. These findings reduce the open eligibility questions; original-runtime and complete expedition checks remain outstanding.


## Independent mining approaches

Original `$31834–$3186A` validates the current hull's scan/class and writes its own approach state/action/countdown. Completion `$31608–$3161A` checks that hull's fuel and sets its mining state; `$30E32` only updates its engine flag. None of these operations reserves a station bay or checks another mining hull. The IOS scheduler walks individual records and dispatches mining for each eligible slot.

The remake reused the ordinary station occupancy guard for the asteroid field. Existing case 492, extended to two independently equipped IOS hulls and a mid-approach reload, reproduced the second hull remaining `Docking` after two updates. Runtime `bcaeb1c` exempts asteroid approaches from that station guard. Both hulls now mine in their own saved phases; one departure leaves the other mining. The same regression keeps ordinary station occupancy blocked. Focused headless/native checks pass; aggregate and Windows results are recorded separately. This is staged fleet evidence, not a normal two-IOS expedition.

## Fuel exhaustion dispatch follow-up

The aligned helper trace resolves previously open effects without changing fuel behavior. The initialized global counter byte is zero; the five absolute references found in this disk image all belong to `$2384E` and its three hull loops. Original action table `$237D8` maps actions `$0C/$0D/$11` to `$31590/$31608/$3145A`.

`$30E2C` sets engine bit 6 and `$30E32` clears it; neither helper checks or reserves a bay. `$3143A` stops active ACC through `$33CAE`, clears the engine bit, then selects stranded state `$10`, destruction action `$11` and a six-update countdown. Its caller `$31590` takes that path when asteroid arrival has zero fuel. By contrast, an already scanning/mining IOS reaches `$23AF8/$23C14` immediately after the shared fuel deduction even when the last unit was consumed; the inspected dispatch does not call the stranded helper there. Mining completion `$31608` simply returns if fuel is zero. Therefore adding a periodic fuel decrement alone would interact incorrectly with the remake's generic five-update undocked loss rule and reachable empty-tank approach.

Raw aligned instructions and action-table targets: `artifacts/research/ama-generation/fuel-exhaustion-helpers.txt` (SHA-256 `110f540606e474ad8752de1b6135bcb446dfa2c17abcc63642065582149fc49f`) and `fuel-exhaustion-manifest.json`. Full original runtime behavior, save/reset ownership of the counter, and a coherent port of exhaustion remain open; no emulator observation is claimed.

## Research task acceptance

The current task asks about yield, short cycles, size and pilot skill, and records provisional cargo observations. These research requirements are locally accepted on 2026-10-03:

| Question | Source-backed answer |
| --- | --- |
| Ore per cycle | 12–43 before available pod capacity: random value masked with 31, plus 12 at `$23C7C–$23C86`. This establishes bounds, not uniformity. |
| Why a one-day cycle? | The consumed-update gate compares `(slot-1)&3` with `(clock>>7)&3`. Whole-day advancement can produce adjacent eligible days; fractional increments change intervals. There is no random instant-cycle roll. |
| Does asteroid size matter? | Entry `$31834` requires visible class 6 or higher. Size does not multiply the mining yield. |
| Does pilot skill matter? | Manual AMA access requires Pilot through `$343B4/$34014`; the yield routine reads no pilot rank. Background scanner and SCG eligibility remain distinct. |
| Full, mixed and partially filled cargo | `$23C30–$23CA2` chooses an empty or compatible partial pod, caps at 250 and preserves incompatible cargo. Partial filling does not immediately launch; a later eligible attempt with no usable pod triggers departure. |

The disk identity, aligned addresses, raw trace hashes and phase calculations above make the findings reproducible. This completes the requested source investigation, not an original-emulator observation or full AMA feature acceptance. Fuel cadence/stranding, original-runtime comparison, SCG mining and unstaged expeditions remain documented follow-ups. No Asana status was changed.
