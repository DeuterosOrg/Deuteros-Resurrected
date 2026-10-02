# Original planet and station palettes

Task **1215685674676259**, traced 2026-10-02. Planet-group palette selection is now established from Disk 1 instructions and tables. Original bitmap-index rendering and matched emulator captures remain unverified; no runtime recolouring is included.

## Reproduce the lookup

Use Disk 1 SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`; disk offset is `0x6E000 + RAM - 0x13000`. Read words big-endian. Confirm instruction boundaries before disassembly.

`$352B0–$35314` decomposes a body ID below 160. It subtracts system totals at `$1C3B8` (`42,4,12,28,6,2,34,10,22`), then parent-group sizes at `$1C3D6`, starting at the system's byte offset from `$1C3CC` (`0,11,13,17,24,26,28,36,39`). It writes system, parent-group and member-within-group indices to `$354C6/$354C8/$354CA`. The 44 groups account for all 160 records. IDs 160 and above use the separate star path.

`$35560–$355C4` takes the selected body's ID from the ship record's `+0A`, calls that decoder, then indexes `system * 11 + parentGroup`. The byte at `$353F5 + index`, **plus 2**, selects a row for `$41170`. The parallel byte at `$35458 + index`, **plus 46**, becomes artwork selector `$414AE`; `$41BB4` subsequently draws record 38. Member-within-group does not enter either lookup. The local cockpit path `$30A34–$30A76` repeats the palette lookup using body ID `+0C`.

`$41170` copies three RGB4 words from `$410DC + row * 6` into palette indices **5–7**. Station entry independently uses row 1, or row 11 for captive base state 5; Earth ground uses row 0. These are indexed palette substitutions, not a whole-image tint.

## Recovered groups

Names are decoded through the original name-offset table `$1E882` and nibble alphabets `$1FEDE/$1FEEE`, following `$1FF08`. Art values below are the raw bytes, before adding 46; the asteroid byte is retained intact.

| System | Parent bodies in table order | Palette rows | Art selector bytes |
| --- | --- | --- | --- |
| the sun | earth, mercury, venus, mars, asteroids, jupiter, saturn, uranus, neptune, pluto, decuria | 8, 9, 7, 3, 9, 6, 7, 4, 5, 9, 9 | 06, 05, 06, 05, 86, 01, 02, 05, 06, 05, 05 |
| proxima | atlantic, pacific | 5, 9 | 06, 05 |
| centauri | chiron, cecrops, cerberus, creon | 6, 3, 8, 9 | 06, 02, 06, 01 |
| barnard | mycenae, tyre, thebes, pompeii, jericho, crete, mari | 4, 6, 8, 9, 7, 10, 8 | 06, 05, 06, 05, 06, 01, 05 |
| lalande | nero, julius | 9, 11 | 06, 04 |
| sirius | romulus, remus | 8, 8 | 06, 05 |
| cygni | helios, lithos, burah, sulfurum, titanes, zargun, osme, radius | 10, 6, 7, 8, 7, 4, 9, 6 | 06, 05, 06, 05, 04, 03, 05, 06 |
| procyon | cambrian, cainozoic, paleozoic | 6, 5, 7 | 06, 01, 02 |
| tau ceti | alpha, beta, gamma, epsilon, zeta | 10, 8, 6, 3, 10 | 06, 05, 06, 03, 02 |

Parent-group members, including moons, use the same palette lookup. For example, Earth and its moon use row 8 (`0048 008A 0AFF`); Mercury uses row 9 (`0AAA 0CCC 0EEE`); Mars and its moons use row 3 (`0500 0800 0A00`). This does not prove that every displayed moon bitmap uses the same pixels as its parent.

The separate star-view routine `$3561C–$35676` reads word rows from `$35380`: `14,15,3,4,11,6,7,2,16`, in the system order above. It does not add 2. Do not apply the planet-byte formula to stars.

The asteroid byte `86` is added intact to 46, selecting resource **180**, not resource 52 with a colour flag. `$41BB4–$41C3E` resolves that index through the long-offset table `$41FAA` relative to `$422FA`. Resource 180 starts at `$74A1A`; ordinary selectors 47–52 resolve to `$60944/$60DBC/$611C6/$61704/$61C66/$61F2C`. This distinguishes a separate asteroid image from the planet images without claiming their compressed pixel format is already decoded.

## Remake comparison and remaining work

`StarMap.UpdateMap` already selects the parent planet's PNG when viewing a moon. Its `PlanetColor` and `PlanetStyle` select precoloured images, so those enum numbers cannot directly index the original table.

A pixel inventory finds mixed colour conversion: `Planet_White_Lines.png` uses `(170,170,170)`, `(204,204,204)`, `(238,238,238)`, matching row 9 expanded by 17. `Planet_WhiteBlue_Lines.png` instead uses `(0,64,128)`, `(0,128,160)`, `(160,224,224)`, which differs from row 8 expanded by 17: `(0,68,136)`, `(0,136,170)`, `(170,255,255)`. Treat this as a concrete asset discrepancy, not evidence to recolour unrelated UI or black pixels.

Before changing artwork, verify which original bitmap pixels use indices 5–7, verify the artwork decode, and capture the same Earth/Mars/moon and working/captive station views in the original. Compare source and exported Windows rendering. The recovered table resolves the earlier missing planet-to-palette link; it does not establish display calibration or complete the task's visual acceptance.

Local raw evidence (ignored): `artifacts/research/mtx/planet-palette-callers.txt`, SHA-256 `fdaa7ec6bb36c42d24c80b48ebcd39663e0ef1b224b390e832ff543174d840f4`; `planet-palette-mapping.json`, SHA-256 `645ce3519d602e1e345136cc1a0b349006e05286d1cd56e5159ec1fab0c77588`. The JSON records every group, member name, palette triplet and raw artwork selector.
