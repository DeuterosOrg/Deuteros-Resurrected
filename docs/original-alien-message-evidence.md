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

Artifact unloading at `$32C10–$32C44` checks the accumulated byte against 84. At or above that threshold it selects stage 13, arms a two-update countdown and writes 100 after the shared addition; earlier deliveries add 12. The full production/ending path is not established by this excerpt.

## Remake work still required

The remake currently has no saved transmission stage/countdown or alien News replay. `CoreData` preassigns nine artifacts, including Earth, while `Unlocker` credits 11 research-limit points per delivery. Those rules differ from the traced flow and cannot safely serve as acceptance fixtures for the final sequence.

Implement the saved sequence together with deferred event delivery, capture-driven artifact assignment, collection credit and replay that preserves progression. Cover both war starts, competing discoveries, interruption, save/load, all eight recoveries and final instructions. Resolve the existing clock integration decision before expressing eligible update counts as elapsed days. Verify cryptographic rendering and campaign progression in source and exported Windows builds; the task remains open.

See the [integration design](alien-message-integration.md) for existing code paths, save compatibility and verification gates.
