# Original construction artwork bank

Traced 2026-10-02 for the missing-animation inventory, task **1215683087492495**. Seven remaining placeholder sheets have genuine construction images on Disk 2. The seven frame sets, original blank SDM/MTX stages and five missing research illustrations are now wired through the existing resource paths. The construction batch passed full 403-case Mac/Windows validation at `5053982`. The illustration follow-up below passes full 406-case Mac/Windows validation at `7c7db5e`; Windows desktop acceptance is pending.

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

The committed [recovery script](../scripts/recover_construction.py) verifies both disk hashes and writes one construction atlas, 27 stage resources and 32 pairs of Research/Production illustrations. It uses the traced RLE codec with output bounded to the image extent: the original returns immediately on filling the last row, so a final repeat command may specify more words than the canvas needs. Requiring the entire run to fit would incorrectly reject real source bitmaps. Encoded record boundaries remain checked. Run `python3 scripts/recover_construction.py <disk1.adf> <disk2.adf> --check` to reproduce and compare all 92 files.

Production entry `$24C60` selects palette row 1 through `$41170`, replacing indices 5–7 with `0000/0024/0046`. The export retains the established remake metal ramp for indices 0–4: black, `(160,160,96)`, `(128,128,64)`, `(96,96,32)`, `(64,64,0)`. Remaining entries come from `$1ED24` plus that production override, using nibble×16 expansion. This keeps the supplied artwork's colour convention; raw original RGB4/default colours and physical display calibration are a separate fidelity question. Construction pixels, including index zero, remain opaque.

`$41966–$419EE` maps a one-based item through byte table `$4190A`, adds 77, composites that bitmap through the `$41BD4/$41EB0` masked-blit path, and copies the scratch image to the small illustration area. The blitter combines all four planes to distinguish transparent index zero from opaque black index five. Missing Pulse Blast Laser/MFL/Prejudice/Prison/Sonic illustrations resolve to resources **87/101/103/107/108**, respectively. They decode with exact boundaries (allowing one alignment byte); the runtime PNGs place their original pixels on a 48×48 transparent canvas with a four-pixel top margin, matching original y54 inside the existing y50 control.

Cases **386–403** exercise paid manual/AOC production for all nine stage sets: material charges, all three stages, completion/idle, local SDM/MTX installation without extra stock and retained fallback behavior. Eighteen native Mac checks pass, including **165,888 construction pixels** and **96,768 research pixels** with background compositing. Cases 386 and 400 reproduced the missing assets and incorrect SDM illustration before the changes. The extractor's `--check` passes. Reviewed screenshots include Pulse Blast Laser, Star Drone and SDM.

The empty MFL graphics section still uses its now-recovered static illustration fallback; this is not claimed as an original construction sequence. Hyperlight remains research-only. The complete illustration follow-up below replaces the oversized diagrams and restores the small-panel geometry. Original stage timing and Windows desktop source/export acceptance remain outstanding.

Ignored reproducible evidence is under `artifacts/research/mtx/`: `construction-loader.txt`, `construction-screen-palette.txt`, `construction-codec.txt`, `decode-construction.py`, and `construction-bank.json`. The JSON records every item, bitmap address, encoded size, used palette indices and decoded SHA-256. Run `python3 artifacts/research/mtx/decode-construction.py` against the pinned disk images; it reuses `decode-planets.py` and requires only Python's standard library.

The initial five-image mappings and decoded/runtime hashes are retained in `artifacts/research/mtx/research-illustration-mapping.json`; the masked-blit and palette call sites are in `research-illustration-lookup-and-mask.txt`. Their original 48×48 canvases are superseded by the complete illustration recovery below.

## Complete item illustrations and original placement

The original Research entry at `$25240` selects palette row 1, builds the item controls, then resolves the selected recipe at `$25298`. Completed research takes `$253AE → $25410 → $254A8`; `$254B0` calls `$4192C`. That routine maps the one-based item through `$4190A`, adds 77 and writes draw descriptor 79 at `$416EC`. The descriptor places the bitmap at **x208/y68**. Unlike Production's `$804D` masked selector, this selector has no high mask bit: `$41BB4/$41C32` draws **opaque index zero**, as well as opaque index five. Research must not reuse Production's transparent bitmap.

Production `$41966` first draws resource 77 into its scratch area, masks the item over it, then copies **48×46 pixels to x136/y54**. The remake's page artwork already supplies that background. Separate transparent Production PNGs preserve index-zero masking; separate opaque Research PNGs preserve the direct draw. Godot's native `Keep` mode draws both at original scale, without interpolation or per-frame image conversion.

All **32 item mappings** now have recovered pairs, including the formerly absent `alien_artifact` path. The artifact intentionally has a blank 32×3 Research bitmap. The other 31 source images are 48 pixels wide. Seven 183×177 diagrams are replaced: SCG chassis, Star Drive, HED fuel, SDM, Hyperlight, MTX and Star Drone. SCG is 45 pixels tall with nine nonzero pixels on its final row. Removing the old four-pixel canvas margin and using the original 46-pixel Production area preserves that row. Existing supplied icons are also normalized from the same source, avoiding mixed offsets and transparency rules.

Case **406** checks every source bitmap's indexed SHA-256, all 32 opaque/masked pairs, exact control origins, unscaled rendering and the complete SCG row. Native Mac validation compares **113,952 rendered pixels across 60 views**; SCG Research/Production captures were reviewed. Existing cases 206/386/389/400/404/405 pass natively with the new resources; the MTX bounds assertion follows the recovered 48×46 display area. The first regression reproduced a 183×177 placeholder. Intermediate fixture failures involving transparent RGB normalization and duplicate staged research-order keys are retained separately; neither was suppressed.

Raw mappings, dimensions and hashes are in ignored `artifacts/research/mtx/all-research-illustrations.json`; the exact Research call chain is `research-screen-original-placement.txt`. Native logs/captures are under `artifacts/validation/evidence/research-diagrams/`. The remake metal palette convention is retained. Original display calibration, stage timing, Windows desktop inspection and the empty MFL construction fallback remain explicit acceptance gaps.
