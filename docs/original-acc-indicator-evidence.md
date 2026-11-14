# Original ACC cockpit indicator

Source follow-up to Craig's [Windows ACC observation](https://app.asana.com/1/507237966097081/project/1214891399253076/task/1215683087492480), checked 4 October 2026. **The original lamp uses a cycling red palette entry; no engaged-only flashing condition was found in the traced paths.** The implementation follow-up below uses both this source trace and the recorded gameplay measurement.

## Source and artwork

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. RAM addresses map to disk offsets by adding `$5B000`. Decode with Capstone 5.0.7, big-endian M68000.

The fitting path `$336D8–$33740` checks/sets ship hardware bit 5 and consumes item `$0F`. Interior rendering `$3018C–$3019E` checks that bit and draws template `$30`. Its record at `$4153A` selects bank `$42`, a 32×13 indexed image. All **41 palette-14 lamp pixels** and **26 palette-13 highlight pixels** match the existing `Ship_Component_ACC.png`, translating its coordinates by `(8, -6)`. This comparison covers the lamp, not the remake's rearranged surrounding frame.

## Pulse and conditions

Interior entry `$30060` selects palette 2. `$41170` reads mode 1 from table `$4115A` and stores it at `$202B8`. Vertical-blank code `$202DC–$2034C` advances palette colour 14 every five interrupts, cycling **RGB4 `$F00`, `$E00`, `$A00`, `$500`**. At nominal PAL 50 Hz this is 0.1 seconds per step and 0.4 seconds per cycle. The recording measurement below corroborates the cycle; original hardware cadence remains unmeasured. Palette 13 stays outside this pulse routine.

The fitted-idle/engaged/finishing text is selected separately at `$3075E–$3078C`. These paths do not select different lamp pulse modes. Inactivity dimming code temporarily selects mode 0, whose four entries are all `$800`, then restores the previous mode. The caller trace below corrects the earlier assumption that this was evidence for modal dimming.

## Verification boundary

The local reproducible check is `artifacts/research/acc-indicator/verify.py`; `facts.json` and aligned `verified.txt` retain the results. Run with `uv run --with capstone==5.0.7 --with pillow python artifacts/research/acc-indicator/verify.py`. Exploratory dumps contain data/misaligned ranges and are not the verified trace.

The recording below covers fitted-idle, engaged and Complete Cycle displays. Modal dimming and return remain unobserved. Implementation must match the pulsing lamp pixels without tinting static highlights or changing ACC simulation. Windows desktop and matching-export checks remain separate.

## Original recording measurement — 2026-10-07

