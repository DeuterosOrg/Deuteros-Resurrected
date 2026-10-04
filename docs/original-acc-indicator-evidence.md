# Original ACC cockpit indicator

Source follow-up to Craig's [Windows ACC observation](https://app.asana.com/1/507237966097081/project/1214891399253076/task/1215683087492480), checked 4 October 2026. **The original lamp uses a cycling red palette entry; no engaged-only flashing condition was found in the traced paths.** No remake animation has been changed on this evidence alone.

## Source and artwork

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. RAM addresses map to disk offsets by adding `$5B000`. Decode with Capstone 5.0.7, big-endian M68000.

The fitting path `$336D8–$33740` checks/sets ship hardware bit 5 and consumes item `$0F`. Interior rendering `$3018C–$3019E` checks that bit and draws template `$30`. Its record at `$4153A` selects bank `$42`, a 32×13 indexed image. All **41 palette-14 lamp pixels** and **26 palette-13 highlight pixels** match the existing `Ship_Component_ACC.png`, translating its coordinates by `(8, -6)`. This comparison covers the lamp, not the remake's rearranged surrounding frame.

## Pulse and conditions

Interior entry `$30060` selects palette 2. `$41170` reads mode 1 from table `$4115A` and stores it at `$202B8`. Vertical-blank code `$202DC–$2034C` advances palette colour 14 every five interrupts, cycling **RGB4 `$F00`, `$E00`, `$A00`, `$500`**. At nominal PAL 50 Hz this is 0.1 seconds per step and 0.4 seconds per cycle; wall-clock cadence has not been measured in an emulator. Palette 13 stays outside this pulse routine.

The fitted-idle/engaged/finishing text is selected separately at `$3075E–$3078C`. These paths do not select different lamp pulse modes. Dimming code temporarily selects mode 0, whose four entries are all `$800`, then restores the previous mode.

## Verification boundary

The local reproducible check is `artifacts/research/acc-indicator/verify.py`; `facts.json` and aligned `verified.txt` retain the results. Run with `uv run --with capstone==5.0.7 --with pillow python artifacts/research/acc-indicator/verify.py`. Exploratory dumps contain data/misaligned ranges and are not the verified trace.

Before implementation acceptance, compare original fitted-idle, engaged and Complete Cycle displays, then modal dimming and return. Match the pulsing lamp pixels without tinting its static highlights or changing ACC simulation. Windows desktop and matching-export checks remain separate.
