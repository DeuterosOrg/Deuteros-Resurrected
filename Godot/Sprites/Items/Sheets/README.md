# Construction source sheets

These PNGs are byte-for-byte copies of the repository's supplied `SourceMaterials/SpriteSheets/` files. No pixels, palettes or labels were regenerated or edited.

| Item | Source filename | SHA-256 |
| --- | --- | --- |
| IOS chassis (`i_chassis`) | `item_interplanetary_chassis.png` | `c9beeab2e220a50195f4384bb7e89ea29763c7245bc76ba9995060ab75658996` |
| Interplanetary drive (`i_drive`) | `item_interplanetary_drive.png` | `03c1b73f1d110e5bc4f86567b7baf3a9cbc05a1f99071c639337a54fa5e5342a` |
| Resource-station frame (`r_frame`) | `item_resource_frame.png` | `5c4a980fb69d58110b6b2f1437cb97638e2b337e7f0298a6cf99e11de815189a` |

All three sheets are 224×216. Their three unlabelled construction frames occupy `Rect2(8, 8, 64, 56)`, `Rect2(80, 8, 64, 56)` and `Rect2(152, 8, 64, 56)`, in left-to-right order. The matching `../Production/<item>_<stage>.tres` atlas resources select those rectangles. Production's existing stage counter chooses the frame; simulation timing and recipes are unchanged.

The sheets use opaque magenta (`#FF00FF`) as a background key. `SourceSheet.tres` masks only that colour at rendering time; black and every other source colour remain visible. The material applies only to construction atlas textures and is removed when returning to an ordinary PNG or idle image. Cropping excludes the larger diagram, book illustration and filename label.

Cases 206–211 check manual/AOC stage transitions and existing PNG fallbacks. Native cases 206/208/210 compare every rendered frame pixel against the sheet or the underlying scene background, and can write captures via `DEUTEROS_SCREENSHOT_DIR`. Headless runs cannot validate shader output. Windows rendering and the original game's frame cadence still require acceptance.

Other candidate sheets contain `place`/`p` labels, blank boxes or copied placeholder drawings. They were not promoted into runtime construction frames; see [the media inventory](../../../../docs/media-gap-inventory.md).
