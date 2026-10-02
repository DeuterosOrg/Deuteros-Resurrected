# Original alien transmission sequence

Asana **1215691951441134 — Alien messages**, direct disk audit on 2026-10-02. The transmission scheduler, message table and capture triggers below are established from instructions; they are not an original-game playthrough or an implemented remake feature.

## Reproduce

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`; RAM maps to disk offset `0x6E000 + address - 0x13000`. Disk 2 SHA-256: `99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a`. Its message block at `$26800` loads at RAM `$29540`; entries are big-endian word offsets relative to that block.

Capstone 5.0.7 extracts and the decoded table are under ignored `artifacts/research/alien-messages/`. Decode each code entry separately: nearby data words can otherwise produce plausible false instructions. The parallel implementation's `sim/hydroid.gd` explicitly labels its random delays and fixed reveal locations as reconstructions; they are not used as the specification here.

## Start and scheduling

Both war transitions write **10** to byte `$1C37E`: `$7C04C` in the trade-threshold branch and `$7C2A0` in the other war cinematic. Both also set war state `$1BF32` to 2. The timer is decremented at `$23E0A` in the master simulation consumer. On reaching zero, it calls `$37E70`. Earlier discovery/event branches can defer this step; these are eligible consumed updates, not a measured number of seconds or an unconditional number of displayed days.

`$37E70` first returns if suppression byte `$1EE16` is nonzero. Otherwise it displays the research department's introductory notification (message 13 initially, 14 subsequently), waits for acknowledgement, then calls the alien renderer `$3A410`. No independent inventory-wide comms-pod test appears in this handler. The encounter prerequisites and suppression behavior still need runtime acceptance.

## Message stages

Word `$1C380` selects message **stage + 24**. Renderer `$3A410` records it with the high bit set for News replay. At the end it loads the next delay from the byte table `$3A37C`:

| Stage | Message entry | Content | Delay after display |
| --- | ---: | --- | ---: |
| 0–2 | 24–26, shared offset `$17F6` | Repeated language-learning transmission | 250, 250, 80 |
| 3 | 27, `$18F3` | Introduction and observation of the war | 200 |
| 4 | 28, `$1AC5` | Trust and request to recover the device | 0 |
| 5–12 | 29–36, shared `$1CA3` | Segment location, inserted from `$1C382` | 0 |
| 13 | 37, `$1DA4` | Final assembly/use instructions | 0 |

The stage advances below 12; stages 12 and 13 remain at their current value. Zero stops automatic countdown scheduling. Text rendering also changes case through `$3A3A6` and selects the alien bitmap font table at `$3A098`; reproducing the words alone does not reproduce the cryptographic presentation.

The glyph reader at `$1FC38–$1FC5E` computes `(character - 32) * 8`, reads the font pointer from `$1F99C`, then reads eight bitmap rows. `$3A098` is **font data, not executable callback code**. Its uppercase letters are readable capitals; lowercase letters are cryptographic symbols. The stage-0 mask is zero, stage 1 uses `$08945949`, and stage 2 onward use a freshly seeded mask at `$3A378`; spaces rotate the selected mask before subsequent character decisions. The existing `deuteros-alien.ttf` and alien themes are unused by runtime scenes. Sampling its vector outlines at the original 8×8 cell centres matches **90 of 91 glyphs** (characters 32–122), including every letter. Only `!` differs: the TTF adds a filled first row (`7070707070007000` versus original `0070707070007000`). This verifies shapes, not native rasterization or text layout. Font SHA-256: `2b91863c340652335f700e5dcfb5533daaa7366bb172d6c3e03d92b80382e4f8`; `font-correspondence.json` records the FontTools winding-number comparison.

News replay at `$397EA` uses the recorded message and temporarily decrements/restores `$1C380` for rendering. It does not call the stage-advancement tail or reset the transmission timer.

## Segment and final triggers

Capture processing at `$35C06–$35C16` decrements the system's remaining hostile count and calls `$35C5C` when it reaches zero. If that system has no previously assigned artifact, the latter chooses and stores a location using the original per-system range/base tables, copies it to `$1C382`, and arms a **two-update** transmission countdown. It also sets a separate research-discovery flag. This is event-driven location assignment, not periodic selection from a fixed eight-planet list.

The initializer at `$385B6` writes `$FF` to the Sun's artifact-location byte `$1C384`, disabling assignment there. This independently rules out the remake's initial Earth artifact.

Artifact unloading at `$32C02–$32C44` selects descriptor `$19E54` and updates its completion byte at offset 2 directly. Deliveries produce **12, 24, 36, 48, 60, 72, 84, 100**. At the eighth delivery it selects stage 13, arms a two-update countdown and appends item ID 1 to the order list at `$19E12`. This is not an increased research ceiling followed by a separate scientist-driven completion step.

## Device manufacture and activation

The original device reuses item **ID 1**, zero-based index 0. Its 40-byte descriptor at `$19E54` begins `01 02 00 01`: item ID, factory mode, initial completion, required production rank. All sixteen material-cost words are zero; the final mass word is `$07D0` (2000). The item-name table entry `$101` reads **Unknown**, and tool label `$51` reads **Unknown Item**. Do not invent an additional item or recipe from the parallel reconstruction.

Factory eligibility at `$24B60` and `$24ED0` checks completion, factory mode and staff rank. Mode 2 is set by the station initializer at `$20FE8`; Earth ground initialization sets mode 1 at `$20DB4`. The material check/debit at `$25034–$25072` accepts the zero recipe. Ordinary production completion reaches `$233EE–$233FA`, then stock increment `$240DC`; item 0 has no dedicated completion bypass. Thus a missing recipe must not be treated as proof that the finished device cannot be manufactured.

The tool-action dispatcher `$34076–$340F6` requires a mounted tool word (`$8000` type bits) and dispatches item ID 1 through `$33F74` to **`$34F96`**. This handler requires ship state 3 and pilot rank at least **4 (Warlord)** through `$34014`. State 3 is written by ordinary arrival at `$312C2`, distinct from docking/docked states 4/5. The rank helper requires an assigned crew record unless override byte `$1BF36` is active. There is no hull-class test inside this activation handler; the upstream fitting selector provides that restriction.

### Fitting restriction

Selector `$32B7E–$32BDA` indexes the longword table at `$32B1E` by `(ship hull byte - 1) * 4`. It shifts each mask right, admitting an item only when its bit is set and descriptor completion is at least 100. The normal hull masks are:

| Hull | Mask | Eligible zero-based item indices |
| --- | --- | --- |
| Shuttle (1) | `$02028022` | 1, 5, 15, 17, 25 |
| IOS (2) | `$8A3A8222` | 1, 5, 9, 15, 17, 19, 20, 21, 25, 27, 31 |
| SCG (3) | `$CA1A8023` | 0, 1, 5, 15, 17, 19, 20, 25, 27, 30, 31 |

Thus item 0 (the completed device) is **SCG-only**. A.M.A. (21) is IOS-only and Prison Pod (30) SCG-only. Each starship list has eleven entries, matching the eleven visible selector rows. The remake currently shares one eleven-item list across hulls; simply adding the device would create twelve choices and an unreachable or out-of-range final row. Both rendering and selection must use the same hull eligibility rule, while old incorrectly fitted equipment remains removable.

Selection `$32F44–$33016` returns existing equipment, requires replacement stock, debits it through `$2415C`, and writes the mounted item ID. Only Derrick (index 1) uses the multi-unit branch; item 0 follows the single-unit path. No device-specific mass check occurs in this selection block. Modified hull codes, broader mass behavior and visual acceptance remain separate checks. `tool-hull-eligibility.txt` and `tool-selection.txt` preserve the instruction extracts.

Success jumps to `$38032`, which selects transition mode 4 and enters the loader at `$12800`. The disk-1 overlay at offset `$2C00` maps to `$12800`; its mode-4 path loads the overlay at disk offset `$5800` into `$20000`. That overlay's dispatcher reaches `$21926`, selecting asset offset `$6E000` from table `$21708`. This establishes the ending transition, not its displayed content: disk selection, script interpretation, assets and an original-runtime ending remain unverified. Normal game RAM-to-disk mapping must not be used for these overlays.

Raw evidence: `transmitter-candidates.txt`, `transmitter-context.txt`, `manufacturing-eligibility.txt`, `production-completion.txt`, `activation-and-cost.txt`, `module-action-dispatch.txt`, `module-eligibility.txt`, and the `transition-*` / `ending-*` extracts in the reproduction directory. Only the named instruction boundaries are evidence; some exploratory extracts include adjacent data or truncated instructions.

## Recovery correction

Artifact unloading now credits completion directly using the original sequence, stops at 100, and announces completion once. Scientists neither advance it nor receive promotion credit. Research displays recovered progress without requiring a research team. Existing `ResearchLimit` stores recovered progress; no save field or format version was added. Loading ordinary legacy credits (multiples of 11 through 99) converts them once, caps the old ninth credit at completion, and preserves ships, cargo and artifact locations. Already completed and nonstandard edited states are left unchanged.

Case **135** reproduced the real grapple unload leaving completion at 1%; it now reaches 12% and rejects duplicate close credit. **410** covers all eight increments, a ninth delivery, scientist independence, visible progress and save references; **411** covers legacy zero-through-nine credits, idempotence, retained cargo and edited-state preservation. Focused headless/native Mac checks pass; full cross-platform validation is pending for this change. This corrects recovery credit, not the entire campaign.

## Remake work still required

The remake still has no saved transmission stage/countdown or alien News replay. `CoreData` still preassigns nine artifacts, including Earth; capture-driven assignment, device manufacture/fitting and the ending are unfinished. These remaining rules cannot safely serve as acceptance fixtures for the final sequence.

Implement the saved sequence together with deferred event delivery, capture-driven artifact assignment, direct completion credit and replay that preserves progression. Cover both war starts, competing discoveries, interruption, save/load, all eight recoveries and final instructions. Device activation also depends on the [Warlord promotion path](original-warlord-evidence.md). Resolve the existing clock integration decision before expressing eligible update counts as elapsed days. Verify cryptographic rendering and campaign progression in source and exported Windows builds; the task remains open.

See the [integration design](alien-message-integration.md) for existing code paths, save compatibility and verification gates.
