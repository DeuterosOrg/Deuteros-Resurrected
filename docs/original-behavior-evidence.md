# Original behavior evidence

Research snapshot: **2026-10-02**. This follows the eight `original-evidence-needed` reports in [Asana triage](asana-triage.md), plus self-destruct and Warlord. The triage source baseline remains **`9817216`**; this document does not reclassify or close tasks and does not certify the current implementation.

The parallel repository is an evidence archive, not a second independent specification. Its current checkout is `ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9`; local `origin/inv/save` retains the reverse-engineering pack at `6fabd73bbc7fff0eb08825b2dd6043f618e596a4`, and `origin/inv/synthesis` retains the survey at `faf3a30beb07868e06e01ebb306f63b2b42affed`. Links below pin those commits. Inspect archived files locally with `git show <commit>:<path>`; switching branches is unnecessary.

**Confidence:** high means a cited instruction sequence or table supports the narrow rule; medium means recorded player observation or an incomplete semantic mapping; low means inherited implementation, hypothesis, or secondhand walkthrough claim. No original-game emulator session was performed. The MTX addendum and [trade investigation](original-trade-evidence.md) independently check bytes from the archived disk image; remake regression evidence is recorded separately in the validation results.

## What the evidence unlocks

| Task | Evidence confidence | Implementable result / remaining gate |
| --- | --- | --- |
| [1215683087492480 — grapple-only ACC](https://app.asana.com/0/1214891399253076/1215683087492480) | Low | Original grapple-only decision is missing; parallel ACC repeats C# behavior. |
| [1215685674676219 — empty supply pod](https://app.asana.com/0/1214891399253076/1215685674676219) | High for selected-pod discard; incomplete original screen availability | Original “Ditch Contents” dispatch and mutation decoded; see [instruction evidence](original-supply-pod-evidence.md). |
| [1215685674676221 — AMA algorithm](https://app.asana.com/0/1214891399253076/1215685674676221) | Low | Five-day / 16–35 / 1-in-20 constants are C#-derived, not original proof. |
| [1215685674676225 — event order](https://app.asana.com/0/1214891399253076/1215685674676225) | High for named call sites; incomplete overall | Partial master-tick order recovered; full subsystem mapping remains open. |
| [1215685674676259 — planet/background colours](https://app.asana.com/0/1214891399253076/1215685674676259) | High for scene palette loading; low for planet mapping | Scene palette loader is decoded; planet-specific palette selection and values are missing. |
| [1215716464570901 — DFCC fuel](https://app.asana.com/0/1214891399253076/1215716464570901) | Original manual fuel ratio traced | DFCC consumes ten stock per gauge unit; fitting unloads the previous tank before changing value. |
| [1215716464570923 — idle clock](https://app.asana.com/0/1214891399253076/1215716464570923) | High for raw clock cadence | Normal mode adds one raw unit per 315.6 seconds at PAL 50 Hz, subject to a consumer handshake; confirm fractional display and pause behavior. |
| [1215716464570927 — staff attrition](https://app.asana.com/0/1214891399253076/1215716464570927) | High for numeric kernel; medium for cryo semantics | Exact countdown/RNG kernel recovered; map freeze/stage identities and time units before full integration. |
| [1215685674676229 — self-destruct](https://app.asana.com/0/1214891399253076/1215685674676229) | Medium | An SDM-rigged enemy station capture destroyed station and capturing fleet in recorded play; full trigger/defusing rules missing. |
| [1215716464570913 — Warlord](https://app.asana.com/0/1214891399253076/1215716464570913) | Low | Hyperlight-specific promotion handler is now traced; travel-phase timing still needs verification. See [follow-up](original-warlord-evidence.md). |
| [1215683087492485 — MTX installation](https://app.asana.com/0/1214891399253076/1215683087492485) | High for completion flag and duplicate queue guard | Item 24 completion sets the local installed flag, bypassing ordinary stock output; automated production skips an already installed module. Implemented with cases 174–190; Windows acceptance remains pending. |
| [1216073204565506 — engine damage](https://app.asana.com/0/1214891399253076/1216073204565506) | Original recovery and escape transition traced | Damaged travel doubles duration; replacement consumes a drive and damaged removal returns none. Reconcile task wording with the original escape roll. |
| [1215691951441128 — Methanoid trading](https://app.asana.com/0/1214891399253076/1215691951441128) | High for verified instruction paths/table | [Accept/decline, exchange and war-gate rules recovered](original-trade-evidence.md). Refusal decrements; later port prose claiming all outcomes increment is contradicted by original bytes. Timeout duration and full cargo-field semantics remain open. |

## 1215685674676241 — SCG and IOS travel

[Original-game observation, M2 Addendum 7](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/docs/m2-findings.md#L219-L244) records an attempted IOS interstellar assault that required SCGs. This supports the **SCG-only cross-star hull rule**. It does not establish journey durations or promote the parallel port's provisional travel formula to original evidence.

The remake previously let IOS choose and engage another star, including through ACC. Shared `Ship.CanTravelTo` now derives star membership from the actual origin/destination body records, gates course selection and departure, and rejects unknown bodies. On valid engagement, star metadata is synchronized with those records. An invalid IOS selection preserves its current course and both ACC endpoints and shows the required hull. Invalid active/queued ACC routes stop before resupply or launch; shuttle ground/orbit automation retains its separate path. Cases 236–247 cover these paths and an SCG outbound/return journey.

A second concrete failure came from the existing duration formula: Earth and Cerberus both have local orbital index 2, yielding zero duration despite different stars. On the next day, remaining time became negative and the `==0` arrival check never resolved. Case 248 reproduced the stranded SCG; arrival now resolves an elapsed deadline (`<=0`). **The formula is unchanged and is not certified as original interstellar timing.** This fixes indefinite transit within the current model, not the missing duration specification.

The assembly follow-up (cases 249–263) fixes remake consistency defects: bay entry no longer discovers star drives, SCG visibility follows its own chassis, fittings consume parts, dismantling returns them with capacity checks, occupied berths reject duplicate hulls, and the bay exposes five usable mounts. These are local model/conservation fixes, not a decoded original dismantling contract.

Full SCG acceptance still needs normal research/manufacture/assembly, equipment and cargo verification, source/export Windows travel, and a trace or measurement of original interstellar duration. No normal SCG/interstellar unlock producer was found in the current remake. The parallel port's `DIVERGENCES.md` row 46 explicitly calls its Sol-cleared unlock triggers reconstructions; do not adopt those as verified original rules. Existing saves already midflight are not migrated by the new departure guard; establish recovery expectations before changing those states.

## 1215683087492485 — MTX installation

On 2026-10-02, the original Disk 1 image at parallel commit `faf3a30beb07868e06e01ebb306f63b2b42affed` was read without changing that checkout. The unmodified image is named `Deuteros - The Next Millennium (1991)(Activision)(M3)(Disk 1 of 2).adf`; SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. Its main image is disk bytes `$6E000..$DAA00`, loaded at RAM `$13000`, as documented by the archived `extract_deuteros.py`. Capstone disassembly independently confirmed the relevant [dispatch analysis](https://github.com/WizzoUK2/deuteros-parallel/blob/faf3a30beb07868e06e01ebb306f63b2b42affed/outputs/decompiled/dispatch.md).

- Record 24 at `$19E54 + 23 * 40` has ID `$18`, mass `$02D2` (722), and nonzero recipe amounts 500, 82, 100 and 40. These match the MTX row in `SourceData/Deuteros.html` and `CoreData.cs`; this establishes the item identity instead of relying on the archive's provisional event labels.
- `$23330` reads the completed item ID and subtracts one. `$233B0` compares with `$17`; `$233B6` executes bytes `08d1 0006` (`bset #6,(a1)`), then branches past the ordinary stock-increment call at `$233FA`. The automated completion path has the same flag-setting branch.
- Caller `$23630..$23638` sets `a1 = a0 + $36` before invoking the completion routine. The flag is local to that production record.
- The automated queue checks zero-based slot `$17` at `$23544`, tests bit 6 at record `+$36`, and clears the pending slot when already installed (`$23552..$23556`). Otherwise it loads the item recipe and charges materials through `$25034` before scheduling production.

These instructions support installing the MTX at its producing station, without a transferable stock item, and stopping repeat production after installation. The previous notes' generic “needs attention” description of event 24 is superseded for this item mapping. Local disassembly evidence is retained under ignored `artifacts/research/mtx/`; Windows can reproduce it from the pinned image. No original UI or timing session was recorded.

**Implementation evidence:** cases 174–190 cover captured-station discovery, research, paid production at a second station, local installation with no extra stock/duplicate material charge, Stores access only where installed, transfer/balance, and save/load. The capture, staff qualifications and an AOC are staged prerequisites; they are not granted as part of MTX discovery. Case 187 uses normal discovery/research/production/transfer controls, with subsystem updates driven by the fixture and a shortened bulletin. Route-safety cases 163–173 cover captured/missing/incomplete destinations, missing modules, stale selection and configurations with no eligible items. See [validation limits](validation-results.md#mtx-installation-and-stores-access--2026-10-02); Windows progression and original animation fidelity remain unverified.

## 1215716464570927 — staff attrition

**Original routine:** `FUN_2376C`, roster `$197C2`, 170 records of eight bytes. The [instruction listing](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/197C0_record_list.md#L64-L107) supports this kernel:

1. Return unless `$20292` is set; clear it once consumed.
2. Skip records whose type byte is zero, whose bit 5 is set, or whose low three bits are 0 or 7.
3. Subtract one from the unsigned 16-bit countdown at record `+4`. A starting value of **1 becomes 0 without an attrition attempt**; a starting value of **0 underflows and attempts attrition**.
4. On underflow, call `FUN_1F42C`, mask with 1, and subtract that result from the member count at `+2` only if the count is nonzero. Call RNG again and reset the countdown to `RNG & 15`. Zero members never wrap to 255.

The [clock disassembly](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/1378E_clock_resolution.md#L102-L117) sets this gate when **`$202C2`**, increased by the broadcast delta, reaches 10,000; it then subtracts 10,000. `$2027C` is the separate mode-rate accumulator. The older roster document's introductory `$2027C` attribution is incorrect.

**Corrections to the research prose:** the roster document's old “resource shipment” interpretation is superseded by the [current roster identification](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/sim/state.gd#L14-L40). The [attrition note's](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/Deuteros-RE-Pack-2026-05-21/02-REFERENCE/team-attrition.md#L70-L114) estimate of one death per 15 gate firings misses the subtract-before-underflow step. Assuming uniform independent RNG, a rearm of 0–15 takes 1–16 gate firings, averaging 8.5; a 50% loss therefore averages **17 gate firings per lost member**, while the team has members. Implement the instruction sequence, not this statistical approximation.

**Cryopod exemption now cross-referenced (2026-10-02):** direct decoding of the [identified original image](original-supply-pod-evidence.md#reproduce-the-lookup) establishes both sides of the transfer. `$329DC–$32A34` selects a ship module, separates its kind bits, and routes supply/tool pods elsewhere; the cryogenic path can display text `$BA`, “There Are No Cryogenics In Resource Colonies !”. Dispatch entry `$2139C` points to the transfer handler `$32A86`.

That handler exchanges the selected local roster reference at `$19D2C` with the pod's low 14 bits, retaining its `$C000` kind. References are one-based: subtract one, multiply by eight, add `$197C2`. `$32AEC` sets bit 5 on the team moved into the pod; `$32B00` clears it on the team moved out. Empty references bypass the write. The same local reference array is rendered as four staff slots by `$31BEA`, reading each team's type and member count. The transfer leaves the attrition countdown at record `+4` unchanged. Thus cryopod loading suspends that countdown, and unloading resumes its existing value; it does not reset or cure attrition. Raw instruction boundaries and bytes are retained under ignored `artifacts/research/staff-attrition/cryo-transfer.txt`. This is static instruction evidence, not an emulator before/after recording.

**Allocation and time integration (2026-10-02):** allocator `$22D86` clears all eight bytes at `$22DC4–$22DCA`, including countdown `+4`. Research training `$22EC4–$22EF4` allocates if needed, assigns type `$C0`, then calls promotion `$22E26`; promotion writes rank and action countdown `+3`, leaving `+4` zero. Production and marine creation use the same allocator/promotion path. Thus a newly allocated qualified team starts attrition at zero, rather than receiving a random initial grace period. `$23CD6–$23CFA` processes training and production/research before attrition. See ignored `artifacts/research/staff-attrition/allocator-and-order.txt`.

The [confirmed 0.01-day clock unit](original-clock-evidence.md) makes each 10,000-unit gate **100 displayed days**. In the remake's existing whole-day clock, crossed `CurrentDay / 100` boundaries preserve this phase without a second saved timer. Each team's countdown is persisted. Loading an older save initializes the absent countdown to zero and applies only future boundaries, without retroactive losses. Updates resolve active ground/orbital staff, factory/research staff and ship pilots once per object; cryopod occupants remain frozen. Synthetic enemy-fleet pilots are outside the human roster. Training, research and production execute before this update, then display observers see the resulting counts.

**Remaining facts and limits:** the semantic identity of immune stage 7 is unknown. It is not the traced [Warlord stage 4](original-warlord-evidence.md#rank-and-notification); neither stage 0 nor 7 is represented by the remake's current qualified staff ranks (1–3). The implementation covers that existing human staff model, not an invented special rank. Original random-sequence identity, fractional/star-clock integration and normal campaign/native acceptance remain separate work. The original routine has no minimum of one member or recovery branch; the remake's existing training can replenish a retained empty research/production team without resetting its rank or attrition countdown.

**Acceptance tests:** gate clear leaves all fields/RNG unchanged; each skipped record retains its countdown; 1→0 makes no RNG calls; 0 uses both RNG calls even for a zero-size eligible team; loss at count 1 can reach 0; rearm endpoints 0/15; saving/loading preserves countdown and gate remainder. Verify cryopod loading preserves countdown/member count, loading swaps freeze states for both teams, and unloading resumes the retained countdown. An original-game before/after recording remains useful corroboration.

## 1215716464570923 — idle time

**2026-10-02 follow-up:** [Direct byte verification and the fractional renderer](original-clock-evidence.md) confirm the normal threshold, restoration behavior and 0.01-day unit. The earlier cadence table below remains applicable; runtime pause/startup acceptance is pending.

**Original routines:** VBL ISR `$202DC`; broadcast loop `$2042C`; displayed-clock update `$2043A`; consumer `$22BB0`. The [resolved clock investigation](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/1378E_clock_resolution.md#L83-L149) gives:

| Mode | Accumulator increase per VBL | Threshold `$2027E` | Raw increment `$20280` | Earliest cadence at PAL 50 Hz |
| --- | ---: | ---: | ---: | ---: |
| Normal | 4 | 63,120 (`$F690`) | 1 | 315.6 seconds |
| Fast | 4 | 60 (`$3C`) | 100 | 0.3 seconds |
| Faster | 4 | 16 (`$10`) | 100 | 0.08 seconds |

The pending flag `$20291` prevents another accumulation/broadcast until the consumer clears it, so the numbers are cadence with the consumer keeping up, not a guarantee during every modal or screen. `$1378E` and the 25 broadcast cells `$13792..$137F5` receive the same increment; five saved states independently match. This is distinct from `$1378A`, which counts unpaused real seconds and is copied to save metadata `$1C2CC`.

**Remaining facts:** the decoded compact day-label formula is `((t / 10000) % 10) * 100 + (t % 10000) / 100`, using integer division. That demonstrates 100 raw units per integer day-label increment; it does not itself show the fractional text renderer. A raw unit representing 0.01 day is consistent with the task, but confirm the visible fractional field. The explicit `$202C6` pause check shown in the notes gates the real-seconds path; the broadcast path instead checks `$20291`. Do not assume the same pause gate without tracing the consumer/menu behavior. The note's later ratio discussion incorrectly multiplies raw-units/second by 100 again; use the disassembly and mode constants above.

**Acceptance tests:** preserve sub-interval remainder across frames and save/load; no advancement at 315.59 seconds and one raw unit at 315.6 in normal PAL-equivalent operation; 100 raw units roll fractional display into the next day once confirmed; pause/menu/blocked-news behavior matches a recorded original session. A modern clock may avoid the original UI handshake deliberately, but record that as a design choice.

## 1215685674676225 — event order

**2026-10-02 follow-up:** [Direct original-disk traces now establish the master call order and conditional event priority](original-event-order-evidence.md). Training and mining precede production, then research, attrition, ships, stock refining, Methanoid processing and SDM simulation expiry. The document records the early return, prioritized discovery branches, screen callback and clock acknowledgement separately; it does not claim all calls always execute.

This supersedes the earlier partial table and its unidentified `$3684A` entry: that routine performs stock conversion with one phase change per call and alternating local records. The remake currently mixes refining into factory processing before ships, puts research/attrition after ships, and mines Earth before training. These are concrete integration differences requiring outcome-based scenarios. The parallel port's invented scheduler is not an authority for correcting them.

**Remaining verification:** instrument simultaneous training, mining, production, research and arrival in the original; compare ACC fuel waits, discovery ordering and SDM expiry. Reconcile fractional/interstellar timing and save phases before claiming scheduler fidelity. See the linked evidence for exact addresses, reproducibility and acceptance cases.

## 1215685674676259 — planet and station background colours

The [palette investigation](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/planet_sprite_colouring.md#L70-L129) decodes a **scene** palette loader at overlay `$25070..$250F8`: relocate the scene's palette pointer at `$250A4`, store it at `$21266`, then call `graphics.library` `LoadRGB4` for 16 colours on both viewports. This establishes palette-based scene rendering. It does **not** establish the asserted five-entry planet lookup: the investigator explicitly could not find the link from planet colour data to `$21266`. Claims later in that note that each planet colour “almost certainly” chooses a full palette remain hypotheses.

**Missing facts:** the actual planet/station palette words, mapping to bodies/colour IDs, which palette indices change, and treatment of moons/occupied stations. Neither the scene loader nor the generic half-brightness routine proves a planet-specific RGB tint or bitmask. The dim routine `$406A2` computes `(word & 0x0EEE) >> 1`; its role is menu/overlay dimming.

**Next verification:** watch writes to `$21266` and palette memory while switching a fixed scene among contrasting planets, then repeat with station present/absent and a moon/parent. Save the 16 raw RGB4 words and matching screenshot for each. The notes propose Disk 2 offsets `$29400` and `$2BC00..$37400` as extraction leads; those locations are hypotheses, not recovered planet tables.

**Direct Disk 1 follow-up:** `$41170–$411B4` uses a different, verifiable scene-colour path. It indexes six-byte rows at `$410DC`, copies three RGB4 words into `$1ED24 + 10` (palette indices **5–7**), then submits the first eight colours through the graphics-library call. The table contains 21 triplets. Earth entry `$20D04` requests row 0 (`0A00 0C20 0600`); local station entry `$20ED8–$20EE4` normally requests row 1 (`0000 0024 0046`) and selects row 11 (`0ACE 0468 068A`) for captive base state 5. Thus that scene's ownership/base state changes three palette entries; it is not a whole-image RGB tint selected by planet colour. The older overlay-loader address alone must not be used as a breakpoint in this disk segment: `$21266` here lies in station-status code.

Raw table and caller instructions: ignored `artifacts/research/mtx/scene-palette-loader.txt`. Planet-body colour selection and the matching bitmap indices remain unverified; do not apply these three scene colours indiscriminately to the remake's planet textures. Compare a working and captive station on the same planet before claiming task completion.

**Related task 1214891399253092 (production-team blue):** the [decoded UI palette](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/planet_sprite_colouring.md#L195-L230) at `$1ED24` contains index 8 `$0028` = `#002288` and index 9 `$00CF` = `#00CCFF`. These are precise candidate colours. The dump alone does not prove which index the production-team sprite uses; compare its pixels/palette index before changing all shared blue UI elements.

## 1215685674676229 — self-destruct mechanism

**2026-10-02 follow-up:** [The original arming, switch, defusing/capture and station-removal paths are now traced](original-self-destruct-evidence.md). Countdown integration and original-screen acceptance remain open; the initial evidence and uncertainty below are retained.

The newest relevant [M2 Addendum 7, 2026-08-26](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/docs/m2-findings.md#L219-L244) records player testimony accompanying a Proxima snapshot: capturing an SDM-rigged Methanoid station destroyed **both the station and capturing fleet**. This provides an acceptance scenario for that observed outcome. It supersedes the parallel port's [older invented rule](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/DIVERGENCES.md#L48) that only enemy capture of a player station detonates and player recapture never triggers it.

**Missing facts:** whether detonation is unconditional, first-contact-only or affected by prior exposure/defusing; when player-installed SDMs arm; whether the carrier/pilot/cargo are included in “fleet”; and behavior of friendly recapture. The note's simultaneous FIFO type 8 / primary 0 is only a hypothesis for an SDM notice. Do not give event type 8 universal detonation semantics or implement symmetry solely from this one observation.

**Next verification:** retain a save before the first rigged enemy-station capture, record every ship/drone/pilot and station state before/after, repeat after SDM research/exposure, then test an enemy taking a player-rigged station. Recover the state bit and detonation writer while stepping capture resolution. Add deterministic regressions for each demonstrated case.

## 1215716464570913 — Warlord rank

**2026-10-02 follow-up:** [The original promotion handler and its Hyperlight-specific SCG trigger are now traced](original-warlord-evidence.md). The earlier missing-facts list below records the initial uncertainty; exact travel-phase timing and remake integration remain open.

The [specification survey](https://github.com/WizzoUK2/deuteros-parallel/blob/faf3a30beb07868e06e01ebb306f63b2b42affed/outputs/Deuteros-Spec-Survey.md#L96-L123) attributes Warlord to walkthroughs, with the claim that Admirals gain it by flying to Proxima and back; its cited manual rank list stops at Admiral. This is a research lead, not an original routine or measured threshold.

The decoded ordinary rank helper `FUN_22E26` stops at low-bit stage 3. Table `$22DEE` has type-0 durations `10,30,0,0`; [instruction/table evidence](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/197C0_record_list.md#L130-L158). This supports ordinary progression only. It neither disproves a separate Warlord path nor proves immune stage 7 means Warlord.

**Missing facts:** rank encoding, exact promotion trigger and threshold, whether a specific destination/return/battle is required, benefits, and persistence/display. **Next verification:** track one Admiral roster record through an uncontested SCG trip to Proxima and return, stopping at arrival/departure; compare with an Admiral gaining equivalent in-system actions. If promotion occurs, watch the writer changing its type/stage or status. Separately test combat participation. Do not add an arbitrary battle-win count based on the title.

## 1215685674676221 — AMA algorithm

The parallel [asteroid implementation](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/sim/asteroids.gd#L3-L19) names this remake's C# as its source. Its [five-day period, class-six minimum and 16–35 yield](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/sim/asteroids.gd#L52-L58), and first-day 1-in-20 roll, are therefore not independent original-game evidence. The decoded planetary mineral table `$1C398` concerns ground extraction; it does not establish asteroid mining constants.

**Missing facts:** eligible asteroid sizes, yield distribution, cycle timing, first-day behavior, effect of pilot rank, depletion, filling multiple/mixed pods, overflow disposal, and automatic departure. **Next verification:** save before attachment to a known asteroid; log original raw clock, asteroid identity/mass and each pod after every broadcast through at least two yields. Repeat with different pilot ranks and cargo states (empty; nearly full; wrong mineral; two pods). For probability/range claims, decode the RNG caller and mask/range operation; a handful of observations cannot prove a distribution.

## 1215683087492480 — grapple-only ACC

The parallel [ACC asteroid branch](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/sim/acc.gd#L290-L310) explicitly ports C# `ACC.cs:154–172` and requires an AMA. No original ACC handler or grapple-only emulator observation was located. This cannot validate the report that activation unexpectedly disengages.

**Missing fact:** whether the original ACC captures a selected small rock with a grapple, waits for a qualifying scan, continues a route, or intentionally deactivates; and how resource filters/full grapples affect it. **Next verification:** in the original, fit a grapple without AMA, use a qualified pilot and empty grapple, scan a capturable rock matching the selected resource, and activate ACC; record immediate action plus the next two broadcasts. Repeat no scan, wrong resource, oversized rock, held rock and a control ship with AMA. This distinguishes a missing action from an equipment prerequisite.

## 1215685674676219 — empty supply pod button

The original **“Ditch Contents !”** control dispatches to `$33F40`, which clears the selected supply pod's cargo type and quantity while preserving its pod kind. It performs no store credit or neighboring-pod write. See [the exact bytes, input dispatch and text lookup](original-supply-pod-evidence.md).

The handler has no ship-state gate, but its screen-entry route has separate state checks; original availability everywhere is not established. The remake's explicit cargo dialog follows the task's requested access during docking, travel and mining. Native Windows acceptance and an original emulator comparison remain pending.

## 1215716464570901 — DFCC fuel

[Original loading, unloading and conversion instructions](original-dfcc-fuel-evidence.md) now establish the tenfold stock cost per gauge unit, independent of drone count. The remake applies that ratio consistently to manual transfers, its supported ACC path and dismantling, while preserving the existing gauge drain. Fitting returns the old tank at its previous value before changing the flag. See [implementation and validation](validation-results.md#dfcc-fuel-cost-and-conservation--2026-10-02).

Original automatic-control availability after full DFCC conversion remains unverified; the decoded auto-refuel block itself transfers 1:1. Applying the manual ratio to modern DFCC ACC prevents a bypass and is documented as a remake consistency choice. Existing-save tanks retain their current range rather than receiving retroactive debits. Native Windows and normal campaign acceptance remain pending.

## 1216073204565506 — engine damage without DFCC

The [live task](https://app.asana.com/1/507237966097081/project/1214891399253076/task/1216073204565506), read on 2026-10-02, specifies two wartime triggers: arriving at an occupied planet, and being in orbit when that planet is attacked. It has no comments or subtasks defining recovery. Before this batch, `ShipInterior.UpdateShips` incremented `AttackedCount` before resolving arrivals and removed ships at count two, without a damaged-engine state. `Ship.Engine` distinguished only installed/not installed, and the bay could not replace an installed engine. The existing countdown is unchanged by this batch.

[Original instruction evidence](original-engine-damage-evidence.md) now establishes that damage is a separate engine bit, damaged travel remains possible at double duration, replacement consumes one matching drive, and damaged removal yields no usable spare. The identified damage write occurs on a random-result gate when escaping danger state `$14`; arrival and attack blocks establish that danger state first. DFCC installation sets the flag which bypasses the roll. The implementation now follows this traced escape rule; task-owner review of the shorthand remains part of acceptance. Original countdown-to-remake-day equivalence remains unverified, and destruction timing is unchanged. See [implementation evidence](validation-results.md#engine-damage-and-recovery--2026-10-02).

## 1215683087492480 — grapple-only ACC at the asteroids

The [original control/scan trace](original-asteroid-acc-evidence.md) proves that normal engaged ACC calls the manual Disengage handler when an asteroid scan finds no fitted AMA. The stop precedes mineral/size filtering and performs no automatic grapple capture. The remake now follows this rule; cases 305–309 cover the stop, no-scan wait, retained AMA behavior, daily scanning and save/manual capture. Complete Cycle flags, scan cadence and wider AMA fidelity remain separate investigation work. Native Windows acceptance is pending.

## Evidence limits and next artifacts

The inspected repository-local `SourceData/Deuteros.html` provides mineral/body and recipe reference material, and `SourceMaterials/Notes.txt` has selected tables/implementation notes; neither supplied the missing mechanics above. Both `SourceMaterials/GameManuals/*.pdf` files in this checkout are Git LFS pointers rather than PDF contents. A read-only attempt to obtain `Manual.pdf` at the baseline revision returned HTTP 404, so no manual-page claim here depends on reading those absent PDFs. The survey's manual/walkthrough summaries remain secondary evidence.

Highest-value next artifacts are: a cryopod roster before/after dump; a normal-mode fractional-clock/pause recording; the full master-tick call trace; five representative scene palettes; controlled DFCC fuel deltas; and an SDM capture save with exact casualty accounting. Store original-game observations with save identity, emulator/video mode, raw addresses, before/after values and reproduction steps. These can turn the remaining research gates into acceptance tests without importing the parallel remake's assumptions.
