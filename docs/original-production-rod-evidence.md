# Original production rod animation

Task **1215691951441144**, traced 2026-10-02. The three frames, draw position, playback order and active-screen gate are identified. This is separate from the three product-construction stages. Runtime wiring and four focused regressions now pass on Mac headless/native; original emulator and desktop acceptance remain pending.

## Image identity

Use Disk 1 SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`, with disk offset `0x6E000 + RAM - 0x13000`. The bitmap-offset table is `$41FAA`, relative to `$422FA`; the [decoded bitmap format](original-planet-palette-evidence.md#bitmap-decode-and-supplied-asset-checks) supplies the four-bitplane pixels.

Resource 23 at `$5580C` is the 256×168 production background: its pixel classes match the left 256 pixels of the supplied 272×168 `Sprites/Scenes/Production.png` exactly. Entry `$24C60` draws it through record 28. The rod in that static background matches resource **128**.

| Resource | RAM | Pixels | Matching UI sheet region | Index-byte SHA-256 |
| --- | --- | --- | --- | --- |
| 128 | `$7144A` | 32×9 | `(544,392,32,9)` | `ca90fe82203780910d2d8af0116a2d6e8957589e23ecbedca593afba2980fa90` |
| 129 | `$714DA` | 32×9 | `(544,456,32,9)` | `ba6322943df714d6cac9f1430d2075163c285b8d897888dc2c71ddd57674273a` |
| 130 | `$7156A` | 32×9 | `(544,424,32,9)` | `100979511b9dd7d25586aeb9bf9a87c04fd2f0ead2ef1c1a164dc82223c4d406` |

Each decoded frame matches all 288 pixels in its corresponding `SourceMaterials/SpriteSheets/UI.png` region after palette identification. The sheet's SHA-256 is `6a8de89008d53b26df9019344e420a384e029dfcf2f997494f5a68c581bcaf90`. Palette indices 0–4 map to black, `(160,160,96)`, `(128,128,64)`, `(96,96,32)`, `(64,64,0)` in that supplied sheet. No resizing, guessed order or substitute art is needed. Each compressed resource ends at the next pointer, allowing one alignment byte for resource 130.

## Trigger and cadence

`$25128–$25166` enters the production screen and sets screen selector `$22D34` to 4. `$250CC–$25126` enables animation word `$202C8 = 2` only on that screen with a nonzero current-product byte at the selected factory record `+3`; otherwise it clears the word. This gate does not inspect the staff count or available materials directly. The same selected record supports the manual/AOC production screen.

The vertical-blank callback calls `$20584` at `$203AA`. `$20584–$205E2` exits when the animation word is zero, and clears it when the current screen is no longer 4. Otherwise it increments counter `$20582`, draws only on even counts, shifts the counter right by the animation word (2), and wraps when the resulting frame reaches 3. It selects resource `130 - frame`, then draws record **87** through `$41B4E`.

That record places the 32×9 image at **x240, y96**: stored x-word 15, y96, screen stride 40 bytes. The repeating resource order is **130 → 129 → 128**. At nominal 50 Hz, each steady frame lasts four vertical blanks (80 ms), or **12.5 fps**; this is independent of simulation fast-forward. The first active draws from a zero counter occur at blank 2 (130), 4 (129), 6 (129), 8 (128), 10 (128), 12 (130). The counter is retained when the gate is inactive, so uniform-loop startup is not a proven substitute for the initial phase.

## Implementation and acceptance

`Production.tscn` now overlays three atlas regions from an unchanged runtime copy of the supplied sheet, with nearest filtering. `Production._Process` maps elapsed display time to nominal 50 Hz ticks and follows the recovered even-tick/shift/wrap algorithm. The display-only counter is retained across screen instances, never serialized into production state; each new screen initially shows resource 128, as the original background did. Idle factories and tree pause stop advancement. Construction-stage pictures remain separate.

Cases **382–385** cover ground/orbit × manual/AOC. They verify all 864 original index bytes against the decoded hashes, initial and repeating phase, sub-tick accumulation, redraw/pause/resume/navigation, active products without staff, selected-factory isolation, idle/completion/cancellation and unchanged serialized factory state. Fast-forward flags do not change cadence. Normal Godot frame processing is also exercised, rather than only calling the callback directly. Native Mac checks compare all 288 rendered pixels of each frame at x240/y96 in all four contexts (**3,456 pixels**) and preserve screenshots. The missing-node failure is retained under `artifacts/validation/evidence/production-rod/red`; focused passing logs/captures are under `native-final`.

Before claiming original timing parity, compare a recording with the original video mode and first-frame phase identified. Native tests should check all frame pixels and exact placement; Windows desktop acceptance must confirm visible motion in source and export.

Raw extract: ignored `artifacts/research/mtx/production-rod-animation.txt`, SHA-256 `d6a4d2e435e584c36fb624f5da10520c7d6115bb66ebd230ce81d7923977a245`. The Asana still establishes the requested location; the instructions and byte-for-byte frame matches establish the animation sequence.