The retained [original-game recording](https://www.youtube.com/watch?v=Kg5hOVYS3eA), identified by its uploader as FS-UAE gameplay, contains all three ACC states. Inspection of the full frames confirms the displayed status; analysis uses the original 25 fps frames without resampling. The lamp's outer red patch repeats every **10 recorded frames (0.4 seconds)** while its central highlight stays comparatively steady.

| Visible ACC state | Local clip interval | Frames | Correlation at a 10-frame lag |
| --- | --- | --- | --- |
| Engaged | 180.00–183.96 s | 100 | 0.9774 |
| Finishing (Complete Cycle) | 189.00–190.96 s | 50 | 0.9551 |
| Disengaged | 205.00–205.96 s | 25 | 0.9524 |

Ten frames is the strongest correlation among lags 1–19 in each interval. The measured lamp red-channel ranges are 157, 147 and 132 levels respectively; the highlight control ranges are only 4, 11.5 and 21.5 levels. This supports a pulse independent of ACC engagement. It does not identify exact RGB4 levels from compressed video; those come from the decoded palette table. The disengaged observation is short, and the 25 fps recording cannot resolve every 50 Hz update.

Reproduce with `python3 artifacts/research/acc-indicator/footage-20261007/measure.py` using the installed FFmpeg and NumPy. The script verifies clip SHA-256 `95a83a3bd404adf5ca253cc677a256a1b44035d6726c1944fd21116f37c73c2f`, records decoded-crop hashes and full signals, and checks cycle and highlight bounds. Full inspected frames, contact sheets and `measurements.json` accompany it. Stream copying retained an earlier keyframe, so these local times are not exact YouTube timestamps. Emulator speed/video settings are unknown. The chart overlay covers the lamp, so its flat patch is **occlusion, not dimming evidence**. No remake animation or acceptance count changes in this research checkpoint.

## Lamp animation implementation — 2026-10-07

`ShipInterior` now replaces only the 41 palette-14 pixels in four cached copies of the existing ACC texture. It displays RGB8 `FF0000`, `EE0000`, `AA0000`, `550000` for 0.1 seconds each, using the same RGB4 expansion as the existing planet views. The 26 highlight pixels and surrounding artwork remain unchanged. Elapsed scene time drives the pulse independently of game-day advancement and ACC Engage/Complete Cycle/Disengage state; ordinary status refreshes preserve its phase. No ACC route, cargo, fuel or save format changes are involved.

Case 633 first failed against the static lamp. It checks every texture pixel, the four-level order and cadence, long-frame rollover, unchanged simulation, fitting/removal and cache reuse across Shuttle, IOS and SCG scenes. Native Mac screenshots retain all four frames; the independent pixel audit compares the rendered button against the source mask, including the unchanged highlights. Evidence and the runnable audit are under `artifacts/acc-pulse-20261007/`.

The bright and dim native frames are included for review: [bright lamp](images/acc-lamp-bright.png), [dim lamp](images/acc-lamp-dim.png).

Build (14 existing warnings, zero errors), **110 focused fresh-process regressions**, native case 633 and strict startup pass. The strengthened cadence assertion also passes in fresh headless and native runs. `python3 artifacts/acc-pulse-20261007/audit.py` verifies their logs, all four rendered frames and original save hashes.

A separate native pointer check loaded the unedited day-15901 campaign, opened WAYFARER's ACC through the animated lamp, selected Engage, Complete Cycle and Disengage, saved to a temporary slot, reloaded and reopened the cockpit. The date and 232 fuel remained unchanged. Engage correctly initiated launch and incremented the pilot's action count once; the saved ACC was stopped. The owned game exited 0 through its window close button with a clean log. Temporary saves/logs are retained; the user's original files were restored byte-for-byte. This bounded control/save check adds no campaign progression or Windows acceptance.

This is a bounded lamp correction. The scene's existing pause/dimming behavior remains: a paused tree freezes its current frame. The source caller investigation below shows that original mode 0's constant `$800` belongs to inactivity dimming; it is not established as a modal requirement. Original pause/inactivity observation, global phase across scene changes and matching Windows/export acceptance remain open. The subsequent [full 633-case aggregate and new export audit](validation-results.md#633-case-complete-integration-checkpoint--2026-10-07) pass at `20ce403`; no new Windows execution or task acceptance is claimed.

## Dimming caller correction — 2026-10-07

The previous notes labelled the dimming gap as modal behavior without tracing its caller. The verified direct caller is the main loop at `$405B6`: it enters `$4069A` only when counter `$40410` reaches **60,000** and the selected screen is not `$11`. The VBlank path at `$203B6–$203C4` adds four to that counter while `$202C6` is clear. The main loop clears the counter when input word `$1FFC8` changes. At nominal 50 Hz the threshold corresponds to **300 seconds**; no original-runtime duration was measured.

The dimmer sets `$202C6`, halves the 16-colour palette, saves the pulse mode and selects mode 0. It waits until any of input words `$1FFC8`, `$1FFCE` or `$1FFD4` changes, then clears the flag, restores the palette and restores the saved pulse mode. This establishes an inactivity path. It does **not** establish that opening an ACC, chart or other modal should force the lamp to `$800`. The recorded chart still occludes the lamp.

Reproduce with `env PYTHONPATH=/Users/craigfletcher/.cache/uv/archive-v0/oakE0kB6MQAF7P-1 python3 artifacts/research/acc-indicator/verify-inactivity.py`, or run the script with Capstone 5.0.7 installed. The script verifies the original disk hash, exact branch/counter/restore bytes, complete aligned ranges and the sole direct branch/call into the dimmer found in `$1D000–$45000`. Retained trace SHA-256: `fd890aebe822db8fef6a7cc0435933d448e662b5766245a1723502d47cb6c7cc`. This scan does not prove the absence of indirect calls or another dimming path. Evidence is in `artifacts/research/acc-indicator/inactivity-20261007/`. No runtime behavior or acceptance count changed in this correction.
