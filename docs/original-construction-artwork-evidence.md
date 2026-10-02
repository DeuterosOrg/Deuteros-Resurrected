# Original construction artwork bank

Traced 2026-10-02 for the missing-animation inventory, task **1215683087492495**. Seven remaining placeholder sheets have genuine construction images on Disk 2. The seven frame sets, original blank SDM/MTX stages and five missing research illustrations are now wired through the existing resource paths. Full 403-case Mac/Windows validation passes at `5053982`; desktop acceptance remains pending.

## Source and loader

Disk 2 SHA-256: `99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a`. The bank begins at disk offset **`0x29400`**, with 32 big-endian 16-bit offsets relative to that base. Each item record begins with its zero-based item ID and contains the graphics-section offset at `+2`. Text descriptions provide an independent identity check against the remake's research IDs minus one.

Disk 1 `$38964–$389A4` loads this `0xF200`-byte bank after the larger image bank and stores its address in `$12FE0`. `$2F5E0–$2F6A8` resolves the selected item through that table; the low-memory path reads the same disk region into the item cache `$2E564`. This supersedes the older RE-pack hypothesis that the second bank was a palette table.

Production drawing `$24D18–$24D76` reads the current product minus one, resolves the record, and chooses its stage using the byte at factory `+2`. Each image has a big-endian encoded-byte length followed by its compressed bitmap; the next image starts after rounding that length up to an even boundary. Stage one uses the first bitmap, stage two the second, and stage three the third. Drawing uses x-word 4 and y44: **64×48 pixels at x64, y44**.

The [four-plane codec](original-planet-palette-evidence.md#bitmap-decode-and-supplied-asset-checks) decodes all 84 nonempty stage records with exact encoded-length consumption. There are 25 items with visible artwork, three with entirely blank stages, and four empty graphics sections. All three decoded derrick stages match every RGB pixel of the supplied 64×56 PNGs at y4 (**9,216 pixels**); alpha was excluded because supplied PNGs mix transparent and opaque black.

## Remaining sheet gaps resolved at source

| Runtime item | Original index | Three stages |
| --- | ---: | --- |
| `pulse_blaster_laser` | 9 | Genuine 64×48 artwork recovered |
| `g_chassis` | 12 | Genuine 64×48 artwork recovered |
| `star_drive` | 13 | Genuine 64×48 artwork recovered |
| `prejudice_torpedo_launcher` | 26 | Genuine 64×48 artwork recovered |
| `star_drone` | 29 | Genuine 64×48 artwork recovered |
| `prison_pod` | 30 | Genuine 64×48 artwork recovered |
| `sonic_blaster` | 31 | Genuine 64×48 artwork recovered |
| `s__d__m`, `m__t__x` | 18, 23 | Each has three valid bitmaps containing only index zero |
| `hyperlight`, `m__f__l` | 22, 24 | Zero-length graphics sections; do not decode into the following record |

Alien artifact index 0 also has blank stages. Fuel indices 4/14 have empty sections. An empty graphics section alone does not establish manufacturing eligibility.

## Runtime recovery and remaining acceptance

The committed [recovery script](../scripts/recover_construction.py) verifies both disk hashes and writes one construction atlas, 27 stage resources and five small research PNGs. It uses the traced RLE codec with output bounded to the image extent: the original returns immediately on filling the last row, so a final repeat command may specify more words than the canvas needs. Requiring the entire run to fit would incorrectly reject real source bitmaps. Encoded record boundaries remain checked. Run `python3 scripts/recover_construction.py <disk1.adf> <disk2.adf> --check` to reproduce and compare all 33 files.

Production entry `$24C60` selects palette row 1 through `$41170`, replacing indices 5–7 with `0000/0024/0046`. The export retains the established remake metal ramp for indices 0–4: black, `(160,160,96)`, `(128,128,64)`, `(96,96,32)`, `(64,64,0)`. Remaining entries come from `$1ED24` plus that production override, using nibble×16 expansion. This keeps the supplied artwork's colour convention; raw original RGB4/default colours and physical display calibration are a separate fidelity question. Construction pixels, including index zero, remain opaque.

`$41966–$419EE` maps a one-based item through byte table `$4190A`, adds 77, composites that bitmap through the `$41BD4/$41EB0` masked-blit path, and copies the scratch image to the small illustration area. The blitter combines all four planes to distinguish transparent index zero from opaque black index five. Missing Pulse Blast Laser/MFL/Prejudice/Prison/Sonic illustrations resolve to resources **87/101/103/107/108**, respectively. They decode with exact boundaries (allowing one alignment byte); the runtime PNGs place their original pixels on a 48×48 transparent canvas with a four-pixel top margin, matching original y54 inside the existing y50 control.

Cases **386–403** exercise paid manual/AOC production for all nine stage sets: material charges, all three stages, completion/idle, local SDM/MTX installation without extra stock and retained fallback behavior. Eighteen native Mac checks pass, including **165,888 construction pixels** and **96,768 research pixels** with background compositing. Cases 386 and 400 reproduced the missing assets and incorrect SDM illustration before the changes. The extractor's `--check` passes. Reviewed screenshots include Pulse Blast Laser, Star Drone and SDM.

The empty MFL graphics section still uses its now-recovered static illustration fallback; this is not claimed as an original construction sequence. Hyperlight remains research-only. Some existing research PNGs (including Star Drone) are oversized diagrams; their replacement and complete original small-panel geometry need a separate check. Original stage timing and Windows desktop source/export acceptance remain outstanding.

Ignored reproducible evidence is under `artifacts/research/mtx/`: `construction-loader.txt`, `construction-screen-palette.txt`, `construction-codec.txt`, `decode-construction.py`, and `construction-bank.json`. The JSON records every item, bitmap address, encoded size, used palette indices and decoded SHA-256. Run `python3 artifacts/research/mtx/decode-construction.py` against the pinned disk images; it reuses `decode-planets.py` and requires only Python's standard library.

The five small-image mappings and decoded/runtime hashes are retained in `artifacts/research/mtx/research-illustration-mapping.json`; the masked-blit and palette call sites are in `research-illustration-lookup-and-mask.txt`. Research-page controls share the recovered resource paths: cases 404–405 and reviewed native Mac captures now exercise those controls at `d0d15f7`; Windows desktop acceptance remains pending. The null-recipe Research correction is recorded in the validation report. Read-only follow-up maps the seven oversized diagrams to original resources 89/91/99/100/95/90/106. SCG resource 89 is 48×45 with nine nonzero pixels on its final row, so adding the current four-pixel margin would exceed a 48-pixel canvas. Resolve placement without cropping before replacing it; ignored `oversized-research-image-mapping.json` retains dimensions and pixel hashes.
