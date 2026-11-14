# Moon chart coverage — 7 October 2026

Uranus defined five moons but exposed four chart buttons. The last button selected Titania, leaving Oberon unavailable through ordinary deposit/course selection. The original disk establishes five positions. The same audit found two misplaced moon positions and three missing planet-image paths elsewhere.

## Original source

Use Disk 1 SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`, with disk offset `RAM + 0x5B000`. The [existing body decoder evidence](original-planet-palette-evidence.md#reproduce-the-lookup) supplies the 44 parent groups and 160 member records.

At `$35578`, the global parent-group index from `$354CE` is doubled, then used to read a big-endian word from `$1C402`. The mask is retained at `$222A6`. `$355F0–$35600` installs coordinate table `$35316`, artwork selectors `$354BB`, and calls `$356F0`.

The renderer sets eleven iterations at `$35700`, discards the parent bit at `$35704`, then shifts the next satellite bit at `$35706`. A clear bit skips drawing. `$35736–$3573A` advances the coordinate/artwork entries and loops. The eleven coordinate pairs have monotonically increasing x values 3 through 13. Thus bits 1–11 describe chart slots 0–10 from left to right; these are display positions, not moon IDs.

| Parent | Original mask | Previous slots | Correct slots |
| --- | --- | --- | --- |
| Mars | `0085` | 1, 8 | 1, 6 |
| Uranus | `0AC9` | 1, 5, 6, 10 | 2, 5, 6, 8, 10 |
| Julius | `0469` | 1, 4, 5, 9 | 2, 4, 5, 9 |

All other parent groups already match the original masks. Uranus is the only group whose visible-button count omitted a defined moon. The audit does **not** establish an artifact blocker: the current artifact selector excludes the already-assigned Sol system, and every other system had enough moon buttons.

The ignored `artifacts/moon-routes-20261007/verify-original.py` checks the pinned disk, lookup/renderer instructions, coordinates, all 44 masks and member counts. Its before/after logs retain the three discrepancies and the corrected zero-discrepancy result. The full recovered table is in `original-chart-comparison.json`.

## Implementation and save compatibility

`CoreData` changes the three default lists. `SaveStorage.Deserialize` repairs only those known old defaults, accepting their equivalent ordering. It preserves custom lists and does not change the save version, bodies, assets, progression or selected destinations. The shared `StarMap` then supplies the corrected buttons to both deposit analysis and ship course selection; its sequential moon-order binding needs no change.

Exhaustive navigation also logged nonexistent `Planet_Red_MassiveRings.png`, `Planet_White_Massive.png` and `Planet_Yellow_MassiveRings.png` for Cercops, Creon and Paleozoic. `PlanetImageName` now resolves those three naming combinations to supplied Red Rings, White Giant and Yellow Rings assets. The original selectors are respectively 48, 47 and 48; the existing [bitmap comparison](original-planet-palette-evidence.md#bitmap-decode-and-supplied-asset-checks) establishes the White Giant match and separately retains known ring-pixel discrepancies. This fixes missing artwork without claiming those older pixel discrepancies are resolved.

## Regression and native evidence

New cases 642–644 reproduce wrong Oberon selection, the misplaced Mars slot and missing legacy repair before the correction. Afterward they pass:

- Real pointer selection of Oberon in IOS and SCG cockpits, accepted local course, ACC endpoint update, serialization and chart reopening.
- Original positions for all 44 parent groups, every one of the 160 bodies reachable, moon-specific deposit readouts, non-null planet artwork and the three corrected image paths. The initial strict run's missing-resource failures remain retained even though its assertions passed.
- Whole-world migration comparison, repeated-load idempotence and preservation of custom charts.

The fresh complete run passes **644/644** regressions, build, strict import and startup, with exit 0. All **19 tooling checks** pass with the original ending disk supplied; the first tooling invocation skipped that disk-dependent check and was rerun with its required input. The final audit checks every discovered case, byte-identical runtime/test/script source and restoration of the isolated project setting. Logs and `audit.py`/`audit.log`/`result.json` are under ignored `artifacts/moon-routes-20261007/`. Existing build warnings remain.

Normal native verification starts from the preserved day-21613 Titania campaign (`2ed001696f65d447286bc99c962dce76fac061b1cc06408db5b1f12dd0529cd9`). Loading and immediately saving changes only the three chart definitions and elapsed timers. Physical clicks select Miranda, Ariel, Umbriel, Titania and Oberon with their respective deposit readouts. PROSPECTOR's course chart selects Oberon; saving/reloading and reopening retains it. A whole-world comparison finds only its course/ACC destination changing from Jupiter to Oberon. No gameplay tick occurs: day 21613, date `3121 514.00`, fast time off throughout. Native close exits zero, strict logs pass, and the original save directory is restored byte-for-byte.

Native evidence is under ignored `artifacts/moon-routes-native-20261007/`: `start-slot-1.json`, `migrated-slot-2.json`, `oberon-course-slot-3.json`, `reloaded-slot-4.json`, `native.log`, `exit-result.json`, and passing `audit.py`/`audit.log`. The main campaign continuation remains the original Titania checkpoint; the Oberon course check is a separate verification branch. Matching Windows acceptance and original-emulator visual comparison remain open. Counts remain **35/48 tasks with implementation evidence; four locally accepted requirements**.
