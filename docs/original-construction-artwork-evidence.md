# Original construction artwork bank

Traced 2026-10-02 for the missing-animation inventory, task **1215683087492495**. Seven remaining placeholder sheets have genuine construction images on Disk 2. This is research evidence; those images have not yet been added to the runtime.

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

## Implementation and acceptance still required

Confirm the display palette and black/transparent treatment before exporting the seven recovered sets. Production entry `$24C60` selects palette row 1 through `$41170`, replacing indices 5–7 with `0000/0024/0046`; other indices need the active palette, rather than a guessed whole-image tint. Existing source sheets use different RGB conversions from the raw RGB4 palette, so matching index shapes is distinct from certifying original colours.

Compare SDM/MTX's original blank stages with the remake's current static illustration fallback. Resolve the empty MFL graphics path separately. Verify stage order through paid manual/AOC production, completion and cancellation, then native rendering and Windows source/export acceptance. Small research illustrations are separate assets and remain unresolved.

Ignored reproducible evidence is under `artifacts/research/mtx/`: `construction-loader.txt`, `construction-screen-palette.txt`, `construction-codec.txt`, `decode-construction.py`, and `construction-bank.json`. The JSON records every item, bitmap address, encoded size, used palette indices and decoded SHA-256. Run `python3 artifacts/research/mtx/decode-construction.py` against the pinned disk images; it reuses `decode-planets.py` and requires only Python's standard library.
